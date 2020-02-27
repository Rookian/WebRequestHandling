using System.Linq;
using Namotion.Reflection;
using NJsonSchema;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace WebRequestHandling.NSwag
{
    public class OperationParameterProcessor : IOperationProcessor
    {
        public bool Process(OperationProcessorContext context)
        {
            var parameterInfo = context.MethodInfo.GetParameters().SingleOrDefault();
            if (parameterInfo == null)
                return false;

            var openApiParameter = new OpenApiParameter
            {
                Kind = OpenApiParameterKind.Query,
                Name = parameterInfo.Name,
                Description = parameterInfo.Name,
                Schema = context.SchemaGenerator.GenerateWithReferenceAndNullability<JsonSchema>(
                    parameterInfo.ParameterType.ToContextualType(), true, context.SchemaResolver)
            };


            context.OperationDescription.Operation.Parameters.Add(openApiParameter);

            return true;
        }
    }
}