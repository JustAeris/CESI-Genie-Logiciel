using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using EasySave.Core;
using EasySave.GUI.MVVM;
using EasySave.GUI.MVVM.Views;
using EasySave.GUI.Services;

namespace EasySave.GUI.ViewModels;

public class BackupJobsViewModel : ViewModelBase
{
    public ObservableCollection<JobViewModel> Jobs { get; } = new();

    private JobViewModel? _selectedJob;
    public JobViewModel? SelectedJob
    {
        get => _selectedJob;
        set => SetField(ref _selectedJob, value);
    }

    public RelayCommand RunJobCommand { get; }
    public RelayCommand RunAllCommand { get; }
    public RelayCommand AddJobCommand { get; }
    public RelayCommand RemoveJobCommand { get; }
    public RelayCommand PauseJobCommand { get; }
    public RelayCommand ResumeJobCommand { get; }
    public RelayCommand StopJobCommand { get; }

    private readonly DispatcherTimer _refreshTimer;

    public BackupJobsViewModel()
    {
        foreach (var job in ConfigManager.Instance.Jobs)
            Jobs.Add(new JobViewModel(job));

        RunJobCommand = new RelayCommand(
            _ => RunSelected(),
            _ => SelectedJob != null);

        RunAllCommand = new RelayCommand(
            _ => Task.Run(() => BackupManager.Instance.RunAll()));

        AddJobCommand = new RelayCommand(
            _ => AddJob());

        RemoveJobCommand = new RelayCommand(
            _ => RemoveSelected(),
            _ => SelectedJob != null);

        PauseJobCommand = new RelayCommand(
            p => { if (p is JobViewModel vm) BackupManager.Instance.PauseJob(vm.Name); },
            p => p is JobViewModel vm && vm.PlaybackState == PlaybackState.Running);

        ResumeJobCommand = new RelayCommand(
            p => { if (p is JobViewModel vm) BackupManager.Instance.ResumeJob(vm.Name); },
            p => p is JobViewModel vm && vm.PlaybackState == PlaybackState.Paused);

        StopJobCommand = new RelayCommand(
            p => { if (p is JobViewModel vm) BackupManager.Instance.StopJob(vm.Name); },
            p => p is JobViewModel vm && vm.PlaybackState != PlaybackState.Stopped);

        _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _refreshTimer.Tick += RefreshJobStates;
        _refreshTimer.Start();
    }

    private void RefreshJobStates(object? sender, EventArgs e)
    {
        var states = StateManager.Instance.GetAll().ToDictionary(s => s.Name);
        foreach (var vm in Jobs)
        {
            vm.PlaybackState = BackupManager.Instance.GetPlaybackState(vm.Name);
            if (states.TryGetValue(vm.Name, out var state))
                vm.Progression = state.Progression;
        }
        CommandManager.InvalidateRequerySuggested();
    }

    private void RunSelected()
    {
        if (SelectedJob == null) return;
        var vm = SelectedJob;
        int idx = Jobs.IndexOf(vm) + 1;
        StatusService.Instance.SetRunning(vm.Name);
        Task.Run(() =>
        {
            var sw = Stopwatch.StartNew();
            BackupManager.Instance.RunJob(idx);
            sw.Stop();
            StatusService.Instance.SetDone(vm.Name, sw.ElapsedMilliseconds);
            ToastNotification.Show("[ DONE ] Sauvegarde terminée", $"{vm.Name} — {sw.ElapsedMilliseconds} ms");
        });
    }

    private void RemoveSelected()
    {
        if (SelectedJob == null) return;
        int idx = Jobs.IndexOf(SelectedJob);
        Jobs.RemoveAt(idx);
        BackupManager.Instance.RemoveJob(idx + 1);
        ConfigManager.Instance.Save();
    }

    private void AddJob()
    {
        var dialog = new AddJobDialog();
        if (dialog.ShowDialog() == true)
        {
            var job = dialog.Result;
            Jobs.Add(new JobViewModel(job));
            BackupManager.Instance.AddJob(job);
            ConfigManager.Instance.Jobs.Add(job);
            ConfigManager.Instance.Save();
        }
    }
}
