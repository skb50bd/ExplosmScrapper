using System.IO;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ExplosmScrapper
{
    internal static class Program
    {
        static void Main()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.ConfigureServices();

            // create service provider
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // run app
            var app = serviceProvider.GetService<App>();
            app.Run().Wait();           
        }

        static IServiceCollection ConfigureServices(this IServiceCollection services) {
            services.AddScoped<HttpClient>();
            services.AddScoped<WebClient>();
            services.AddScoped<Explosm>();
            services.AddScoped<DownloadHelper>();
            services.AddSingleton<App>();

            // build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .Build();

            services.AddOptions();
            services.Configure<ExplosmOptions>(configuration.GetSection("Explosm"));
            
            return services;
        }
    }
}
