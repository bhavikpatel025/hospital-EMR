using Microsoft.AspNetCore.Http;

namespace EMR.Application.DTOs.Documents;

public class DocumentUploadRequestDto
{
    public IFormFile? File { get; set; }
    public string? Category { get; set; }
}
