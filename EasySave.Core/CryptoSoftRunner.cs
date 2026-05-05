namespace EasySave.Core;

public class CryptoSoftRunner : ICryptoService
{
    // Run cryptosoft.exe and return execution time in ms (negative = error)
    public long Encrypt(string filePath)
    {
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
    }
}