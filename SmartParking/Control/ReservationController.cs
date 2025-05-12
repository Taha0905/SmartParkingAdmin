using Newtonsoft.Json;
using SmartParking.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SmartParking.Control
{
    public class ReservationController
    {
        private const string Token = "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpYXQiOjE3NDIzMDg4NTEsImV4cCI6MTAxNzQyMzA4ODUxLCJkYXRhIjp7ImlkIjoxLCJ1c2VybmFtZSI6IlNtYXJ0UGFya2luZyJ9fQ.B-dPPnoL4DnwsZ6_j6GRxs74Zn5XLQw-K8OjWIbegjk";
        private const string ApiReservations = "https://smartparking.alwaysdata.net/getAllReservations";
        private const string ApiDeleteReservation = "https://smartparking.alwaysdata.net/deleteReservation";

        public async Task<List<Reservation>> GetReservationsAsync()
        {
            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", Token);
            HttpResponseMessage response = await client.GetAsync(ApiReservations);

            if (!response.IsSuccessStatusCode)
                throw new Exception("Erreur API : Impossible de récupérer les réservations.");

            string jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Reservation>>(jsonResponse);
        }

        public async Task DeleteReservationAsync(int reservationId)
        {
            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", Token);

            HttpResponseMessage deleteResponse = await client.DeleteAsync($"{ApiDeleteReservation}/{reservationId}");

            if (!deleteResponse.IsSuccessStatusCode)
                throw new Exception($"Erreur API : {await deleteResponse.Content.ReadAsStringAsync()}");
        }
    }
}
