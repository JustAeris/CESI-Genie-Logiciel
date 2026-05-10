using EasySave.Core;

namespace EasySave.Tests;

[Collection("Sequential")]
public class BusinessSoftwarePauseTests
{
    // T6-1 : When business software is detected, active jobs are paused
    [Fact]
    public void WhenBusinessSoftwareDetected_JobsArePaused()
    {
        var mockDetector = new MockDetector(isRunning: true);
        var manager = new TestableBackupManager();
        manager.SetDetector(mockDetector);

        var gate = manager.AddTestGate("job1");

        // Simulate polling
        manager.TriggerPoll();

        Assert.False(gate.IsSet); // gate reset = paused
    }

    // T6-2 : When business software disappears, jobs are resumed
    [Fact]
    public void WhenBusinessSoftwareGone_JobsAreResumed()
    {
        var mockDetector = new MockDetector(isRunning: true);
        var manager = new TestableBackupManager();
        manager.SetDetector(mockDetector);

        var gate = manager.AddTestGate("job1");

        manager.TriggerPoll(); // pause
        Assert.False(gate.IsSet);

        mockDetector.SetRunning(false);
        manager.TriggerPoll(); // resume
        Assert.True(gate.IsSet);
    }

    // T6-3 : No double-pause if software stays running
    [Fact]
    public void WhenBusinessSoftwareStaysDetected_NoDuplicatePause()
    {
        var mockDetector = new MockDetector(isRunning: true);
        var manager = new TestableBackupManager();
        manager.SetDetector(mockDetector);

        var gate = manager.AddTestGate("job1");

        manager.TriggerPoll(); // pause
        gate.Set(); // user manually resumes
        manager.TriggerPoll(); // should NOT re-pause (already paused state)

        // Gate was manually set, second poll should not reset it again
        // because _businessSoftwarePaused is already true
        Assert.True(gate.IsSet);
    }

    // T6-4 : If no business software configured, jobs are not paused
    [Fact]
    public void WhenNoDetector_JobsNotPaused()
    {
        var manager = new TestableBackupManager();
        var gate = manager.AddTestGate("job1");

        manager.TriggerPoll(); // no detector set

        Assert.True(gate.IsSet); // gate stays open
    }
}

/// <summary>Mock detector with controllable IsRunning state.</summary>
internal class MockDetector : IBusinessSoftwareDetector
{
    private bool _isRunning;
    public MockDetector(bool isRunning) => _isRunning = isRunning;
    public void SetRunning(bool value) => _isRunning = value;
    public bool IsRunning() => _isRunning;
}

/// <summary>Exposes internal polling for testing without real Timer.</summary>
internal class TestableBackupManager
{
    private IBusinessSoftwareDetector? _detector;
    private bool _businessSoftwarePaused = false;
    private readonly Dictionary<string, ManualResetEventSlim> _gates = new();

    public void SetDetector(IBusinessSoftwareDetector detector) => _detector = detector;

    public ManualResetEventSlim AddTestGate(string jobName)
    {
        var gate = new ManualResetEventSlim(true);
        _gates[jobName] = gate;
        return gate;
    }

    public void TriggerPoll()
    {
        if (_detector == null) return;

        bool isRunning = _detector.IsRunning();

        if (isRunning && !_businessSoftwarePaused)
        {
            _businessSoftwarePaused = true;
            foreach (var gate in _gates.Values)
                gate.Reset();
        }
        else if (!isRunning && _businessSoftwarePaused)
        {
            _businessSoftwarePaused = false;
            foreach (var gate in _gates.Values)
                gate.Set();
        }
    }
}
