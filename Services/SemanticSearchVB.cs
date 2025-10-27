using Microsoft.Extensions.VectorData;
using NashAI_app.Model;

namespace NashAI_app.Services;

public class SemanticSearchVB
{
     private readonly IVectorSearchService _vectorSearch;

     public SemanticSearchVB(IVectorSearchService vectorSearch)
     {
          _vectorSearch = vectorSearch;
     }

     public Task<IEnumerable<DocumentEmbeddingVB>> SearchAsync(string text, string? documentId, int maxResults)
     {
          return _vectorSearch.SearchVectorAsync(query: text, documentId: documentId, maxResults: maxResults);
     }

     public Task UpdateVectorAsync(IEnumerable<IngestedChunk> ingestedchunks)
     {
          return _vectorSearch.UpdateVectorAsync(ingestedchunks: ingestedchunks);
     }
}
     