using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Namotion.Reflection;
using Newtonsoft.Json;
using NJsonSchema;
using NJsonSchema.Generation;
using NSwag;
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
            services.AddScoped<IHandler<Request, Response>, RequestHandler>();

            services.AddOpenApiDocument();
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

                endpoints.MapPost("rpc/{type}", async context =>
                {
                    var (request, requestType, responseType) = await GetRequest(context);
                    await InvokeHandler(context, request, requestType, responseType);
                });
            });

            var openApiDocument = Get();

            app.UseSwaggerUi3();
        }

        private static async Task InvokeHandler(HttpContext context, object request, Type requestType, Type responseType)
        {
            var handlerType = typeof(IHandler<,>).MakeGenericType(requestType, responseType);

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

        private static OpenApiDocument Get()
        {
            var document = new OpenApiDocument
            {
                //BasePath = "swagger",
                Info = { Title = "Test Doc", Description = "#1st Doc", Version = "v1" }
            };

            var jsonSchemaGeneratorSettings = new JsonSchemaGeneratorSettings();
            var jsonSchemaGenerator = new JsonSchemaGenerator(jsonSchemaGeneratorSettings);
            var openApiSchemaResolver = new OpenApiSchemaResolver(document, jsonSchemaGeneratorSettings);


            //document.Components.Responses.Add();

            var openApiOperation = new OpenApiOperation
            {
                Produces = new List<string> { "application/json" },
                OperationId = "WebRequestHandling.Request",
                Schemes = new List<OpenApiSchema> { OpenApiSchema.Https },
                RequestBody = new OpenApiRequestBody()

            };

            openApiOperation.RequestBody.IsRequired = true;
            openApiOperation.RequestBody.Position = 1;
            openApiOperation.RequestBody.Content["application/json"] = new OpenApiMediaType { Schema = JsonSchema.CreateAnySchema() };

            var openApiResponse = new OpenApiResponse();
            openApiOperation.Responses.Add("200", openApiResponse);

            openApiResponse.Description = "Response Description";
            openApiResponse.Schema = jsonSchemaGenerator.GenerateWithReferenceAndNullability<JsonSchema>(
                typeof(Response).ToContextualType(), true, openApiSchemaResolver);



            var openApiPathItem = new OpenApiPathItem();
            document.Paths.Add("rpc / WebRequestHandling.Request", openApiPathItem);
            openApiPathItem.Add(OpenApiOperationMethod.Post, openApiOperation);
                
            

            var json = document.ToJson(SchemaType.OpenApi3, Formatting.Indented);

            return document;
        }
    }




    public interface IHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        Task<TResponse> Handle(TRequest request);
    }

    public interface IRequest<TResponse> { }


    public class Request : IRequest<Response>
    {
        public string Message { get; set; }
    }

    public class Response
    {
        public string Message { get; set; }
    }

    /// <summary>
    /// Interface based
    /// </summary>
    public class RequestHandler : IHandler<Request, Response>
    {
        public Task<Response> Handle(Request request)
        {
            return Task.FromResult(new Response { Message = request.Message + " handled." });
        }
    }

    // Convention based
    public class RequestHandler2
    {
        public Task<Response> Handle(Request request)
        {
            return Task.FromResult(new Response { Message = request.Message + " handled." });
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
