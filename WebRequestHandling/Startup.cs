using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSwag.AspNetCore;
using WebRequestHandling.NSwag;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace WebRequestHandling
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IQueryRequestHandler<GetOrdersById, OrderResponse>, GetOrdersByRequestQueryRequestHandler>();
            
            //services.AddOpenApiDocument();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("rpc", async context =>
                {
                    await context.Response.WriteAsync("works");
                });

                endpoints.MapGet("rpc/{type}", async context =>
                {
                    var (request, requestType, responseType) = await GetRequest(context);
                    await InvokeHandler(context, request, requestType, responseType);
                });
            });

            app.UseOpenApiWithRequestHandling();
            app.UseSwaggerUi3(x => x.DocumentPath = "swagger/v1/swagger.json");
        }

        private static async Task InvokeHandler(HttpContext context, object request, Type requestType, Type responseType)
        {
            var handlerType = typeof(IQueryRequestHandler<,>).MakeGenericType(requestType, responseType);

            var handler = (dynamic)context.RequestServices.GetService(handlerType);
            var response = await handler.Handle((dynamic)request);

            await JsonSerializer.SerializeAsync(context.Response.Body, response);
        }

        private static async Task<(object request, Type requestType, Type responseType)> GetRequest(HttpContext context)
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

            var responseType = requestType.GetInterface(typeof(IRequest<>).FullName).GenericTypeArguments.First();

            return (request, requestType, responseType);
        }
    }


    public static class BuilderExtensions
    {
        public static IApplicationBuilder UseOpenApiWithRequestHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<WebOpenApiDocumentMiddleware>();
        }
    }

    public interface IQueryRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        Task<TResponse> Handle(TRequest request);
    }

    public interface IRequest<TResponse> { }


    public class GetOrdersById : IRequest<OrderResponse>
    {
        public string Id { get; set; }
    }

    public class OrderResponse
    {
        public string Message { get; set; }
        public Order Order { get; set; }
    }

    public class Order
    {
        public string Id { get; set; }
        public DateTimeOffset OrderDate { get; set; }
    }

    /// <summary>
    /// Interface based
    /// </summary>
    public class GetOrdersByRequestQueryRequestHandler : IQueryRequestHandler<GetOrdersById, OrderResponse>
    {
        public Task<OrderResponse> Handle(GetOrdersById getOrdersById)
        {
            return Task.FromResult(new OrderResponse { Message = getOrdersById.Id + " handled." });
        }
    }

    // Convention based
    public class RequestHandler2
    {
        public Task<OrderResponse> Handle(GetOrdersById getOrdersById)
        {
            return Task.FromResult(new OrderResponse { Message = getOrdersById.Id + " handled." });
        }
    }

    /*
        AspNetCoreOpenApiDocumentGenerator
            OpenApiDocumentRegistration (in NSwagServiceCollectionExtensions.AddSwaggerDocument or NSwagServiceCollectionExtensions.AddOpenApiDocument)
            OpenApiDocument (Meta Infos)
            OpenApiSchemaResolver
            OpenApiOperation

        OperationProcessorContext
            
        IOperationProcessor
            OperationTagsProcessor


        1. Create Meta Data for document
        2. OpenApiSchemaResolver
        3. RunOperationProcessors

    */
}
