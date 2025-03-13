using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Net.Http;
using MQTTnet.Client.Receiving;

namespace SmartParking
{
    public partial class VueDensemble : Page
    {
        private readonly string apiReservations = "https://smartparking.alwaysdata.net/getAllReservations";
        private readonly string apiDeleteReservation = "https://smartparking.alwaysdata.net/deleteReservation";

        private readonly DispatcherTimer refreshTimer;
        private IMqttClient mqttClient;
        private Dictionary<string, string> placesEtat = new Dictionary<string, string>(); // Stocke l'état des places

        public ObservableCollection<Reservation> Reservations { get; set; }

        public VueDensemble()
        {
            InitializeComponent();
            Reservations = new ObservableCollection<Reservation>();
            ListViewReservations.ItemsSource = Reservations;

            // Charger uniquement les réservations depuis la BDD
            LoadReservationsData();

            // Connexion au Broker MQTT pour gérer les places
            Task.Run(async () => await ConnectToMqttBrokerAsync());

            // Rafraîchir les réservations toutes les 5 secondes
            refreshTimer = new DispatcherTimer();
            refreshTimer.Interval = TimeSpan.FromSeconds(5);
            refreshTimer.Tick += (s, e) => { LoadReservationsData(); };
            refreshTimer.Start();
        }

        private async Task ConnectToMqttBrokerAsync()
        {
            var factory = new MqttFactory();
            mqttClient = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithClientId("SmartParking_Client")
                .WithTcpServer("172.31.254.254", 1883) // Adresse et port de ton broker MQTT
                .WithCleanSession(false) // Garde l'historique des messages
                .Build();

            mqttClient.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(e =>
            {
                string topic = e.ApplicationMessage.Topic;
                string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                ProcessMqttMessage(topic, payload);
            });

            await mqttClient.ConnectAsync(options, CancellationToken.None);

            // Liste des topics MQTT correspondant aux places du parking
            string[] topics = { "places/place1", "places/place2", "places/place3",
                                "places/place4", "places/place5", "places/place6" };

            foreach (var topic in topics)
            {
                placesEtat[topic] = "Inconnu"; // Valeur par défaut
                await mqttClient.SubscribeAsync(topic);
            }
        }

        private void ProcessMqttMessage(string topic, string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                placesEtat[topic] = message;

                // Recalculer les totaux
                int placesLibres = 0;
                int placesOccupees = 0;

                foreach (var etat in placesEtat.Values)
                {
                    if (etat.Equals("Libre", StringComparison.OrdinalIgnoreCase))
                        placesLibres++;
                    else if (etat.Equals("Prise", StringComparison.OrdinalIgnoreCase))
                        placesOccupees++;
                }

                // Mise à jour des TextBlocks dans VueDensemble.xaml
                TextLibre.Text = placesLibres.ToString();
                TextOccupe.Text = placesOccupees.ToString();
            });
        }

        private async void LoadReservationsData()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(apiReservations);
                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        List<Reservation> reservationsList = JsonConvert.DeserializeObject<List<Reservation>>(jsonResponse);

                        Reservations.Clear();
                        foreach (var res in reservationsList)
                        {
                            res.DateReservationFormatted = DateTime.Parse(res.DateReservation).ToString("dd/MM/yyyy HH:mm");
                            TimeSpan duration = TimeSpan.Parse(res.TempsReservation);
                            res.TempsReservationFormatted = $"{duration.Hours}h {duration.Minutes}m";
                            res.DeleteCommand = new RelayCommand(async () => await DeleteReservation(res.IDReservation));
                            Reservations.Add(res);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Erreur lors de la récupération des réservations.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteReservation(int reservationId)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage deleteResponse = await client.DeleteAsync($"{apiDeleteReservation}/{reservationId}");
                    if (!deleteResponse.IsSuccessStatusCode)
                    {
                        MessageBox.Show($"Erreur lors de la suppression de la réservation. Réponse API: {await deleteResponse.Content.ReadAsStringAsync()}",
                                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Mise à jour des données après la suppression
                    LoadReservationsData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class Reservation
    {
        public int IDReservation { get; set; }
        public string DateReservation { get; set; }
        public string TempsReservation { get; set; }
        public string Immatriculation { get; set; }

        public string DateReservationFormatted { get; set; }
        public string TempsReservationFormatted { get; set; }

        public ICommand DeleteCommand { get; set; }
    }

    public class RelayCommand : ICommand
    {
        private readonly Func<Task> _executeAsync;
        public event EventHandler CanExecuteChanged;

        public RelayCommand(Func<Task> executeAsync) => _executeAsync = executeAsync;

        public bool CanExecute(object parameter) => true;

        public async void Execute(object parameter) => await _executeAsync();
    }
}
