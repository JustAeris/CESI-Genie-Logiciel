namespace EasySave.Core;

// Interface Strategy (GoF) — FullBackup et DifferentialBackup l'implémentent toutes les deux.
// BackupManager appelle Execute() sans connaître la stratégie active (OCP, DIP).
public interface IBackupStrategy
{
    // Exécute la sauvegarde. token permet l'annulation ; pauseGate suspend la copie entre deux fichiers.
    void Execute(BackupJob job, BackupState state, CancellationToken token = default, ManualResetEventSlim? pauseGate = null);

    // Injecte le service de chiffrement avant le lancement de l'exécution
    void SetCryptoService(ICryptoService cryptoService);
}
