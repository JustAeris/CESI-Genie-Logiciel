using System.Diagnostics;
using EasyLog;

namespace EasySave.Core;

public abstract class BackupStrategyBase
{
    private ICryptoService? _cryptoService;

    // Shared across all strategy instances — counts pending priority files globally.
    private static int _pendingPriorityFiles = 0;
    private static readonly SemaphoreSlim _nonPriorityGate = new(1, 1);

    public void SetCryptoService(EasySave.Core.ICryptoService cryptoService)
    {
        _cryptoService = cryptoService;
    }

    protected string BuildDestPath(string srcFile, string srcRoot, string dstRoot)
        => Path.Combine(dstRoot, Path.GetRelativePath(srcRoot, srcFile));

    // Pre-registers all priority files in the list before copying begins.
    // Closes the gate on first registration so non-priority copies block immediately,
    // even if jobs are running in parallel and copies haven't started yet.
    protected void RegisterPriorityFiles(IEnumerable<string> files)
    {
        var extensions = ConfigManager.Instance.Config.PriorityExtensions;
        if (extensions.Count == 0) return;

        int count = files.Count(f => extensions.Any(e => e.Equals(Path.GetExtension(f), StringComparison.OrdinalIgnoreCase)));
        if (count == 0) return;

        int prev = Interlocked.Add(ref _pendingPriorityFiles, count) - count;
        if (prev == 0)
            _nonPriorityGate.Wait(); // close the gate
    }

    // Blocks non-priority files while any priority file is pending globally.
    // Priority files are already counted by RegisterPriorityFiles; no action needed here.
    protected void WaitIfBlockedByPriority(string filePath)
    {
        var extensions = ConfigManager.Instance.Config.PriorityExtensions;
        if (extensions.Count == 0) return;

        var ext = Path.GetExtension(filePath);
        bool isPriority = extensions.Any(e => e.Equals(ext, StringComparison.OrdinalIgnoreCase));

        if (!isPriority)
        {
            _nonPriorityGate.Wait();    // block until all priority files are done
            _nonPriorityGate.Release(); // pass through
        }
    }

    protected void CopyFile(string src, string dst, BackupState state, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        Directory.CreateDirectory(Path.GetDirectoryName(dst)!);

        state.SourceFilePath = src;
        state.TargetFilePath = dst;
        state.Timestamp = DateTime.Now;
        StateManager.Instance.Update(state);

        var sw = Stopwatch.StartNew();
        File.Copy(src, dst, overwrite: true);
        sw.Stop();

        // Release the priority slot once the file is on disk
        var extensions = ConfigManager.Instance.Config.PriorityExtensions;
        if (extensions.Count > 0)
        {
            var ext = Path.GetExtension(src);
            if (extensions.Any(e => e.Equals(ext, StringComparison.OrdinalIgnoreCase)))
                if (Interlocked.Decrement(ref _pendingPriorityFiles) == 0)
                    _nonPriorityGate.Release(); // reopen the gate when last priority file is done
        }

        long encryptionTime = 0;
        if (_cryptoService != null)
            encryptionTime = _cryptoService.Encrypt(dst);

        var entry = new LogEntry
        {
            Name = state.Name,
            FileSource = src,
            FileTarget = dst,
            FileSize = new FileInfo(src).Length,
            FileTransferTime = sw.ElapsedMilliseconds,
            EncryptionTime = encryptionTime,
            Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        Logger.Instance.Log(entry);

        state.DecrementProgress(new FileInfo(dst).Length, state.TotalFilesToCopy);

        StateManager.Instance.Update(state);
    }
}