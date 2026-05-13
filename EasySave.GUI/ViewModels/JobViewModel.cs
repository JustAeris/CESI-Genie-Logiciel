using EasySave.Core;
using EasySave.GUI.MVVM;

namespace EasySave.GUI.ViewModels;

public class JobViewModel : ViewModelBase
{
    public BackupJob Job { get; }

    private double _progression;
    public double Progression
    {
        get => _progression;
        set => SetField(ref _progression, value);
    }

    private PlaybackState _playbackState = PlaybackState.Stopped;
    public PlaybackState PlaybackState
    {
        get => _playbackState;
        set => SetField(ref _playbackState, value);
    }

    public string Name => Job.Name;
    public string SourceDir => Job.SourceDir;

    public JobViewModel(BackupJob job) => Job = job;
}
