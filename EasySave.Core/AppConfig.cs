namespace EasySave.Core;

public class AppConfig
{
    public List<BackupJob> Jobs { get; set; } = [];
    public string LogFormat { get; set; } = "json"; // "json" | "xml"

    // File extensions that must be transferred before any non-priority file.
    // Example: [".pdf", ".docx"]
    public List<string> PriorityExtensions { get; set; } = [];

    // Max file size (in KB) that can be transferred simultaneously across jobs.
    // 0 = no restriction.
    public int LargeFileSizeKb { get; set; } = 0;

    // Log destination: "local" | "remote" | "both"
    public string LogDestination { get; set; } = "local";

    // Remote log server URL (Docker)
    public string LogServerUrl { get; set; } = "http://localhost:5000/logs";

    // Business software name to detect
    public string BusinessSoftwareName { get; set; } = "";
}