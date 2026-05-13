using EasySave.GUI.MVVM;

namespace EasySave.GUI.Services;

/// <summary>
/// Singleton service for broadcasting status messages to the main window status bar.
/// </summary>
public class StatusService : ViewModelBase
{
    private static readonly Lazy<StatusService> _instance = new(() => new StatusService());
    public static StatusService Instance => _instance.Value;

    private string _message = "";
    public string Message
    {
        get => _message;
        set => SetField(ref _message, value);
    }

    private StatusService() => SetIdle();

    public void SetIdle() => Message = LocalizationService.Instance.StatusIdle;
    public void SetRunning(string jobName) => Message = $"[ RUN  ] — {jobName} {LocalizationService.Instance.StatusRunning}";
    public void SetDone(string jobName, long ms) => Message = $"[ DONE ] — {jobName} {LocalizationService.Instance.StatusDone} {ms} ms";
    public void SetError(string jobName) => Message = $"[ ERR  ] — {jobName} {LocalizationService.Instance.StatusError}";
}
