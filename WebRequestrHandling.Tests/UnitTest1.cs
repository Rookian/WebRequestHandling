using System.Threading.Tasks;
using Newtonsoft.Json;
using NJsonSchema;
using WebRequestHandling.NSwag;
using Xunit;

namespace WebRequestrHandling.Tests
{
    public class UnitTest1
    {
        [Fact]
        public async Task Test1()
        {
            var generator = new WebRequestHandlingSwaggerGenerator(new WebRequestHandlingSwaggerGeneratorSettings());
            var document = await generator.GenerateDocument();

            var json = document.ToJson(SchemaType.OpenApi3, Formatting.Indented);
        }
    }
}
