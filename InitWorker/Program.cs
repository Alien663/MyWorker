using InitWorker;
using InitWorker.Option;
using SampleService1.Entities;
using SampleService1.Interfaces;
using SampleService1.Services;
using SampleService2.Interfaces;
using SampleService2.Services;
using SampleService2.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using System.Net;

Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostContext, config) =>
    {
        config.SetBasePath(Directory.GetCurrentDirectory());
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        if(hostContext.HostingEnvironment.IsDevelopment())
        {
            config.AddJsonFile($"appsettings.Development.json", optional: true, reloadOnChange: true);
            config.AddUserSecrets<Program>(optional: true);
        }
    })
    .ConfigureServices((hostContext, services) =>
    {
        // Configure Setup
        IConfiguration config = hostContext.Configuration;
        services.AddSingleton(config);
        services.Configure<ProxyOptions>(config.GetSection("Proxy"));
        services.Configure<SampleOption>(config.GetSection("SampleOption"));

        // Http Client Factory
        static HttpClientHandler BuildHttpHandler(IServiceProvider sp)
        {
            var handler = new HttpClientHandler();
            var proxyOptions = sp.GetRequiredService<IOptions<ProxyOptions>>().Value;
            var proxyIP = proxyOptions?.IP;
            if (!string.IsNullOrEmpty(proxyIP))
            {
                handler.Proxy = new WebProxy(proxyIP)
                {
                    Credentials = new NetworkCredential(proxyOptions?.Account, proxyOptions?.Password)
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