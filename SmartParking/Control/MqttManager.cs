using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System;
using System.Text;
using System.Windows;

namespace SmartParking
{
    public class MqttManager
    {
        private IMqttClient mqttClient;
        public static MqttManager Instance { get; } = new MqttManager();

        private MqttManager() { InitMQTT(); }

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

                // Vérification de seuils critiques
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (topic == "climat/temperature" && double.TryParse(value, out double temp) && temp > 40)
                    {
                        ShowEmergencyPopup("Température critique : " + temp + " °C");
                    }
                    else if (topic == "climat/air_quality_10" && double.TryParse(value, out double pm10) && pm10 > 200)
                    {
                        ShowEmergencyPopup("PM10 élevé : " + pm10 + " µg/m³");
                    }
                    // Ajoute d'autres conditions ici si besoin
                });
            });

            try
            {
                await mqttClient.ConnectAsync(options);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur de connexion MQTT : " + ex.Message);
            }
        }

        private void ShowEmergencyPopup(string message)
        {
            MessageBox.Show(message, "⚠ Alerte Critique", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
