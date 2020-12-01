using System.Text.Json.Serialization;

namespace ApiDocumentationComparer.Infrastructure.Models
{
    public class ParameterConfiguration
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("in")]
        public string In { get; set; }

        [JsonPropertyName("parameterRequired")]
        public bool ParameterRequired { get; set; }

        [JsonPropertyName("schema")]
        public ParameterSchemaConfiguration Schema { get; set; }
    }
}
