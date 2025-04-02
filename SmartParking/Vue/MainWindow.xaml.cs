using SmartParking.Control;
using SmartParking.Model;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace SmartParking
{
    public partial class MainWindow : Window
    {
        private readonly AuthController _authController = new AuthController();

        public MainWindow()
        {
            InitializeComponent();
            this.WindowStyle = WindowStyle.None;
            this.WindowState = WindowState.Maximized;
            this.ResizeMode = ResizeMode.NoResize;
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Veuillez remplir tous les champs.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var credentials = new AdminCredentials
            {
                Username = username,
                Password = password
            };

            bool isAuthenticated = await _authController.AuthenticateUser(credentials);

            if (isAuthenticated)
            {
                Admin adminPage = new Admin();
                this.Content = adminPage;
            }
            else
            {
                MessageBox.Show("Identifiant ou mot de passe incorrect.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
