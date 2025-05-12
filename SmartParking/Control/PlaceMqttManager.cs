using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmartParking.Control
{
    public class PlaceMqttManager
    {
        private readonly string[] _topics = { "places/place1", "places/place2", "places/place3", "places/place4", "places/place5", "places/place6" };
        public Dictionary<string, string> PlacesEtat { get; private set; } = new();

        private IMqttClient mqttClient;

        public event Action<string, string> OnMessageReceived;

        public async Task ConnectAsync()
        {
            var factory = new MqttFactory();
            mqttClient = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithClientId("SmartParking_Client")
                .WithTcpServer("172.31.254.254", 1883)
                .WithCleanSession(false)
                .Build();

            mqttClient.ApplicationMessageReceivedHandler = new MQTTnet.Client.Receiving.MqttApplicationMessageReceivedHandlerDelegate(e =>
            {
                string topic = e.ApplicationMessage.Topic;
                string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

                PlacesEtat[topic] = payload;
                OnMessageReceived?.Invoke(topic, payload);
            });

            await mqttClient.ConnectAsync(options, CancellationToken.None);

            foreach (var topic in _topics)
            {
                PlacesEtat[topic] = "Inconnu";
                await mqttClient.SubscribeAsync(topic);
            }
        }
    }
}
