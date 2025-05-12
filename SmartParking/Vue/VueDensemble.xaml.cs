using SmartParking.Control;
using SmartParking.Model;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace SmartParking
{
    public partial class VueDensemble : Page
    {

        private ObservableCollection<Reservation> AllReservations = new ObservableCollection<Reservation>();

        private readonly ReservationController reservationController = new ReservationController();
        private readonly PlaceMqttManager mqttManager = new PlaceMqttManager();

        private readonly DispatcherTimer refreshTimer;

        public ObservableCollection<Reservation> Reservations { get; set; }

        public VueDensemble()
        {
            InitializeComponent();

            Reservations = new ObservableCollection<Reservation>();
            ListViewReservations.ItemsSource = Reservations;

            LoadReservations();

            // Connexion MQTT + mise à jour UI
            mqttManager.OnMessageReceived += (topic, message) =>
            {
                if (Application.Current == null || Application.Current.Dispatcher == null)
                    return;

                Application.Current.Dispatcher.Invoke(() => UpdatePlacesState());
            };
            _ = mqttManager.ConnectAsync();

            // Timer de rafraîchissement automatique
            refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            refreshTimer.Tick += (s, e) => LoadReservations();
            refreshTimer.Start();
        }

        private async void LoadReservations()
        {
            try
            {
                var reservationsList = await reservationController.GetReservationsAsync();
                Reservations.Clear();
                AllReservations.Clear();

                foreach (var res in reservationsList)
                {
                    if (!string.IsNullOrEmpty(res.DateReservation))
                        res.DateReservationFormatted = DateTime.Parse(res.DateReservation).ToString("dd/MM/yyyy HH:mm");

                    if (!string.IsNullOrEmpty(res.TempsReservation))
                    {
                        TimeSpan duration = TimeSpan.Parse(res.TempsReservation);
                        res.TempsReservationFormatted = $"{duration.Hours}h {duration.Minutes}m";
                    }

                    res.DeleteCommand = new RelayCommand(async () =>
                    {
                        await DeleteReservationAsync(res.IDReservation);
                    });

                    Reservations.Add(res);
                    AllReservations.Add(res);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteReservationAsync(int reservationId)
        {
            try
            {
                //Ajout de la confirmation
                var result = MessageBox.Show("Voulez-vous vraiment supprimer cette réservation ?",
                                             "Confirmation",
                                             MessageBoxButton.YesNo,
                                             MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return; // Si l'utilisateur annule, on sort sans supprimer

                await reservationController.DeleteReservationAsync(reservationId);
                LoadReservations();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdatePlacesState()
        {
            int libres = 0, occupees = 0;
            foreach (var etat in mqttManager.PlacesEtat.Values)
            {
                if (etat.Equals("Libre", StringComparison.OrdinalIgnoreCase)) libres++;
                else if (etat.Equals("Prise", StringComparison.OrdinalIgnoreCase)) occupees++;
            }

            if (TextLibre != null)
                TextLibre.Text = libres.ToString();

            if (TextOccupe != null)
                TextOccupe.Text = occupees.ToString();
        }

        private void BtnFiltrer_Click(object sender, RoutedEventArgs e)
        {
            if (DateFiltre.SelectedDate.HasValue)
            {
                refreshTimer.Stop(); // stop le refresh auto

                DateTime dateSelectionnee = DateFiltre.SelectedDate.Value.Date;

                var reservationsFiltrees = AllReservations.Where(r =>
                {
                    if (DateTime.TryParse(r.DateReservation, out DateTime dateResa))
                        return dateResa.Date == dateSelectionnee;
                    return false;
                }).ToList();

                Reservations.Clear();
                foreach (var res in reservationsFiltrees)
                    Reservations.Add(res);
            }
        }


        private void BtnReinitialiser_Click(object sender, RoutedEventArgs e)
        {
            Reservations.Clear();
            foreach (var res in AllReservations)
                Reservations.Add(res);

            refreshTimer.Start(); // relance le refresh auto !
        }
    }
}
