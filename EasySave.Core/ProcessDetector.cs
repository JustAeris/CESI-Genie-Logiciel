using System.Diagnostics;

namespace EasySave.Core;

// Stratégie concrète (GoF) — vérifie si le logiciel métier configuré est en cours d'exécution.
// Interroge la liste des processus Windows via System.Diagnostics.Process.
public class ProcessDetector : IBusinessSoftwareDetector
{
    // Nom du processus à surveiller, sans extension (ex. "notepad" et non "notepad.exe")
    private readonly string _processName;

    public ProcessDetector(string processName)
    {
        _processName = processName;
    }

    // Retourne false immédiatement si aucun nom n'est configuré — pas de recherche inutile.
    // Retourne true si au moins un processus portant ce nom est actif.
    public bool IsRunning()
    {
        if (string.IsNullOrWhiteSpace(_processName))
            return false;

        return Process.GetProcessesByName(_processName).Length > 0;
    }
}
