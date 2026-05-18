using System.Collections.ObjectModel;
using EasyLog;
using EasySave.Core;
using EasySave.GUI.MVVM;

namespace EasySave.GUI.ViewModels;

// ViewModel de la vue Paramètres — charge les valeurs depuis ConfigManager au démarrage
// et les persiste sur le disque quand l'utilisateur clique sur Sauvegarder.
public class SettingsViewModel : ViewModelBase
{
    // Listes fixes des valeurs acceptées pour les dropdowns — liées à ItemsSource dans le XAML
    public ObservableCollection<string> LogFormats { get; } = new() { "json", "xml" };
    public ObservableCollection<string> LogDestinations { get; } = new() { "local", "remote", "both" };

    // Listes modifiables des extensions — l'utilisateur peut en ajouter ou en supprimer
    public ObservableCollection<string> EncryptedExtensions { get; } = new();
    public ObservableCollection<string> PriorityExtensions { get; } = new();

    // Format de log sélectionné dans le dropdown ("json" ou "xml")
    private string _selectedLogFormat;
    public string SelectedLogFormat
    {
        get => _selectedLogFormat;
        set => SetField(ref _selectedLogFormat, value);
    }

    // Destination des logs sélectionnée ("local", "remote" ou "both")
    private string _selectedLogDestination;
    public string SelectedLogDestination
    {
        get => _selectedLogDestination;
        set => SetField(ref _selectedLogDestination, value);
    }

    // Nom du logiciel métier dont la présence met en pause les jobs actifs
    private string _businessSoftwareName;
    public string BusinessSoftwareName
    {
        get => _businessSoftwareName;
        set => SetField(ref _businessSoftwareName, value);
    }

    // Seuil en Ko au-delà duquel un fichier est considéré "volumineux" (0 = désactivé)
    private int _largeFileSizeKb;
    public int LargeFileSizeKb
    {
        get => _largeFileSizeKb;
        set => SetField(ref _largeFileSizeKb, value);
    }

    // URL du serveur Docker de logs distant
    private string _logServerUrl;
    public string LogServerUrl
    {
        get => _logServerUrl;
        set => SetField(ref _logServerUrl, value);
    }

    // Valeur saisie dans le champ texte pour ajouter une extension chiffrée
    private string _newEncryptedExtension = "";
    public string NewEncryptedExtension
    {
        get => _newEncryptedExtension;
        set => SetField(ref _newEncryptedExtension, value);
    }

    // Valeur saisie dans le champ texte pour ajouter une extension prioritaire
    private string _newPriorityExtension = "";
    public string NewPriorityExtension
    {
        get => _newPriorityExtension;
        set => SetField(ref _newPriorityExtension, value);
    }

    // Commandes liées aux boutons d'ajout/suppression d'extensions et de sauvegarde
    public RelayCommand AddEncryptedExtensionCommand { get; }
    public RelayCommand RemoveEncryptedExtensionCommand { get; }
    public RelayCommand AddPriorityExtensionCommand { get; }
    public RelayCommand RemovePriorityExtensionCommand { get; }
    public RelayCommand SaveCommand { get; }

    public SettingsViewModel()
    {
        // Charge les valeurs courantes depuis ConfigManager pour pré-remplir tous les champs
        var config = ConfigManager.Instance.Config;

        // Initialise les backing fields directement (pas de setter) pour éviter des notifications prématurées
        _selectedLogFormat = config.LogFormat;
        _selectedLogDestination = config.LogDestination;
        _businessSoftwareName = config.BusinessSoftwareName;
        _largeFileSizeKb = config.LargeFileSizeKb;
        _logServerUrl = config.LogServerUrl;

        // Peuple les listes d'extensions depuis la configuration persistée
        foreach (var ext in config.EncryptedExtensions) EncryptedExtensions.Add(ext);
        foreach (var ext in config.PriorityExtensions) PriorityExtensions.Add(ext);

        // Ajoute l'extension saisie à la liste si elle n'est pas vide, puis vide le champ de saisie
        AddEncryptedExtensionCommand = new RelayCommand(_ =>
        {
            if (!string.IsNullOrWhiteSpace(NewEncryptedExtension))
            {
                EncryptedExtensions.Add(NewEncryptedExtension.Trim());
                NewEncryptedExtension = "";
            }
        });

        // Supprime l'extension passée en paramètre (CommandParameter dans le DataTemplate)
        RemoveEncryptedExtensionCommand = new RelayCommand(
            p => EncryptedExtensions.Remove(p as string ?? ""));

        // Ajoute l'extension prioritaire saisie à la liste, puis vide le champ
        AddPriorityExtensionCommand = new RelayCommand(_ =>
        {
            if (!string.IsNullOrWhiteSpace(NewPriorityExtension))
            {
                PriorityExtensions.Add(NewPriorityExtension.Trim());
                NewPriorityExtension = "";
            }
        });

        // Supprime l'extension prioritaire passée en paramètre
        RemovePriorityExtensionCommand = new RelayCommand(
            p => PriorityExtensions.Remove(p as string ?? ""));

        // Déclenche la sauvegarde et l'application des nouveaux paramètres
        SaveCommand = new RelayCommand(_ => Save());
    }

    // Écrit les valeurs modifiées dans ConfigManager, persiste sur le disque,
    // puis applique immédiatement les changements aux services actifs (Logger, StateManager, BackupManager).
    private void Save()
    {
        var config = ConfigManager.Instance.Config;

        // Copie les valeurs de la vue vers l'objet de configuration en mémoire
        config.LogFormat = SelectedLogFormat;
        config.LogDestination = SelectedLogDestination;
        config.BusinessSoftwareName = BusinessSoftwareName;
        config.LargeFileSizeKb = LargeFileSizeKb;
        config.LogServerUrl = LogServerUrl;
        config.EncryptedExtensions = EncryptedExtensions.ToList();
        config.PriorityExtensions = PriorityExtensions.ToList();

        // Persiste la configuration sur le disque
        ConfigManager.Instance.Save();

        // Met à jour le sérialiseur de Logger et StateManager selon le nouveau format choisi
        ILogSerializer serializer = config.LogFormat == "xml"
            ? new XmlLogSerializer()
            : new JsonLogSerializer();
        Logger.Instance.SetSerializer(serializer);
        StateManager.Instance.SetSerializer(serializer);

        // Reconfigure la destination des logs et le forwarder réseau
        Logger.Instance.SetLogDestination(config.LogDestination);
        Logger.Instance.SetForwarder(config.LogDestination != "local"
            ? new LogForwarder(config.LogServerUrl)
            : null);

        // Recrée le détecteur de logiciel métier avec le nouveau nom (ou le supprime si le champ est vide)
        BackupManager.Instance.SetDetector(
            string.IsNullOrWhiteSpace(config.BusinessSoftwareName)
                ? null
                : new ProcessDetector(config.BusinessSoftwareName));
    }
}
