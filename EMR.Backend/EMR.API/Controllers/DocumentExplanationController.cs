using System;
using System.Threading.Tasks;
using EMR.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EMR.API.Controllers;

[ApiController]
[Route("api/documents")]
public class DocumentExplanationController : ControllerBase
{
    private readonly IPatientDocumentService _documentService;
    private readonly ILogger<DocumentExplanationController> _logger;

    public DocumentExplanationController(
        IPatientDocumentService documentService,
        ILogger<DocumentExplanationController> logger)
    {
        _documentService = documentService;
        _logger = logger;
    }

    [HttpGet("{id}/explain")]
    public async Task<IActionResult> ExplainDocument(int id, [FromQuery] string language = "English", [FromQuery] string type = "DOC")
    {
        try
        {
            var result = await _documentService.ExplainDocumentAsync(id, language, type);
            return Ok(new { explanation = result.explanation, cached = result.cached });
        }
        catch (Exception ex)
        {
            if (ex.Message == "Document not found")
                return NotFound(new { message = ex.Message });
                
            if (ex.Message == "No extracted text found in this document to explain.")
                return BadRequest(new { message = ex.Message });

            _logger.LogError(ex, "Error explaining document {Id} in {Language}", id, language);
            return StatusCode(500, new { message = "An error occurred while generating explanation" });
        }
    }
}
