using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace WebRequestHandling.NSwag
{
    public static class HttpRequestExtension
    {

        private static string GetHttpScheme(this HttpRequest request)
        {
            return request.Headers.TryGetFirstHeader("X-Forwarded-Proto") ?? request.Scheme;
        }

        public static string GetServerUrl(this HttpRequest request)
        {
            var baseUrl = request.Headers.ContainsKey("X-Forwarded-Host") ?
                new Uri($"{request.GetHttpScheme()}://{request.Headers.TryGetFirstHeader("X-Forwarded-Host")}").ToString().TrimEnd('/') :
                new Uri($"{request.GetHttpScheme()}://{request.Host}").ToString().TrimEnd('/');

            return $"{baseUrl}{request.GetBasePath()}".TrimEnd('/');
        }

        public static string GetBasePath(this HttpRequest request)
        {
            if (request.Headers.ContainsKey("X-Forwarded-Prefix"))
            {
                return "/" + request.Headers.TryGetFirstHeader("X-Forwarded-Prefix").Trim('/');
            }

            var basePath = request.Headers.ContainsKey("X-Forwarded-Host") ?
                new Uri($"http://{request.Headers.TryGetFirstHeader("X-Forwarded-Host")}").AbsolutePath :
                "";

            if (request.PathBase.HasValue)
            {
                basePath = basePath.TrimEnd('/') + "/" + request.PathBase.Value;
            }

            return ("/" + basePath.Trim('/')).TrimEnd('/');
        }

        private static string TryGetFirstHeader(this IHeaderDictionary headers, string name)
        {
            return headers[name].FirstOrDefault()?.Split(',').Select(s => s.Trim()).First();
        }
    }
}