using System.Windows;
using EasyLog;
using EasySave.Console;
using EasySave.Core;

namespace EasySave.GUI;

// Point d'entrée de l'application WPF.
// Charge la configuration, connecte les services, puis lance l'interface graphique ou la CLI selon les arguments.
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // Lit config.json depuis %AppData%\EasySave et peuple ConfigManager.Config
        ConfigManager.Instance.Load();

        // Enregistre chaque job persisté dans BackupManager pour qu'il soit exécutable
        foreach (var job in ConfigManager.Instance.Jobs)
            BackupManager.Instance.AddJob(job);

        // Sélectionne le sérialiseur de logs selon le format configuré (JSON par défaut)
        ILogSerializer serializer = ConfigManager.Instance.LogFormat == "xml"
            ? new XmlLogSerializer()
            : new JsonLogSerializer();

        // Synchronise Logger et StateManager sur le même format de sérialisation
        Logger.Instance.SetSerializer(serializer);
        StateManager.Instance.SetSerializer(serializer);

        var cfg = ConfigManager.Instance.Config;

        // Injecte le runner CryptoSoft pour que les jobs puissent chiffrer les fichiers copiés
        BackupManager.Instance.CryptoService = new CryptoSoftRunner();

        // Active la détection du logiciel métier si un nom est configuré dans les paramètres
        if (!string.IsNullOrWhiteSpace(cfg.BusinessSoftwareName))
            BackupManager.Instance.SetDetector(new ProcessDetector(cfg.BusinessSoftwareName));

        // Configure la destination des logs et le forwarder réseau si la destination n'est pas locale
        Logger.Instance.SetLogDestination(cfg.LogDestination);
        if (cfg.LogDestination != "local")
            Logger.Instance.SetForwarder(new LogForwarder(cfg.LogServerUrl));

        if (e.Args.Length > 0)
        {
            // Mode CLI : exécute les jobs passés en arguments puis quitte sans ouvrir la fenêtre
            try
            {
                var indices = ArgParser.Parse(e.Args[0]);
                foreach (var idx in indices)
                    BackupManager.Instance.RunJob(idx);
            }
            catch { }
            Shutdown();
        }
        else
        {
            // Mode GUI : ouvre la fenêtre principale WPF
            base.OnStartup(e);
            new MainWindow().Show();
        }
    }
}
