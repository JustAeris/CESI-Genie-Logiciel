using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EasySave.GUI.MVVM;

// Classe de base pour tous les ViewModels.
// Implémente INotifyPropertyChanged pour que l'interface WPF se mette à jour automatiquement quand une propriété change.
public abstract class ViewModelBase : INotifyPropertyChanged
{
    // Événement abonné par WPF pour détecter les changements de propriétés
    public event PropertyChangedEventHandler? PropertyChanged;

    // Déclenche PropertyChanged pour notifier l'UI qu'une propriété a changé.
    // [CallerMemberName] injecte automatiquement le nom de la propriété appelante.
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // Raccourci pour les setters de propriétés : ne notifie que si la valeur change réellement.
    // Retourne true si la valeur a effectivement changé, false si elle était déjà identique.
    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(name);
        return true;
    }
}
