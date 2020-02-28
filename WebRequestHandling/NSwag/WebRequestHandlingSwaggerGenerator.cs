using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NJsonSchema;
using NSwag;
using NSwag.Generation;
using NSwag.Generation.Processors.Contexts;

namespace WebRequestHandling.NSwag
{
    public class WebRequestHandlingSwaggerGenerator 
    {
        public WebRequestHandlingSwaggerGeneratorSettings Settings { get; }

        public WebRequestHandlingSwaggerGenerator(WebRequestHandlingSwaggerGeneratorSettings settings)
        {
            Settings = settings;
        }

        public async Task<OpenApiDocument> GenerateDocument()
        {
            var document = await CreateDocumentAsync().ConfigureAwait(false);
            var schemaResolver = new OpenApiSchemaResolver(document, Settings);

            var handlerTypes = new List<Type> { typeof(GetOrdersByRequestQueryRequestHandler) }; // TODO scan for more handlers
            var usedHandlerTypes = new List<Type>();

            foreach (var handlerType in handlerTypes)
            {
                var generator = new OpenApiDocumentGenerator(Settings, schemaResolver);
                var isIncluded = GenerateForHandler(document, handlerType, generator, schemaResolver);
                if (isIncluded)
                {
                    usedHandlerTypes.Add(handlerType);
                }
            }

            document.GenerateOperationIds();

            foreach (var processor in Settings.DocumentProcessors)
            {
                processor.Process(new DocumentProcessorContext(document, handlerTypes,
                    usedHandlerTypes, schemaResolver, Settings.SchemaGenerator, Settings));
            }

            return document;
        }

        private bool GenerateForHandler(OpenApiDocument document, Type handlerType, OpenApiDocumentGenerator swaggerGenerator, OpenApiSchemaResolver schemaResolver)
        {
            var methodInfo = handlerType.GetMethods().Single();
            var isQuery = handlerType.IsAssignableToGenericType(typeof(IQueryRequestHandler<,>));

            var httpMethod = isQuery ? "GET" : "POST";
            var httpPath = $"/rpc/{GetRequestTypeFullName(methodInfo)}";

            var operationDescription = new OpenApiOperationDescription
            {
                Path = httpPath,
                Method = httpMethod,
                Operation = new OpenApiOperation
                {
                    IsDeprecated = methodInfo.GetCustomAttribute<ObsoleteAttribute>() != null,
                    OperationId = GetOperationId(document, handlerType.Name)
                }
            };

            return AddOperationDescriptionsToDocumentAsync(document, handlerType, (operationDescription, methodInfo), swaggerGenerator, schemaResolver);
        }

        private static string GetRequestTypeFullName(MethodInfo methodInfo)
        {
            return methodInfo.GetParameters().Single().ParameterType.FullName;
        }

        private bool AddOperationDescriptionsToDocumentAsync(OpenApiDocument document,
            Type handlerType,
            (OpenApiOperationDescription openApiOperationDescription, MethodInfo methodInfo) operation, OpenApiDocumentGenerator swaggerGenerator,
            OpenApiSchemaResolver schemaResolver)
        {
            var addOperation = RunOperationProcessorsAsync(document, handlerType, operation, swaggerGenerator, schemaResolver);
            if (addOperation)
            {
                var operationDescription = operation.openApiOperationDescription;

                var path = operationDescription.Path.Replace("//", "/");

                if (!document.Paths.ContainsKey(path))
                    document.Paths[path] = new OpenApiPathItem();

                if (document.Paths[path].ContainsKey(operationDescription.Method))
                {
                    throw new InvalidOperationException("The method '" + operationDescription.Method + "' on path '" + path + "' is registered multiple times");
                }

                document.Paths[path][operationDescription.Method] = operationDescription.Operation;
            }

            return addOperation;
        }

        private bool RunOperationProcessorsAsync(OpenApiDocument document, Type handlerType,
            (OpenApiOperationDescription openApiOperationDescription, MethodInfo methodInfo) operation,
            OpenApiDocumentGenerator swaggerGenerator, OpenApiSchemaResolver schemaResolver)
        {
            var allOperations = new List<OpenApiOperationDescription> { operation.openApiOperationDescription }; // TODO ?!

            var context = new OperationProcessorContext(document, operation.openApiOperationDescription, handlerType,
                operation.methodInfo, swaggerGenerator, Settings.SchemaGenerator, schemaResolver, Settings, allOperations);

            foreach (var operationProcessor in Settings.OperationProcessors)
            {
                if (operationProcessor.Process(context) == false)
                    return false;
            }

            return true;
        }

        private static string GetOperationId(OpenApiDocument document, string handlerName)
        {
            var operationId = handlerName;
            var number = 1;
            while (document.Operations.Any(o => o.Operation.OperationId == operationId + (number > 1 ? "_" + number : string.Empty)))
                number++;

            return operationId + (number > 1 ? number.ToString() : string.Empty);
        }

        private async Task<OpenApiDocument> CreateDocumentAsync()
        {
            var document = !string.IsNullOrEmpty(Settings.DocumentTemplate) ?
                await OpenApiDocument.FromJsonAsync(Settings.DocumentTemplate).ConfigureAwait(false) :
                new OpenApiDocument();

            document.Generator = "NSwag v" + OpenApiDocument.ToolchainVersion + " (NJsonSchema v" + JsonSchema.ToolchainVersion + ")";
            document.SchemaType = Settings.SchemaType;

            document.Consumes = new List<string> { "application/json" };
            document.Produces = new List<string> { "application/json" };

            if (document.Info == null)
                document.Info = new OpenApiInfo();

            if (string.IsNullOrEmpty(Settings.DocumentTemplate))
            {
                if (!string.IsNullOrEmpty(Settings.Title))
                    document.Info.Title = Settings.Title;
                if (!string.IsNullOrEmpty(Settings.Description))
                    document.Info.Description = Settings.Description;
                if (!string.IsNullOrEmpty(Settings.Version))
                    document.Info.Version = Settings.Version;
            }

            return document;
        }
    }

    public class WebRequestHandlingSwaggerGeneratorSettings : OpenApiDocumentGeneratorSettings
    {
        public WebRequestHandlingSwaggerGeneratorSettings()
        {
            OperationProcessors.Add(new OperationResponseProcessor(this));
            OperationProcessors.Add(new OperationParameterProcessor());
        }
    }
}