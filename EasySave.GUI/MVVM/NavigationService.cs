namespace EasySave.GUI.MVVM;

// Singleton (GoF) — gère quel ViewModel est actuellement affiché dans la zone de contenu principale.
// La fenêtre principale lie sa zone de contenu à CurrentViewModel via un DataTemplate.
public class NavigationService : ViewModelBase
{
    private static readonly Lazy<NavigationService> _instance = new(() => new NavigationService());
    public static NavigationService Instance => _instance.Value;

    // ViewModel actuellement affiché — WPF sélectionne automatiquement la vue correspondante via DataTemplate
    private ViewModelBase? _currentViewModel;

    public ViewModelBase? CurrentViewModel
    {
        get => _currentViewModel;
        set => SetField(ref _currentViewModel, value); // notifie l'UI pour changer la vue affichée
    }

    private NavigationService() { }

    // Remplace le ViewModel actuel — provoque immédiatement le changement de vue dans l'interface
    public void NavigateTo(ViewModelBase viewModel)
        => CurrentViewModel = viewModel;
}
