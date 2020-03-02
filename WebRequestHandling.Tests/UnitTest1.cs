using System.Threading.Tasks;
using Newtonsoft.Json;
using NJsonSchema;
using WebRequestHandling.Infrastructure.NSwag;
using Xunit;

namespace WebRequestHandling.Tests
{
    public class UnitTest1
    {
        [Fact]
        public async Task Test1()
        {
            var generator = new WebRequestHandlingSwaggerGenerator(new WebRequestHandlingSwaggerGeneratorSettings());
            var document = await generator.GenerateDocument(typeof(Startup).Assembly);

            var json = document.ToJson(SchemaType.OpenApi3, Formatting.Indented);
        }
    }
}
