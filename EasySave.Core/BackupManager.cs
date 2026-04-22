namespace EasySave.Core;

public class BackupManager
{
    // Singleton
    private static BackupManager? _instance;
    private static readonly object _lock = new object();

    // Attributes
    private List<BackupJob> _jobs = new List<BackupJob>();

    // Private constructor
    private BackupManager() { }

    // Unique instance
    public static BackupManager Instance
    {
        get
        {
            lock (_lock)
            {
                if (_instance == null)
                    _instance = new BackupManager();
                return _instance;
            }
        }
    }

    // Add a job (max 5)
    public void AddJob(BackupJob job)
    {
        if (_jobs.Count < 5)
            _jobs.Add(job);
    }

    // Remove a job by index
    public void RemoveJob(int index)
    {
        if (index >= 0 && index < _jobs.Count)
            _jobs.RemoveAt(index);
    }

    // Run a specific job by index
    public void RunJob(int index)
    {
        if (index >= 0 && index < _jobs.Count)
        {
            var job = _jobs[index];
            var state = new BackupState { Name = job.Name };
            var strategy = GetStrategy(job.Type);
            strategy.Execute(job, state);
        }
    }

    // Run all jobs
    public void RunAll()
    {
        for (int i = 0; i < _jobs.Count; i++)
            RunJob(i);
    }

    // Ce qui était déjà là - on ne touche pas
    private IBackupStrategy GetStrategy(BackupType type) => type switch
    {
        BackupType.Full => new FullBackup(),
        BackupType.Differential => new DifferentialBackup(),
        _ => throw new ArgumentOutOfRangeException()
    };
}
