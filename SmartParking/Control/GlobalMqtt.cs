using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Control
{
    public static class GlobalMqtt
    {
        public static PlaceMqttManager Instance { get; } = new PlaceMqttManager();

        public static async Task InitAsync()
        {
            if (!Instance.IsConnected)
                await Instance.ConnectAsync();
        }
    }
}
