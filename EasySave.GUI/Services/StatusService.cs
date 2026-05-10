using EasySave.GUI.MVVM;

namespace EasySave.GUI.Services;

/// <summary>
/// Singleton service for broadcasting status messages to the main window status bar.
/// </summary>
public class StatusService : ViewModelBase
{
    private static readonly Lazy<StatusService> _instance = new(() => new StatusService());
    public static StatusService Instance => _instance.Value;

    private string _message = "[ IDLE ] — Prêt";
    public string Message
    {
        get => _message;
        set => SetField(ref _message, value);
    }

    private StatusService() { }

    public void SetIdle() => Message = "[ IDLE ] — Prêt";
    public void SetRunning(string jobName) => Message = $"[ RUN  ] — {jobName} en cours...";
    public void SetDone(string jobName, long ms) => Message = $"[ DONE ] — {jobName} terminé en {ms} ms";
    public void SetError(string jobName) => Message = $"[ ERR  ] — {jobName} échoué";
}
