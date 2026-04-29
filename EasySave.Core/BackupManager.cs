using EasyLog;

namespace EasySave.Core;

public class BackupManager
{
    private static readonly Lazy<BackupManager> _instance = new(() => new BackupManager());
    public static BackupManager Instance => _instance.Value;

    private readonly List<BackupJob> _jobs = [];

    // Business software detector (optional)
    private IBusinessSoftwareDetector? _detector;

    // Set the business software detector
    public void SetDetector(IBusinessSoftwareDetector detector)
    {
        _detector = detector;
    }

    public void AddJob(BackupJob job) => _jobs.Add(job);

    public void RemoveJob(int index)
    {
        if (index < 1 || index > _jobs.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        _jobs.RemoveAt(index - 1);
    }

    public void RunJob(int index)
    {
        if (index < 1 || index > _jobs.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        RunJob(_jobs[index - 1]);
    }

    public void RunAll()
    {
        foreach (var job in _jobs)
            RunJob(job);
    }

    private void RunJob(BackupJob job)
    {
        // Block if business software detected
        if (_detector != null && _detector.IsRunning())
        {
            Logger.Instance.Log(new EasyLog.LogEntry
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

        var strategy = GetStrategy(job.Type);
        var state = new BackupState { Name = job.Name };
        strategy.Execute(job, state);
    }

    private IBackupStrategy GetStrategy(BackupType type) => type switch
    {
        BackupType.Full => new FullBackup(),
        BackupType.Differential => new DifferentialBackup(),
        _ => throw new ArgumentOutOfRangeException()
    };
}