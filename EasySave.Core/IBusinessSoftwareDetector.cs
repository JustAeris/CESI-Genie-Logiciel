namespace EasySave.Core;

// Strategy interface (GoF) — decouples business-software detection from BackupManager.
// Swap ProcessDetector for a mock in tests without touching BackupManager (OCP, DIP).
public interface IBusinessSoftwareDetector
{
    bool IsRunning();
}