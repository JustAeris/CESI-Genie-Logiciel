using System.Windows;
using EasySave.Core;

namespace EasySave.GUI.MVVM.Views;

// Boîte de dialogue modale pour la création d'un nouveau job de sauvegarde.
// DialogResult = true si l'utilisateur confirme, false s'il annule.
public partial class AddJobDialog : Window
{
    // Résultat du dialogue — rempli par Add_Click avant de fermer la fenêtre
    public BackupJob? Result { get; private set; }

    public AddJobDialog()
    {
        InitializeComponent();
    }

    // Valide les champs, construit le BackupJob et ferme la fenêtre avec DialogResult = true
    private void Add_Click(object sender, RoutedEventArgs e)
    {
        // Vérifie que tous les champs obligatoires sont remplis avant de créer le job
        if (string.IsNullOrWhiteSpace(NameBox.Text) ||
            string.IsNullOrWhiteSpace(SourceBox.Text) ||
            string.IsNullOrWhiteSpace(TargetBox.Text))
        {
            MessageBox.Show("Veuillez remplir tous les champs.");
            return;
        }

        // Crée le job avec les valeurs saisies — type Full par défaut (différentiel non exposé dans la GUI)
        Result = new BackupJob
        {
            Name = NameBox.Text.Trim(),
            SourceDir = SourceBox.Text.Trim(),
            TargetDir = TargetBox.Text.Trim(),
            Type = BackupType.Full
        };

        // Signale à ShowDialog() que l'utilisateur a confirmé
        DialogResult = true;
    }

    // Ferme la fenêtre sans créer de job — Result reste null
    private void Cancel_Click(object sender, RoutedEventArgs e)
        => DialogResult = false;
}
