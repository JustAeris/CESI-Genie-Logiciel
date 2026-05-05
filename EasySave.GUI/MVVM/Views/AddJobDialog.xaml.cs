using System.Windows;
using EasySave.Core;

namespace EasySave.GUI.MVVM.Views;

/// <summary>
/// Dialog for adding a new backup job.
/// </summary>
public partial class AddJobDialog : Window
{
    public BackupJob? Result { get; private set; }

    public AddJobDialog()
    {
        InitializeComponent();
    }

    private void Add_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text) ||
            string.IsNullOrWhiteSpace(SourceBox.Text) ||
            string.IsNullOrWhiteSpace(TargetBox.Text))
        {
            MessageBox.Show("Veuillez remplir tous les champs.");
            return;
        }

        Result = new BackupJob
        {
            Name = NameBox.Text.Trim(),
            SourceDir = SourceBox.Text.Trim(),
            TargetDir = TargetBox.Text.Trim(),
            Type = BackupType.Full
        };

        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
        => DialogResult = false;
}
