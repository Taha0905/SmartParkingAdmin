using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Newtonsoft.Json;

namespace SmartParking
{
    public partial class Admin : Page
    {
        private const string ApiUrl = "https://smartparking.alwaysdata.net/getAllPlaces";
        private DispatcherTimer timer;

        public Admin()
        {
            InitializeComponent();

            // Charger les données immédiatement au démarrage
            ChargerPlaces();

            // Initialiser le timer pour les mises à jour régulières
            InitializeTimer();
        }

        // Initialisation du timer
        private void InitializeTimer()
        {
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5) // Mise à jour toutes les 5 secondes
            };
            timer.Tick += Timer_Tick; // Événement déclenché à chaque tick
            timer.Start();
        }

        // Événement appelé par le timer
        private void Timer_Tick(object sender, EventArgs e)
        {
            ChargerPlaces(); // Mettre à jour les places
        }

        // Charger les places depuis l'API
        private async void ChargerPlaces()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(ApiUrl);
                    response.EnsureSuccessStatusCode(); // Vérifier si l'API répond
                    string jsonResponse = await response.Content.ReadAsStringAsync();

                    List<Place> places = JsonConvert.DeserializeObject<List<Place>>(jsonResponse);

                    // Compter le nombre de places selon leur statut
                    int libres = places.FindAll(p => p.StatutPlace == "Libre").Count;
                    int reserves = places.FindAll(p => p.StatutPlace == "Reserver").Count;
                    int occupees = places.Count - (libres + reserves); // Si autre statut

                    // Mettre à jour l'interface utilisateur
                    TextLibre.Text = $"Places libres : {libres}";
                    TextOccupe.Text = $"Places occupées : {occupees}";
                    TextReserve.Text = $"Places réservées : {reserves}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des places : {ex.Message}");
            }
        }
    }

    // Classe pour représenter une place
    public class Place
    {
        public int IDPlace { get; set; }
        public int NumeroPlace { get; set; }
        public string StatutPlace { get; set; }
    }
}
