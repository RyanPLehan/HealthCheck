using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthCheck.Tests.HttpListener
{
    internal static class UrlBuilder
    {
        public static string Host { get => "localhost"; }

        public static Uri BuildHttp(string endpoint, int port = 80)
        {
            string baseUrl = $"http://{Host}:{port}";
            Uri uri = new Uri(baseUrl, UriKind.Absolute);
            return new Uri(uri, endpoint);
        }

        public static Uri BuildHttps(string endpoint, int port = 443)
        {
            string baseUrl = $"https://{Host}:{port}";
            Uri uri = new Uri(baseUrl, UriKind.Absolute);
            return new Uri(uri, endpoint);
        }
    }
}
