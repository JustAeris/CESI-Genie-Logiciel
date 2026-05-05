using EasySave.Core;
using EasyLog;

namespace EasySave.Tests;

[Collection("Singletons")]
public class BackupPipelineTests : IDisposable
{
    private readonly string _src;
    private readonly string _dst;
    private readonly string _logs;

    public BackupPipelineTests()
    {
        _src = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _dst = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _logs = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_src);
        Directory.CreateDirectory(_dst);
        Directory.CreateDirectory(_logs);
        Logger.Instance.SetLogDirectory(_logs);
        StateManager.Instance.SetStateDirectory(_logs);
        StateManager.Instance.ClearStates();
    }

    public void Dispose()
    {
        StateManager.Instance.ClearStates();
        Logger.Instance.SetLogDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
        Directory.Delete(_src, recursive: true);
        Directory.Delete(_dst, recursive: true);
        Directory.Delete(_logs, recursive: true);
    }

    private BackupJob MakeJob(string name = "job1")
        => new() { Name = name, SourceDir = _src, TargetDir = _dst, Type = BackupType.Full };

    private static BackupState MakeState(string name)
        => new() { Name = name };

    private static FullBackup MakeFullBackup(ICryptoService? crypto = null)
    {
        var strategy = new FullBackup();
        if (crypto != null) strategy.SetCryptoService(crypto);
        return strategy;
    }

    // T26-1 : File copied without encryption when no ICryptoService is set
    [Fact]
    public void Execute_WithoutCrypto_CopiesFile()
    {
        File.WriteAllText(Path.Combine(_src, "data.txt"), "hello");

        MakeFullBackup().Execute(MakeJob(), MakeState("job1"));

        Assert.True(File.Exists(Path.Combine(_dst, "data.txt")));
        Assert.Equal("hello", File.ReadAllText(Path.Combine(_dst, "data.txt")));
    }

    // T26-2 : ICryptoService.Encrypt is called once per file when set
    [Fact]
    public void Execute_WithCrypto_CallsEncryptOnce()
    {
        File.WriteAllText(Path.Combine(_src, "secret.txt"), "data");

        var mock = new MockCryptoService();
        MakeFullBackup(mock).Execute(MakeJob(), MakeState("job1"));

        Assert.Equal(1, mock.CallCount);
    }

    // T26-3 : EncryptionTime logged as negative when encryption fails
    [Fact]
    public void Execute_WhenEncryptFails_LogsNegativeEncryptionTime()
    {
        File.WriteAllText(Path.Combine(_src, "fail.txt"), "data");

        MakeFullBackup(new MockCryptoService(returnValue: -1))
            .Execute(MakeJob(), MakeState("job1"));

        var logFile = Directory.GetFiles(_logs, "*.*").FirstOrDefault();
        Assert.NotNull(logFile);
        Assert.Contains("-1", File.ReadAllText(logFile));
    }

    // T26-4 : EncryptionTime logged as positive when encryption succeeds
    [Fact]
    public void Execute_WhenEncryptSucceeds_LogsPositiveEncryptionTime()
    {
        File.WriteAllText(Path.Combine(_src, "ok.txt"), "data");

        MakeFullBackup(new MockCryptoService(returnValue: 42))
            .Execute(MakeJob(), MakeState("job1"));

        var logFile = Directory.GetFiles(_logs, "*.*").FirstOrDefault();
        Assert.NotNull(logFile);
        Assert.Contains("42", File.ReadAllText(logFile));
    }

    // T26-5 : Without ICryptoService, EncryptionTime is 0 in log
    [Fact]
    public void Execute_WithoutCrypto_LogsZeroEncryptionTime()
    {
        File.WriteAllText(Path.Combine(_src, "plain.txt"), "data");

        MakeFullBackup().Execute(MakeJob(), MakeState("job1"));

        var logFile = Directory.GetFiles(_logs, "*.*").FirstOrDefault();
        Assert.NotNull(logFile);
        var content = File.ReadAllText(logFile);
        Assert.True(
            content.Contains("\"encryptionTime\": 0") ||
            content.Contains("\"EncryptionTime\": 0") ||
            content.Contains("\"encryptionTime\":0") ||
            content.Contains("\"EncryptionTime\":0"),
            $"Expected encryptionTime=0 in log. Actual: {content[..Math.Min(200, content.Length)]}");
    }
}

/// <summary>Fake ICryptoService for testing without cryptosoft.exe.</summary>
internal class MockCryptoService : ICryptoService
{
    private readonly long _returnValue;
    public int CallCount { get; private set; }

    public MockCryptoService(long returnValue = 10) => _returnValue = returnValue;

    public long Encrypt(string filePath)
    {
        CallCount++;
        return _returnValue;
    }
}
