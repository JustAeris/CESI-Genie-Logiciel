namespace EasySave.Core;

// Interface Strategy (GoF) — découple la détection du logiciel métier de BackupManager.
// Permet d'injecter un mock dans les tests sans modifier BackupManager (OCP, DIP).
public interface IBusinessSoftwareDetector
{
    // Retourne true si le logiciel métier est actuellement en cours d'exécution
    bool IsRunning();
}
