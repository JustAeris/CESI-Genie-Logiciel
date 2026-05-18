using EasySave.GUI.MVVM;

namespace EasySave.GUI.Services;

// Singleton qui diffuse les messages de statut vers la barre d'état de la fenêtre principale.
// Le ViewModel principal se bind à Message pour l'afficher.
public class StatusService : ViewModelBase
{
    private static readonly Lazy<StatusService> _instance = new(() => new StatusService());
    public static StatusService Instance => _instance.Value;

    // Message actuellement affiché dans la barre de statut
    private string _message = "";
    public string Message
    {
        get => _message;
        set => SetField(ref _message, value); // notifie l'UI pour rafraîchir la barre
    }

    // Initialise avec le message "Prêt / Ready" dès la création du singleton
    private StatusService() => SetIdle();

    // Affiche l'état de repos — utilisé au démarrage et après un changement de langue
    public void SetIdle() => Message = LocalizationService.Instance.StatusIdle;

    // Affiche qu'un job est en cours d'exécution
    public void SetRunning(string jobName) => Message = $"[ RUN  ] — {jobName} {LocalizationService.Instance.StatusRunning}";

    // Affiche qu'un job s'est terminé avec sa durée en millisecondes
    public void SetDone(string jobName, long ms) => Message = $"[ DONE ] — {jobName} {LocalizationService.Instance.StatusDone} {ms} ms";

    // Affiche qu'un job a échoué
    public void SetError(string jobName) => Message = $"[ ERR  ] — {jobName} {LocalizationService.Instance.StatusError}";
}
