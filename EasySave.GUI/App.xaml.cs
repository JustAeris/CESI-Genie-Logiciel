using System.Windows;
using EasyLog;
using EasySave.Console;
using EasySave.Core;

namespace EasySave.GUI;

/// <summary>
/// Entry point for EasySave.
/// If CLI arguments are provided, runs the console pipeline.
/// Otherwise launches the WPF GUI.
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        ConfigManager.Instance.Load();

        foreach (var job in ConfigManager.Instance.Jobs)
            BackupManager.Instance.AddJob(job);

        ILogSerializer serializer = ConfigManager.Instance.LogFormat == "xml"
            ? new XmlLogSerializer()
            : new JsonLogSerializer();

        Logger.Instance.SetSerializer(serializer);
        StateManager.Instance.SetSerializer(serializer);

        if (e.Args.Length > 0)
        {
            // CLI mode — run jobs and exit
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
            // GUI mode
            base.OnStartup(e);
            new MainWindow().Show();
        }
    }
}
