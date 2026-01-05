using Microsoft.EntityFrameworkCore;
using NashAI_app.Dto;
using Pgvector;
using Project_Manassas.Database;
using Project_Manassas.Model;
using UglyToad.PdfPig;

namespace NashAI_app.Services;

public class PDFIngestionService
{
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorSearchService _vectorSearchService;
    private readonly ILogger<PDFIngestionService> _logger;
    private readonly ProjectContext _projectContext;

    public PDFIngestionService(IEmbeddingService embeddingService, IVectorSearchService vectorSearchService,
        ILogger<PDFIngestionService> logger, ProjectContext projectContext)
    {
        _embeddingService = embeddingService;
        _vectorSearchService = vectorSearchService;
        _logger = logger;
        _projectContext = projectContext;
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

    public async Task<IReadOnlyList<IngestedPdfDto>> GetIngestedPdfsAsync()
    {
        return await _projectContext.DocumentEmbeddings
            .AsNoTracking()
            .GroupBy(d => d.DocumentId)
            .Select(document => new IngestedPdfDto
            {
                DocumentId = document.Key,
                ChunkCount = document.Count(),
                PageCount = document.Select(x => x.PageNumber).Distinct().Count()
            })
            .OrderByDescending(x => x.ChunkCount)
            .ToListAsync();
    }

    public async Task<int> DeleteByDocumentEmbeddingIdAsync(string documentId)
    {
        if (string.IsNullOrWhiteSpace(documentId))
            throw new ArgumentException("Document ID cannot be null or empty", nameof(documentId));

        return await _projectContext.DocumentEmbeddings
            .Where(d => d.DocumentId == documentId)
            .ExecuteDeleteAsync();
    }

    public async Task<int> DeleteByDocumentPdfAsync(string documentId)
    {
        if (string.IsNullOrWhiteSpace(documentId))
            throw new ArgumentException("Document ID cannot be null or empty", nameof(documentId));
        
        return await _projectContext.PdfFiles
            .Where(p => p.DocumentId == documentId)
            .ExecuteDeleteAsync();
    }
    
    private static IEnumerable<string> SplitIntoChunks(string text, int maxWords)
    {
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < words.Length; i += maxWords)
        {
            yield return string.Join(' ', words.Skip(i).Take(maxWords));
        }
    }

    public async Task SavePdfFileAsync(string documentId, string fileName, byte[] data)
    {
        var createPdf = new PdfFileEntity
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            FileName = fileName,
            Data = data,
        };
        
        _projectContext.PdfFiles.Add(createPdf);
        
        await _projectContext.SaveChangesAsync();
    }

    public async Task<byte[]?> GetPdfAsync(string documentId)
    {
        var pdf = await _projectContext.PdfFiles
            .AsNoTracking()
            .Where(p => p.DocumentId == documentId)
            .Select(p => p.Data)
            .FirstOrDefaultAsync();
        
        return pdf;
    }
}