
using CryptoTrackerApp.Infrastructure;
using CryptoTrackerApp.Infrastructure.Http;
using CryptoTrackerApp.Services;
using CryptoTrackerApp.Services.Mappers;
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

            services.AddSingleton<IApiClient, CoinGeckoApiClient>();
            services.AddSingleton<ICurrencyMapper, CoinGeckoCurrencyMapper>();
            services.AddSingleton<IChartMapper, OxyPlotChartMapper>();
            services.AddTransient<ICoinGeckoService, CoinGeckoService>();
            services.AddTransient<ConverterViewModel>();

            services.AddSingleton<ShellViewModel>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<CurrencyDetailsViewModel>();
            services.AddSingleton<ShellView>();
            services.AddTransient<MainView>();
            services.AddTransient<CurrencyDetailsView>();
            services.AddTransient<ConverterView>();

            return services.BuildServiceProvider();
        }
    }
}
