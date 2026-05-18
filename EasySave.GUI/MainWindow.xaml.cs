using System.Windows;
using EasySave.GUI.ViewModels;

namespace EasySave.GUI;

// Code-behind de la fenêtre principale — crée le MainWindowViewModel et l'assigne comme DataContext.
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        // Le MainWindowViewModel expose Navigation et Status pour les bindings XAML de la fenêtre
        DataContext = new MainWindowViewModel();
    }
}
