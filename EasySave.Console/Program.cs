using EasySave.Console;
using EasySave.Core;

ConfigManager.Instance.Load();

foreach (var job in ConfigManager.Instance.Jobs)
    BackupManager.Instance.AddJob(job);

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
