using System.Collections.ObjectModel;
using EasySave.Core;
using EasySave.GUI.MVVM;

namespace EasySave.GUI.ViewModels;

/// <summary>
/// ViewModel for the settings view.
/// Handles log format, business software name and encrypted extensions.
/// </summary>
public class SettingsViewModel : ViewModelBase
{
    public ObservableCollection<string> LogFormats { get; } = new() { "JSON", "XML" };
    public ObservableCollection<string> EncryptedExtensions { get; } = new();

    private string _selectedLogFormat = "JSON";
    public string SelectedLogFormat
    {
        get => _selectedLogFormat;
        set => SetField(ref _selectedLogFormat, value);
    }

    private string _businessSoftwareName = "";
    public string BusinessSoftwareName
    {
        get => _businessSoftwareName;
        set => SetField(ref _businessSoftwareName, value);
    }

    private string _newExtension = "";
    public string NewExtension
    {
        get => _newExtension;
        set => SetField(ref _newExtension, value);
    }

    public RelayCommand AddExtensionCommand { get; }
    public RelayCommand RemoveExtensionCommand { get; }
    public RelayCommand SaveCommand { get; }

    public SettingsViewModel()
    {
        AddExtensionCommand = new RelayCommand(
            _ =>
            {
                if (!string.IsNullOrWhiteSpace(NewExtension))
                {
                    EncryptedExtensions.Add(NewExtension.Trim());
                    NewExtension = "";
                }
            });

        RemoveExtensionCommand = new RelayCommand(
            param => EncryptedExtensions.Remove(param as string ?? ""));

        SaveCommand = new RelayCommand(_ => Save());
    }

    private void Save()
    {
        ConfigManager.Instance.Save();
    }
}
