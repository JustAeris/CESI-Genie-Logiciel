// Entry point: loads config, wires serializers, then either runs jobs from CLI args
// or launches the interactive console menu.
using EasyLog;
using EasySave.Console;
using EasySave.Core;

ConfigManager.Instance.Load();

foreach (var job in ConfigManager.Instance.Jobs)
    BackupManager.Instance.AddJob(job);

// Strategy (GoF): wire the serializer chosen in config to Logger and StateManager (DIP).
ILogSerializer serializer = ConfigManager.Instance.LogFormat == "xml"
    ? new XmlLogSerializer()
    : new JsonLogSerializer();

Logger.Instance.SetSerializer(serializer);
StateManager.Instance.SetSerializer(serializer);

var cfg = ConfigManager.Instance.Config;

if (!string.IsNullOrWhiteSpace(cfg.BusinessSoftwareName))
    BackupManager.Instance.SetDetector(new ProcessDetector(cfg.BusinessSoftwareName));

Logger.Instance.SetLogDestination(cfg.LogDestination);
if (cfg.LogDestination != "local")
    Logger.Instance.SetForwarder(new LogForwarder(cfg.LogServerUrl));

var indices = ParseArgs(args);
if (indices.Length > 0)
{
    foreach (var idx in indices)
        BackupManager.Instance.RunJob(idx);
}
else
{
    new ConsoleMenu().Show();
}

static int[] ParseArgs(string[] arguments)
{
    if (arguments.Length == 0) return [];
    try { return ArgParser.Parse(arguments[0]); }
    catch { return []; }
}
