using System.Text.Json.Serialization;

namespace ApiDocumentationComparer.Infrastructure.Models
{
    public class ParameterSchemaConfiguration
    {
        [JsonPropertyName("$ref")]
        public string Ref { get; set; }
    }
}
