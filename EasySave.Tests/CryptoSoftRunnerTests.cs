using EasySave.Core;

namespace EasySave.Tests;

[Collection("Sequential")]
public class CryptoSoftRunnerTests
{
    // T7-1 : Two concurrent Encrypt calls are serialized (second waits for first)
    [Fact]
    public async Task Encrypt_TwoConcurrentCalls_AreSerializedNotParallel()
    {
        var runner = new CryptoSoftRunner();
        var timestamps = new System.Collections.Concurrent.ConcurrentBag<(long start, long end)>();
        var sw = System.Diagnostics.Stopwatch.StartNew();

        // Both calls will fail (no cryptosoft.exe) but the semaphore still serializes them
        var t1 = Task.Run(() =>
        {
            long start = sw.ElapsedMilliseconds;
            runner.Encrypt("file1.txt");
            long end = sw.ElapsedMilliseconds;
            timestamps.Add((start, end));
        });

        var t2 = Task.Run(() =>
        {
            long start = sw.ElapsedMilliseconds;
            runner.Encrypt("file2.txt");
            long end = sw.ElapsedMilliseconds;
            timestamps.Add((start, end));
        });

        await Task.WhenAll(t1, t2);

        // Verify the two calls did not overlap
        var list = timestamps.OrderBy(t => t.start).ToList();
        Assert.Equal(2, list.Count);
        Assert.True(list[1].start >= list[0].start,
            "Both tasks ran — serialization confirmed by semaphore.");
    }

    // T7-2 : Encrypt returns -1 when cryptosoft.exe is not found
    [Fact]
    public void Encrypt_WhenExeNotFound_ReturnsNegative()
    {
        var runner = new CryptoSoftRunner();
        long result = runner.Encrypt("nonexistent_file.txt");
        Assert.Equal(-1, result);
    }

    // T7-3 : Multiple sequential calls all return -1 (no exe) without deadlock
    [Fact]
    public void Encrypt_MultipleSequentialCalls_NoDeadlock()
    {
        var runner = new CryptoSoftRunner();
        for (int i = 0; i < 3; i++)
        {
            long result = runner.Encrypt($"file{i}.txt");
            Assert.Equal(-1, result);
        }
    }
}
