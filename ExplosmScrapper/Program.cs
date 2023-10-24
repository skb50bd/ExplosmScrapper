using ExplosmScrapper;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

ServiceCollection serviceCollection = new();
serviceCollection.AddTransient<HttpClient>();
serviceCollection.AddTransient<WebClient>();
serviceCollection.AddTransient<Explosm>();
serviceCollection.AddTransient<DownloadHelper>();
serviceCollection.AddSingleton<App>();

var environmentName =
    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
    ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
    ?? "Production";

var configuration =
    new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile(
            path: $"appsettings.{environmentName}.json",
            optional: true,
            reloadOnChange: true
        )
        .AddEnvironmentVariables()
        .AddCommandLine(args)
        .Build();

serviceCollection.AddOptions();
serviceCollection.Configure<ExplosmOptions>(configuration.GetSection("Explosm"));

var serviceProvider =
    serviceCollection.BuildServiceProvider();

var app =
    serviceProvider.GetService<App>()
    ?? throw new NullReferenceException("App is null");

await app.Run();
