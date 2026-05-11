using System.Collections.Concurrent;
using EasyLog;

namespace EasySave.Core;

/// <summary>
/// Singleton manager for all backup jobs.
/// Supports parallel execution, Pause/Resume/Stop per job (T5).
/// Auto-pauses all active jobs when business software is detected (T6).
/// </summary>
public class BackupManager
{
    private static readonly Lazy<BackupManager> _instance = new(() => new BackupManager());
    public static BackupManager Instance => _instance.Value;

    private readonly List<BackupJob> _jobs = [];

    // CancellationTokenSource per job — used for Stop
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _cts = new();

    // ManualResetEventSlim per job — set=running, reset=paused
    private readonly ConcurrentDictionary<string, ManualResetEventSlim> _pauseGates = new();

    // Business software detector + polling timer (T6)
    private IBusinessSoftwareDetector? _detector;
    private Timer? _businessSoftwareTimer;
    private bool _businessSoftwarePaused = false;

    /// <summary>
    /// Sets the business software detector and starts polling every second.
    /// When detected: all active jobs are paused.
    /// When gone: all paused jobs are resumed.
    /// </summary>
    public void SetDetector(IBusinessSoftwareDetector detector)
    {
        _detector = detector;
        _businessSoftwareTimer?.Dispose();
        _businessSoftwareTimer = new Timer(_ => PollBusinessSoftware(), null,
            TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }

    private void PollBusinessSoftware()
    {
        if (_detector == null) return;

        bool isRunning = _detector.IsRunning();

        if (isRunning && !_businessSoftwarePaused)
        {
            _businessSoftwarePaused = true;
            foreach (var jobName in _pauseGates.Keys)
            {
                PauseJob(jobName);
                Logger.Instance.Log(new LogEntry
                {
                    Name = jobName,
                    FileSource = "",
                    FileTarget = "",
                    FileSize = 0,
                    FileTransferTime = -2, // -2 = paused by business software
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
        }
        else if (!isRunning && _businessSoftwarePaused)
        {
            _businessSoftwarePaused = false;
            foreach (var jobName in _pauseGates.Keys)
            {
                ResumeJob(jobName);
                Logger.Instance.Log(new LogEntry
                {
                    Name = jobName,
                    FileSource = "",
                    FileTarget = "",
                    FileSize = 0,
                    FileTransferTime = -3, // -3 = resumed after business software
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
        }
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
            gate.Reset();
    }

    /// <summary>Resumes a paused job.</summary>
    public void ResumeJob(string jobName)
    {
        if (_pauseGates.TryGetValue(jobName, out var gate))
            gate.Set();
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
        var cts = new CancellationTokenSource();
        var gate = new ManualResetEventSlim(true); // starts as "running"

        // If business software already running, start paused
        if (_businessSoftwarePaused)
            gate.Reset();

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
    }

    private static IBackupStrategy GetStrategy(BackupType type) => type switch
    {
        BackupType.Full => new FullBackup(),
        BackupType.Differential => new DifferentialBackup(),
        _ => throw new ArgumentOutOfRangeException()
    };
}
