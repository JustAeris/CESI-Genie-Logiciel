namespace EasySave.Core;

// Lance cryptosoft.exe pour chiffrer un fichier.
// Un SemaphoreSlim(1,1) statique garantit qu'une seule instance de cryptosoft.exe
// tourne à la fois, même entre des jobs de sauvegarde parallèles (T7).
public class CryptoSoftRunner : ICryptoService
{
    // Verrou mono-instance : une seule exécution de cryptosoft.exe autorisée à la fois sur tous les threads
    private static readonly SemaphoreSlim _mutex = new(1, 1);

    // Chiffre le fichier indiqué en lançant cryptosoft.exe.
    // Attend la fin de toute instance déjà en cours avant de démarrer.
    // Retourne le temps d'exécution en ms, ou -1 en cas d'erreur.
    public long Encrypt(string filePath)
    {
        _mutex.Wait(); // attend la libération si une autre instance tourne déjà
        try
        {
            // Configure le processus pour qu'il s'exécute sans fenêtre visible
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "cryptosoft.exe",
                Arguments = filePath,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            var process = System.Diagnostics.Process.Start(startInfo);
            process?.WaitForExit(); // attend la fin du chiffrement avant de continuer

            stopwatch.Stop();

            // Code de sortie > 0 = succès ; 0 ou négatif = erreur côté cryptosoft
            return process?.ExitCode > 0 ? stopwatch.ElapsedMilliseconds : -1;
        }
        catch
        {
            // En cas d'exception (fichier introuvable, accès refusé…), on retourne -1 sans faire planter le job
            return -1;
        }
        finally
        {
            // Libère toujours le sémaphore pour ne pas bloquer les autres jobs indéfiniment
            _mutex.Release();
        }
    }
}
