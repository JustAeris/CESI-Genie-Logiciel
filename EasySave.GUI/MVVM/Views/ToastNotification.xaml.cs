using System.Windows;

namespace EasySave.GUI.MVVM.Views;

// Fenêtre de notification temporaire affichée en bas à droite de l'écran.
// Se ferme automatiquement après 3 secondes.
public partial class ToastNotification : Window
{
    public ToastNotification(string title, string body)
    {
        InitializeComponent();
        TitleText.Text = title;
        BodyText.Text = body;

        // Positionne la fenêtre une fois que WPF connaît ses dimensions réelles
        Loaded += (_, _) => PositionBottomRight();
    }

    // Place la fenêtre dans le coin inférieur droit de la zone de travail (hors barre des tâches)
    private void PositionBottomRight()
    {
        var screen = SystemParameters.WorkArea; // zone de travail sans la barre des tâches
        Left = screen.Right - ActualWidth - 20;
        Top = screen.Bottom - ActualHeight - 20;
    }

    // Crée et affiche une notification sur le thread UI, puis la ferme automatiquement après 3 secondes.
    public static void Show(string title, string body)
    {
        // Dispatch sur le thread principal — cette méthode peut être appelée depuis un thread de fond
        Application.Current.Dispatcher.Invoke(() =>
        {
            var toast = new ToastNotification(title, body);
            toast.Show();

            // Timer WPF qui ferme la fenêtre après 3 secondes
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            timer.Tick += (_, _) => { timer.Stop(); toast.Close(); };
            timer.Start();
        });
    }
}
