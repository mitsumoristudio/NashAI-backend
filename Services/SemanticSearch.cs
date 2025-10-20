using Microsoft.Extensions.VectorData;

namespace NashAI_app.Services;

public class SemanticSearch(
    VectorStoreCollection<string, IngestedChunk> vectorCollection)
{
    public async Task<IReadOnlyList<IngestedChunk>> SearchAsync(string text, string? documentIdFilter, int maxResults)
    {
        // var nearest = vectorCollection.SearchAsync(text, maxResults, new VectorSearchOptions<IngestedChunk>
        // {
        //     Filter = documentIdFilter is { Length: > 0 } ? record => record.DocumentId == documentIdFilter : null,
        // });
        var nearest = vectorCollection.SearchAsync(text, maxResults, new VectorSearchOptions<IngestedChunk>
        {
            // If filesystem is null, search across ALL documents
            Filter = string.IsNullOrEmpty(documentIdFilter)
                ? null
                : record => record.DocumentId == documentIdFilter
        });

        return await nearest.Select(result => result.Record).ToListAsync();
    }
}

