using EasySave.Core;

namespace EasySave.Console;

/// <summary>
/// Interactive console menu that drives the EasySave application.
/// </summary>
public class ConsoleMenu
{
    /// <summary>
    /// Displays the main menu in a loop until the user chooses to exit.
    /// </summary>
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
            case "6": return true;
            default:  System.Console.WriteLine(Resources.Get("error.invalid")); break;
        }
        return false;
    }

    /// <summary>Lists all configured backup jobs to the console.</summary>
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
            System.Console.WriteLine(
                $"  {i + 1}. [{j.Type}] {j.Name}  {j.SourceDir} -> {j.TargetDir}");
        }
    }

    /// <summary>Prompts the user for job details and adds a new backup job (max 5).</summary>
    private void AddJob()
    {
        if (ConfigManager.Instance.Jobs.Count >= 5)
        {
            System.Console.WriteLine(Resources.Get("job.max"));
            return;
        }

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

    /// <summary>Prompts the user to select a job by index and runs it.</summary>
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

    /// <summary>Runs all configured backup jobs sequentially.</summary>
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

    /// <summary>Prints the ASCII art banner once at startup.</summary>
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
        string top    = "╔" + new string('═', inner) + "╗";
        string bottom = "╚" + new string('═', inner) + "╝";
        string blank  = "║" + new string(' ', inner) + "║";

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

        string cesi = "[ CESI • v1.0 ]";
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

    /// <summary>Prompts the user to switch the UI language (EN/FR).</summary>
    private void ChangeLanguage()
    {
        System.Console.Write(Resources.Get("lang.choice"));
        var lang = System.Console.ReadLine()?.Trim().ToUpperInvariant();
        Resources.Current = lang == "FR" ? Language.FR : Language.EN;
    }
}
