namespace EasySave.Core;

// Value object holding all application settings (v1.1).
// v2.0 will extend this with EncryptedExtensions and BusinessSoftwareName.
public class AppConfig
{
    public List<BackupJob> Jobs { get; set; } = [];
    public string LogFormat { get; set; } = "json"; // "json" | "xml"
}
