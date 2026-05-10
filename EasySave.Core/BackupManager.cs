using System.Collections.Concurrent;
using EasyLog;

namespace EasySave.Core;

/// <summary>
/// Singleton manager for all backup jobs.
/// Supports parallel execution, Pause/Resume/Stop per job (T5).
/// </summary>
public class BackupManager
{
    private static readonly Lazy<BackupManager> _instance = new(() => new BackupManager());
    public static BackupManager Instance => _instance.Value;

    private readonly List<BackupJob> _jobs = [];
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _cts = new();

    // CancellationTokenSource per job — used for Stop
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _cts = new();

    // ManualResetEventSlim per job — set=running, reset=paused
    private readonly ConcurrentDictionary<string, ManualResetEventSlim> _pauseGates = new();

    // Business software detector (optional)
    private IBusinessSoftwareDetector? _detector;

    /// <summary>Sets the business software detector used to block/pause jobs.</summary>
    public void SetDetector(IBusinessSoftwareDetector detector) => _detector = detector;

    public void CancelJob(string jobName)
    {
        if (_cts.TryGetValue(jobName, out var cts))
            cts.Cancel();
    }

    public void AddJob(BackupJob job) => _jobs.Add(job);

    public void RemoveJob(int index)
    {
        if (index < 1 || index > _jobs.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        _jobs.RemoveAt(index - 1);
    }

    // --- Playback controls (T5) ---

    /// <summary>Pauses the job after the current file finishes copying.</summary>
    public void PauseJob(string jobName)
    {
        if (_pauseGates.TryGetValue(jobName, out var gate))
            gate.Reset(); // block the next CopyFile
    }

    /// <summary>Resumes a paused job.</summary>
    public void ResumeJob(string jobName)
    {
        if (_pauseGates.TryGetValue(jobName, out var gate))
            gate.Set(); // unblock
    }

    /// <summary>Stops a job immediately via CancellationToken.</summary>
    public void StopJob(string jobName)
    {
        if (_cts.TryGetValue(jobName, out var cts))
            cts.Cancel();
    }

    /// <summary>Alias for StopJob — kept for compatibility.</summary>
    public void CancelJob(string jobName) => StopJob(jobName);

    /// <summary>Returns the current playback state of a job.</summary>
    public PlaybackState GetPlaybackState(string jobName)
    {
        if (!_cts.ContainsKey(jobName)) return PlaybackState.Stopped;
        if (_pauseGates.TryGetValue(jobName, out var gate) && !gate.IsSet)
            return PlaybackState.Paused;
        return PlaybackState.Running;
    }

    // --- Run ---

    public void RunJob(int index)
    {
        if (index < 1 || index > _jobs.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        RunJob(_jobs[index - 1]);
    }

    public void RunAll()
    {
        var tasks = _jobs.Select(job => Task.Run(() => RunJob(job))).ToArray();
        Task.WaitAll(tasks);
    }

    private void RunJob(BackupJob job)
    {
        // Block if business software detected
        if (_detector != null && _detector.IsRunning())
        {
            Logger.Instance.Log(new LogEntry
            {
                Name = job.Name,
                FileSource = "",
                FileTarget = "",
                FileSize = 0,
                FileTransferTime = -1,
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            });
            return;
        }

        var cts = new CancellationTokenSource();
<<<<<<< HEAD
        var gate = new ManualResetEventSlim(true); // starts as "running"

        _cts[job.Name] = cts;
        _pauseGates[job.Name] = gate;

        try
        {
            var strategy = GetStrategy(job.Type);
            var state = new BackupState { Name = job.Name, PlaybackState = PlaybackState.Running };
            strategy.Execute(job, state, cts.Token, gate);
        }
        finally
        {
            _cts.TryRemove(job.Name, out _);
            _pauseGates.TryRemove(job.Name, out _);
        }
=======
        _cts[job.Name] = cts;

        var strategy = GetStrategy(job.Type);
        var state = new BackupState { Name = job.Name };
        strategy.Execute(job, state, cts.Token);

        _cts.TryRemove(job.Name, out _);
>>>>>>> fac1da1 (fix: resolve merge conflict in BackupPipelineTests)
    }

    private static IBackupStrategy GetStrategy(BackupType type) => type switch
    {
        BackupType.Full => new FullBackup(),
        BackupType.Differential => new DifferentialBackup(),
        _ => throw new ArgumentOutOfRangeException()
    };
}
