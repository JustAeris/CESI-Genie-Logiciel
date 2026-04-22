using System.Text.Json;

namespace EasySave.Core;

public class StateManager
{
    // Singleton
    private static readonly Lazy<StateManager> _instance = new(() => new StateManager());
    public static StateManager Instance => _instance.Value;
    private StateManager() { }
    // Store all backup states
    private List<BackupState> _states = new List<BackupState>();

    // Update or add a state
    public void Update(BackupState state)
    {
        _states.RemoveAll(s => s.Name == state.Name);
        _states.Add(state);
        Persist();
    }

    // Return all states
    public List<BackupState> GetAll()
    {
        return _states;
    }

    // Save states to JSON file
    private void Persist()
    {
        string json = JsonSerializer.Serialize(_states, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText("state.json", json);
    }
}