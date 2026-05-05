namespace EasySave.GUI.MVVM;

/// <summary>
/// Singleton service that manages which ViewModel is currently displayed.
/// The MainWindow binds its content area to CurrentViewModel.
/// </summary>
public class NavigationService : ViewModelBase
{
    private static readonly Lazy<NavigationService> _instance = new(() => new NavigationService());
    public static NavigationService Instance => _instance.Value;

    private ViewModelBase? _currentViewModel;

    /// <summary>The ViewModel currently displayed in the main content area.</summary>
    public ViewModelBase? CurrentViewModel
    {
        get => _currentViewModel;
        set => SetField(ref _currentViewModel, value);
    }

    private NavigationService() { }

    /// <summary>Navigates to the given ViewModel.</summary>
    public void NavigateTo(ViewModelBase viewModel)
        => CurrentViewModel = viewModel;
}
