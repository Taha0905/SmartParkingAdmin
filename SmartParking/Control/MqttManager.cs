using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace SmartParking
{
    public class MqttManager
    {
        private IMqttClient mqttClient;
        public static MqttManager Instance { get; } = new MqttManager();
        public bool AutoriserPopups { get; set; } = false;

        // Anti-spam des alertes
        private Dictionary<string, DateTime> dernierAlerte = new Dictionary<string, DateTime>();
        private readonly TimeSpan delaiAlerte = TimeSpan.FromSeconds(20);

        private MqttManager()
        {
            InitMQTT();
        }

        public async void InitMQTT()
        {
            var factory = new MqttFactory();
            mqttClient = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer("172.31.254.254", 1883)
                .WithClientId(Guid.NewGuid().ToString())
                .WithCleanSession()
                .Build();

            mqttClient.UseConnectedHandler(async e =>
            {
                string[] topics = {
                    "climat/humidity",
                    "climat/temperature",
                    "climat/decibels",
                    "climat/air_quality_10",
                    "climat/air_quality_25"
                };

                foreach (var topic in topics)
                    await mqttClient.SubscribeAsync(topic);
            });

            mqttClient.UseApplicationMessageReceivedHandler(e =>
            {
                string topic = e.ApplicationMessage.Topic;
                string value = Encoding.UTF8.GetString(e.ApplicationMessage.Payload).Trim();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (double.TryParse(value.Replace('.', ','), out double numericValue))
                    {
                        bool PeutAfficher(string capteur)
                        {
                            if (!dernierAlerte.ContainsKey(capteur)) return true;
                            return (DateTime.Now - dernierAlerte[capteur]) > delaiAlerte;
                        }

                        void AfficherEtMarquer(string capteur, string message)
                        {
                            if (AutoriserPopups && PeutAfficher(capteur))
                            {
                                MessageBox.Show(message, "Alerte capteur", MessageBoxButton.OK, MessageBoxImage.Warning);
                                dernierAlerte[capteur] = DateTime.Now;
                            }
                        }

                        switch (topic)
                        {
                            case "climat/temperature":
                                if (numericValue > 40)
                                    AfficherEtMarquer("temperature", "⚠ Température critique : " + numericValue + " °C");
                                break;

                            case "climat/humidity":
                                if (numericValue > 70)
                                    AfficherEtMarquer("humidity", "⚠ Humidité excessive : " + numericValue + " %");
                                break;

                            case "climat/decibels":
                                if (numericValue > 100)
                                    AfficherEtMarquer("decibels", "⚠ Bruit élevé : " + numericValue + " dB");
                                break;

                            case "climat/air_quality_10":
                                if (numericValue > 200)
                                    AfficherEtMarquer("pm10", "⚠ Particules PM10 élevées : " + numericValue + " µg/m³");
                                break;

                            case "climat/air_quality_25":
                                if (numericValue > 100)
                                    AfficherEtMarquer("pm25", "⚠ Particules PM2.5 critiques : " + numericValue + " µg/m³");
                                break;
                        }
                    }
                });
            });

            try
            {
                await mqttClient.ConnectAsync(options);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur de connexion MQTT : " + ex.Message, "Erreur MQTT", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
