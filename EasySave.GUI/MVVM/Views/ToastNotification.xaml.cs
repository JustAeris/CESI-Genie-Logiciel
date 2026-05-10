using System.Windows;

namespace EasySave.GUI.MVVM.Views;

public partial class ToastNotification : Window
{
    public ToastNotification(string title, string body)
    {
        InitializeComponent();
        TitleText.Text = title;
        BodyText.Text = body;

        Loaded += (_, _) => PositionBottomRight();
    }

    private void PositionBottomRight()
    {
        var screen = SystemParameters.WorkArea;
        Left = screen.Right - ActualWidth - 20;
        Top = screen.Bottom - ActualHeight - 20;
    }

    public static void Show(string title, string body)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var toast = new ToastNotification(title, body);
            toast.Show();

            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            timer.Tick += (_, _) => { timer.Stop(); toast.Close(); };
            timer.Start();
        });
    }
}
