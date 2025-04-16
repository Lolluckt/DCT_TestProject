using System;

namespace CryptoTrackerApp.Infrastructure.Http
{
    public sealed class ApiException : Exception
    {
        public ApiException(string message, Exception? inner = null)
            : base(message, inner) { }
    }
}
