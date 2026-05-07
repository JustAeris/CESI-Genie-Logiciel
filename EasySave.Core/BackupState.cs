namespace EasySave.Core;

public class BackupState
{
    private readonly object _lock = new();

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

    public void DecrementProgress(long fileSize, int totalFiles)
    {
        lock (_lock)
        {
            NbFilesLeftToDo--;
            SizeLeft -= fileSize;
            Progression = totalFiles == 0
                ? 100.0
                : (totalFiles - NbFilesLeftToDo) / (double)totalFiles * 100.0;
        }
    }
}
