using System.Collections.Concurrent;
using System.Reflection;
using EasySave.Core;
using EasyLog;

namespace EasySave.Tests;

[Collection("Singletons")]
public class PriorityFilesTests : IDisposable
{
    private readonly string _root;

    public PriorityFilesTests()
    {
        _root = Path.Combine(Path.GetTempPath(), $"PriorityTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_root);
        Logger.Instance.SetSerializer(new NullLogSerializer());
        Logger.Instance.SetLogDirectory(Path.Combine(_root, "logs"));
        StateManager.Instance.SetStateDirectory(Path.Combine(_root, "state"));
        StateManager.Instance.SetSerializer(new JsonLogSerializer());
        StateManager.Instance.ClearStates();
        ResetBarrier();
    }

    public void Dispose()
    {
        ConfigManager.Instance.Config.PriorityExtensions = [];
        StateManager.Instance.ClearStates();
        StateManager.Instance.SetStateDirectory(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasySave"));
        Logger.Instance.SetSerializer(new JsonLogSerializer());
        Logger.Instance.SetLogDirectory(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasySave", "logs"));
        ResetBarrier();
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }

    // T-PRI-1: Within a single job, .pdf files (priority) must all be copied before any .txt file.
    // Files are created in alphabetical order so .txt sorts first — the sort in Execute must reorder them.
    [Fact]
    public void PriorityFiles_AreCopiedBeforeNonPriorityFiles()
    {
        var src = MakeDir("src");
        var dst = MakeDir("dst");

        // Alphabetical order: .txt files come before .pdf — deliberately tests reordering
        File.WriteAllBytes(Path.Combine(src, "aaa.txt"), new byte[256]);
        File.WriteAllBytes(Path.Combine(src, "bbb.pdf"), new byte[256]);
        File.WriteAllBytes(Path.Combine(src, "ccc.txt"), new byte[256]);
        File.WriteAllBytes(Path.Combine(src, "ddd.pdf"), new byte[256]);

        ConfigManager.Instance.Config.PriorityExtensions = [".pdf"];

        var recorder = new OrderRecordingCryptoService();
        var strategy = new FullBackup();
        strategy.SetCryptoService(recorder);
        strategy.Execute(
            new BackupJob { Name = "order", SourceDir = src, TargetDir = dst, Type = BackupType.Full },
            new BackupState { Name = "order" });

        var order = recorder.CopiedFiles.ToList();
        var lastPdfIdx = order.FindLastIndex(f => f.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase));
        var firstTxtIdx = order.FindIndex(f => f.EndsWith(".txt", StringComparison.OrdinalIgnoreCase));

        Assert.True(lastPdfIdx < firstTxtIdx,
            $"All .pdf copies must precede any .txt copy.\nOrder: {string.Join(", ", order.Select(Path.GetFileName))}");
    }

    // T-PRI-2: A non-priority copy blocks while the global gate is closed (priority file in flight).
    // The gate is closed externally via reflection to simulate a concurrent job's pending priority file.
    [Fact]
    public async Task NonPriorityFile_BlocksWhilePriorityGateIsClosed()
    {
        var src = MakeDir("src");
        var dst = MakeDir("dst");
        File.WriteAllText(Path.Combine(src, "note.txt"), "text");

        ConfigManager.Instance.Config.PriorityExtensions = [".pdf"]; // .txt is non-priority

        CloseGate(); // simulate another job's priority file being in flight

        var completed = false;
        var task = Task.Run(() =>
        {
            new FullBackup().Execute(
                new BackupJob { Name = "blocked", SourceDir = src, TargetDir = dst, Type = BackupType.Full },
                new BackupState { Name = "blocked" });
            completed = true;
        });

        await Task.Delay(100); // enough time for the task to start and block on the gate
        Assert.False(completed, "Non-priority copy should be blocked while gate is closed");

        OpenGate(); // simulate priority file completing

        await task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.True(completed, "Backup should complete after gate opens");
        Assert.True(File.Exists(Path.Combine(dst, "note.txt")));
    }

    // T-PRI-3: _pendingPriorityFiles must be 0 after a complete execution.
    [Fact]
    public void PriorityCounter_ReturnsToZeroAfterExecution()
    {
        var src = MakeDir("src");
        var dst = MakeDir("dst");

        File.WriteAllBytes(Path.Combine(src, "a.pdf"), new byte[256]);
        File.WriteAllBytes(Path.Combine(src, "b.pdf"), new byte[256]);
        File.WriteAllText(Path.Combine(src, "c.txt"), "text");

        ConfigManager.Instance.Config.PriorityExtensions = [".pdf"];

        new FullBackup().Execute(
            new BackupJob { Name = "cnt", SourceDir = src, TargetDir = dst, Type = BackupType.Full },
            new BackupState { Name = "cnt" });

        var field = typeof(BackupStrategyBase).GetField("_pendingPriorityFiles",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.Equal(0, (int)field!.GetValue(null)!);
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private string MakeDir(string name)
    {
        var path = Path.Combine(_root, name);
        Directory.CreateDirectory(path);
        return path;
    }

    private static SemaphoreSlim GetGate()
    {
        var field = typeof(BackupStrategyBase).GetField("_nonPriorityGate",
            BindingFlags.NonPublic | BindingFlags.Static);
        return (SemaphoreSlim)field!.GetValue(null)!;
    }

    private static void CloseGate()
    {
        var gate = GetGate();
        if (gate.CurrentCount > 0) gate.Wait(0); // consume the permit
    }

    private static void OpenGate()
    {
        var gate = GetGate();
        if (gate.CurrentCount == 0) gate.Release();
    }

    // Resets the shared static barrier to its initial state between tests.
    private static void ResetBarrier()
    {
        var type = typeof(BackupStrategyBase);
        type.GetField("_pendingPriorityFiles", BindingFlags.NonPublic | BindingFlags.Static)!
            .SetValue(null, 0);
        var gate = GetGate();
        while (gate.CurrentCount < 1) gate.Release();
        while (gate.CurrentCount > 1) gate.Wait(0);
    }

    // Records destination paths in the order CopyFile processes them.
    // Called from BackupStrategyBase.CopyFile right after File.Copy via ICryptoService.
    private sealed class OrderRecordingCryptoService : ICryptoService
    {
        public readonly ConcurrentQueue<string> CopiedFiles = new();
        public long Encrypt(string filePath) { CopiedFiles.Enqueue(filePath); return 0; }
    }

    private sealed class NullLogSerializer : ILogSerializer
    {
        public string FileExtension => ".null";
        public string Serialize(IEnumerable<LogEntry> entries) => "";
        public IEnumerable<LogEntry> Deserialize(string raw) => [];
    }
}
