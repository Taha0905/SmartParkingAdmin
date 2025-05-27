using SmartParking.Control;
using System.Configuration;
using System.Data;
using System.Windows;

namespace SmartParking
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            await GlobalMqtt.InitAsync(); // Connexion au broker MQTT au lancement
        }
    }

}
