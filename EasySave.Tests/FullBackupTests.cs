using EasySave.Core;

namespace EasySave.Tests;

public class FullBackupTests : IDisposable
{
    private readonly string _src;
    private readonly string _dst;

    public FullBackupTests()
    {
        _src = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _dst = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        Directory.CreateDirectory(Path.Combine(_src, "sub1"));
        Directory.CreateDirectory(Path.Combine(_src, "sub2"));
        File.WriteAllText(Path.Combine(_src, "file1.txt"), "a");
        File.WriteAllText(Path.Combine(_src, "sub1", "file2.txt"), "b");
        File.WriteAllText(Path.Combine(_src, "sub2", "file3.txt"), "c");
    }

    public void Dispose()
    {
        if (Directory.Exists(_src)) Directory.Delete(_src, recursive: true);
        if (Directory.Exists(_dst)) Directory.Delete(_dst, recursive: true);
    }

    private (BackupJob job, BackupState state) Setup() => (
        new BackupJob { Name = "test", SourceDir = _src, TargetDir = _dst, Type = BackupType.Full },
        new BackupState { Name = "test" }
    );

    [Fact]
    public void CopiesAllFiles()
    {
        var (job, state) = Setup();
        new FullBackup().Execute(job, state);

        Assert.Equal(3, Directory.GetFiles(_dst, "*", SearchOption.AllDirectories).Length);
        Assert.True(File.Exists(Path.Combine(_dst, "file1.txt")));
        Assert.True(File.Exists(Path.Combine(_dst, "sub1", "file2.txt")));
        Assert.True(File.Exists(Path.Combine(_dst, "sub2", "file3.txt")));
    }

    [Fact]
    public void StateIsEndAfterRun()
    {
        var (job, state) = Setup();
        new FullBackup().Execute(job, state);

        Assert.Equal("END", state.State);
    }

    [Fact]
    public void ProgressionIs100()
    {
        var (job, state) = Setup();
        new FullBackup().Execute(job, state);

        Assert.Equal(100.0, state.Progression);
    }
}
