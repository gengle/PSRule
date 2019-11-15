using Newtonsoft.Json;
using PSRule.Pipeline;
using System.Globalization;
using System.Management.Automation;
using Xunit;

namespace PSRule
{
    public sealed class PSObjectJsonConverterTests
    {
        [Fact]
        public void DeserializeObjectPSObject()
        {
            var settings = GetJsonSettings();

            var expected = JsonConvert.SerializeObject(value: GetTestObject(), settings: settings);
            var actual = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<PSObject>(value: GetTestObjectJson(), settings: settings), settings: settings);

            Assert.Equal(expected, actual);
            
        }

        private string GetTestObjectJson()
        {
            return "{ \"name\": \"value\", \"object\": { \"name\": \"child\" } }";
        }

        private PSObject GetTestObject()
        {
            var result = new PSObject();
            result.Properties.Add(new PSNoteProperty("name", "value"));

            var child = new PSObject();
            child.Properties.Add(new PSNoteProperty("name", "child"));

            result.Properties.Add(new PSNoteProperty("object", child));

            return result;
        }

        private JsonSerializerSettings GetJsonSettings()
        {
            var settings = new JsonSerializerSettings { Formatting = Formatting.None, TypeNameHandling = TypeNameHandling.None, MaxDepth = 1024, Culture = CultureInfo.InvariantCulture };
            settings.Converters.Insert(0, new PSObjectJsonConverter());

            return settings;
        }
    }
}
