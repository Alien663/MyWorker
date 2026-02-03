using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InitWorker;

public class Worker: BackgroundService
{
    private readonly IHostApplicationLifetime _host;
    private readonly IConfiguration _config;
    private readonly ILogger<Worker> _logger;

    public Worker(IHostApplicationLifetime host, IConfiguration config, ILogger<Worker> logger)
    {
        _host = host;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Initialization started.");
            while (!stoppingToken.IsCancellationRequested)
            {
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
