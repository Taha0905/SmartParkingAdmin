using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;

namespace SmartParking
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            // Vérifier si les champs sont remplis
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Veuillez remplir tous les champs.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Appeler l'API pour l'authentification
            bool isAuthenticated = await AuthenticateUser(username, password);

            if (isAuthenticated)
            {
                // Navigation vers la page Admin
                Admin adminPage = new Admin();
                this.Content = adminPage;
            }
            else
            {
                // Afficher un message d'erreur
                MessageBox.Show("Identifiant ou mot de passe incorrect.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<bool> AuthenticateUser(string username, string password)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Construire le corps de la requête
                    var requestBody = new
                    {
                        Username = username,
                        Password = password
                    };

                    string json = JsonConvert.SerializeObject(requestBody);
                    StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                    // Envoyer la requête POST
                    HttpResponseMessage response = await client.PostAsync("https://smartparking.alwaysdata.net/LoginAdmin", content);

                    // Lire la réponse
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        dynamic result = JsonConvert.DeserializeObject(responseContent);

                        // Vérifier si l'authentification est réussie
                        if (result.message == "Authentification réussie.")
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'authentification : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return false;
        }
    }
}
