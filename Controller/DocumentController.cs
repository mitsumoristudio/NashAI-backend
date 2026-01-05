using Microsoft.AspNetCore.Mvc;
using NashAI_app.Services;
using NashAI_app.utils;

namespace NashAI_app.Controller;

[ApiController]
[Route(ApiEndPoints.Pdfs.PdfBase)]
public class DocumentController: ControllerBase
{
    private readonly PDFIngestionService _pdfingestionService;
    private readonly ILogger<DocumentController> _logger;

    public DocumentController(PDFIngestionService pdfingestionService, ILogger<DocumentController> logger)
    {
        _pdfingestionService = pdfingestionService;
        _logger = logger;
    }

    [HttpPost(ApiEndPoints.Pdfs.INGEST_PDF)]
    [RequestSizeLimit(50_000_000)] // 50MB Limit
    public async Task<IActionResult> IngestPdfAsync([FromForm] IFormFile formFile, [FromForm] string documentId)
    {
        if (formFile == null || formFile.Length == 0)
        {
            return BadRequest("No PDF file was uploaded.");
        }
        if (string.IsNullOrWhiteSpace(documentId))
            
            documentId = Guid.NewGuid().ToString();

        var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_{formFile.FileName}");
        
        // Read PDF bytes
        byte[] pdfBytes;
        using (var stream = new MemoryStream())
        {
            await formFile.CopyToAsync(stream);
            pdfBytes = stream.ToArray();
        }
        
        await using (var fileStream = new FileStream(tempPath, FileMode.Create))
        {
            await formFile.CopyToAsync(fileStream);
        }
        
        _logger.LogInformation("PDF upload completed: {FileName}", formFile.FileName);
        
        _logger.LogInformation("Received formFile: {HasFile}, documentId: {DocumentId}", 
            formFile != null, documentId);
        
        // Store PDF binary
        await _pdfingestionService.SavePdfFileAsync(
            documentId: documentId,
            fileName: formFile.FileName,
            data: pdfBytes);
        
        // Ingest text + Embedding
        await _pdfingestionService.IngestPdfAsync(tempPath, documentId);
        
        return Ok(new
        {
            Message = "PDF uploaded successfully.",
            FileName = formFile?.FileName,
            DocumentId = documentId
        });
    }

    [HttpGet(ApiEndPoints.Pdfs.LIST_PDF)]
    public async Task<IActionResult> ListIngestedPdfs()
    {
        var results = await _pdfingestionService.GetIngestedPdfsAsync();
        return Ok(results);
    }

    [HttpGet(ApiEndPoints.Pdfs.PREVIEW_PDF)]
    public async Task<IActionResult> PreviewPdf(string documentId)
    {
      var pdfBytes = await _pdfingestionService.GetPdfAsync(documentId);
      
      if (pdfBytes == null)
      {
          return NotFound();
      }
      return File(pdfBytes, "application/pdf", $"{documentId}.pdf");
    }

    [HttpDelete(ApiEndPoints.Pdfs.DELETE_PDF)]
    public async Task<IActionResult> DeletePdf([FromRoute] string documentId)
    {
        var deleteCount = await _pdfingestionService.DeleteByDocumentPdfAsync(documentId);

        if (deleteCount == 0)
        {
            return NotFound(new {Message = $"Document {documentId} was not found."});
        }
        
        return Ok( new
        {
            Message = "PDF was deleted successfully",
            DocumentId = documentId,
            DeleteCount = deleteCount
        });
    }
    
    
    [HttpDelete(ApiEndPoints.Pdfs.DELETEEMBEDDING_PDF)]
    public async Task<IActionResult> DeleteEmbeddingPdfAsync([FromRoute] string documentId)
    {
        var deletedCount = await _pdfingestionService.DeleteByDocumentEmbeddingIdAsync(documentId);

        if (deletedCount == 0)
        {
            return NotFound(new { Message = "Document not found" });
        }
        
        return Ok(new
        {
            Message = "PDF embedding was deleted successfully",
            DocumentId = documentId,
            DeletedCounts = deletedCount
        });
    }
}