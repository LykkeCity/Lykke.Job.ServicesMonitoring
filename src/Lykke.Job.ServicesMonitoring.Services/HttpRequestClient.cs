using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Job.ServicesMonitoring.Services
{
    public static class HttpRequestClient
    {
        public static async Task<string> PostRequestAsync(string url, string json)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(url, content);
            return await response.Content.ReadAsStringAsync();
        }

        public static async Task<string> GetRequestAsync(string url)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var responseContent = await httpClient.GetStringAsync(url);

            return responseContent;
        }
    }
}
