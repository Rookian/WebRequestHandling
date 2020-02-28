using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace WebRequestHandling.NSwag
{
    public class WebOpenApiDocumentMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly WebRequestHandlingSwaggerGenerator _generator;

        public WebOpenApiDocumentMiddleware(RequestDelegate next)
        {
            _next = next;
            _generator = new WebRequestHandlingSwaggerGenerator(new WebRequestHandlingSwaggerGeneratorSettings());
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = "/swagger/v1/swagger.json";

            if (context.Request.Path.HasValue && string.Equals(context.Request.Path.Value.Trim('/'), path.Trim('/'),
                StringComparison.OrdinalIgnoreCase))
            {
                var document = await _generator.GenerateDocument();
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