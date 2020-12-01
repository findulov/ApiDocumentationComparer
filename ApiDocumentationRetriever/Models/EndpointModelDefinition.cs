using System.Collections.Generic;

namespace ApiDocumentationComparer.Infrastructure.Models
{
    public class EndpointModelDefinition
    {
        public string Name { get; set; }

        public IEnumerable<EndpointModelProperty> Properties { get; set; } = new List<EndpointModelProperty>();
    }
}
