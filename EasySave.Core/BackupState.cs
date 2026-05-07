namespace EasySave.Core;

public class BackupState
{
    public string Name { get; set; } = "";
    public string State { get; set; } = "IDLE";
    public int TotalFilesToCopy { get; set; }
    public long TotalFilesSize { get; set; }
    public int NbFilesLeftToDo { get; set; }
    public long SizeLeft { get; set; }
    public double Progression { get; set; }
    public string SourceFilePath { get; set; } = "";
    public string TargetFilePath { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public PlaybackState PlaybackState { get; set; } = PlaybackState.Stopped;
}
