using Microsoft.Extensions.VectorData;
using NashAI_app.Model;

namespace NashAI_app.Services;

public class SemanticSearch_sqlite(
    VectorStoreCollection<string, IngestedChunk> vectorCollection)
{
    public async Task<IReadOnlyList<SearchResultModel>> SearchAsync(string text, string? documentIdFilter, int maxResults)
    {
        // Run Semantic Vector Search
        var nearest = vectorCollection.SearchAsync(text, maxResults, new VectorSearchOptions<IngestedChunk>
        {
            // If filesystem is null, search across ALL documents
            Filter = string.IsNullOrEmpty(documentIdFilter)
                ? null
                : record => record.DocumentId == documentIdFilter
        });

        // get both the chunk and the similarity Score
      //  return await nearest.Select(result => result.Record).ToListAsync();
      return await nearest
          .Select(result => new SearchResultModel
          {
              DocumentId = result.Record.DocumentId,
              PageNumber = result.Record.PageNumber,
              Content = result.Record.Content,
              Score = result.Score,
          }).ToListAsync();
    }
}

