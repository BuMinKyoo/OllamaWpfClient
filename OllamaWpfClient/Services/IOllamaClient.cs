using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OllamaWpfClient.Models;

namespace OllamaWpfClient.Services
{
    public interface IOllamaClient
    {
        Task<IReadOnlyList<OllamaModelInfo>> ListModelsAsync(CancellationToken cancellationToken = default);

        Task<ChatMessage> ChatAsync(string model, IEnumerable<ChatMessage> history, CancellationToken cancellationToken = default);
    }
}
