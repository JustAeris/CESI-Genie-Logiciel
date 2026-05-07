using System.Collections.Concurrent;
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
        ResetPriorityBarrier();
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
        ResetPriorityBarrier();
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }

    // T-PRI-1: .pdf files (priority) must all be copied before the first .txt file
    [Fact]
    public async Task PriorityFiles_AreCopiedBeforeNonPriorityFiles()
    {
        var src1 = MakeDir("src1");
        var src2 = MakeDir("src2");
        var dst1 = MakeDir("dst1");
        var dst2 = MakeDir("dst2");

        for (var i = 0; i < 3; i++)
        {
            File.WriteAllBytes(Path.Combine(src1, $"doc{i}.pdf"), new byte[512]);
            File.WriteAllBytes(Path.Combine(src1, $"note{i}.txt"), new byte[512]);
            File.WriteAllBytes(Path.Combine(src2, $"doc{i}.pdf"), new byte[512]);
            File.WriteAllBytes(Path.Combine(src2, $"note{i}.txt"), new byte[512]);
        }

        ConfigManager.Instance.Config.PriorityExtensions = [".pdf"];

        var recorder = new OrderRecordingCryptoService();
        var strategy1 = new FullBackup();
        var strategy2 = new FullBackup();
        strategy1.SetCryptoService(recorder);
        strategy2.SetCryptoService(recorder);

        var job1 = new BackupJob { Name = "pri-j1", SourceDir = src1, TargetDir = dst1, Type = BackupType.Full };
        var job2 = new BackupJob { Name = "pri-j2", SourceDir = src2, TargetDir = dst2, Type = BackupType.Full };

        await Task.WhenAll(
            Task.Run(() => strategy1.Execute(job1, new BackupState { Name = "pri-j1" })),
            Task.Run(() => strategy2.Execute(job2, new BackupState { Name = "pri-j2" })));

        var order = recorder.CopiedFiles.ToList();
        Assert.NotEmpty(order);

        var lastPdfIdx = order.FindLastIndex(f => f.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase));
        var firstTxtIdx = order.FindIndex(f => f.EndsWith(".txt", StringComparison.OrdinalIgnoreCase));

        Assert.True(lastPdfIdx < firstTxtIdx,
            $"Expected all .pdf copies before any .txt, but last .pdf was at index {lastPdfIdx} and first .txt at {firstTxtIdx}.\nOrder: {string.Join(", ", order.Select(Path.GetFileName))}");
    }

    // T-PRI-2: empty PriorityExtensions → all files copy normally without any blocking
    [Fact]
    public void NoPriorityExtensions_AllFilesCopyWithoutBlocking()
    {
        var src = MakeDir("src");
        var dst = MakeDir("dst");

        File.WriteAllText(Path.Combine(src, "a.txt"), "text");
        File.WriteAllText(Path.Combine(src, "b.pdf"), "pdf content");

        ConfigManager.Instance.Config.PriorityExtensions = [];

        var job = new BackupJob { Name = "no-pri", SourceDir = src, TargetDir = dst, Type = BackupType.Full };
        new FullBackup().Execute(job, new BackupState { Name = "no-pri" });

        Assert.True(File.Exists(Path.Combine(dst, "a.txt")));
        Assert.True(File.Exists(Path.Combine(dst, "b.pdf")));
    }

    // T-PRI-3: _pendingPriorityFiles must be 0 after a complete execution
    [Fact]
    public void PriorityCounter_ReturnsToZeroAfterExecution()
    {
        var src = MakeDir("src");
        var dst = MakeDir("dst");

        File.WriteAllBytes(Path.Combine(src, "a.pdf"), new byte[256]);
        File.WriteAllBytes(Path.Combine(src, "b.pdf"), new byte[256]);
        File.WriteAllText(Path.Combine(src, "c.txt"), "text");

        ConfigManager.Instance.Config.PriorityExtensions = [".pdf"];

        var job = new BackupJob { Name = "cnt-job", SourceDir = src, TargetDir = dst, Type = BackupType.Full };
        new FullBackup().Execute(job, new BackupState { Name = "cnt-job" });

        var field = typeof(BackupStrategyBase).GetField("_pendingPriorityFiles",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var count = (int)field!.GetValue(null)!;

        Assert.Equal(0, count);
    }

    private string MakeDir(string name)
    {
        var path = Path.Combine(_root, name);
        Directory.CreateDirectory(path);
        return path;
    }

    // Resets the shared static barrier so tests don't interfere with each other.
    private static void ResetPriorityBarrier()
    {
        var type = typeof(BackupStrategyBase);
        var counterField = type.GetField("_pendingPriorityFiles",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        counterField!.SetValue(null, 0);

        var gateField = type.GetField("_nonPriorityGate",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var gate = (SemaphoreSlim)gateField!.GetValue(null)!;
        // Restore to exactly 1 permit (open state)
        while (gate.CurrentCount < 1) gate.Release();
        while (gate.CurrentCount > 1) gate.Wait(0);
    }

    // Records destination paths in copy order (called from CopyFile right after File.Copy).
    private sealed class OrderRecordingCryptoService : ICryptoService
    {
        public readonly ConcurrentQueue<string> CopiedFiles = new();

        public long Encrypt(string filePath)
        {
            CopiedFiles.Enqueue(filePath);
            return 0;
        }
    }

    private sealed class NullLogSerializer : ILogSerializer
    {
        public string FileExtension => ".null";
        public string Serialize(IEnumerable<LogEntry> entries) => "";
        public IEnumerable<LogEntry> Deserialize(string raw) => [];
    }
}
