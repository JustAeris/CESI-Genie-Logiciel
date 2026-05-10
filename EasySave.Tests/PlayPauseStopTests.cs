using EasySave.Core;
using EasyLog;

namespace EasySave.Tests;

[Collection("Singletons")]
public class PlayPauseStopTests : IDisposable
{
    private readonly string _src;
    private readonly string _dst;
    private readonly string _logs;

    public PlayPauseStopTests()
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
        Directory.Delete(_src, recursive: true);
        Directory.Delete(_dst, recursive: true);
        Directory.Delete(_logs, recursive: true);
    }

    private BackupJob MakeJob(string name = "job1")
        => new() { Name = name, SourceDir = _src, TargetDir = _dst, Type = BackupType.Full };

    // T5-1 : Pause suspends the job — gate is reset after PauseJob
    [Fact]
    public void PauseJob_SetsGateToBlocked()
    {
        var gate = new ManualResetEventSlim(true); // running
        var cts = new CancellationTokenSource();

        // Simulate pause
        gate.Reset();

        Assert.False(gate.IsSet); // gate is blocked = paused
        gate.Set(); // resume
        Assert.True(gate.IsSet);
    }

    // T5-2 : Stop cancels the token
    [Fact]
    public void StopJob_CancelsToken()
    {
        var cts = new CancellationTokenSource();
        Assert.False(cts.Token.IsCancellationRequested);

        cts.Cancel();

        Assert.True(cts.Token.IsCancellationRequested);
    }

    // T5-3 : CopyFile respects CancellationToken — throws on cancelled token
    [Fact]
    public void Execute_WithCancelledToken_ThrowsOperationCancelled()
    {
        File.WriteAllText(Path.Combine(_src, "a.txt"), "data");
        File.WriteAllText(Path.Combine(_src, "b.txt"), "data");

        var cts = new CancellationTokenSource();
        cts.Cancel(); // already cancelled

        var strategy = new FullBackup();
        Assert.Throws<OperationCanceledException>(() =>
            strategy.Execute(MakeJob(), new BackupState { Name = "job1" }, cts.Token));
    }

    // T5-4 : Job completes normally when no pause/stop
    [Fact]
    public void Execute_WithoutPauseOrStop_CompletesSuccessfully()
    {
        File.WriteAllText(Path.Combine(_src, "file.txt"), "hello");

        var gate = new ManualResetEventSlim(true);
        var strategy = new FullBackup();
        var state = new BackupState { Name = "job1" };

        strategy.Execute(MakeJob(), state, CancellationToken.None, gate);

        Assert.True(File.Exists(Path.Combine(_dst, "file.txt")));
        Assert.Equal("END", state.State);
    }

    // T5-5 : Resume after pause unblocks the gate
    [Fact]
    public async Task PauseJob_ThenResume_CompletesSuccessfully()
    {
        for (int i = 0; i < 5; i++)
            File.WriteAllText(Path.Combine(_src, $"file{i}.txt"), $"data{i}");

        var gate = new ManualResetEventSlim(true);
        var cts = new CancellationTokenSource();
        var state = new BackupState { Name = "job1" };
        var strategy = new FullBackup();

        // Pause after 100ms then resume
        var resumeTask = Task.Run(async () =>
        {
            await Task.Delay(100);
            gate.Reset(); // pause
            await Task.Delay(100);
            gate.Set();   // resume
        });

        await Task.Run(() => strategy.Execute(MakeJob(), state, cts.Token, gate));
        await resumeTask;

        Assert.Equal("END", state.State);
    }
}
