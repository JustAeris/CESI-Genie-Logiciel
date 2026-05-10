namespace EasySave.Core;

public class CryptoSoftRunner : ICryptoService
{
    // SemaphoreSlim(1,1) — only one call to cryptosoft.exe at a time
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    // Run cryptosoft.exe and return execution time in ms (negative = error)
    public long Encrypt(string filePath)
    {
        _semaphore.Wait();
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

            // Exit code > 0 = success, < 0 = error
            if (process?.ExitCode > 0)
                return stopwatch.ElapsedMilliseconds;
            else
                return -1;
        }
        catch
        {
            return -1;
        }
        finally
        {
            // Always release the semaphore
            _semaphore.Release();
        }
    }
}