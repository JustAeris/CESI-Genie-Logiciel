using EasySave.Core;

namespace EasySave.Tests;

public class DifferentialBackupTests : IDisposable
{
    private readonly string _src;
    private readonly string _dst;

    public DifferentialBackupTests()
    {
        _src = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _dst = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        Directory.CreateDirectory(_src);
        File.WriteAllText(Path.Combine(_src, "file1.txt"), "original1");
        File.WriteAllText(Path.Combine(_src, "file2.txt"), "original2");
    }

    public void Dispose()
    {
        if (Directory.Exists(_src)) Directory.Delete(_src, recursive: true);
        if (Directory.Exists(_dst)) Directory.Delete(_dst, recursive: true);
    }

    private (BackupJob job, BackupState state) Setup() => (
        new BackupJob { Name = "diff-test", SourceDir = _src, TargetDir = _dst, Type = BackupType.Differential },
        new BackupState { Name = "diff-test" }
    );

    [Fact]
    public void FirstRunCopiesAllFiles()
    {
        var (job, state) = Setup();
        new DifferentialBackup().Execute(job, state);

        Assert.Equal(2, Directory.GetFiles(_dst, "*", SearchOption.AllDirectories).Length);
    }

    [Fact]
    public void SecondRunCopiesOnlyModified()
    {
        var (job, state) = Setup();
        new DifferentialBackup().Execute(job, state);

        var dst1Before = File.GetLastWriteTime(Path.Combine(_dst, "file1.txt"));
        var dst2Before = File.GetLastWriteTime(Path.Combine(_dst, "file2.txt"));

        Thread.Sleep(10);
        File.WriteAllText(Path.Combine(_src, "file2.txt"), "modified");

        new DifferentialBackup().Execute(job, new BackupState { Name = "diff-test" });

        Assert.Equal(dst1Before, File.GetLastWriteTime(Path.Combine(_dst, "file1.txt")));
        Assert.True(File.GetLastWriteTime(Path.Combine(_dst, "file2.txt")) > dst2Before);
    }
}
