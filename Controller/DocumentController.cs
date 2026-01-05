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
        
        

        await using (var fileStream = new FileStream(tempPath, FileMode.Create))
        {
            await formFile.CopyToAsync(fileStream);
        }
        
        _logger.LogInformation("PDF upload completed: {FileName}", formFile.FileName);
        
        _logger.LogInformation("Received formFile: {HasFile}, documentId: {DocumentId}", 
            formFile != null, documentId);
        
        await _pdfingestionService.IngestPdfAsync(tempPath, documentId);
        
        // Persist PDF for preview
        var pdfStorage = Path.Combine(
            Directory.GetCurrentDirectory(),
            "PdfStorage");
        
        Directory.CreateDirectory(pdfStorage);

        var finalPath = Path.Combine(
            pdfStorage,
            Path.GetFileName(documentId)
        );
        
        System.IO.File.Copy(tempPath, finalPath, overwrite: true);
        System.IO.File.Delete(tempPath);
        
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
        var safeName = Path.GetFileName(documentId);

        if (!safeName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Pdf file name must end with '.pdf'");
        }
        
        var path = Path.Combine("PdfStorage", safeName);

        if (!System.IO.File.Exists(path))
        {
            return NotFound();
        }

        return File(
            new FileStream(path, FileMode.Open, FileAccess.Read),
            "application/pdf");
    }
    

    [HttpDelete(ApiEndPoints.Pdfs.DELETE_PDF)]
    public async Task<IActionResult> DeletePdfAsync([FromRoute] string documentId)
    {
        var deletedCount = await _pdfingestionService.DeleteByDocumentIdAsync(documentId);

        if (deletedCount == 0)
        {
            return NotFound(new { Message = "Document not found" });
        }
        
        // Delete Stored PDF
        var path = Path.Combine("PdfStorage", documentId);
        if (System.IO.File.Exists(path))
            System.IO.File.Delete(path);

        return Ok(new
        {
            Message = "PDF deleted successfully",
            DocumentId = documentId,
            DeletedCounts = deletedCount
        });
    }
}