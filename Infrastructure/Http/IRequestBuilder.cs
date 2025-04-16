using System.Collections.Generic;

namespace CryptoTrackerApp.Infrastructure.Http
{
    public interface IRequestBuilder
    {
        string BuildUrl(string endpoint, Dictionary<string, string>? parameters = null);
    }
}
