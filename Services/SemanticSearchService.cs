using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using NashAI_app.Model;

namespace NashAI_app.Services;

public class SemanticSearchService
{
     private readonly IVectorSearchService _vectorSearch;
     private readonly IChatClient _chatClient;

     public SemanticSearchService(IVectorSearchService vectorSearch, IChatClient chatClient)
     {
          _vectorSearch = vectorSearch;
          _chatClient = chatClient;
     }

     public async Task<string> SummarizeContractClause(string query, IEnumerable<DocumentEmbeddingVB> topResults)
     {
          // Combine semantic search results
          var contextText = string.Join("\n\n---\n\n", topResults.Select(r => r.Content));
          var contextLength = contextText.Length;

          string styleInstruction;
          if (contextLength > 2500)
          {
               // Long clause: executive summary mode
               styleInstruction = @"
     Summarize briefly in 3–5 sentences.
     Focus on the main purpose and effect of the clause — what it means overall, 
     who is responsible, and any financial or timing consequences.
     Avoid line-by-line detail or quotes.";
               }
               else
               {
                    // Shorter clause: detailed plain-language explanation
                    styleInstruction = @"
     Explain in simple, clear language.
     Highlight:
     - What the clause requires or allows.
     - Which party (owner, contractor, subcontractor) it affects most.
     - Key risks or obligations.
     - Any important dates, limits, or penalties.";
               }
               
               var systemPrompt = $@"
     You are a construction contract assistant. 
     Your job is to summarize or explain clauses in plain English, 
     so that non-lawyers can understand what the contract means.

{styleInstruction}

If the context does not contain enough details to answer, say:
'I could not find enough information in the provided text.'

### User Question:
{query}

### Contract Context:
{contextText}";
          
          var response = await _chatClient.GetResponseAsync(new[]
          {
               new ChatMessage(ChatRole.System, systemPrompt)
          });
          
          return response?.ToString() ?? "No summary was generated";

     }

     public async Task<string> AnalyzeContractClause(string query, IEnumerable<DocumentEmbeddingVB> topResults)
     {
          var contextText = string.Join("\n\n---\n\n", topResults.Select(r => r.Content));
          
          var systemPrompt = $@"
You are a legal analyst trained in contract interpretation.
Use structured reasoning to explain the meaning of each clause.

Apply these reasoning patterns:
- Obligation: who must do what
- Right: who may do what
- Condition: when something applies
- Limitation: what cannot be done
- Remedy: what happens if breached

If the context does not contain enough details to answer, say:
'I could not find enough information in the provided text.'

### User Request:
{query}

### Clauses:
{contextText}

### Output:
Summarize each clause using this format:
Clause Type:
Responsible Party:
Trigger Condition:
Effect:
Notice/Deadline (if any):
Plain-English Summary:
";
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
     