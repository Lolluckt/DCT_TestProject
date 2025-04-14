using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CryptoTrackerApp.Services;
using CryptoTrackerApp.ViewModels;
using CryptoTrackerApp.Views;

namespace CryptoTrackerApp.Infrastructure
{
    public static class DIContainer
    {
        public static ServiceProvider ConfigureServices(IConfiguration configuration)
        {
            var services = new ServiceCollection();
            services.AddSingleton(configuration);
            services.AddSingleton<ICoinGeckoService, CoinGeckoService>();
            services.AddSingleton<NavigationService>();
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
