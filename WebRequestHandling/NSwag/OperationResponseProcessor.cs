using System;
using System.Collections.Generic;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace WebRequestHandling.NSwag
{
    public class OperationResponseProcessor : OperationResponseProcessorBase, IOperationProcessor
    {
        public OperationResponseProcessor(WebRequestHandlingSwaggerGeneratorSettings settings) : base(settings)
        {
        }

        protected override string GetVoidResponseStatusCode()
        {
            return "200";
        }

        public bool Process(OperationProcessorContext context)
        {
             ProcessResponseTypeAttributes(context, new List<Attribute>() );
             return true;
        }
    }
}