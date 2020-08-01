using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ExplosmScrapper
{
    internal static class Program
    {
        static async Task Main()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.ConfigureServices();

            // create service provider
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // run app
            var app = serviceProvider.GetService<App>();
            await app.Run();
        }

        static IServiceCollection ConfigureServices(this IServiceCollection services)
        {
            services.AddTransient<HttpClient>();
            services.AddTransient<WebClient>();
            services.AddTransient<Explosm>();
            services.AddTransient<DownloadHelper>();
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
