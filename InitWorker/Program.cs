using InitWorker;
using InitWorker.Interface;
using InitWorker.Service;
using InitWorker.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Net;

Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        // Configure Setup
        IConfiguration config = hostContext.Configuration;
        services.AddSingleton(config);

        // Http Client Factory
        static HttpClientHandler BuildHttpHandler(IServiceProvider sp)
        {
            var handler = new HttpClientHandler();
            var cfg = sp.GetRequiredService<IConfiguration>();
            var proxyIP = cfg["Proxy:IP"];
            if (!string.IsNullOrEmpty(proxyIP))
            {
                handler.Proxy = new WebProxy(proxyIP)
                {
                    Credentials = new NetworkCredential(cfg["Proxy:Account"], cfg["Proxy:Password"])
                };
                handler.UseProxy = true;
            }
            return handler;
        }

        services.AddHttpClient<IMyHttpClient, MyHttpClient>((sp, client) =>
        {
            var cfg = sp.GetRequiredService<IConfiguration>();
            var resourceURL = cfg["MyProject:Client:ResourceURL"];
            if (!string.IsNullOrEmpty(resourceURL))
            {
                client.BaseAddress = new Uri(resourceURL);
            }
        })
        // To Verify Proxy
        .ConfigurePrimaryHttpMessageHandler(BuildHttpHandler);

        // Entity Framework DbContext
        services.AddDbContext<SampleContext>(options =>
        {
            var connectionString = config.GetConnectionString("Default");
            options.UseSqlServer(connectionString);
        });

        // Register Services
        services.AddScoped<ISampleService, SampleService>();

        // Register Logging
        services.AddLogging();

        services.AddHostedService<Worker>();
        // Register other dependencies here
    })
    .Build()
    .Run();