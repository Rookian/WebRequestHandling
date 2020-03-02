using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using WebRequestHandling.Infrastructure.NSwag;

namespace WebRequestHandling.Infrastructure.ExecutionPipeline
{
    public static class BuilderExtensions
    {
        public static IApplicationBuilder UseOpenApiWithRequestHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<WebOpenApiDocumentMiddleware>();
        }

        public static void UseRpc(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("rpc/{type}", async context =>
            {
                var (request, requestType, responseType) = await RpcHandling.GetCommandRequest(context);
                await RpcHandling.InvokeCommandHandler(context, request, requestType, responseType);
            });

            endpoints.MapGet("rpc/{type}", async context =>
            {
                var (request, requestType, responseType) = RpcHandling.GetQueryRequest(context);
                await RpcHandling.InvokeQueryHandler(context, request, requestType, responseType);
            });
        }
    }
}