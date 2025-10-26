using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using NashAI_app.Model;
using Project_Manassas.Database;
using UglyToad.PdfPig;
using Pgvector;

namespace NashAI_app.Services.Ingestion;

public class DataIngestorVB
{
    private readonly ProjectContext _dbContext;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;

    public DataIngestorVB(
        ProjectContext dbContext,
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
    {
        _dbContext = dbContext;
        _embeddingGenerator = embeddingGenerator;
    }

    /// <summary>
    /// Splits text into chunks of a maximum character length.
    /// </summary>
    private static List<string> ChunkText(string text, int maxLength)
    {
        var chunks = new List<string>();
        for (int i = 0; i < text.Length; i += maxLength)
        {
            chunks.Add(text.Substring(i, Math.Min(maxLength, text.Length - i)));
        }
        return chunks;
    }

    /// <summary>
    /// Ingest all PDFs from a directory.
    /// </summary>
    public async Task IngestPdfsAsync(string pdfDirectory)
    {
        if (!Directory.Exists(pdfDirectory))
        {
            Console.WriteLine($"⚠️ PDF directory not found: {pdfDirectory}");
            return;
        }

        var pdfFiles = Directory.GetFiles(pdfDirectory, "*.pdf", SearchOption.AllDirectories);
        if (pdfFiles.Length == 0)
        {
            Console.WriteLine("📂 No PDFs found to ingest.");
            return;
        }

        Console.WriteLine($"📄 Found {pdfFiles.Length} PDFs to ingest...");

        foreach (var pdfPath in pdfFiles)
        {
            var fileName = Path.GetFileName(pdfPath);
            Console.WriteLine($"🔍 Processing {fileName}...");

            using var pdf = PdfDocument.Open(pdfPath);

            foreach (var page in pdf.GetPages())
            {
                var text = page.Text.Trim();
                if (string.IsNullOrWhiteSpace(text)) continue;
                
                // ✅ Check if this page is already embedded
                bool alreadyExists = await _dbContext.DocumentEmbeddings
                    .AnyAsync(e => e.DocumentId == fileName && e.PageNumber == page.Number);

                if (alreadyExists)
                {
                    Console.WriteLine($"⚠️ Skipping page {page.Number} of {fileName} (already embedded).");
                    continue;
                }
                
                var chunks = ChunkText(text, 1000); // adjust chunk size as needed

                foreach (var chunk in chunks)
                {
                    // Generate embedding
                    var embedding = await _embeddingGenerator.GenerateVectorAsync(chunk);
                    var vector = new Vector(embedding.ToArray());
                    // Create entity
                    var entity = new DocumentEmbeddingVB
                    {
                        DocumentId = fileName,
                        PageNumber = page.Number,
                        Content = chunk.Length > 250 ? chunk[..250] : chunk,
                        Embeddings = vector
                    };

                    _dbContext.DocumentEmbeddings.Add(entity);
                }

                // Save after each page to avoid large transactions
                await _dbContext.SaveChangesAsync();
                Console.WriteLine($"✅ Page {page.Number} of {fileName} ingested.");
            }
        }

        Console.WriteLine("📌 PDF ingestion completed.");
    }
}
