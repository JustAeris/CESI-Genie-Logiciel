using System.Diagnostics;
using EasyLog;
using EasySave.Core;

namespace EasySave.Tests;

[Collection("Singletons")]
public class ParallelBackupTests : IDisposable
{
    private readonly List<string> _tempDirs = new();
    private readonly string _logDir;
    private readonly string _stateDir;
    private int _jobsAdded;

    public ParallelBackupTests()
    {
        _logDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _stateDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Logger.Instance.SetSerializer(new NullLogSerializer());
        Logger.Instance.SetLogDirectory(_logDir);
        StateManager.Instance.SetStateDirectory(_stateDir);
        StateManager.Instance.SetSerializer(new JsonLogSerializer());
        StateManager.Instance.ClearStates();
    }

    public void Dispose()
    {
        for (var i = 0; i < _jobsAdded; i++)
        {
            try { BackupManager.Instance.RemoveJob(1); } catch { }
        }
        foreach (var dir in _tempDirs)
            if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true);
        StateManager.Instance.ClearStates();
        StateManager.Instance.SetStateDirectory(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasySave"));
        Logger.Instance.SetSerializer(new JsonLogSerializer());
        Logger.Instance.SetLogDirectory(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasySave", "logs"));
        if (Directory.Exists(_logDir)) Directory.Delete(_logDir, recursive: true);
        if (Directory.Exists(_stateDir)) Directory.Delete(_stateDir, recursive: true);
    }

    private string MakeTempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);
        _tempDirs.Add(dir);
        return dir;
    }

    private string MakeSourceDir(int fileCount, int fileSizeBytes)
    {
        var dir = MakeTempDir();
        for (var i = 0; i < fileCount; i++)
            File.WriteAllBytes(Path.Combine(dir, $"file{i:D4}.dat"), new byte[fileSizeBytes]);
        return dir;
    }

    private void AddJob(string name, string src, string dst)
    {
        BackupManager.Instance.AddJob(new BackupJob
        { Name = name, SourceDir = src, TargetDir = dst, Type = BackupType.Full });
        _jobsAdded++;
    }

    [Fact]
    public void TwoJobsRunInParallel()
    {
        const int fileCount = 3;
        const int fileSize = 4 * 1024 * 1024; // 4 MB

        var src1 = MakeSourceDir(fileCount, fileSize);
        var src2 = MakeSourceDir(fileCount, fileSize);

        // Sequential baseline: run each job individually to measure individual durations
        var dst1Seq = MakeTempDir();
        var dst2Seq = MakeTempDir();
        var j1 = new BackupJob { Name = "par-s1", SourceDir = src1, TargetDir = dst1Seq, Type = BackupType.Full };
        var j2 = new BackupJob { Name = "par-s2", SourceDir = src2, TargetDir = dst2Seq, Type = BackupType.Full };

        var sw1 = Stopwatch.StartNew();
        new FullBackup().Execute(j1, new BackupState { Name = "par-s1" });
        sw1.Stop();

        var sw2 = Stopwatch.StartNew();
        new FullBackup().Execute(j2, new BackupState { Name = "par-s2" });
        sw2.Stop();

        // Parallel run via BackupManager
        var dst1Par = MakeTempDir();
        var dst2Par = MakeTempDir();
        AddJob("par-p1", src1, dst1Par);
        AddJob("par-p2", src2, dst2Par);

        var swPar = Stopwatch.StartNew();
        BackupManager.Instance.RunAll();
        swPar.Stop();

        Assert.Equal(fileCount, Directory.GetFiles(dst1Par, "*", SearchOption.AllDirectories).Length);
        Assert.Equal(fileCount, Directory.GetFiles(dst2Par, "*", SearchOption.AllDirectories).Length);
        Assert.True(
            swPar.ElapsedMilliseconds < sw1.ElapsedMilliseconds + sw2.ElapsedMilliseconds,
            $"Parallel ({swPar.ElapsedMilliseconds}ms) >= sequential sum ({sw1.ElapsedMilliseconds + sw2.ElapsedMilliseconds}ms)");
    }

    [Fact]
    public async Task CancelJobStopsExecutionBetweenFiles()
    {
        var src = MakeSourceDir(200, 1024);
        var dst = MakeTempDir();
        AddJob("cancel-job", src, dst);

        var runTask = Task.Run(() => BackupManager.Instance.RunAll());

        // Wait until the job has started copying files, then cancel
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            if (Directory.Exists(dst) && Directory.GetFiles(dst, "*", SearchOption.AllDirectories).Length > 0)
                break;
            await Task.Delay(5);
        }

        BackupManager.Instance.CancelJob("cancel-job");

        try { await runTask; }
        catch (Exception) { /* expected when a job is cancelled */ }

        var copied = Directory.GetFiles(dst, "*", SearchOption.AllDirectories).Length;
        Assert.True(copied < 200, $"Expected fewer than 200 files copied, got {copied}");
    }

    [Fact]
    public void BackupStateConsistentAfterParallelRun()
    {
        var src1 = MakeSourceDir(10, 512);
        var src2 = MakeSourceDir(10, 512);
        var dst1 = MakeTempDir();
        var dst2 = MakeTempDir();
        AddJob("cons-j1", src1, dst1);
        AddJob("cons-j2", src2, dst2);

        BackupManager.Instance.RunAll();

        var states = StateManager.Instance.GetAll();
        var s1 = states.First(s => s.Name == "cons-j1");
        var s2 = states.First(s => s.Name == "cons-j2");

        Assert.Equal(100.0, s1.Progression);
        Assert.Equal(0, s1.NbFilesLeftToDo);
        Assert.Equal(100.0, s2.Progression);
        Assert.Equal(0, s2.NbFilesLeftToDo);
    }

    // Swallows all log entries to avoid serialization bottleneck during parallel timing test.
    private sealed class NullLogSerializer : ILogSerializer
    {
        public string FileExtension => ".null";
        public string Serialize(IEnumerable<LogEntry> entries) => "";
        public IEnumerable<LogEntry> Deserialize(string raw) => [];
    }
}
