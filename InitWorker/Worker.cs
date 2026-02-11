using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SampleService1.Interfaces;
using SampleService2.Interfaces;

namespace InitWorker;

public class Worker: BackgroundService
{
    private readonly IHostApplicationLifetime _host;
    private readonly IConfiguration _config;
    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public Worker(IHostApplicationLifetime host, IConfiguration config, ILogger<Worker> logger, IServiceScopeFactory scopeFactory)
    {
        _host = host;
        _config = config;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Initialization started.");
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    // Perform initialization tasks here
                    // For example, you can resolve services from the scope and call their methods
                    var myService = scope.ServiceProvider.GetRequiredService<ISampleService>();
                    var myHttpClient = scope.ServiceProvider.GetRequiredService<IMyHttpClient>();

                    // Call methods on the services to perform initialization tasks

                }
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
            _logger.LogInformation("Initialization completed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during initialization.");
        }
        finally
        {
            // Stop the application once initialization is complete
            _host.StopApplication();
        }
    }
}
