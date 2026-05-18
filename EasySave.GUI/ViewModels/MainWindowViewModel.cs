using EasySave.GUI.MVVM;
using EasySave.GUI.Services;
using EasySave.Console;

namespace EasySave.GUI.ViewModels;

// ViewModel de la fenêtre principale — gère la navigation entre les vues et le bouton de langue.
public class MainWindowViewModel : ViewModelBase
{
    // Expose le service de navigation pour que le XAML puisse se lier à CurrentViewModel
    public MVVM.NavigationService Navigation => MVVM.NavigationService.Instance;

    // Expose le service de statut pour que le XAML puisse afficher Message dans la barre de statut
    public StatusService Status => StatusService.Instance;

    // Commandes liées aux boutons du menu latéral et du header
    public RelayCommand NavigateToJobsCommand { get; }
    public RelayCommand NavigateToSettingsCommand { get; }
    public RelayCommand ToggleLanguageCommand { get; }

    // Instance unique de SettingsViewModel — réutilisée à chaque navigation pour conserver l'état de la vue
    private readonly SettingsViewModel _settingsViewModel = new();

    public MainWindowViewModel()
    {
        // Crée un nouveau BackupJobsViewModel à chaque navigation (rafraîchit la liste des jobs)
        NavigateToJobsCommand = new RelayCommand(_ =>
            Navigation.NavigateTo(new BackupJobsViewModel()));

        // Réutilise toujours la même instance de SettingsViewModel pour ne pas perdre les saisies non sauvegardées
        NavigateToSettingsCommand = new RelayCommand(_ =>
            Navigation.NavigateTo(_settingsViewModel));

        // Bascule entre FR et EN, puis rafraîchit la barre de statut pour mettre à jour son texte localisé
        ToggleLanguageCommand = new RelayCommand(_ =>
        {
            var next = LocalizationService.Instance.Language == GuiLanguage.FR
                ? GuiLanguage.EN
                : GuiLanguage.FR;
            LocalizationService.Instance.SetLanguage(next);
            Status.SetIdle();
        });

        // Affiche la vue des sauvegardes par défaut au démarrage
        Navigation.NavigateTo(new BackupJobsViewModel());
    }
}
