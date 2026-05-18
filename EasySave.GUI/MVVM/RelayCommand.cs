using System.Windows.Input;

namespace EasySave.GUI.MVVM;

// Implémentation de ICommand qui délègue Execute et CanExecute à des lambdas.
// Permet de lier les boutons XAML à des méthodes du ViewModel sans code-behind.
public class RelayCommand : ICommand
{
    // Action exécutée quand le bouton est cliqué
    private readonly Action<object?> _execute;

    // Prédicat optionnel qui détermine si le bouton est actif ; null = toujours actif
    private readonly Func<object?, bool>? _canExecute;

    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    // Se branche sur CommandManager.RequerySuggested pour que WPF réévalue CanExecute automatiquement
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    // Retourne true si aucun prédicat n'est défini, sinon évalue le prédicat avec le paramètre
    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    // Exécute l'action associée à la commande
    public void Execute(object? parameter) => _execute(parameter);
}
