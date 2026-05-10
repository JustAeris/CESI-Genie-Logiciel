namespace EasySave.Core;

/// <summary>
/// Runs cryptosoft.exe to encrypt a file.
/// A static SemaphoreSlim(1,1) ensures only one instance of cryptosoft.exe
/// runs at a time, even across parallel backup jobs (T7).
/// </summary>
public class CryptoSoftRunner : ICryptoService
{
    // Mono-instance guard: only one cryptosoft.exe at a time across all threads
    private static readonly SemaphoreSlim _mutex = new(1, 1);

    /// <summary>
    /// Encrypts a file using cryptosoft.exe.
    /// Waits for any running instance to finish before launching.
    /// </summary>
    /// <param name="filePath">Path to the file to encrypt.</param>
    /// <returns>Execution time in ms, or -1 on error.</returns>
    public long Encrypt(string filePath)
    {
        _mutex.Wait();
        try
        {
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "cryptosoft.exe",
                Arguments = filePath,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            var process = System.Diagnostics.Process.Start(startInfo);
            process?.WaitForExit();

            stopwatch.Stop();

            // Exit code > 0 = success, negative = error
            return process?.ExitCode > 0 ? stopwatch.ElapsedMilliseconds : -1;
        }
        catch
        {
            return -1;
        }
        finally
        {
            _mutex.Release();
        }
    }
}
