using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Newtonsoft.Json;
using System.Windows.Input;
using System.Collections.ObjectModel;

namespace SmartParking
{
    public partial class VueDensemble : Page
    {
        private readonly string apiPlaces = "https://smartparking.alwaysdata.net/getAllPlaces";
        private readonly string apiReservations = "https://smartparking.alwaysdata.net/getAllReservations";
        private readonly string apiDeleteReservation = "https://smartparking.alwaysdata.net/deleteReservation";
        private readonly string apiUpdatePlace = "https://smartparking.alwaysdata.net/updatePlace";
        private readonly DispatcherTimer refreshTimer;

        public ObservableCollection<Reservation> Reservations { get; set; }

        public VueDensemble()
        {
            InitializeComponent();
            Reservations = new ObservableCollection<Reservation>();
            ListViewReservations.ItemsSource = Reservations;

            LoadParkingData();
            LoadReservationsData();

            // Rafraîchir toutes les 5 secondes
            refreshTimer = new DispatcherTimer();
            refreshTimer.Interval = TimeSpan.FromSeconds(5);
            refreshTimer.Tick += (s, e) => { LoadParkingData(); LoadReservationsData(); };
            refreshTimer.Start();
        }

        private async void LoadParkingData()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(apiPlaces);
                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        List<Place> places = JsonConvert.DeserializeObject<List<Place>>(jsonResponse);

                        int placesDisponibles = 0;
                        int placesOccupees = 0;
                        int placesReservees = 0;
                        int totalPlaces = places.Count;

                        foreach (var place in places)
                        {
                            switch (place.StatutPlace.ToLower())
                            {
                                case "libre":
                                    placesDisponibles++;
                                    break;
                                case "occuper":
                                    placesOccupees++;
                                    break;
                                case "reserver":
                                    placesReservees++;
                                    break;
                            }
                        }

                        TextLibre.Text = placesDisponibles.ToString();
                        TextOccupe.Text = placesOccupees.ToString();
                        TextReserve.Text = placesReservees.ToString();
                        
                    }
                    else
                    {
                        MessageBox.Show("Erreur lors de la récupération des places.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                            res.DeleteCommand = new RelayCommand(async () => await DeleteReservation(res.IDReservation, res.FKIDPlace));
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

        private async Task DeleteReservation(int reservationId, int placeId)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Suppression de la réservation
                    HttpResponseMessage deleteResponse = await client.DeleteAsync($"{apiDeleteReservation}/{reservationId}");
                    if (!deleteResponse.IsSuccessStatusCode)
                    {
                        MessageBox.Show($"Erreur lors de la suppression de la réservation. Réponse API: {await deleteResponse.Content.ReadAsStringAsync()}",
                                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Charger toutes les places et chercher la place concernée
                    HttpResponseMessage placesResponse = await client.GetAsync(apiPlaces);
                    if (!placesResponse.IsSuccessStatusCode)
                    {
                        MessageBox.Show($"Erreur lors de la récupération des places. Réponse API: {await placesResponse.Content.ReadAsStringAsync()}",
                                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    string placesJson = await placesResponse.Content.ReadAsStringAsync();
                    List<Place> places = JsonConvert.DeserializeObject<List<Place>>(placesJson);

                    // Trouver la place correspondante
                    Place place = places.Find(p => p.IDPlace == placeId);
                    if (place == null)
                    {
                        MessageBox.Show("Impossible de trouver la place associée à la réservation.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Mise à jour de la place en libre
                    var updateData = new StringContent(JsonConvert.SerializeObject(new
                    {
                        NumeroPlace = place.NumeroPlace,
                        StatutPlace = "libre"
                    }), Encoding.UTF8, "application/json");

                    HttpResponseMessage updateResponse = await client.PutAsync($"{apiUpdatePlace}/{placeId}", updateData);

                    if (!updateResponse.IsSuccessStatusCode)
                    {
                        MessageBox.Show($"Erreur lors de la mise à jour de la place. Réponse API: {await updateResponse.Content.ReadAsStringAsync()}",
                                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Mise à jour des données après la suppression
                    LoadParkingData();
                    LoadReservationsData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class Place
    {
        public int IDPlace { get; set; }
        public int NumeroPlace { get; set; }
        public string StatutPlace { get; set; }
    }

    public class Reservation
    {
        public int IDReservation { get; set; }
        public string DateReservation { get; set; }
        public string TempsReservation { get; set; }
        public string Immatriculation { get; set; }
        public int FKIDPlace { get; set; }

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
