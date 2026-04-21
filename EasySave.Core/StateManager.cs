using System.Text.Json;

namespace EasySave.Core;

public class StateManager
{
    private static readonly Lazy<StateManager> _instance = new(() => new StateManager());
    public static StateManager Instance => _instance.Value;


    public void Update(BackupState state) { }
}
 