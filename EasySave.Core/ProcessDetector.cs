using System.Diagnostics;

namespace EasySave.Core;

// Concrete Strategy (GoF) — checks whether the configured business software is running.
// Reads BusinessSoftwareName from AppConfig v2.0 (not yet in AppConfig v1.1, handled defensively).
public class ProcessDetector : IBusinessSoftwareDetector
{
    private readonly string _processName;

    public ProcessDetector(string processName)
    {
        _processName = processName;
    }

    // Returns false immediately if no process name is configured — safe no-op.
    public bool IsRunning()
    {
        if (string.IsNullOrWhiteSpace(_processName))
            return false;

        return Process.GetProcessesByName(_processName).Length > 0;
    }
}
