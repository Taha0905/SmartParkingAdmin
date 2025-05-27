using SmartParking.Control;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;

namespace SmartParking.Vue
{
    public partial class Parking : Page
    {
        private readonly PlaceMqttManager mqttManager = GlobalMqtt.Instance;

        private readonly Dictionary<string, Border> placeBorders = new();
        private readonly Dictionary<string, TextBlock> placeStatusText = new();
        private readonly Dictionary<string, PackIcon> placeIcons = new();

        private readonly HashSet<string> alreadyApplied = new(); // évite redessiner

        private readonly Dictionary<string, string> topicToPlaceName = new()
        {
            { "places/place1", "Place1" },
            { "places/place2", "Place2" },
            { "places/place3", "Place3" },
            { "places/place4", "Place4" },
            { "places/place5", "Place5" },
            { "places/place6", "Place6" },
        };

        public Parking()
        {
            InitializeComponent();

            foreach (var (topic, name) in topicToPlaceName)
            {
                var border = FindName($"{name}Border") as Border;
                var status = FindName($"{name}Status") as TextBlock;
                var icon = FindName($"{name}Icon") as PackIcon;

                if (border != null && status != null && icon != null)
                {
                    placeBorders[topic] = border;
                    placeStatusText[topic] = status;
                    placeIcons[topic] = icon;
                }
            }

            mqttManager.OnMessageReceived += MqttManager_OnMessageReceived;

            // Appliquer les états déjà reçus avant affichage
            foreach (var (topic, etat) in mqttManager.PlacesEtat)
                ApplyVisual(topic, etat);

            UpdateStats();
        }

        private void MqttManager_OnMessageReceived(string topic, string payload)
        {
            Dispatcher.Invoke(() =>
            {
                ApplyVisual(topic, payload);
                UpdateStats();
            });
        }

        private void ApplyVisual(string topic, string payload)
        {
            string key = $"{topic}:{payload}";
            if (alreadyApplied.Contains(key)) return;

            alreadyApplied.RemoveWhere(k => k.StartsWith($"{topic}:")); // efface ancien état
            alreadyApplied.Add(key);

            if (placeBorders.ContainsKey(topic) && placeStatusText.ContainsKey(topic) && placeIcons.ContainsKey(topic))
            {
                var border = placeBorders[topic];
                var status = placeStatusText[topic];
                var icon = placeIcons[topic];

                if (payload == "Libre")
                {
                    border.Style = (Style)FindResource("ParkingSpaceStyle");
                    status.Text = "Disponible";
                    status.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#48BB78"));
                    icon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#48BB78"));
                }
                else if (payload == "Prise")
                {
                    border.Style = (Style)FindResource("ParkingSpaceOccupiedStyle");
                    status.Text = "Occupée";
                    status.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F56565"));
                    icon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F56565"));
                }
            }
        }

        private void UpdateStats()
        {
            int libres = 0, prises = 0;

            foreach (var topic in placeStatusText.Keys)
            {
                var color = (placeStatusText[topic].Foreground as SolidColorBrush)?.Color;

                if (color == (Color)ColorConverter.ConvertFromString("#48BB78")) // vert
                    libres++;
                else if (color == (Color)ColorConverter.ConvertFromString("#F56565")) // rouge
                    prises++;
            }

            int total = libres + prises;

            PlacesDispoText.Text = libres.ToString();
            PlacesOccupeesText.Text = prises.ToString();
            TauxOccupationText.Text = total > 0 ? $"{(prises * 100 / total)}%" : "0%";
        }
    }
}
