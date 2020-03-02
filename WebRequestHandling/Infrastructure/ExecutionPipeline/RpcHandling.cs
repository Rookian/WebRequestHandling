using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace WebRequestHandling.Infrastructure.ExecutionPipeline
{
    public static class RpcHandling
    {
        public static async Task InvokeQueryHandler(HttpContext context, object request, Type requestType, Type responseType)
        {
            var handlerType = typeof(IQueryRequestHandler<,>).MakeGenericType(requestType, responseType);

            var handler = (dynamic)context.RequestServices.GetRequiredService(handlerType);
            var response = await handler.Handle((dynamic)request);

            await JsonSerializer.SerializeAsync(context.Response.Body, response);
        }

        public static async Task InvokeCommandHandler(HttpContext context, object request, Type requestType, Type responseType)
        {
            var handlerType = typeof(ICommandRequestHandler<,>).MakeGenericType(requestType, responseType);

            var handler = (dynamic)context.RequestServices.GetRequiredService(handlerType);
            var response = await handler.Handle((dynamic)request);

            await JsonSerializer.SerializeAsync(context.Response.Body, response);
        }

        public static async Task<(object request, Type requestType, Type responseType)> GetCommandRequest(HttpContext context)
        {
            if (!context.Request.RouteValues.TryGetValue("type", out var type))
                throw new Exception($"No type provided.");

            var requestType = Type.GetType(type.ToString());

            if (requestType == null)
                throw new Exception($"Could not find request type '{type}'.");

            var request = await JsonSerializer.DeserializeAsync(context.Request.Body, requestType, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var responseType = requestType.GetInterface(typeof(ICommandRequest<>).FullName).GenericTypeArguments.First();

            return (request, requestType, responseType);
        }

        public static (object request, Type requestType, Type responseType) GetQueryRequest(HttpContext context)
        {
            if (!context.Request.RouteValues.TryGetValue("type", out var type))
                throw new Exception($"No type provided.");

            var requestType = Type.GetType(type.ToString());

            if (requestType == null)
                throw new Exception($"Could not find request type '{type}'.");


            var request = Activator.CreateInstance(requestType);

            foreach (var param in context.Request.Query)
            {
                requestType.GetProperty(param.Key)?.SetValue(request, param.Value[0]);
            }

            var responseType = requestType.GetInterface(typeof(IQueryRequest<>).FullName).GenericTypeArguments.First();

            return (request, requestType, responseType);
        }
    }
}