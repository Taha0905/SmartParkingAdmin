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
        private readonly string[] _topics = {
            "places/place1", "places/place2", "places/place3",
            "places/place4", "places/place5", "places/place6"
        };

        public Dictionary<string, string> PlacesEtat { get; private set; } = new();
        private readonly Dictionary<string, string> bufferEtat = new();
        private readonly Dictionary<string, DateTime> timestampEtat = new();
        private readonly object lockObj = new();

        private IMqttClient mqttClient;
        public bool IsConnected => mqttClient?.IsConnected == true;

        public event Action<string, string> OnMessageReceived;

        public async Task ConnectAsync()
        {
            if (mqttClient != null && mqttClient.IsConnected)
                return;

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

                lock (lockObj)
                {
                    if (!bufferEtat.ContainsKey(topic) || bufferEtat[topic] != payload)
                    {
                        bufferEtat[topic] = payload;
                        timestampEtat[topic] = DateTime.Now;
                        Task.Run(() => VerifierEtatStable(topic, payload));
                    }
                }
            });

            await mqttClient.ConnectAsync(options, CancellationToken.None);

            foreach (var topic in _topics)
            {
                PlacesEtat[topic] = "Inconnu";
                bufferEtat[topic] = "Inconnu";
                timestampEtat[topic] = DateTime.Now;
                await mqttClient.SubscribeAsync(topic);
            }
        }

        private async Task VerifierEtatStable(string topic, string payloadInitial)
        {
            await Task.Delay(3000);

            lock (lockObj)
            {
                var maintenant = DateTime.Now;
                if (bufferEtat.TryGetValue(topic, out string current) &&
                    current == payloadInitial &&
                    (maintenant - timestampEtat[topic]).TotalSeconds >= 2)
                {
                    if (!PlacesEtat.ContainsKey(topic) || PlacesEtat[topic] != payloadInitial)
                    {
                        PlacesEtat[topic] = payloadInitial;
                        OnMessageReceived?.Invoke(topic, payloadInitial);
                    }
                }
            }
        }
    }
}
