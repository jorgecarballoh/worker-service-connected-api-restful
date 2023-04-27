using Core.Management.DynamicKey.Windows.Service.Clients;
using Polly;

namespace Core.Management.DynamicKey.Windows.Service;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ClientConfig _clientConfig;
    public Worker(ILogger<Worker> logger, IHttpClientFactory  httpClientFactory)
    {
        _logger = logger;
        _clientConfig = new ClientConfig(httpClientFactory);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Console.WriteLine("Worker running");

                var listWeather =  await _clientConfig.GetWheaterForecastsListAsync();

                if(listWeather is not null)
                {
                   Console.WriteLine("API Response successfully");
                }
                else
                {
                    Console.WriteLine($"API Error");
                } 

           Console.WriteLine("Adding Delay of API  30 seconds to do the next request"); 
           await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        } 
    }

   


    
}
