using EasySave.GUI.MVVM;

namespace EasySave.GUI.Services;

// Langues supportées par l'interface graphique
public enum GuiLanguage { FR, EN }

// Singleton qui expose toutes les chaînes de l'UI dans la langue active.
// Appeler SetLanguage() pour basculer ; tous les bindings se rafraîchissent via INotifyPropertyChanged("").
public class LocalizationService : ViewModelBase
{
    public static LocalizationService Instance { get; } = new();
    private LocalizationService() { }

    // Langue actuellement active — FR par défaut
    private GuiLanguage _language = GuiLanguage.FR;
    public GuiLanguage Language => _language;

    // Change la langue et déclenche un rafraîchissement global de tous les bindings de l'UI
    public void SetLanguage(GuiLanguage lang)
    {
        _language = lang;
        OnPropertyChanged(""); // chaîne vide = notifie toutes les propriétés d'un coup
    }

    // Retourne la chaîne fr ou en selon la langue active — utilisé par toutes les propriétés ci-dessous
    private string T(string fr, string en) => _language == GuiLanguage.FR ? fr : en;

    // --- Navigation ---
    public string NavLabel => T("> NAVIGATION", "> NAVIGATION");
    public string NavBackups => T("> sauvegardes", "> backups");
    public string NavSettings => T("> paramètres", "> settings");
    // Affiche la langue vers laquelle basculer (l'inverse de la langue active)
    public string LangToggle => _language == GuiLanguage.FR ? "EN" : "FR";

    // --- Vue liste des sauvegardes ---
    public string BackupsTitle => T("// SAUVEGARDES", "// BACKUPS");
    public string RunJob => T("▶ LANCER", "▶ RUN");
    public string RunAll => T("▶▶ TOUT LANCER", "▶▶ RUN ALL");
    public string AddJob => T("+ AJOUTER", "+ ADD");
    public string RemoveJob => T("✕ SUPPR", "✕ DEL");

    // --- Boîte de dialogue Ajouter un job ---
    public string AddJobTitle => T("Ajouter une sauvegarde", "Add backup");
    public string NameLabel => T("Nom", "Name");
    public string SourceLabel => T("Source", "Source");
    public string DestLabel => T("Destination", "Destination");
    public string CancelButton => T("Annuler", "Cancel");
    public string AddButton => T("Ajouter", "Add");

    // --- Vue Paramètres ---
    public string SettingsTitle => T("// PARAMÈTRES", "// SETTINGS");
    public string LogFormatLabel => T("> FORMAT DES LOGS", "> LOG FORMAT");
    public string LogDestinationLabel => T("> DESTINATION DES LOGS", "> LOG DESTINATION");
    public string LogServerUrlLabel => T("> URL SERVEUR DE LOGS", "> LOG SERVER URL");
    public string BusinessSoftwareLabel => T("> LOGICIEL MÉTIER", "> BUSINESS SOFTWARE");
    public string LargeFileLabel => T("> SEUIL GROS FICHIERS (KB, 0 = désactivé)", "> LARGE FILE THRESHOLD (KB, 0 = disabled)");
    public string PriorityExtLabel => T("> EXTENSIONS PRIORITAIRES", "> PRIORITY EXTENSIONS");
    public string EncryptedExtLabel => T("> EXTENSIONS CRYPTÉES", "> ENCRYPTED EXTENSIONS");
    public string AddExtension => T("+ AJOUTER", "+ ADD");
    public string SaveButton => T("[ SAUVEGARDER ]", "[ SAVE ]");

    // --- Barre de statut ---
    public string StatusIdle => T("[ IDLE ] — Prêt", "[ IDLE ] — Ready");
    public string StatusRunning => T("en cours...", "running...");
    public string StatusDone => T("terminé en", "done in");
    public string StatusError => T("échoué", "failed");
}
