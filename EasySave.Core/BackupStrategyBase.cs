using System.Diagnostics;
using EasyLog;

namespace EasySave.Core;

// Classe de base partagée par FullBackup et DifferentialBackup.
// Centralise la logique de copie de fichiers : pause/reprise, priorité, gros fichiers, chiffrement et logging.
public abstract class BackupStrategyBase
{
    // Service de chiffrement injecté avant l'exécution ; null = pas de chiffrement
    private ICryptoService? _cryptoService;

    // Compteur global de fichiers prioritaires encore en attente de copie (partagé entre tous les jobs parallèles)
    private static int _pendingPriorityFiles = 0;

    // Sémaphore qui bloque les fichiers non-prioritaires tant que des fichiers prioritaires restent à copier
    private static readonly SemaphoreSlim _nonPriorityGate = new(1, 1);

    // Garantit qu'un seul gros fichier est transféré à la fois entre tous les jobs parallèles (T7)
    private static readonly SemaphoreSlim _largeFileLock = new(1, 1);

    // Injecte le service de chiffrement depuis BackupManager avant le lancement d'Execute()
    public void SetCryptoService(EasySave.Core.ICryptoService cryptoService)
    {
        _cryptoService = cryptoService;
    }

    // Calcule le chemin de destination d'un fichier en reproduisant l'arborescence relative depuis la source
    protected string BuildDestPath(string srcFile, string srcRoot, string dstRoot)
        => Path.Combine(dstRoot, Path.GetRelativePath(srcRoot, srcFile));

    // Pré-enregistre tous les fichiers prioritaires avant que la copie ne commence.
    // Ferme le verrou au premier enregistrement pour bloquer immédiatement les fichiers non-prioritaires,
    // même si des jobs parallèles n'ont pas encore démarré leurs copies.
    protected void RegisterPriorityFiles(IEnumerable<string> files)
    {
        var extensions = ConfigManager.Instance.Config.PriorityExtensions;
        if (extensions.Count == 0) return;

        // Compte combien de fichiers de la liste ont une extension prioritaire
        int count = files.Count(f => extensions.Any(e => e.Equals(Path.GetExtension(f), StringComparison.OrdinalIgnoreCase)));
        if (count == 0) return;

        // Ajoute atomiquement au compteur global ; si c'était 0 avant, ferme le verrou maintenant
        int prev = Interlocked.Add(ref _pendingPriorityFiles, count) - count;
        if (prev == 0)
            _nonPriorityGate.Wait(); // ferme le verrou pour bloquer les fichiers non-prioritaires
    }

    // Bloque les fichiers non-prioritaires tant qu'au moins un fichier prioritaire est en attente globalement.
    // Les fichiers prioritaires passent directement sans attendre.
    protected void WaitIfBlockedByPriority(string filePath)
    {
        var extensions = ConfigManager.Instance.Config.PriorityExtensions;
        if (extensions.Count == 0) return;

        var ext = Path.GetExtension(filePath);
        bool isPriority = extensions.Any(e => e.Equals(ext, StringComparison.OrdinalIgnoreCase));

        if (!isPriority)
        {
            _nonPriorityGate.Wait();    // attend que tous les fichiers prioritaires soient copiés
            _nonPriorityGate.Release(); // laisse passer (le verrou est rouvert), puis continue
        }
    }

    // Copie un fichier de src vers dst en gérant : pause, gros fichiers, priorité, chiffrement et logging.
    protected void CopyFile(string src, string dst, BackupState state, CancellationToken token = default, ManualResetEventSlim? pauseGate = null)
    {
        // Vérifie une demande d'annulation avant de commencer la copie
        token.ThrowIfCancellationRequested();

        // Point de pause — attend ici jusqu'à ce que ResumeJob() soit appelé (T5)
        pauseGate?.Wait(token);

        // Crée les dossiers intermédiaires si nécessaire
        Directory.CreateDirectory(Path.GetDirectoryName(dst)!);

        // Met à jour l'état avec les chemins du fichier en cours pour l'affichage dans l'UI
        state.SourceFilePath = src;
        state.TargetFilePath = dst;
        state.Timestamp = DateTime.Now;
        StateManager.Instance.Update(state);

        // Détermine si ce fichier dépasse le seuil des gros fichiers (T7)
        var sizeKb = new FileInfo(src).Length / 1024;
        var threshold = ConfigManager.Instance.Config.LargeFileSizeKb;
        var isLarge = threshold > 0 && sizeKb > threshold;

        // Sérialise les gros fichiers : attend que le précédent gros fichier soit terminé avant de commencer
        if (isLarge)
            _largeFileLock.Wait(token);

        // Mesure le temps de transfert réel
        var sw = Stopwatch.StartNew();
        try
        {
            File.Copy(src, dst, overwrite: true);
        }
        finally
        {
            // Libère toujours le verrou, même en cas d'exception, pour ne pas bloquer les autres jobs
            if (isLarge)
                _largeFileLock.Release();
        }
        sw.Stop();

        // Décrémente le compteur de fichiers prioritaires une fois le fichier sur le disque
        var extensions = ConfigManager.Instance.Config.PriorityExtensions;
        if (extensions.Count > 0)
        {
            var ext = Path.GetExtension(src);
            if (extensions.Any(e => e.Equals(ext, StringComparison.OrdinalIgnoreCase)))
                // Si c'était le dernier fichier prioritaire, rouvre le verrou pour libérer les fichiers non-prioritaires
                if (Interlocked.Decrement(ref _pendingPriorityFiles) == 0)
                    _nonPriorityGate.Release();
        }

        // Chiffre le fichier copié si le service est actif et si l'extension correspond
        long encryptionTime = 0;
        if (_cryptoService != null)
        {
            var encryptedExtensions = ConfigManager.Instance.Config.EncryptedExtensions;
            var ext = Path.GetExtension(src);
            // Liste vide = chiffrer tous les fichiers ; sinon, chiffrer uniquement les extensions configurées
            bool shouldEncrypt = encryptedExtensions.Count == 0
                || encryptedExtensions.Any(e => e.Equals(ext, StringComparison.OrdinalIgnoreCase));
            if (shouldEncrypt)
                encryptionTime = _cryptoService.Encrypt(dst);
        }

        // Construit et écrit l'entrée de log avec les métriques de cette copie
        var entry = new LogEntry
        {
            Name = state.Name,
            FileSource = src,
            FileTarget = dst,
            FileSize = new FileInfo(src).Length,
            FileTransferTime = sw.ElapsedMilliseconds,
            EncryptionTime = encryptionTime,
            Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        Logger.Instance.Log(entry);

        // Met à jour les compteurs de progression et persiste l'état
        state.DecrementProgress(new FileInfo(dst).Length, state.TotalFilesToCopy);
        StateManager.Instance.Update(state);
    }
}
