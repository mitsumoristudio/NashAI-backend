using NashAI_app.Model;

namespace NashAI_app.Services;

public interface IVectorSearchService
{
    Task<IEnumerable<DocumentEmbeddingVB>> SearchVectorAsync(string query, string? documentId, int maxResults);
    Task UpdateVectorAsync(IEnumerable<IngestedChunk> ingestedchunks);
}