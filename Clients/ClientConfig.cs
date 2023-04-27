using System.Text.Json;
using Polly;
using Polly.Retry;
using static Core.Management.DynamicKey.Windows.Service.Dto;

namespace Core.Management.DynamicKey.Windows.Service.Clients
{
    public class ClientConfig 
    {
        private readonly HttpClient _httpClient;
            private readonly AsyncRetryPolicy _retryPolicy;

        public ClientConfig(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5224");
                     _retryPolicy =  Policy.Handle<HttpRequestException>()
                                           .WaitAndRetryAsync(
                                                3, 
                                                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), 
                                                (exception, timeSpan, retryCount, context) =>
                                                {
                                                    Console.WriteLine($"Error during request HTTP. Intent number  {retryCount} in {timeSpan.TotalSeconds} seconds. Exception: {exception.Message}");
                                                });
                                                
        }

         public async Task<IReadOnlyCollection<Dummy>> GetWheaterForecastsListAsync()
         {
             
                IReadOnlyCollection<Dummy> dummiesList = new List<Dummy>();
                try
                {
                    await _retryPolicy.ExecuteAsync(async () =>
                    {
                        var response = await _httpClient.GetAsync("/api/v1/Dummy");

                        response.EnsureSuccessStatusCode();

                        var content = await response.Content.ReadAsStringAsync();

                        dummiesList = JsonSerializer.Deserialize<IReadOnlyCollection<Dummy>>(content) ?? new List<Dummy>();

                        Console.WriteLine($"Response API: {content}");

                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error HTTP: {ex.Message}");
                }
            return dummiesList;
         }
    }
}