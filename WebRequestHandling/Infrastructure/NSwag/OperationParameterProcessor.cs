using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Namotion.Reflection;
using NJsonSchema;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using WebRequestHandling.Infrastructure.ExecutionPipeline;

namespace WebRequestHandling.Infrastructure.NSwag
{
    public class OperationParameterProcessor : IOperationProcessor
    {
        public bool Process(OperationProcessorContext context)
        {
            var parameterInfos = context.MethodInfo.GetParameters();
            var parameterInfo = parameterInfos.SingleOrDefault();
            if (parameterInfo == null)
                return false;

            var isQuery = context.MethodInfo.DeclaringType.IsAssignableToGenericType(typeof(IQueryRequestHandler<,>));

            if (isQuery)
            {
                // Query
                foreach (var contextualProperty in parameterInfo.ToContextualParameter().Type.GetContextualProperties())
                {
                    var operationParameter = context.DocumentGenerator.CreatePrimitiveParameter(contextualProperty.Name, contextualProperty.Name,
                        contextualProperty);

                    operationParameter.Kind = OpenApiParameterKind.Query;
                    context.OperationDescription.Operation.Parameters.Add(operationParameter);
                }
            }
            else
            {
                // Command
                var operationParameter = new OpenApiParameter
                {
                    Kind = OpenApiParameterKind.Body,
                    Name = parameterInfo.Name,
                    Description = parameterInfo.Name,
                    Schema = context.SchemaGenerator.GenerateWithReferenceAndNullability<JsonSchema>(parameterInfo.ParameterType.ToContextualType(), true, context.SchemaResolver),
                    Position = 1
                };

                context.OperationDescription.Operation.Parameters.Add(operationParameter);
                ((Dictionary<ParameterInfo, OpenApiParameter>)context.Parameters)[parameterInfo] = operationParameter;
            }
            return true;
        }
    }
}