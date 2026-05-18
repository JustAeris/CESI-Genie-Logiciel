// Point d'entrée : charge la configuration, connecte les sérialiseurs,
// puis exécute les jobs passés en arguments ou lance le menu interactif.
using EasyLog;
using EasySave.Console;
using EasySave.Core;

// Charge la configuration depuis %AppData%\EasySave\config.json
ConfigManager.Instance.Load();

// Enregistre chaque job configuré dans BackupManager pour pouvoir l'exécuter
foreach (var job in ConfigManager.Instance.Jobs)
    BackupManager.Instance.AddJob(job);

// Strategy (GoF) : connecte le sérialiseur choisi dans la config à Logger et StateManager (DIP)
ILogSerializer serializer = ConfigManager.Instance.LogFormat == "xml"
    ? new XmlLogSerializer()
    : new JsonLogSerializer();

Logger.Instance.SetSerializer(serializer);
StateManager.Instance.SetSerializer(serializer);

var cfg = ConfigManager.Instance.Config;

// Injecte le runner CryptoSoft pour que les stratégies puissent chiffrer les fichiers copiés
BackupManager.Instance.CryptoService = new CryptoSoftRunner();

// Active la détection du logiciel métier si un nom est configuré
if (!string.IsNullOrWhiteSpace(cfg.BusinessSoftwareName))
    BackupManager.Instance.SetDetector(new ProcessDetector(cfg.BusinessSoftwareName));

// Configure la destination des logs et le forwarder réseau si besoin
Logger.Instance.SetLogDestination(cfg.LogDestination);
if (cfg.LogDestination != "local")
    Logger.Instance.SetForwarder(new LogForwarder(cfg.LogServerUrl));

// Si des index de jobs sont passés en argument, les exécute directement sans menu
var indices = ParseArgs(args);
if (indices.Length > 0)
{
    foreach (var idx in indices)
        BackupManager.Instance.RunJob(idx);
}
else
{
    // Aucun argument — lance le menu console interactif
    new ConsoleMenu().Show();
}

// Tente de parser les arguments ; retourne un tableau vide si le format est invalide
static int[] ParseArgs(string[] arguments)
{
    if (arguments.Length == 0) return [];
    try { return ArgParser.Parse(arguments[0]); }
    catch { return []; }
}
