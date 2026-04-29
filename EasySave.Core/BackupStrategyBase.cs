using System.Diagnostics;
using EasyLog;

namespace EasySave.Core;

public abstract class BackupStrategyBase
{
    // Crypto service (can be null if no encryption needed)
    private ICryptoService? _cryptoService;

    // Set the crypto service
    public void SetCryptoService(ICryptoService cryptoService)
    {
        _cryptoService = cryptoService;
    }

    // Replaces the source root with the target root to build the destination path
    protected string BuildDestPath(string srcFile, string srcRoot, string dstRoot)
        => Path.Combine(dstRoot, Path.GetRelativePath(srcRoot, srcFile));

    /// <summary>
    /// Copies one file: creates dest directory, updates state, measures transfer time,
    /// copies the file, logs the entry, updates progression.
    /// </summary>
    protected void CopyFile(string src, string dst, BackupState state)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(dst)!);

        state.SourceFilePath = src;
        state.TargetFilePath = dst;
        state.Timestamp = DateTime.Now;
        StateManager.Instance.Update(state);

        var sw = Stopwatch.StartNew();
        File.Copy(src, dst, overwrite: true);
        sw.Stop();

        // Encrypt file if crypto service is set
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
            Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };
        Logger.Instance.Log(entry);

        state.NbFilesLeftToDo--;
        state.SizeLeft -= new FileInfo(dst).Length;
        state.Progression = state.TotalFilesToCopy == 0
            ? 100.0
            : (state.TotalFilesToCopy - state.NbFilesLeftToDo) / (double)state.TotalFilesToCopy * 100.0;
        StateManager.Instance.Update(state);
    }
}