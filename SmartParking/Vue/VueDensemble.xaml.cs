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
using System.Net.Http;
using MQTTnet.Client.Receiving;
using SmartParking.Model;

namespace SmartParking
{
    public partial class VueDensemble : Page
    {
        private const string Token = "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpYXQiOjE3NDIzMDg4NTEsImV4cCI6MTAxNzQyMzA4ODUxLCJkYXRhIjp7ImlkIjoxLCJ1c2VybmFtZSI6IlNtYXJ0UGFya2luZyJ9fQ.B-dPPnoL4DnwsZ6_j6GRxs74Zn5XLQw-K8OjWIbegjk";
        private const string ApiReservations = "https://smartparking.alwaysdata.net/getAllReservations";
        private const string ApiDeleteReservation = "https://smartparking.alwaysdata.net/deleteReservation";

        private readonly DispatcherTimer refreshTimer;
        private IMqttClient mqttClient;
        private Dictionary<string, string> placesEtat = new Dictionary<string, string>();

        public ObservableCollection<Reservation> Reservations { get; set; }

        public VueDensemble()
        {
            InitializeComponent();
            Reservations = new ObservableCollection<Reservation>();
            ListViewReservations.ItemsSource = Reservations;

            LoadReservationsData();

            Task.Run(async () => await ConnectToMqttBrokerAsync());

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
                .WithTcpServer("172.31.254.254", 1883)
                .WithCleanSession(false)
                .Build();

            mqttClient.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(e =>
            {
                string topic = e.ApplicationMessage.Topic;
                string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                ProcessMqttMessage(topic, payload);
            });

            await mqttClient.ConnectAsync(options, CancellationToken.None);

            string[] topics = { "places/place1", "places/place2", "places/place3",
                                "places/place4", "places/place5", "places/place6" };

            foreach (var topic in topics)
            {
                placesEtat[topic] = "Inconnu";
                await mqttClient.SubscribeAsync(topic);
            }
        }

        private void ProcessMqttMessage(string topic, string message)
        {
            // Vérifier si l'application est en cours de fermeture
            if (Application.Current == null || Application.Current.Dispatcher == null)
                return;

            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (placesEtat == null) return; // Vérifie que le dictionnaire est bien initialisé

                    placesEtat[topic] = message;

                    int placesLibres = 0;
                    int placesOccupees = 0;

                    foreach (var etat in placesEtat.Values)
                    {
                        if (etat.Equals("Libre", StringComparison.OrdinalIgnoreCase))
                            placesLibres++;
                        else if (etat.Equals("Prise", StringComparison.OrdinalIgnoreCase))
                            placesOccupees++;
                    }

                    // Vérifier que les contrôles existent avant de les modifier
                    if (TextLibre != null && TextOccupe != null)
                    {
                        TextLibre.Text = placesLibres.ToString();
                        TextOccupe.Text = placesOccupees.ToString();
                    }
                });
            }
            catch (TaskCanceledException)
            {
                // Ne rien faire, l'application est en train de se fermer
            }
            catch (Exception ex)
            {
                // Log de l'erreur sans bloquer l'application
                Console.WriteLine($"Erreur dans ProcessMqttMessage : {ex.Message}");
            }
        }


        private async void LoadReservationsData()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", Token);

                    HttpResponseMessage response = await client.GetAsync(ApiReservations);
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
                    client.DefaultRequestHeaders.Add("Authorization", Token);

                    HttpResponseMessage deleteResponse = await client.DeleteAsync($"{ApiDeleteReservation}/{reservationId}");
                    if (!deleteResponse.IsSuccessStatusCode)
                    {
                        MessageBox.Show($"Erreur lors de la suppression de la réservation. Réponse API: {await deleteResponse.Content.ReadAsStringAsync()}",
                                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    LoadReservationsData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
