using System.Text.Json.Serialization;

namespace ApiDocumentationComparer.Infrastructure.Models
{
    public class ApiException
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("exceptionMessage")]
        public string ExceptionMessage { get; set; }

        [JsonPropertyName("exceptionType")]
        public string ExceptionType { get; set; }

        [JsonPropertyName("stackTrace")]
        public string StackTrace { get; set; }
    }
}
