using CryptoTrackerApp.Infrastructure;
using CryptoTrackerApp.Services;
using CryptoTrackerApp.ViewModels;
using CryptoTrackerApp.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace CryptoTrackerApp.Infrastructure
{
    public static class DIContainer
    {
        public static ServiceProvider ConfigureServices(IConfiguration cfg)
        {
            var services = new ServiceCollection();

            services.AddSingleton(cfg);
            services.AddSingleton<NavigationService>();
            services.AddSingleton<HttpClient>(_ =>
            {
                var c = new HttpClient();
                c.DefaultRequestHeaders.UserAgent.ParseAdd("CryptoTrackerApp/1.1");
                return c;
            });

            services.AddTransient<ICoinGeckoService, CoinGeckoService>();
            services.AddSingleton<ShellViewModel>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<CurrencyDetailsViewModel>();
            services.AddSingleton<ShellView>();
            services.AddTransient<MainView>();
            services.AddTransient<CurrencyDetailsView>();

            return services.BuildServiceProvider();
        }
    }
}
