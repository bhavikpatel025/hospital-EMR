using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using EMR.Application.DTOs.Documents;
using EMR.Application.Interfaces;
using EMR.Domain.Entities;
using EMR.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Tesseract;
using UglyToad.PdfPig;

namespace EMR.API.Controllers;

[ApiController]
[Route("api/ai-extraction")]
public class AiDocumentExtractionController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IAiDocumentExtractionService _aiExtractionService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<AiDocumentExtractionController> _logger;
    private readonly string _uploadRootDirectory;
    private readonly ICloudStorageService _cloudStorageService;

    public AiDocumentExtractionController(
        AppDbContext context,
        IAiDocumentExtractionService aiExtractionService,
        IWebHostEnvironment environment,
        ILogger<AiDocumentExtractionController> logger,
        ICloudStorageService cloudStorageService = null)
    {
        _context = context;
        _aiExtractionService = aiExtractionService;
        _environment = environment;
        _logger = logger;
        _cloudStorageService = cloudStorageService;

        _uploadRootDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "medical_records");
        if (!Directory.Exists(_uploadRootDirectory))
        {
            Directory.CreateDirectory(_uploadRootDirectory);
        }
    }

    [HttpPost("{patientId}/extract")]
    public async Task<IActionResult> ExtractStructuredDataWithAiAsync(int patientId, [FromForm] DocumentUploadRequestDto request)
    {
        var file = request?.File;
        var category = request?.Category;

        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No valid medical document or image was uploaded." });
        }

        var docCategory = string.IsNullOrWhiteSpace(category) || category.Equals("null", StringComparison.OrdinalIgnoreCase) || category.Equals("undefined", StringComparison.OrdinalIgnoreCase)
            ? "Prescription"
            : category;

        // 1. Save file temporarily for OCR
        var tempDirectory = Path.GetTempPath();
        var uniqueFileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{file.FileName}";
        var tempFilePath = Path.Combine(tempDirectory, uniqueFileName);

        using (var stream = new FileStream(tempFilePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // 2. Perform OCR Text Extraction (Option B: self-contained duplicate of OCR helper)
        string rawExtractedText = await ExtractTextWithTesseractOrPdfAsync(tempFilePath, file.FileName);

        // 3. Smart Handwriting Auto-Detection:
        // If the file is an image (.jpg/.jpeg/.png) AND Tesseract extracted very little/empty text (< 30 chars),
        // automatically trigger Multimodal Vision AI (`llama-3.2-90b-vision-preview`) using the physical file!
        AiExtractedDocumentDto aiDto;
        bool isImageFile = file.FileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                           file.FileName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                           file.FileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase);

        if (isImageFile && (string.IsNullOrWhiteSpace(rawExtractedText) || rawExtractedText.Trim().Length < 30))
        {
            byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(tempFilePath);
            string base64Image = Convert.ToBase64String(fileBytes);
            aiDto = await _aiExtractionService.ExtractFromHandwrittenImageAsync(base64Image, file.FileName, docCategory);
        }
        else
        {
            // 4. Run standard Text AI Structured Extraction (`llama-3.3-70b-versatile` / `grok-beta`)
            aiDto = await _aiExtractionService.ExtractStructuredDataAsync(rawExtractedText, file.FileName, docCategory);
        }

        if (!string.IsNullOrWhiteSpace(aiDto.Category))
        {
            docCategory = aiDto.Category;
        }

        // 4. Upload file to Cloudinary for permanent scalable storage
        string cloudinaryUrl = "";
        try
        {
            cloudinaryUrl = await _cloudStorageService.UploadFileAsync(tempFilePath, file.FileName, $"patient_{patientId}");
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to upload document to Cloudinary: {ex.Message}");
        }
        finally
        {
            if (System.IO.File.Exists(tempFilePath))
            {
                System.IO.File.Delete(tempFilePath);
            }
        }

        // 5. Create database record for the uploaded document (`PatientDocuments`)
        var patientDoc = new PatientDocument
        {
            PatientId = patientId,
            Category = docCategory,
            FileName = file.FileName,
            FilePath = cloudinaryUrl,
            RawTextSummary = aiDto.ClinicalSummary ?? $"AI document summary generated for {docCategory}",
            UploadedAt = DateTime.UtcNow
        };

        _context.PatientDocuments.Add(patientDoc);
        await _context.SaveChangesAsync();

        string? fileUrl = cloudinaryUrl;

        // 6. Return response matching exact frontend expectations (`ExtractedMedicalDataDto`)
        return Ok(new
        {
            documentId = patientDoc.Id,
            category = docCategory,
            documentTitle = file.FileName,
            extractedDate = DateTime.Now.ToString("yyyy-MM-dd"),
            fileUrl,
            doctorName = aiDto.DoctorName,
            hospitalName = aiDto.HospitalName,
            diagnoses = aiDto.Diagnoses,
            medications = aiDto.Medications,
            labFindings = aiDto.LabFindings,
            radiologyImpression = aiDto.RadiologyImpression,
            rawTextSummary = aiDto.ClinicalSummary,
            rawOcrText = rawExtractedText
        });
    }

    private string _patientFolder(string path) => path;

    // ==================================================================================
    // SELF-CONTAINED REAL OCR TEXT EXTRACTION (Per Option B Decision)
    // ==================================================================================

    private async Task<string> ExtractTextWithTesseractOrPdfAsync(string filePath, string fileName)
    {
        try
        {
            if (fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                var textBuilder = new StringBuilder();
                using (var document = UglyToad.PdfPig.PdfDocument.Open(filePath))
                {
                    foreach (var page in document.GetPages())
                    {
                        textBuilder.AppendLine(page.Text);
                    }
                }

                var pdfText = textBuilder.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(pdfText) && pdfText.Length > 10)
                {
                    return pdfText;
                }

                var tessDataPath = Path.Combine(Directory.GetCurrentDirectory(), "tessdata");
                if (Directory.Exists(tessDataPath))
                {
                    using var engine = new TesseractEngine(tessDataPath, "eng", EngineMode.Default);
                    engine.SetVariable("tessedit_char_whitelist",
                        "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789.,;:/-()%@ '\"\n\r\t");
                    engine.SetVariable("preserve_interword_spaces", "1");

                    using var document = UglyToad.PdfPig.PdfDocument.Open(filePath);
                    foreach (var page in document.GetPages())
                    {
                        foreach (var img in page.GetImages())
                        {
                            try
                            {
                                if (img.TryGetPng(out var pngBytes) && pngBytes != null && pngBytes.Length > 0)
                                {
                                    var tempPath = Path.Combine(Path.GetTempPath(), $"ocr_pdf_{Guid.NewGuid()}.png");
                                    await System.IO.File.WriteAllBytesAsync(tempPath, pngBytes);
                                    try
                                    {
                                        using var pix = Pix.LoadFromFile(tempPath);
                                        if (pix != null)
                                        {
                                            using var grayImg = pix.ConvertRGBToGray() ?? pix;
                                            using var pageOcr = engine.Process(grayImg);
                                            textBuilder.AppendLine(pageOcr.GetText());
                                        }
                                    }
                                    finally
                                    {
                                        if (System.IO.File.Exists(tempPath)) System.IO.File.Delete(tempPath);
                                    }
                                }
                                else if (img.RawBytes != null && img.RawBytes.Count > 0)
                                {
                                    var tempPath = Path.Combine(Path.GetTempPath(), $"ocr_pdf_{Guid.NewGuid()}.jpg");
                                    await System.IO.File.WriteAllBytesAsync(tempPath, img.RawBytes.ToArray());
                                    try
                                    {
                                        using var pix = Pix.LoadFromFile(tempPath);
                                        if (pix != null)
                                        {
                                            using var grayImg = pix.ConvertRGBToGray() ?? pix;
                                            using var pageOcr = engine.Process(grayImg);
                                            textBuilder.AppendLine(pageOcr.GetText());
                                        }
                                    }
                                    catch
                                    {
                                        // Ignore non-JPEG raw bytes
                                    }
                                    finally
                                    {
                                        if (System.IO.File.Exists(tempPath)) System.IO.File.Delete(tempPath);
                                    }
                                }
                            }
                            catch (Exception imgEx)
                            {
                                _logger.LogWarning("PDF Image OCR Error: {Message}", imgEx.Message);
                            }
                        }
                    }
                }

                pdfText = textBuilder.ToString().Trim();
                return pdfText;
            }
            else if (fileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                return await System.IO.File.ReadAllTextAsync(filePath);
            }
            else
            {
                var tessDataPath = Path.Combine(Directory.GetCurrentDirectory(), "tessdata");
                if (!Directory.Exists(tessDataPath))
                {
                    return string.Empty;
                }

                using var engine = new TesseractEngine(tessDataPath, "eng", EngineMode.Default);
                engine.SetVariable("tessedit_char_whitelist",
                    "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789.,;:/-()%@ '\"\n\r\t");
                engine.SetVariable("preserve_interword_spaces", "1");

                using var img = Pix.LoadFromFile(filePath);
                using var grayImg = img.ConvertRGBToGray();
                Pix? binarizedImg = null;
                try
                {
                    if (grayImg != null)
                    {
                        binarizedImg = grayImg.BinarizeOtsuAdaptiveThreshold(200, 200, 0, 0, 0.0f);
                    }
                }
                catch
                {
                    // Ignore binarization failure
                }

                var processImg = binarizedImg ?? grayImg ?? img!;
                using var page = engine.Process(processImg);

                var confidence = page.GetMeanConfidence();
                var text = page.GetText()?.Trim() ?? string.Empty;

                if ((string.IsNullOrWhiteSpace(text) || text.Length < 10 || confidence < 0.3))
                {
                    using var fallbackPage = engine.Process(img);
                    var fallbackText = fallbackPage.GetText()?.Trim() ?? string.Empty;
                    if (fallbackText.Length > text.Length)
                    {
                        text = fallbackText;
                    }
                }

                binarizedImg?.Dispose();
                return text;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OCR text extraction failed for {FileName}", fileName);
            return string.Empty;
        }
    }
}
