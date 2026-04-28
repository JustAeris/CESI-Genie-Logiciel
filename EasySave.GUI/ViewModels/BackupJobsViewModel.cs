using System.Collections.ObjectModel;
using System.Windows;
using EasySave.Core;
using EasySave.GUI.MVVM;
using EasySave.GUI.MVVM.Views;

namespace EasySave.GUI.ViewModels;

/// <summary>
/// ViewModel for the backup jobs view.
/// Handles listing, adding, removing and running backup jobs.
/// </summary>
public class BackupJobsViewModel : ViewModelBase
{
    public ObservableCollection<BackupJob> Jobs { get; } = new();

    private BackupJob? _selectedJob;
    public BackupJob? SelectedJob
    {
        get => _selectedJob;
        set => SetField(ref _selectedJob, value);
    }

    public RelayCommand RunJobCommand { get; }
    public RelayCommand RunAllCommand { get; }
    public RelayCommand AddJobCommand { get; }
    public RelayCommand RemoveJobCommand { get; }

    public BackupJobsViewModel()
    {
        foreach (var job in ConfigManager.Instance.Jobs)
            Jobs.Add(job);

        RunJobCommand = new RelayCommand(
            _ => RunSelected(),
            _ => SelectedJob != null);

        RunAllCommand = new RelayCommand(
            _ => BackupManager.Instance.RunAll());

        AddJobCommand = new RelayCommand(
            _ => AddJob());

        RemoveJobCommand = new RelayCommand(
            _ => RemoveSelected(),
            _ => SelectedJob != null);
    }

    private void RunSelected()
    {
        if (SelectedJob == null) return;
        int idx = Jobs.IndexOf(SelectedJob) + 1;
        BackupManager.Instance.RunJob(idx);
    }

    private void RemoveSelected()
    {
        if (SelectedJob == null) return;
        int idx = Jobs.IndexOf(SelectedJob);
        Jobs.RemoveAt(idx);
        BackupManager.Instance.RemoveJob(idx);
        ConfigManager.Instance.Save();
    }

    private void AddJob()
    {
        var dialog = new AddJobDialog();
        if (dialog.ShowDialog() == true)
        {
            var job = dialog.Result;
            Jobs.Add(job);
            BackupManager.Instance.AddJob(job);
            ConfigManager.Instance.Save();
        }
    }
}
