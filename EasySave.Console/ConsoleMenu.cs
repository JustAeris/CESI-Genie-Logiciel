using EasyLog;
using EasySave.Core;

namespace EasySave.Console;

/// <summary>
/// Interactive console menu that drives the EasySave application.
/// </summary>
public class ConsoleMenu
{
    public void Show()
    {
        PrintBanner();
        while (true)
        {
            System.Console.WriteLine();
            System.Console.WriteLine(Resources.Get("menu.list"));
            System.Console.WriteLine(Resources.Get("menu.add"));
            System.Console.WriteLine(Resources.Get("menu.run"));
            System.Console.WriteLine(Resources.Get("menu.runall"));
            System.Console.WriteLine(Resources.Get("menu.language"));
            System.Console.WriteLine(Resources.Get("menu.settings"));
            System.Console.WriteLine(Resources.Get("menu.exit"));
            System.Console.Write(Resources.Get("menu.choice"));

            var input = System.Console.ReadLine()?.Trim() ?? "";
            if (HandleInput(input))
                return;
        }
    }

    /// <summary>Returns true when the user chooses to exit.</summary>
    public bool HandleInput(string input)
    {
        switch (input)
        {
            case "1": DisplayJobs(); break;
            case "2": AddJob(); break;
            case "3": RunJob(); break;
            case "4": RunAll(); break;
            case "5": ChangeLanguage(); break;
            case "6": ShowSettings(); break;
            case "7": return true;
            default: System.Console.WriteLine(Resources.Get("error.invalid")); break;
        }
        return false;
    }

    private void DisplayJobs()
    {
        var jobs = ConfigManager.Instance.Jobs;
        if (jobs.Count == 0)
        {
            System.Console.WriteLine(Resources.Get("job.none"));
            return;
        }
        for (int i = 0; i < jobs.Count; i++)
        {
            var j = jobs[i];
            System.Console.WriteLine($"  {i + 1}. [{j.Type}] {j.Name}  {j.SourceDir} -> {j.TargetDir}");
        }
    }

    private void AddJob()
    {
        System.Console.Write(Resources.Get("job.name"));
        var name = System.Console.ReadLine()?.Trim() ?? "";

        System.Console.Write(Resources.Get("job.source"));
        var src = System.Console.ReadLine()?.Trim() ?? "";

        System.Console.Write(Resources.Get("job.target"));
        var dst = System.Console.ReadLine()?.Trim() ?? "";

        System.Console.Write(Resources.Get("job.type"));
        var typeInput = System.Console.ReadLine()?.Trim();
        var type = typeInput == "2" ? BackupType.Differential : BackupType.Full;

        var job = new BackupJob { Name = name, SourceDir = src, TargetDir = dst, Type = type };
        ConfigManager.Instance.Jobs.Add(job);
        BackupManager.Instance.AddJob(job);
        ConfigManager.Instance.Save();

        System.Console.WriteLine(Resources.Get("job.added"));
    }

    private void RunJob()
    {
        DisplayJobs();
        if (ConfigManager.Instance.Jobs.Count == 0) return;

        System.Console.Write(Resources.Get("job.index"));
        if (!int.TryParse(System.Console.ReadLine(), out int idx)
            || idx < 1 || idx > ConfigManager.Instance.Jobs.Count)
        {
            System.Console.WriteLine(Resources.Get("error.invalid"));
            return;
        }

        BackupManager.Instance.RunJob(idx);
        System.Console.WriteLine(Resources.Get("job.done"));
    }

    private void RunAll()
    {
        if (ConfigManager.Instance.Jobs.Count == 0)
        {
            System.Console.WriteLine(Resources.Get("job.none"));
            return;
        }
        BackupManager.Instance.RunAll();
        System.Console.WriteLine(Resources.Get("job.done"));
    }

    private void ShowSettings()
    {
        System.Console.WriteLine();
        System.Console.WriteLine(Resources.Get("settings.title"));
        System.Console.WriteLine($"  {Resources.Get("settings.format")}: {ConfigManager.Instance.Config.LogFormat.ToUpperInvariant()}");
        System.Console.WriteLine($"  Log destination: {ConfigManager.Instance.Config.LogDestination}");
        System.Console.WriteLine($"  Business software: {ConfigManager.Instance.Config.BusinessSoftwareName}");
        System.Console.WriteLine($"  Large file threshold: {ConfigManager.Instance.Config.LargeFileSizeKb} KB");
        System.Console.WriteLine($"  Priority extensions: {string.Join(", ", ConfigManager.Instance.Config.PriorityExtensions)}");

        System.Console.WriteLine();
        System.Console.WriteLine("  1. Change log format (JSON/XML)");
        System.Console.WriteLine("  2. Change log destination (local/remote/both)");
        System.Console.WriteLine("  3. Change business software name");
        System.Console.WriteLine("  4. Change large file threshold (KB)");
        System.Console.WriteLine("  5. Change priority extensions");
        System.Console.WriteLine("  6. Back");
        System.Console.Write(Resources.Get("menu.choice"));

        var choice = System.Console.ReadLine()?.Trim() ?? "";
        switch (choice)
        {
            case "1": ChangeLogFormat(); break;
            case "2": ChangeLogDestination(); break;
            case "3": ChangeBusinessSoftware(); break;
            case "4": ChangeLargeFileThreshold(); break;
            case "5": ChangePriorityExtensions(); break;
        }
    }

    private void ChangeLogFormat()
    {
        System.Console.Write(Resources.Get("settings.format.choose"));
        var choice = System.Console.ReadLine()?.Trim().ToLowerInvariant() ?? "";
        if (choice != "json" && choice != "xml")
        {
            System.Console.WriteLine(Resources.Get("settings.format.invalid"));
            return;
        }
        ConfigManager.Instance.Config.LogFormat = choice;
        ConfigManager.Instance.Save();
        ILogSerializer serializer = choice == "xml" ? new XmlLogSerializer() : new JsonLogSerializer();
        Logger.Instance.SetSerializer(serializer);
        StateManager.Instance.SetSerializer(serializer);
        System.Console.WriteLine(Resources.Get("settings.format.updated"));
    }

    private void ChangeLogDestination()
    {
        System.Console.Write("  Log destination (local/remote/both): ");
        var choice = System.Console.ReadLine()?.Trim().ToLowerInvariant() ?? "";
        if (choice != "local" && choice != "remote" && choice != "both")
        {
            System.Console.WriteLine(Resources.Get("error.invalid"));
            return;
        }
        ConfigManager.Instance.Config.LogDestination = choice;
        ConfigManager.Instance.Save();
        System.Console.WriteLine("  Log destination updated.");
    }

    private void ChangeBusinessSoftware()
    {
        System.Console.Write("  Business software name: ");
        var name = System.Console.ReadLine()?.Trim() ?? "";
        ConfigManager.Instance.Config.BusinessSoftwareName = name;
        ConfigManager.Instance.Save();
        System.Console.WriteLine("  Business software updated.");
    }

    private void ChangeLargeFileThreshold()
    {
        System.Console.Write("  Large file threshold (KB, 0 = no limit): ");
        if (!int.TryParse(System.Console.ReadLine(), out int kb) || kb < 0)
        {
            System.Console.WriteLine(Resources.Get("error.invalid"));
            return;
        }
        ConfigManager.Instance.Config.LargeFileSizeKb = kb;
        ConfigManager.Instance.Save();
        System.Console.WriteLine("  Threshold updated.");
    }

    private void ChangePriorityExtensions()
    {
        System.Console.Write("  Priority extensions (comma-separated, e.g. .pdf,.docx): ");
        var input = System.Console.ReadLine()?.Trim() ?? "";
        var extensions = input.Split(',').Select(e => e.Trim()).Where(e => !string.IsNullOrEmpty(e)).ToList();
        ConfigManager.Instance.Config.PriorityExtensions = extensions;
        ConfigManager.Instance.Save();
        System.Console.WriteLine("  Priority extensions updated.");
    }

    private void ChangeLanguage()
    {
        System.Console.Write(Resources.Get("lang.choice"));
        var lang = System.Console.ReadLine()?.Trim().ToUpperInvariant();
        Resources.Current = lang == "FR" ? Language.FR : Language.EN;
    }

    private static void PrintBanner()
    {
        string[] easy =
        [
            "███████╗ █████╗ ███████╗██╗   ██╗",
            "██╔════╝██╔══██╗██╔════╝╚██╗ ██╔╝",
            "█████╗  ███████║███████╗ ╚████╔╝ ",
            "██╔══╝  ██╔══██║╚════██║  ╚██╔╝  ",
            "███████╗██║  ██║███████║   ██║   ",
            "╚══════╝╚═╝  ╚═╝╚══════╝   ╚═╝   ",
        ];
        string[] save =
        [
            "███████╗ █████╗ ██╗   ██╗███████╗",
            "██╔════╝██╔══██╗██║   ██║██╔════╝",
            "███████╗███████║██║   ██║█████╗  ",
            "╚════██║██╔══██║╚██╗ ██╔╝██╔══╝  ",
            "███████║██║  ██║ ╚████╔╝ ███████╗",
            "╚══════╝╚═╝  ╚═╝  ╚═══╝  ╚══════╝",
        ];

        int inner = easy.Concat(save).Max(l => l.Length) + 4;
        string top = "╔" + new string('═', inner) + "╗";
        string bottom = "╚" + new string('═', inner) + "╝";
        string blank = "║" + new string(' ', inner) + "║";

        void PrintLine(string text, ConsoleColor color)
        {
            string padded = "  " + text.PadRight(inner - 2);
            System.Console.ForegroundColor = ConsoleColor.White;
            System.Console.Write("║");
            System.Console.ForegroundColor = color;
            System.Console.Write(padded);
            System.Console.ForegroundColor = ConsoleColor.White;
            System.Console.WriteLine("║");
        }

        string cesi = "[ CESI • v1.1 ] ";
        int lp = (inner - cesi.Length) / 2;
        string cesiPadded = new string(' ', lp) + cesi + new string(' ', inner - cesi.Length - lp);

        System.Console.ForegroundColor = ConsoleColor.White;
        System.Console.WriteLine(top);
        System.Console.WriteLine(blank);
        foreach (var l in easy) PrintLine(l, ConsoleColor.Green);
        System.Console.WriteLine(blank);
        foreach (var l in save) PrintLine(l, ConsoleColor.Red);
        System.Console.WriteLine(blank);
        System.Console.Write("║");
        System.Console.ForegroundColor = ConsoleColor.Yellow;
        System.Console.Write(cesiPadded);
        System.Console.ForegroundColor = ConsoleColor.White;
        System.Console.WriteLine("║");
        System.Console.WriteLine(blank);
        System.Console.WriteLine(bottom);
        System.Console.ResetColor();
    }
}