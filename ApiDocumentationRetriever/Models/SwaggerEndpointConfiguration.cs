using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ApiDocumentationComparer.Infrastructure.Models
{
    public class SwaggerEndpointConfiguration
    {
        public string Url { get; set; }

        public string HttpMethod { get; set; }

        [JsonPropertyName("tags")]
        public string[] Tags { get; set; }

        [JsonPropertyName("operationId")]
        public string OperationId { get; set; }

        [JsonPropertyName("consumes")]
        public string[] Consumes { get; set; }

        [JsonPropertyName("produces")]
        public string[] Produces { get; set; }

        [JsonPropertyName("parameters")]
        public IEnumerable<ParameterConfiguration> Parameters { get; set; } = new List<ParameterConfiguration>();

        public Dictionary<string, IEnumerable<EndpointModelProperty>> Properties { get; set; } = new Dictionary<string, IEnumerable<EndpointModelProperty>>();
    }
}
