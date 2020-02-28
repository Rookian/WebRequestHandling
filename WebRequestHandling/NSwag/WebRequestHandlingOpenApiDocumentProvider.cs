using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NSwag;
using NSwag.AspNetCore;
using NSwag.AspNetCore.Middlewares;

namespace WebRequestHandling.NSwag
{
    public class WebOpenApiDocumentMiddleware : OpenApiDocumentMiddleware
    {
        private readonly WebRequestHandlingSwaggerGenerator _generator;
        private readonly OpenApiDocumentMiddlewareSettings _settings;

        public WebOpenApiDocumentMiddleware(RequestDelegate nextDelegate, IServiceProvider serviceProvider, string documentName, string path, OpenApiDocumentMiddlewareSettings settings) : base(nextDelegate, serviceProvider, documentName, path, settings)
        {
            _settings = settings;
            _generator = new WebRequestHandlingSwaggerGenerator(new WebRequestHandlingSwaggerGeneratorSettings());
        }

        protected override async Task<OpenApiDocument> GenerateDocumentAsync(HttpContext context)
        {
            var document = await _generator.GenerateDocument();

            document.Servers.Clear();
            document.Servers.Add(new OpenApiServer
            {
                Url = context.Request.GetServerUrl()
            });

            _settings.PostProcess?.Invoke(document, context.Request);

            return document;
        }
    }
}