using System.Text.Json.Serialization;

namespace ApiDocumentationComparer.Infrastructure.Models
{
    public class ModelDefinitionConfiguration
    {
        [JsonPropertyName("required")]
        public string[] RequiredProperties { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        public PropertiesConfiguration[] Properties { get; set; }
    }

    public class PropertiesConfiguration
    {
        public string Name { get; set; }

        public string Type { get; set; }
    }
}