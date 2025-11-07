using Pgvector;
using UglyToad.PdfPig;

namespace NashAI_app.Services;

public class PDFIngestionService
{
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorSearchService _vectorSearchService;
    private readonly ILogger<PDFIngestionService> _logger;

    public PDFIngestionService(IEmbeddingService embeddingService, IVectorSearchService vectorSearchService,
        ILogger<PDFIngestionService> logger)
    {
        _embeddingService = embeddingService;
        _vectorSearchService = vectorSearchService;
        _logger = logger;
    }

    public async Task IngestPdfAsync(string filePath, string documentId)
    {
        var chunks = new List<IngestedChunk>();
        
        
        // Open the PDF
        using var openPdf = PdfDocument.Open(filePath);

        foreach (var page in openPdf.GetPages())
        {
            var text = page.Text.Trim();

            if (string.IsNullOrEmpty(text))
                continue;

            var pageChunks = SplitIntoChunks(text, 800);

            foreach (var chunk in pageChunks)
            {
                var embedding = await _embeddingService.CreateEmbeddingAsync(chunk);
                var vector = new Vector(embedding);

                chunks.Add(new IngestedChunk
                {
                    DocumentId = documentId,
                    PageNumber = page.Number,
                    Content = chunk,
                    Embeddings = vector.ToArray()
                });
            }
            _logger.LogInformation($"Processed page {page.Number}");
            _logger.LogInformation($"Ingesting {chunks.Count} chunks");
        }
        await _vectorSearchService.UpdateVectorAsync(chunks);
    }
    
    private static IEnumerable<string> SplitIntoChunks(string text, int maxWords)
    {
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < words.Length; i += maxWords)
        {
            yield return string.Join(' ', words.Skip(i).Take(maxWords));
        }
    }
}