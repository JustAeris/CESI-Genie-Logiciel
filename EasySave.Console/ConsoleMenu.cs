using EasySave.Core;

namespace EasySave.Console;

public class ConsoleMenu
{
    public void Show()
    {
        while (true)
        {
            System.Console.WriteLine();
            System.Console.WriteLine(Resources.Get("menu.title"));
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

    private void ChangeLanguage()
    {
        System.Console.Write(Resources.Get("lang.choice"));
        var lang = System.Console.ReadLine()?.Trim().ToUpperInvariant();
        Resources.Current = lang == "FR" ? Language.FR : Language.EN;
    }
}
