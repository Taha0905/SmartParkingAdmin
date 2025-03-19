using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SmartParking.Source.Modele
{
    public class Reservation
    {
        public int IDReservation { get; set; }
        public string DateReservation { get; set; }
        public string TempsReservation { get; set; }
        public string Immatriculation { get; set; }

        public string DateReservationFormatted { get; set; }
        public string TempsReservationFormatted { get; set; }

        public ICommand DeleteCommand { get; set; }
    }
}
