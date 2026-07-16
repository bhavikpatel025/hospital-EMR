using System.Threading.Tasks;
using EMR.Application.DTOs.Documents;

namespace EMR.Application.Interfaces;

public interface IAiDocumentExtractionService
{
    Task<AiExtractedDocumentDto> ExtractStructuredDataAsync(string rawOcrText, string fileName, string category);
    Task<AiExtractedDocumentDto> ExtractFromHandwrittenImageAsync(string base64Image, string fileName, string category);
}
