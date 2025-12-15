namespace NashAI_app.Services;
using OpenAI;
using OpenAI.Embeddings;
using System.Linq;

public class OpenAIEmbeddingService : IEmbeddingService
{
    private readonly OpenAIClient _client;
    private readonly string _model;

    public OpenAIEmbeddingService(OpenAIClient client, string model = "text-embedding-3-small")
    {
        _client = client;
        _model = model;
    }

    public async Task<float[]> CreateEmbeddingAsync(string text)
    {
        // Use GetEmbeddingClient for the model
        var embeddingClient = _client.GetEmbeddingClient(_model);

        // Generate embedding
        var embeddingResult = await embeddingClient.GenerateEmbeddingAsync(text);

        // Convert embedding to float array (latest SDK)
        return embeddingResult.Value.ToFloats().ToArray();
    }
}
