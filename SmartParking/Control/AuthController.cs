using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SmartParking.Model;

namespace SmartParking.Control
{
    public class AuthController
    {
        private const string Token = "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpYXQiOjE3NDIzMDg4NTEsImV4cCI6MTAxNzQyMzA4ODUxLCJkYXRhIjp7ImlkIjoxLCJ1c2VybmFtZSI6IlNtYXJ0UGFya2luZyJ9fQ.B-dPPnoL4DnwsZ6_j6GRxs74Zn5XLQw-K8OjWIbegjk";
        private const string ApiUrl = "http://smartparking.alwaysdata.net/ConnexionAdmin";

        public async Task<bool> AuthenticateUser(AdminCredentials credentials)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", Token);

                string json = JsonConvert.SerializeObject(credentials);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(ApiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    dynamic result = JsonConvert.DeserializeObject(responseContent);

                    return result.message == "Authentification réussie.";
                }
            }

            return false;
        }
    }
}
