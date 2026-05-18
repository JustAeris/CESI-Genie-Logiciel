using EasyLog;
using EasySave.Core;

namespace EasySave.Console;

// Menu console interactif qui pilote l'application EasySave en mode texte.
public class ConsoleMenu
{
    // Lance la boucle principale d'affichage du menu jusqu'à ce que l'utilisateur choisisse de quitter.
    public void Show()
    {
        PrintBanner();
        while (true)
        {
            // Affiche les options du menu à chaque itération
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
            // Retourne true quand l'utilisateur choisit de quitter
            if (HandleInput(input))
                return;
        }
    }

    // Aiguille l'entrée utilisateur vers l'action correspondante. Retourne true pour signaler une sortie.
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
            case "7": return true; // quitter
            default: System.Console.WriteLine(Resources.Get("error.invalid")); break;
        }
        return false;
    }

    // Affiche la liste de tous les jobs configurés avec leur index, type, nom et chemins
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

    // Demande les informations du job à l'utilisateur, crée le job et le persiste dans la configuration
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
        // 2 = Différentiel, tout autre choix = Complet
        var type = typeInput == "2" ? BackupType.Differential : BackupType.Full;

        var job = new BackupJob { Name = name, SourceDir = src, TargetDir = dst, Type = type };
        ConfigManager.Instance.Jobs.Add(job);
        BackupManager.Instance.AddJob(job);
        ConfigManager.Instance.Save(); // persiste immédiatement sur le disque

        System.Console.WriteLine(Resources.Get("job.added"));
    }

    // Affiche la liste puis demande l'index du job à lancer ; exécute le job sélectionné
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

    // Lance tous les jobs en parallèle et attend leur fin
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

    // Affiche les paramètres actuels puis propose un sous-menu pour les modifier
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

    // Demande le nouveau format (json/xml), valide, persiste et met à jour les sérialiseurs actifs
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
        // Synchronise Logger et StateManager avec le nouveau format
        ILogSerializer serializer = choice == "xml" ? new XmlLogSerializer() : new JsonLogSerializer();
        Logger.Instance.SetSerializer(serializer);
        StateManager.Instance.SetSerializer(serializer);
        System.Console.WriteLine(Resources.Get("settings.format.updated"));
    }

    // Demande la nouvelle destination (local/remote/both), valide, persiste et reconfigure Logger
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
        Logger.Instance.SetLogDestination(choice);
        // Active ou désactive le forwarder réseau selon la destination choisie
        Logger.Instance.SetForwarder(choice != "local"
            ? new LogForwarder(ConfigManager.Instance.Config.LogServerUrl)
            : null);
        System.Console.WriteLine("  Log destination updated.");
    }

    // Demande le nom du logiciel métier, persiste et reconfigure le détecteur dans BackupManager
    private void ChangeBusinessSoftware()
    {
        System.Console.Write("  Business software name: ");
        var name = System.Console.ReadLine()?.Trim() ?? "";
        ConfigManager.Instance.Config.BusinessSoftwareName = name;
        ConfigManager.Instance.Save();
        // Supprime le détecteur si le nom est vide, sinon en crée un nouveau
        BackupManager.Instance.SetDetector(
            string.IsNullOrWhiteSpace(name) ? null : new ProcessDetector(name));
        System.Console.WriteLine("  Business software updated.");
    }

    // Demande le seuil en Ko pour les gros fichiers, valide (>= 0), persiste
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

    // Demande la liste d'extensions prioritaires séparées par des virgules, puis persiste
    private void ChangePriorityExtensions()
    {
        System.Console.Write("  Priority extensions (comma-separated, e.g. .pdf,.docx): ");
        var input = System.Console.ReadLine()?.Trim() ?? "";
        // Découpe, nettoie et filtre les valeurs vides
        var extensions = input.Split(',').Select(e => e.Trim()).Where(e => !string.IsNullOrEmpty(e)).ToList();
        ConfigManager.Instance.Config.PriorityExtensions = extensions;
        ConfigManager.Instance.Save();
        System.Console.WriteLine("  Priority extensions updated.");
    }

    // Demande la langue souhaitée (EN/FR) et met à jour Resources.Current
    private void ChangeLanguage()
    {
        System.Console.Write(Resources.Get("lang.choice"));
        var lang = System.Console.ReadLine()?.Trim().ToUpperInvariant();
        Resources.Current = lang == "FR" ? Language.FR : Language.EN;
    }

    // Affiche le logo ASCII EasySave en deux couleurs (vert pour EASY, rouge pour SAVE) avec une bordure blanche
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

        // Calcule la largeur interne du cadre à partir de la ligne la plus longue
        int inner = easy.Concat(save).Max(l => l.Length) + 4;
        string top = "╔" + new string('═', inner) + "╗";
        string bottom = "╚" + new string('═', inner) + "╝";
        string blank = "║" + new string(' ', inner) + "║";

        // Fonction locale pour afficher une ligne colorée encadrée
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

        // Centre le sous-titre CESI dans le cadre
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
