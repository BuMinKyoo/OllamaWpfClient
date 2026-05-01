using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OllamaWpfClient.Models
{
    public class OllamaTagsResponse
    {
        [JsonPropertyName("models")]
        public List<OllamaModelInfo> Models { get; set; } = new List<OllamaModelInfo>();
    }

    public class OllamaModelInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("size")]
        public long Size { get; set; }

        [JsonPropertyName("modified_at")]
        public string ModifiedAt { get; set; } = string.Empty;
    }

    public class OllamaChatRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("messages")]
        public List<OllamaChatMessageDto> Messages { get; set; } = new List<OllamaChatMessageDto>();

        [JsonPropertyName("stream")]
        public bool Stream { get; set; }
    }

    public class OllamaChatMessageDto
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }

    public class OllamaChatResponse
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public OllamaChatMessageDto? Message { get; set; }

        [JsonPropertyName("done")]
        public bool Done { get; set; }
    }
}
