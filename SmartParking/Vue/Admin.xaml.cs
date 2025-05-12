using System.Windows;
using System.Windows.Controls;

namespace SmartParking
{
    public partial class Admin : Page
    {
        private Analyse analysePage;

        public Admin()
        {
            InitializeComponent();

            // Autoriser les popups MQTT uniquement ici (admin connecté)
            MqttManager.Instance.AutoriserPopups = true;

            // Initialiser la page Analyse (facultatif si tu veux déjà démarrer le MQTT ici aussi)
            analysePage = new Analyse();
            analysePage.StartMQTT();

            // Affichage par défaut
            ContentFrame.Navigate(new VueDensemble());
        }

        private void AnalyseButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(analysePage);
        }

        private void VueDensembleButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new VueDensemble());
        }

        private void CameraButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new Camera());
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Désactiver les popups lors de la déconnexion
            MqttManager.Instance.AutoriserPopups = false;

            MainWindow loginWindow = new MainWindow();
            loginWindow.Show();
            Window.GetWindow(this).Close();
        }

        private void AideButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new Aide());
        }
    }
}
