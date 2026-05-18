using System.Collections.Concurrent;
using EasyLog;

namespace EasySave.Core;

// Singleton (GoF) — gestionnaire central de tous les jobs de sauvegarde.
// Gère l'exécution parallèle, la Pause/Reprise/Arrêt par job (T5)
// et la mise en pause automatique quand le logiciel métier est détecté (T6).
public class BackupManager
{
    private static readonly Lazy<BackupManager> _instance = new(() => new BackupManager());
    public static BackupManager Instance => _instance.Value;

    // Liste de tous les jobs configurés, indexés à partir de 1 dans l'interface
    private readonly List<BackupJob> _jobs = [];

    // Un CancellationTokenSource par job — utilisé pour Stop (T5)
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _cts = new();

    // Un ManualResetEventSlim par job — Set = en cours, Reset = en pause (T5)
    private readonly ConcurrentDictionary<string, ManualResetEventSlim> _pauseGates = new();

    // Détecteur de logiciel métier injecté via SetDetector() + timer de polling toutes les secondes (T6)
    private IBusinessSoftwareDetector? _detector;
    private Timer? _businessSoftwareTimer;

    // Flag pour éviter de répéter la mise en pause si le logiciel métier est déjà détecté
    private bool _businessSoftwarePaused = false;

    // Injecte le détecteur de logiciel métier et démarre le polling toutes les secondes.
    // Quand le logiciel est détecté : tous les jobs actifs sont mis en pause.
    // Quand il disparaît : tous les jobs en pause sont repris.
    public void SetDetector(IBusinessSoftwareDetector? detector)
    {
        _detector = detector;
        // Arrête le timer précédent avant d'en créer un nouveau
        _businessSoftwareTimer?.Dispose();
        if (detector == null) return;
        _businessSoftwareTimer = new Timer(_ => PollBusinessSoftware(), null,
            TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }

    // Vérifie toutes les secondes si le logiciel métier est actif et ajuste l'état des jobs en conséquence.
    private void PollBusinessSoftware()
    {
        if (_detector == null) return;

        bool isRunning = _detector.IsRunning();

        // Logiciel détecté pour la première fois — met en pause tous les jobs actifs
        if (isRunning && !_businessSoftwarePaused)
        {
            _businessSoftwarePaused = true;
            foreach (var jobName in _pauseGates.Keys)
            {
                PauseJob(jobName);
                // Enregistre l'événement de pause dans le log (FileTransferTime = -2 = code spécial)
                Logger.Instance.Log(new LogEntry
                {
                    Name = jobName,
                    FileSource = "",
                    FileTarget = "",
                    FileSize = 0,
                    FileTransferTime = -2, // -2 = mise en pause par le logiciel métier
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
        }
        // Logiciel fermé — reprend tous les jobs qui étaient en pause
        else if (!isRunning && _businessSoftwarePaused)
        {
            _businessSoftwarePaused = false;
            foreach (var jobName in _pauseGates.Keys)
            {
                ResumeJob(jobName);
                // Enregistre l'événement de reprise dans le log (FileTransferTime = -3 = code spécial)
                Logger.Instance.Log(new LogEntry
                {
                    Name = jobName,
                    FileSource = "",
                    FileTarget = "",
                    FileSize = 0,
                    FileTransferTime = -3, // -3 = reprise après fermeture du logiciel métier
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
        }
    }

    // Ajoute un job à la liste interne (ne démarre pas l'exécution)
    public void AddJob(BackupJob job) => _jobs.Add(job);

    // Supprime le job à l'index 1-based donné ; lance une exception si l'index est hors limites
    public void RemoveJob(int index)
    {
        if (index < 1 || index > _jobs.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        _jobs.RemoveAt(index - 1);
    }

    // --- Contrôles de lecture (T5) ---

    // Met en pause le job après la fin du fichier en cours de copie (le verrou suspend la prochaine copie)
    public void PauseJob(string jobName)
    {
        if (_pauseGates.TryGetValue(jobName, out var gate))
            gate.Reset(); // Reset = fermé = le thread de copie se bloquera au prochain Wait()
    }

    // Reprend un job en pause en ouvrant son verrou
    public void ResumeJob(string jobName)
    {
        if (_pauseGates.TryGetValue(jobName, out var gate))
            gate.Set(); // Set = ouvert = la copie peut continuer
    }

    // Arrête immédiatement un job via CancellationToken ; la prochaine vérification dans CopyFile lèvera une exception
    public void StopJob(string jobName)
    {
        if (_cts.TryGetValue(jobName, out var cts))
            cts.Cancel();
    }

    // Alias de StopJob — conservé pour la compatibilité avec d'anciens appels
    public void CancelJob(string jobName) => StopJob(jobName);

    // Retourne l'état de lecture actuel d'un job : Stopped si inconnu, Paused si son verrou est fermé, Running sinon
    public PlaybackState GetPlaybackState(string jobName)
    {
        if (!_cts.ContainsKey(jobName)) return PlaybackState.Stopped;
        if (_pauseGates.TryGetValue(jobName, out var gate) && !gate.IsSet)
            return PlaybackState.Paused;
        return PlaybackState.Running;
    }

    // Service de chiffrement injecté depuis App.xaml.cs, transmis à la stratégie avant chaque exécution
    public ICryptoService? CryptoService { get; set; }

    // --- Lancement des jobs ---

    // Lance le job à l'index 1-based donné sur le thread courant (bloquant)
    public void RunJob(int index)
    {
        if (index < 1 || index > _jobs.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        RunJob(_jobs[index - 1]);
    }

    // Lance tous les jobs en parallèle et attend que tous soient terminés
    public void RunAll()
    {
        var tasks = _jobs.Select(job => Task.Run(() => RunJob(job))).ToArray();
        Task.WaitAll(tasks);
    }

    // Prépare et exécute un job : crée son CancellationTokenSource et son ManualResetEventSlim,
    // choisit la stratégie (Full ou Differential), injecte le crypto, puis lance Execute().
    private void RunJob(BackupJob job)
    {
        var cts = new CancellationTokenSource();
        var gate = new ManualResetEventSlim(true); // initialement ouvert = job démarre en mode "running"

        // Si le logiciel métier est déjà actif, démarre directement en pause
        if (_businessSoftwarePaused)
            gate.Reset();

        // Enregistre les contrôles pour que PauseJob/ResumeJob/StopJob puissent les retrouver par nom
        _cts[job.Name] = cts;
        _pauseGates[job.Name] = gate;

        try
        {
            var strategy = GetStrategy(job.Type);
            if (CryptoService != null)
                strategy.SetCryptoService(CryptoService);
            var state = new BackupState { Name = job.Name, PlaybackState = PlaybackState.Running };
            strategy.Execute(job, state, cts.Token, gate);
        }
        finally
        {
            // Nettoie toujours les contrôles, même en cas d'annulation ou d'erreur
            _cts.TryRemove(job.Name, out _);
            _pauseGates.TryRemove(job.Name, out _);
        }
    }

    // Retourne la stratégie correspondant au type de job via un pattern switch expression
    private static IBackupStrategy GetStrategy(BackupType type) => type switch
    {
        BackupType.Full => new FullBackup(),
        BackupType.Differential => new DifferentialBackup(),
        _ => throw new ArgumentOutOfRangeException()
    };
}
