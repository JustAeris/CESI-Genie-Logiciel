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

// ViewModel de la vue principale des sauvegardes.
// Gère la liste des jobs, les commandes d'exécution/pause/arrêt et le rafraîchissement de la progression.
public class BackupJobsViewModel : ViewModelBase
{
    // Liste observable des jobs affichés dans le DataGrid — WPF se met à jour automatiquement à chaque ajout/suppression
    public ObservableCollection<JobViewModel> Jobs { get; } = new();

    // Job actuellement sélectionné dans le DataGrid — utilisé par RunJob et RemoveJob
    private JobViewModel? _selectedJob;
    public JobViewModel? SelectedJob
    {
        get => _selectedJob;
        set => SetField(ref _selectedJob, value);
    }

    // Commandes liées aux boutons de la barre d'outils et du DataGrid
    public RelayCommand RunJobCommand { get; }
    public RelayCommand RunAllCommand { get; }
    public RelayCommand AddJobCommand { get; }
    public RelayCommand RemoveJobCommand { get; }
    public RelayCommand PauseJobCommand { get; }
    public RelayCommand ResumeJobCommand { get; }
    public RelayCommand StopJobCommand { get; }

    // Timer WPF qui interroge StateManager et BackupManager toutes les 500 ms pour mettre à jour la progression
    private readonly DispatcherTimer _refreshTimer;

    public BackupJobsViewModel()
    {
        // Charge les jobs persistés depuis ConfigManager et crée un JobViewModel par job
        foreach (var job in ConfigManager.Instance.Jobs)
            Jobs.Add(new JobViewModel(job));

        // Lance le job sélectionné ; désactivé si aucun job n'est sélectionné
        RunJobCommand = new RelayCommand(
            _ => RunSelected(),
            _ => SelectedJob != null);

        // Lance tous les jobs en parallèle sur un thread de fond pour ne pas bloquer l'UI
        RunAllCommand = new RelayCommand(
            _ => Task.Run(() => BackupManager.Instance.RunAll()));

        // Ouvre la boîte de dialogue d'ajout de job
        AddJobCommand = new RelayCommand(
            _ => AddJob());

        // Supprime le job sélectionné ; désactivé si aucun job n'est sélectionné
        RemoveJobCommand = new RelayCommand(
            _ => RemoveSelected(),
            _ => SelectedJob != null);

        // Met en pause le job passé en paramètre (CommandParameter = JobViewModel de la ligne)
        PauseJobCommand = new RelayCommand(
            p => { if (p is JobViewModel vm) BackupManager.Instance.PauseJob(vm.Name); },
            p => p is JobViewModel vm && vm.PlaybackState == PlaybackState.Running);

        // Reprend le job mis en pause passé en paramètre
        ResumeJobCommand = new RelayCommand(
            p => { if (p is JobViewModel vm) BackupManager.Instance.ResumeJob(vm.Name); },
            p => p is JobViewModel vm && vm.PlaybackState == PlaybackState.Paused);

        // Arrête le job passé en paramètre via CancellationToken
        StopJobCommand = new RelayCommand(
            p => { if (p is JobViewModel vm) BackupManager.Instance.StopJob(vm.Name); },
            p => p is JobViewModel vm && vm.PlaybackState != PlaybackState.Stopped);

        // Démarre le timer de rafraîchissement de la progression
        _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _refreshTimer.Tick += RefreshJobStates;
        _refreshTimer.Start();
    }

    // Appelé toutes les 500 ms par le timer — met à jour Progression et PlaybackState de chaque job affiché.
    private void RefreshJobStates(object? sender, EventArgs e)
    {
        // Charge l'état le plus récent de chaque job depuis StateManager (dictionnaire indexé par nom)
        var states = StateManager.Instance.GetAll().ToDictionary(s => s.Name);
        foreach (var vm in Jobs)
        {
            // Récupère l'état Running/Paused/Stopped depuis BackupManager
            vm.PlaybackState = BackupManager.Instance.GetPlaybackState(vm.Name);
            // Met à jour la barre de progression si un état existe pour ce job
            if (states.TryGetValue(vm.Name, out var state))
                vm.Progression = state.Progression;
        }
        // Demande à WPF de réévaluer CanExecute sur toutes les commandes (active/désactive les boutons)
        CommandManager.InvalidateRequerySuggested();
    }

    // Lance le job sélectionné sur un thread de fond, met à jour la barre de statut et affiche une notification à la fin.
    private void RunSelected()
    {
        if (SelectedJob == null) return;
        var vm = SelectedJob;
        int idx = Jobs.IndexOf(vm) + 1; // index 1-based attendu par BackupManager
        StatusService.Instance.SetRunning(vm.Name);
        Task.Run(() =>
        {
            var sw = Stopwatch.StartNew();
            BackupManager.Instance.RunJob(idx);
            sw.Stop();
            // Met à jour le statut et affiche une toast notification une fois le job terminé
            StatusService.Instance.SetDone(vm.Name, sw.ElapsedMilliseconds);
            ToastNotification.Show("[ DONE ] Sauvegarde terminée", $"{vm.Name} — {sw.ElapsedMilliseconds} ms");
        });
    }

    // Supprime le job sélectionné de la liste, de BackupManager et persiste la configuration
    private void RemoveSelected()
    {
        if (SelectedJob == null) return;
        int idx = Jobs.IndexOf(SelectedJob);
        Jobs.RemoveAt(idx);
        BackupManager.Instance.RemoveJob(idx + 1); // index 1-based
        ConfigManager.Instance.Save();
    }

    // Ouvre la boîte de dialogue d'ajout ; si l'utilisateur confirme, ajoute le job dans toutes les couches
    private void AddJob()
    {
        var dialog = new AddJobDialog();
        if (dialog.ShowDialog() == true)
        {
            var job = dialog.Result;
            Jobs.Add(new JobViewModel(job));           // ajoute à la liste affichée
            BackupManager.Instance.AddJob(job);        // enregistre dans le gestionnaire d'exécution
            ConfigManager.Instance.Jobs.Add(job);      // ajoute à la liste persistée
            ConfigManager.Instance.Save();             // sauvegarde immédiatement sur le disque
        }
    }
}
