namespace EasySave.Core;

public class AppConfig
{
    public List<BackupJob> Jobs { get; set; } = [];
    public string LogFormat { get; set; } = "json"; // "json" | "xml"

    // File extensions that must be transferred before any non-priority file.
    // Example: [".pdf", ".docx"]
    public List<string> PriorityExtensions { get; set; } = [];
}
