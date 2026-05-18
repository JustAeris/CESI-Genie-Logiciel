using EasySave.Core;
using EasySave.GUI.MVVM;

namespace EasySave.GUI.ViewModels;

// Wrapper ViewModel autour d'un BackupJob pour l'affichage dans la liste des sauvegardes.
// Expose Progression et PlaybackState comme propriétés observables rafraîchies par le timer toutes les 500 ms.
public class JobViewModel : ViewModelBase
{
    // Référence au job de domaine sous-jacent (nom, chemins, type)
    public BackupJob Job { get; }

    // Pourcentage de progression de 0 à 100, mis à jour depuis StateManager.GetAll()
    private double _progression;
    public double Progression
    {
        get => _progression;
        set => SetField(ref _progression, value);
    }

    // État de lecture courant (Running/Paused/Stopped), mis à jour depuis BackupManager.GetPlaybackState()
    private PlaybackState _playbackState = PlaybackState.Stopped;
    public PlaybackState PlaybackState
    {
        get => _playbackState;
        set => SetField(ref _playbackState, value);
    }

    // Raccourcis en lecture seule délégués au BackupJob pour simplifier les bindings XAML
    public string Name => Job.Name;
    public string SourceDir => Job.SourceDir;

    // Initialise le ViewModel avec le job de domaine correspondant
    public JobViewModel(BackupJob job) => Job = job;
}
