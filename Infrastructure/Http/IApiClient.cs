using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace CryptoTrackerApp.Infrastructure.Http
{
    public interface IApiClient
    {
        Task<JsonElement> GetAsync(string endpoint, Dictionary<string, string> parameters);
        Task<string> GetRawAsync(string endpoint, Dictionary<string, string> parameters);
    }
}