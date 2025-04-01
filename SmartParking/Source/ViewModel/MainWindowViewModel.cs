using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SmartParking.Source.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private const string Token = "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpYXQiOjE3NDIzMDg4NTEsImV4cCI6MTAxNzQyMzA4ODUxLCJkYXRhIjp7ImlkIjoxLCJ1c2VybmFtZSI6IlNtYXJ0UGFya2luZyJ9fQ.B-dPPnoL4DnwsZ6_j6GRxs74Zn5XLQw-K8OjWIbegjk";
        private const string ApiUrl = "http://smartparking.alwaysdata.net/ConnexionAdmin";

        private string _username;
        private string _password;
        private string _errorMessage;

        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(nameof(Username)); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(nameof(Password)); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(nameof(ErrorMessage)); }
        }

        public ICommand LoginCommand { get; }

        public event Action OnLoginSuccess;

        public MainWindowViewModel()
        {
            LoginCommand = new SmartParking.Source.Modele.RelayCommand(async () => await AuthenticateUser());
        }

        private async Task AuthenticateUser()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Veuillez remplir tous les champs.";
                return;
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", Token);

                    var requestBody = new
                    {
                        Username = this.Username,
                        Password = this.Password
                    };

                    string json = JsonConvert.SerializeObject(requestBody);
                    StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(ApiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        dynamic result = JsonConvert.DeserializeObject(responseContent);

                        if (result.message == "Authentification réussie.")
                        {
                            OnLoginSuccess?.Invoke();
                            return;
                        }
                    }

                    ErrorMessage = "Identifiant ou mot de passe incorrect.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur : {ex.Message}";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
