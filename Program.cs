using Core.Management.DynamicKey.Windows.Service;
using Core.Management.DynamicKey.Windows.Service.Clients;
using Polly;
using Polly.Timeout;

IHost host = Host.CreateDefaultBuilder(args)
                .UseWindowsService(options=> 
                {
                    options.ServiceName = "Core-Management-DynamicKey Service";
                })
                .ConfigureServices((hostContext, services) =>
                {                      
                         var jitter = new Random();
                         
                            services.AddHttpClient<ClientConfig>("clientConfig")
                                   .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().WaitAndRetryAsync(
                                       5,
                                       retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(jitter.Next(0,1000)),
                                       onRetry: (outcome,timespan, retryAttempt ) =>
                                       {
                                          Console.WriteLine($"Delaying for {timespan.TotalSeconds} seconds. Making retry attempt {retryAttempt}");
                                       }
                                    ))
                                    .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().CircuitBreakerAsync(
                                        3,
                                        TimeSpan.FromSeconds(15),
                                        onBreak: (outcome,timespan)=>
                                        {
                                            Console.WriteLine($"Opnening the circuit for {timespan.TotalSeconds} seconds...");
                                        },
                                        onReset: ()=>
                                        {
                                            Console.WriteLine($"Closing the circuit for seconds.");
                                        }
                                    ))
                                    .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(1)));
                                    
                            services.AddHostedService<Worker>();
                }).Build();
host.Run();