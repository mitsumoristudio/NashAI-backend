using System.Security.Cryptography;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using NashAI_app.Model;
using UglyToad.PdfPig.Fonts.Encodings;
using UglyToad.PdfPig.Tokens;
using ZiggyCreatures.Caching.Fusion;
using Encoding = System.Text.Encoding;

namespace NashAI_app.Services;

public class SemanticSearchService
{
     private readonly IVectorSearchService _vectorSearch;
     private readonly IChatClient _chatClient;
     private readonly IFusionCache _fusionCache;

     public SemanticSearchService(IVectorSearchService vectorSearch, IChatClient chatClient, IFusionCache fusionCache)
     {
          _vectorSearch = vectorSearch;
          _chatClient = chatClient;
          _fusionCache = fusionCache;
     }
     
     // Simple hash helper for cache keys
     private string Hash(string input) => Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(input)));

     // CACHED Vector Search
     public async Task<string> SummarizeOshaStandardAsync(string query, IEnumerable<DocumentEmbeddingVB> topResults)
     {
          // Combine semantic search results
          var contextText = string.Join("\n\n---\n\n", topResults.Select(r => r.Content));
          var contextLength = contextText.Length;
          
          var styleInstruction = contextLength > 2000
               ? "Give a short overview (3–5 sentences) of the main OSHA requirements related to the question."
               : "Give a clear, plain-English summary of the OSHA rules. Highlight the main safety obligations and employer responsibilities.";

          var systemPrompt = $@"
You are a workplace safety compliance assistant specializing in OSHA regulations.
Your job is to summarize OSHA safety standards related to the user’s question.

{styleInstruction}

Focus on:
- The key safety requirement(s)
- What the employer must ensure
- What the employee must do
- Any important thresholds (distances, heights, limits)
- Keep the explanation factual and concise.
- If the text doesn’t include relevant details, say so.

### User Question:
{query}

### OSHA Context:
{contextText}";
          
          var response = await _chatClient.GetResponseAsync(new[]
          {
               new ChatMessage(ChatRole.System, systemPrompt)
          });
          
          return response?.ToString() ?? "No summary was generated";
     }

     public async Task<string> SummarizeContractClause(string query, IEnumerable<DocumentEmbeddingVB> topResults)
     {
          // Combine semantic search results
          var contextText = string.Join("\n\n---\n\n", topResults.Select(r => r.Content));
          var contextLength = contextText.Length;
          
          var cacheKey = $"analysis:{Hash(query + contextText)}";

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
          
               return await _fusionCache.GetOrSetAsync(cacheKey, async _ =>
                    {
                         var response = await _chatClient.GetResponseAsync(new[]
                         {
                              new ChatMessage(ChatRole.System, systemPrompt),
                              new ChatMessage(ChatRole.User, $"Summarize this clause based on: {contextText}\n\nQuestion: {query}")
                         });

                         return response?.ToString() ?? "No summary was generated";
                    },
                    options => options.SetDuration(TimeSpan.FromHours(1))
               );
     }
     
     public async Task<string> AnalyzeContractClause(string query, IEnumerable<DocumentEmbeddingVB> topResults)
     {
          var contextText = string.Join("\n\n---\n\n", topResults.Select(r => r.Content));
          var cacheKey = $"analysis:{Hash(query + contextText)}";
          
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

          return await _fusionCache.GetOrSetAsync(cacheKey, async _ =>
          {
               var response = await _chatClient.GetResponseAsync(new[]
               {
                    new ChatMessage(ChatRole.System, systemPrompt),
                    new ChatMessage(ChatRole.User, $"Analyze this clause based on: {contextText}\n\nQuestion: {query}")
               });

               return response?.ToString() ?? "No summary was generated";
          },
               options => options.SetDuration(TimeSpan.FromHours(1))
               );
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
     