using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace WebRequestHandling.Infrastructure.NSwag
{
    public class WebOpenApiDocumentMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly WebRequestHandlingSwaggerGenerator _generator;

        public WebOpenApiDocumentMiddleware(RequestDelegate next)
        {
            _next = next;
            _generator = new WebRequestHandlingSwaggerGenerator(new WebRequestHandlingSwaggerGeneratorSettings
            {
                Assembly = typeof(Startup).Assembly // TODO
            });
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = "/swagger/v1/swagger.json";

            if (context.Request.Path.HasValue && string.Equals(context.Request.Path.Value.Trim('/'), path.Trim('/'),
                StringComparison.OrdinalIgnoreCase))
            {
                var document = await _generator.GenerateDocument(_generator.Settings.Assembly);
                context.Response.StatusCode = 200;
                context.Response.Headers["Content-Type"] = "application/json; charset=utf-8";
                await context.Response.WriteAsync(document.ToJson());
            }
            else
            {
                await _next(context);
            }
        }
    }
}