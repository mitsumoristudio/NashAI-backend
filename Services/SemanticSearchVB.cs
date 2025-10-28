using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using NashAI_app.Model;

namespace NashAI_app.Services;

public class SemanticSearchVB
{
     private readonly IVectorSearchService _vectorSearch;
     private readonly IChatClient _chatClient;

     public SemanticSearchVB(IVectorSearchService vectorSearch, IChatClient chatClient)
     {
          _vectorSearch = vectorSearch;
          _chatClient = chatClient;
     }

     public async Task<string> SummarizeResultsAsync(string query, IEnumerable<DocumentEmbeddingVB> topResults)
     {
          var contextText = string.Join("\n\n---\n\n", topResults.Select(r => r.Content));

          var systemPrompt = $@"
You are a helpful assistant that summarizes retrieved context.
Summarize the following context in relation to the user's query.
Focus on accuracy and conciseness. 
If information is missing, note it.

### User Query:
{query}

### Context:
{contextText}";

          var response = await _chatClient.GetResponseAsync(new[]
          {
               new ChatMessage(ChatRole.System, systemPrompt)
          });
          
          return response?.ToString() ?? "No summary was generated";

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
     