namespace NashAI_app.Services;

public interface IEmbeddingService
{
        Task<float[]> CreateEmbeddingAsync(string text);

        

}