using ApiDocumentationComparer.Infrastructure.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ApiDocumentationComparer.Infrastructure.Handlers
{
    public class ApiDocumentationRetrieveHandler : IRequestHandler<ApiDocumentationRetrieveRequest, ApiDocumentationRetrieveResponse>
    {
        public async Task<ApiDocumentationRetrieveResponse> Handle(ApiDocumentationRetrieveRequest request, CancellationToken cancellationToken)
        {
            ApiDocumentationRetrieveResponse apiDocumentationRetrieveResponse = new ApiDocumentationRetrieveResponse();

            using (HttpClient httpClient = new HttpClient())
            {
                HttpResponseMessage response;

                try
                {
                    response = await httpClient.GetAsync(new Uri(request.ApiUrl));
                }
                catch (HttpRequestException ex)
                {
                    apiDocumentationRetrieveResponse.Errors.Add(ex.Message);
                    return apiDocumentationRetrieveResponse;
                }

                if (!response.IsSuccessStatusCode)
                {
                    string jsonError = await response.Content.ReadAsStringAsync();
                    string error;

                    try
                    {
                        ApiException apiException = JsonSerializer.Deserialize<ApiException>(jsonError);
                        error = $"{apiException.Message} {apiException.ExceptionMessage}";
                    }
                    catch (Exception ex)
                    {
                        error = ex.Message;
                    }

                    apiDocumentationRetrieveResponse.Errors.Add(error);
                    return apiDocumentationRetrieveResponse;
                }

                var swaggerResponse = await JsonSerializer.DeserializeAsync<Dictionary<string, JsonElement>>(await response.Content.ReadAsStreamAsync());

                var (endpointModelDefinitions, errors) = LoadModelDefinitions(swaggerResponse["definitions"].EnumerateObject().ToList());
                var endpoints = LoadEndpointsConfiguration(
                    swaggerResponse["paths"].EnumerateObject().ToList(),
                    endpointModelDefinitions);

                apiDocumentationRetrieveResponse.Endpoints = endpoints;
                apiDocumentationRetrieveResponse.Errors = errors.ToList();

                return apiDocumentationRetrieveResponse;
            }
        }

        private List<SwaggerEndpointConfiguration> LoadEndpointsConfiguration(
            List<JsonProperty> endpointConfigs,
            IEnumerable<EndpointModelDefinition> endpointModelDefinitions)
        {
            List<SwaggerEndpointConfiguration> endpoints = new List<SwaggerEndpointConfiguration>();

            foreach (var endpointConfig in endpointConfigs)
            {
                var requestMethodWithEndpointInfo = JsonSerializer.Deserialize<Dictionary<string, object>>(endpointConfig.Value.GetRawText()).FirstOrDefault();
                string requestMethod = requestMethodWithEndpointInfo.Key;

                SwaggerEndpointConfiguration endpointConfiguration = JsonSerializer.Deserialize<SwaggerEndpointConfiguration>(requestMethodWithEndpointInfo.Value.ToString());
                endpointConfiguration.Url = endpointConfig.Name;
                endpointConfiguration.HttpMethod = requestMethod;

                foreach (var param in endpointConfiguration.Parameters)
                {
                    if (param.In == "body" && param.Schema != null && param.Schema.Ref != null)
                    {
                        string modelName = param.Schema.Ref.Replace("#/definitions/", string.Empty);

                        var properties = endpointModelDefinitions
                            .FirstOrDefault(e => e.Name == modelName)
                            ?.Properties
                            ?.ToList();

                        if (properties != null)
                        {
                            endpointConfiguration.Properties.Add(param.Name, properties);
                        }
                    }
                }

                endpoints.Add(endpointConfiguration);
            }

            return endpoints;
        }

        private (IEnumerable<EndpointModelDefinition> endpointModelDefinitions, IEnumerable<string> errors)
            LoadModelDefinitions(List<JsonProperty> definitionsConfig)
        {
            List<EndpointModelDefinition> endpointModelDefinitions = new List<EndpointModelDefinition>();
            List<string> errors = new List<string>();

            foreach (var definitionConfig in definitionsConfig)
            {
                try
                {
                    var properties = FetchPropertiesFromDefinition(definitionConfig, definitionsConfig);

                    bool requiredDefinitionExists = definitionConfig.Value
                        .EnumerateObject()
                        .Any(c => c.Name == "required");

                    List<string> requiredProperties = new List<string>();

                    if (requiredDefinitionExists)
                    {
                        requiredProperties = definitionConfig.Value
                            .EnumerateObject()
                            .First(c => c.Name == "required")
                            .Value
                            .EnumerateArray()
                            .Select(p => p.GetString())
                            .ToList();
                    }

                    properties.ForEach(p => p.IsRequired = requiredProperties.Contains(p.Name));

                    endpointModelDefinitions.Add(new EndpointModelDefinition
                    {
                        Name = definitionConfig.Name,
                        Properties = properties
                    });
                }
                catch (Exception ex)
                {
                    errors.Add($"An error has occurred: {Environment.NewLine}{ex}");
                }
            }

            return (endpointModelDefinitions, errors);
        }

        private List<EndpointModelProperty> FetchPropertiesFromDefinition(
            JsonProperty definitionConfig,
            List<JsonProperty> allDefinitionsConfig,
            string referencedDefinitionObjectPropertyName = null)
        {
            var propertiesJsonElement = definitionConfig.Value
                .EnumerateObject()
                .FirstOrDefault(c => c.Name == "properties");

            var propertiesEnumerator = propertiesJsonElement.Value.EnumerateObject();
            List<EndpointModelProperty> properties = new List<EndpointModelProperty>();

            foreach (var property in propertiesEnumerator)
            {
                EndpointModelProperty endpointModelProperty = new EndpointModelProperty();

                if (property.Value.TryGetProperty("type", out JsonElement type))
                {
                    endpointModelProperty.Name = !string.IsNullOrWhiteSpace(referencedDefinitionObjectPropertyName)
                        ? $"{referencedDefinitionObjectPropertyName}_{property.Name}"
                        : property.Name;
                    endpointModelProperty.IsRequired = false;

                    endpointModelProperty.Type = type.ToString();

                    properties.Add(endpointModelProperty);
                }
                else
                {
                    // There is a reference to a definition... ("$ref": "#/definitions/....")
                    // We need to fetch recursively the properties of the definition object.
                    string referencedDefinitionName = property.Value.GetProperty("$ref")
                        .ToString()
                        .Replace("#/definitions/", string.Empty);

                    var referencedDefinitionConfig = allDefinitionsConfig.SingleOrDefault(d => d.Name == referencedDefinitionName);

                    if (!string.IsNullOrWhiteSpace(referencedDefinitionConfig.Name))
                    {
                        properties.AddRange(FetchPropertiesFromDefinition(
                            referencedDefinitionConfig,
                            allDefinitionsConfig,
                            referencedDefinitionObjectPropertyName: property.Name));
                    }
                }
            }

            return properties;
        }
    }

    public class ApiDocumentationRetrieveRequest : IRequest<ApiDocumentationRetrieveResponse>
    {
        public ApiDocumentationRetrieveRequest(string apiUrl)
        {
            ApiUrl = apiUrl;
        }

        public string ApiUrl { get; set; }
    }

    public class ApiDocumentationRetrieveResponse
    {
        public IEnumerable<SwaggerEndpointConfiguration> Endpoints { get; set; } = new List<SwaggerEndpointConfiguration>();

        public ICollection<string> Errors { get; set; } = new HashSet<string>();
    }
}
