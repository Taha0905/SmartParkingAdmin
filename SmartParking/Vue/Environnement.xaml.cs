using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SmartParking
{
    public partial class Analyse : Page
    {
        private IMqttClient mqttClient;

        public Analyse()
        {
            InitializeComponent();
            // Le MQTT sera démarré manuellement depuis Admin
        }

        // Méthode publique pour démarrer MQTT
        public async void StartMQTT()
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
                Console.WriteLine("MQTT connecté !");

                string[] topics = {
                    "climat/humidity",
                    "climat/temperature",
                    "climat/decibels",
                    "climat/air_quality_10",
                    "climat/air_quality_25"
                };

                foreach (var topic in topics)
                {
                    await mqttClient.SubscribeAsync(topic);
                    Console.WriteLine("Abonné à " + topic);
                }
            });

            mqttClient.UseApplicationMessageReceivedHandler(e =>
            {
                string topic = e.ApplicationMessage.Topic;
                string value = Encoding.UTF8.GetString(e.ApplicationMessage.Payload).Trim();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    UpdateCapteurUI(topic, value);
                });
            }); mqttClient.UseApplicationMessageReceivedHandler(e =>
            {
                string topic = e.ApplicationMessage.Topic;
                string value = Encoding.UTF8.GetString(e.ApplicationMessage.Payload).Trim();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    UpdateCapteurUI(topic, value);
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

        //2
        public void UpdateCapteurUI(string topic, string value)
        {
            if (!double.TryParse(value.Replace('.', ','), out double val))
                return;

            switch (topic)
            {
                case "climat/temperature":
                    txtTemperature.Text = val + " °C";

                    if (val < 10)
                        txtStatusTemp.Text = "❄ Froid";
                    else if (val <= 28)
                        txtStatusTemp.Text = "✅ Température idéale";
                    else if (val <= 40)
                        txtStatusTemp.Text = "🔥 Chaud";
                    else
                        txtStatusTemp.Text = "🚨 Anormal";
                    break;

                case "climat/humidity":
                    txtHumidite.Text = val + " %";

                    if (val < 30)
                        txtStatusHumidite.Text = "💧 Trop sec";
                    else if (val <= 70)
                        txtStatusHumidite.Text = "✅ Optimal";
                    else
                        txtStatusHumidite.Text = "🌧 Humidité élevée";
                    break;

                case "climat/decibels":
                    txtSon.Text = val + " dB";

                    if (val < 50)
                        txtStatusSon.Text = "🔈 Calme";
                    else if (val <= 80)
                        txtStatusSon.Text = "✅ Modéré";
                    else
                        txtStatusSon.Text = "🔊 Bruit élevé";
                    break;

                case "climat/air_quality_10":
                    txtCO2p10.Text = "PM10 : " + val + " µg/m³";

                    if (val <= 50)
                        txtStatusCO2.Text = "✅ Qualité excellente";
                    else if (val <= 100)
                        txtStatusCO2.Text = "😐 Moyenne";
                    else if (val <= 200)
                        txtStatusCO2.Text = "⚠ Mauvaise";
                    else
                        txtStatusCO2.Text = "🚨 Très mauvaise";
                    break;

                case "climat/air_quality_25":
                    txtCO2p25.Text = "PM2.5 : " + val + " µg/m³";
                    // Statut déjà géré via PM10 ci-dessus (txtStatusCO2)
                    break;
            }
        }


    }
}
