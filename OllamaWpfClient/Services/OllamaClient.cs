using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using OllamaWpfClient.Models;

namespace OllamaWpfClient.Services
{
    public class OllamaClient : IOllamaClient
    {
        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(5)
        };

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly string _baseUrl;

        public OllamaClient(string baseUrl = "http://127.0.0.1:11434")
        {
            _baseUrl = baseUrl.TrimEnd('/');
        }

        public async Task<IReadOnlyList<OllamaModelInfo>> ListModelsAsync(CancellationToken cancellationToken = default)
        {
            string url = $"{_baseUrl}/api/tags";

            using var response = await _httpClient.GetAsync(url, cancellationToken);
            string responseText = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Ollama API 오류 {(int)response.StatusCode}: {responseText}");
            }

            var tags = JsonSerializer.Deserialize<OllamaTagsResponse>(responseText, _jsonOptions);
            if (tags == null)
            {
                return Array.Empty<OllamaModelInfo>();
            }
            return tags.Models;
        }

        public async Task<ChatMessage> ChatAsync(string model, IEnumerable<ChatMessage> history, CancellationToken cancellationToken = default)
        {
            string url = $"{_baseUrl}/api/chat";

            var requestBody = new OllamaChatRequest
            {
                Model = model,
                Stream = false,
                Messages = history.Select(m => new OllamaChatMessageDto
                {
                    Role = m.Role,
                    Content = m.Content
                }).ToList()
            };

            string json = JsonSerializer.Serialize(requestBody);

            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await _httpClient.PostAsync(url, content, cancellationToken);

            string responseText = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Ollama API 오류 {(int)response.StatusCode}: {responseText}");
            }

            var chat = JsonSerializer.Deserialize<OllamaChatResponse>(responseText, _jsonOptions);
            string replyContent = chat?.Message?.Content ?? "응답을 추출하지 못했습니다.";
            return new ChatMessage("assistant", replyContent);
        }
    }
}
