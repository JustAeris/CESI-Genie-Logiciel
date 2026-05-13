using EasySave.GUI.MVVM;

namespace EasySave.GUI.Services;

public enum GuiLanguage { FR, EN }

/// <summary>
/// Singleton that exposes all UI strings for the current language.
/// Call SetLanguage() to switch; all bindings refresh via INotifyPropertyChanged("").
/// </summary>
public class LocalizationService : ViewModelBase
{
    public static LocalizationService Instance { get; } = new();
    private LocalizationService() { }

    private GuiLanguage _language = GuiLanguage.FR;
    public GuiLanguage Language => _language;

    public void SetLanguage(GuiLanguage lang)
    {
        _language = lang;
        OnPropertyChanged("");   // refreshes every bound property at once
    }

    private string T(string fr, string en) => _language == GuiLanguage.FR ? fr : en;

    // --- Navigation ---
    public string NavLabel     => T("> NAVIGATION",  "> NAVIGATION");
    public string NavBackups   => T("> sauvegardes", "> backups");
    public string NavSettings  => T("> paramètres",  "> settings");
    public string LangToggle   => _language == GuiLanguage.FR ? "EN" : "FR";

    // --- Backup jobs view ---
    public string BackupsTitle   => T("// SAUVEGARDES", "// BACKUPS");
    public string RunJob         => T("▶ RUN",          "▶ RUN");
    public string RunAll         => T("▶▶ RUN ALL",     "▶▶ RUN ALL");
    public string AddJob         => T("+ ADD",          "+ ADD");
    public string RemoveJob      => T("✕ DEL",          "✕ DEL");

    // --- Add job dialog ---
    public string AddJobTitle    => T("Ajouter une sauvegarde", "Add backup");
    public string NameLabel      => T("Nom",        "Name");
    public string SourceLabel    => T("Source",     "Source");
    public string DestLabel      => T("Destination","Destination");
    public string CancelButton   => T("Annuler",    "Cancel");
    public string AddButton      => T("Ajouter",    "Add");

    // --- Settings view ---
    public string SettingsTitle           => T("// PARAMÈTRES",                             "// SETTINGS");
    public string LogFormatLabel          => T("> LOG FORMAT",                              "> LOG FORMAT");
    public string LogDestinationLabel     => T("> LOG DESTINATION",                        "> LOG DESTINATION");
    public string LogServerUrlLabel       => T("> LOG SERVER URL",                         "> LOG SERVER URL");
    public string BusinessSoftwareLabel   => T("> LOGICIEL MÉTIER",                        "> BUSINESS SOFTWARE");
    public string LargeFileLabel          => T("> SEUIL GROS FICHIERS (KB, 0 = désactivé)","> LARGE FILE THRESHOLD (KB, 0 = disabled)");
    public string PriorityExtLabel        => T("> EXTENSIONS PRIORITAIRES",                "> PRIORITY EXTENSIONS");
    public string EncryptedExtLabel       => T("> EXTENSIONS CRYPTÉES",                    "> ENCRYPTED EXTENSIONS");
    public string SaveButton              => T("[ SAUVEGARDER ]",                          "[ SAVE ]");

    // --- Status bar ---
    public string StatusIdle    => T("[ IDLE ] — Prêt",  "[ IDLE ] — Ready");
    public string StatusRunning => T("en cours...",       "running...");
    public string StatusDone    => T("terminé en",        "done in");
    public string StatusError   => T("échoué",            "failed");
}
