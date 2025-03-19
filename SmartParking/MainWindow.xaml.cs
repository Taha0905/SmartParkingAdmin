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
        private const string Token = "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpYXQiOjE3NDIzMDg4NTEsImV4cCI6MTAxNzQyMzA4ODUxLCJkYXRhIjp7ImlkIjoxLCJ1c2VybmFtZSI6IlNtYXJ0UGFya2luZyJ9fQ.B-dPPnoL4DnwsZ6_j6GRxs74Zn5XLQw-K8OjWIbegjk";
        private const string ApiUrl = "http://smartparking.alwaysdata.net/ConnexionAdmin";

        public MainWindow()
        {
            InitializeComponent();

            // Configurer la fenêtre pour qu'elle s'ouvre en plein écran
            this.WindowStyle = WindowStyle.None;
            this.WindowState = WindowState.Maximized;
            this.ResizeMode = ResizeMode.NoResize;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Gestion du clic sur le bouton de connexion
        // Vérifie les entrées utilisateur et tente une authentification via l'API
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            // Vérifie que les champs ne sont pas vides
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Veuillez remplir tous les champs.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Tente l'authentification
            bool isAuthenticated = await AuthenticateUser(username, password);

            if (isAuthenticated)
            {
                // Si succès, naviguer vers la page Admin
                Admin adminPage = new Admin();
                this.Content = adminPage;
            }
            else
            {
                // Sinon, afficher un message d'erreur
                MessageBox.Show("Identifiant ou mot de passe incorrect.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Fonction d'authentification de l'utilisateur
        // Envoie une requête POST avec les identifiants et vérifie la réponse de l'API
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private async Task<bool> AuthenticateUser(string username, string password)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Ajouter le token dans les en-têtes de la requête
                    client.DefaultRequestHeaders.Add("Authorization", Token);

                    // Préparer le corps de la requête avec les identifiants
                    var requestBody = new
                    {
                        Username = username,
                        Password = password
                    };

                    string json = JsonConvert.SerializeObject(requestBody);
                    StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                    // Envoyer la requête POST
                    HttpResponseMessage response = await client.PostAsync(ApiUrl, content);

                    // Vérifier la réponse
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        dynamic result = JsonConvert.DeserializeObject(responseContent);

                        // Si l'authentification réussit, retourner vrai
                        if (result.message == "Authentification réussie.")
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Gérer les erreurs et afficher un message
                MessageBox.Show($"Erreur lors de l'authentification : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return false;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
