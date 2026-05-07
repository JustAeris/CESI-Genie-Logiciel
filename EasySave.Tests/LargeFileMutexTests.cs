using System.Reflection;
using EasyLog;
using EasySave.Core;

namespace EasySave.Tests;

[Collection("Singletons")]
public class LargeFileMutexTests : IDisposable
{
    private readonly string _root;

    public LargeFileMutexTests()
    {
        _root = Path.Combine(Path.GetTempPath(), $"LargeFileMutexTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_root);
        Logger.Instance.SetSerializer(new NullLogSerializer());
        Logger.Instance.SetLogDirectory(Path.Combine(_root, "logs"));
        StateManager.Instance.SetStateDirectory(Path.Combine(_root, "state"));
        StateManager.Instance.SetSerializer(new JsonLogSerializer());
        StateManager.Instance.ClearStates();
        ResetLock();
    }

    public void Dispose()
    {
        ConfigManager.Instance.Config.LargeFileSizeKb = 0;
        ConfigManager.Instance.Config.PriorityExtensions = [];
        StateManager.Instance.ClearStates();
        StateManager.Instance.SetStateDirectory(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasySave"));
        Logger.Instance.SetSerializer(new JsonLogSerializer());
        Logger.Instance.SetLogDirectory(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasySave", "logs"));
        ResetLock();
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }

    // T-LMX-1: A large file backup blocks while the global large-file lock is held.
    // Once released, the backup completes normally.
    [Fact]
    public async Task LargeFile_BlocksWhileLockIsHeld()
    {
        const int thresholdKb = 1;
        ConfigManager.Instance.Config.LargeFileSizeKb = thresholdKb;

        var src = MakeDir("src");
        var dst = MakeDir("dst");
        File.WriteAllBytes(Path.Combine(src, "big.dat"), new byte[4 * 1024]); // 4 KB > 1 KB threshold

        HoldLock(); // simulate another job holding the large-file lock

        var completed = false;
        var task = Task.Run(() =>
        {
            new FullBackup().Execute(
                new BackupJob { Name = "lmx-blocked", SourceDir = src, TargetDir = dst, Type = BackupType.Full },
                new BackupState { Name = "lmx-blocked" });
            completed = true;
        });

        await Task.Delay(100);
        Assert.False(completed, "Large file copy should be blocked while lock is held");

        ReleaseLock();

        await task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.True(completed, "Backup should complete after lock is released");
        Assert.True(File.Exists(Path.Combine(dst, "big.dat")));
    }

    // T-LMX-2: Small files are not blocked even when the large-file lock is held.
    [Fact]
    public async Task SmallFile_NotBlockedByLargeLock()
    {
        const int thresholdKb = 100; // 100 KB threshold — our file will be far smaller
        ConfigManager.Instance.Config.LargeFileSizeKb = thresholdKb;

        var src = MakeDir("src");
        var dst = MakeDir("dst");
        File.WriteAllBytes(Path.Combine(src, "small.dat"), new byte[512]); // 0 KB < 100 KB threshold

        HoldLock(); // simulate a large file in flight on another job

        var completed = false;
        var task = Task.Run(() =>
        {
            new FullBackup().Execute(
                new BackupJob { Name = "lmx-small", SourceDir = src, TargetDir = dst, Type = BackupType.Full },
                new BackupState { Name = "lmx-small" });
            completed = true;
        });

        await task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.True(completed, "Small file should not be blocked by the large-file lock");
        Assert.True(File.Exists(Path.Combine(dst, "small.dat")));

        ReleaseLock(); // cleanup
    }

    // T-LMX-3: When LargeFileSizeKb = 0, no restriction applies — even a large file copies freely.
    [Fact]
    public async Task ThresholdZero_NoRestriction()
    {
        ConfigManager.Instance.Config.LargeFileSizeKb = 0; // disabled

        var src = MakeDir("src");
        var dst = MakeDir("dst");
        File.WriteAllBytes(Path.Combine(src, "big.dat"), new byte[4 * 1024]); // 4 KB

        HoldLock(); // lock is held, but threshold = 0 means we never try to acquire it

        var completed = false;
        var task = Task.Run(() =>
        {
            new FullBackup().Execute(
                new BackupJob { Name = "lmx-zero", SourceDir = src, TargetDir = dst, Type = BackupType.Full },
                new BackupState { Name = "lmx-zero" });
            completed = true;
        });

        await task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.True(completed, "Backup should complete immediately when threshold is 0");
        Assert.True(File.Exists(Path.Combine(dst, "big.dat")));

        ReleaseLock(); // cleanup
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private string MakeDir(string name)
    {
        var path = Path.Combine(_root, name);
        Directory.CreateDirectory(path);
        return path;
    }

    private static SemaphoreSlim GetLock()
    {
        var field = typeof(BackupStrategyBase).GetField("_largeFileLock",
            BindingFlags.NonPublic | BindingFlags.Static);
        return (SemaphoreSlim)field!.GetValue(null)!;
    }

    private static void HoldLock()
    {
        var sem = GetLock();
        if (sem.CurrentCount > 0) sem.Wait(0);
    }

    private static void ReleaseLock()
    {
        var sem = GetLock();
        if (sem.CurrentCount == 0) sem.Release();
    }

    private static void ResetLock()
    {
        var sem = GetLock();
        while (sem.CurrentCount < 1) sem.Release();
        while (sem.CurrentCount > 1) sem.Wait(0);
    }

    private sealed class NullLogSerializer : ILogSerializer
    {
        public string FileExtension => ".null";
        public string Serialize(IEnumerable<LogEntry> entries) => "";
        public IEnumerable<LogEntry> Deserialize(string raw) => [];
    }
}
