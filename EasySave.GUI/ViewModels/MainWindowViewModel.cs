using EasySave.GUI.MVVM;

namespace EasySave.GUI.ViewModels;

/// <summary>
/// ViewModel for the main window. Handles navigation between views.
/// </summary>
public class MainWindowViewModel : ViewModelBase
{
    public MVVM.NavigationService Navigation => MVVM.NavigationService.Instance;

    public RelayCommand NavigateToJobsCommand { get; }
    public RelayCommand NavigateToSettingsCommand { get; }

    public MainWindowViewModel()
    {
        NavigateToJobsCommand = new RelayCommand(_ =>
            Navigation.NavigateTo(new BackupJobsViewModel()));

        NavigateToSettingsCommand = new RelayCommand(_ =>
            Navigation.NavigateTo(new SettingsViewModel()));

        // Default view
        Navigation.NavigateTo(new BackupJobsViewModel());
    }
}
