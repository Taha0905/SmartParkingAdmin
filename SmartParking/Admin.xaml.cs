using System.Windows;
using System.Windows.Controls;

namespace SmartParking
{
    public partial class Admin : Page
    {
        public Admin()
        {
            InitializeComponent();
            // Affichage de la page "Vue d'ensemble" par défaut
            ContentFrame.Navigate(new VueDensemble());
        }

        // Gestion du clic sur le bouton "Analyse"
        private void AnalyseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ContentFrame.Navigate(new Analyse());
        }

        // Gestion du clic sur le bouton "Vue d'ensemble"
        private void VueDensembleButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ContentFrame.Navigate(new VueDensemble());
        }

        // Gestion du clic sur le bouton "Caméra"
        private void CameraButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ContentFrame.Navigate(new Camera());
        }

        private void LogoutButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Retour à la page de connexion
            MainWindow loginWindow = new MainWindow();
            loginWindow.Show(); // Afficher la fenêtre de connexion
            Window.GetWindow(this).Close(); // Fermer la fenêtre actuelle (Admin)
        }
    }
}
