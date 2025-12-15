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
        
        System.IO.File.Delete(tempPath);
        
        return Ok(new
        {
            Message = "PDF uploaded successfully.",
            FileName = formFile?.FileName,
            DocumentId = documentId
        });
    }
}