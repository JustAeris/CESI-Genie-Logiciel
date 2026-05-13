using System.Collections.ObjectModel;
using EasyLog;
using EasySave.Core;
using EasySave.GUI.MVVM;

namespace EasySave.GUI.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    public ObservableCollection<string> LogFormats { get; } = new() { "json", "xml" };
    public ObservableCollection<string> LogDestinations { get; } = new() { "local", "remote", "both" };
    public ObservableCollection<string> EncryptedExtensions { get; } = new();
    public ObservableCollection<string> PriorityExtensions { get; } = new();

    private string _selectedLogFormat;
    public string SelectedLogFormat
    {
        get => _selectedLogFormat;
        set => SetField(ref _selectedLogFormat, value);
    }

    private string _selectedLogDestination;
    public string SelectedLogDestination
    {
        get => _selectedLogDestination;
        set => SetField(ref _selectedLogDestination, value);
    }

    private string _businessSoftwareName;
    public string BusinessSoftwareName
    {
        get => _businessSoftwareName;
        set => SetField(ref _businessSoftwareName, value);
    }

    private int _largeFileSizeKb;
    public int LargeFileSizeKb
    {
        get => _largeFileSizeKb;
        set => SetField(ref _largeFileSizeKb, value);
    }

    private string _logServerUrl;
    public string LogServerUrl
    {
        get => _logServerUrl;
        set => SetField(ref _logServerUrl, value);
    }

    private string _newEncryptedExtension = "";
    public string NewEncryptedExtension
    {
        get => _newEncryptedExtension;
        set => SetField(ref _newEncryptedExtension, value);
    }

    private string _newPriorityExtension = "";
    public string NewPriorityExtension
    {
        get => _newPriorityExtension;
        set => SetField(ref _newPriorityExtension, value);
    }

    public RelayCommand AddEncryptedExtensionCommand { get; }
    public RelayCommand RemoveEncryptedExtensionCommand { get; }
    public RelayCommand AddPriorityExtensionCommand { get; }
    public RelayCommand RemovePriorityExtensionCommand { get; }
    public RelayCommand SaveCommand { get; }

    public SettingsViewModel()
    {
        var config = ConfigManager.Instance.Config;

        _selectedLogFormat = config.LogFormat;
        _selectedLogDestination = config.LogDestination;
        _businessSoftwareName = config.BusinessSoftwareName;
        _largeFileSizeKb = config.LargeFileSizeKb;
        _logServerUrl = config.LogServerUrl;

        foreach (var ext in config.EncryptedExtensions) EncryptedExtensions.Add(ext);
        foreach (var ext in config.PriorityExtensions) PriorityExtensions.Add(ext);

        AddEncryptedExtensionCommand = new RelayCommand(_ =>
        {
            if (!string.IsNullOrWhiteSpace(NewEncryptedExtension))
            {
                EncryptedExtensions.Add(NewEncryptedExtension.Trim());
                NewEncryptedExtension = "";
            }
        });

        RemoveEncryptedExtensionCommand = new RelayCommand(
            p => EncryptedExtensions.Remove(p as string ?? ""));

        AddPriorityExtensionCommand = new RelayCommand(_ =>
        {
            if (!string.IsNullOrWhiteSpace(NewPriorityExtension))
            {
                PriorityExtensions.Add(NewPriorityExtension.Trim());
                NewPriorityExtension = "";
            }
        });

        RemovePriorityExtensionCommand = new RelayCommand(
            p => PriorityExtensions.Remove(p as string ?? ""));

        SaveCommand = new RelayCommand(_ => Save());
    }

    private void Save()
    {
        var config = ConfigManager.Instance.Config;

        config.LogFormat = SelectedLogFormat;
        config.LogDestination = SelectedLogDestination;
        config.BusinessSoftwareName = BusinessSoftwareName;
        config.LargeFileSizeKb = LargeFileSizeKb;
        config.LogServerUrl = LogServerUrl;
        config.EncryptedExtensions = EncryptedExtensions.ToList();
        config.PriorityExtensions = PriorityExtensions.ToList();

        ConfigManager.Instance.Save();

        // Update serializer
        ILogSerializer serializer = config.LogFormat == "xml"
            ? new XmlLogSerializer()
            : new JsonLogSerializer();
        Logger.Instance.SetSerializer(serializer);
        StateManager.Instance.SetSerializer(serializer);

        // Update log destination / forwarder
        Logger.Instance.SetLogDestination(config.LogDestination);
        Logger.Instance.SetForwarder(config.LogDestination != "local"
            ? new LogForwarder(config.LogServerUrl)
            : null);

        // Update business software detector
        BackupManager.Instance.SetDetector(
            string.IsNullOrWhiteSpace(config.BusinessSoftwareName)
                ? null
                : new ProcessDetector(config.BusinessSoftwareName));
    }
}
