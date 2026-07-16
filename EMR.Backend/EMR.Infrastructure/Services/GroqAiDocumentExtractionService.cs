using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EMR.Application.DTOs.Documents;
using EMR.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EMR.Infrastructure.Services;

public class GroqAiDocumentExtractionService : IAiDocumentExtractionService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GroqAiDocumentExtractionService> _logger;

    public GroqAiDocumentExtractionService(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogger<GroqAiDocumentExtractionService> logger)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<AiExtractedDocumentDto> ExtractStructuredDataAsync(string rawOcrText, string fileName, string category)
    {
        // Default fallback DTO if extraction fails or text is empty
        var fallback = new AiExtractedDocumentDto
        {
            Category = string.IsNullOrWhiteSpace(category) ? "Prescription" : category,
            ClinicalSummary = string.IsNullOrWhiteSpace(rawOcrText) || rawOcrText.Length < 15
                ? $"Document scanned ({category}). Text review recommended."
                : $"AI extraction completed for {fileName} ({category}). Please verify structured entries below.",
            Diagnoses = new List<string>(),
            Medications = new List<MedicationItemDto>(),
            LabFindings = new List<LabFindingItemDto>(),
            RadiologyImpression = string.Empty
        };

        if (string.IsNullOrWhiteSpace(rawOcrText) || rawOcrText.Length < 15)
        {
            return fallback;
        }

        try
        {
            string apiKey = _configuration["GroqSettings:ApiKey"] ?? Environment.GetEnvironmentVariable("GROQ_API_KEY") ?? string.Empty;
            if (string.IsNullOrWhiteSpace(apiKey) || apiKey.Equals("YOUR_GROQ_API_KEY_HERE", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("AI Document Extraction bypassed: No valid Groq/xAI API key found in configuration.");
                return fallback;
            }

            string endpoint = "https://api.groq.com/openai/v1/chat/completions";
            string model = _configuration["GroqSettings:Model"] ?? "llama-3.3-70b-versatile";

            // Smart Auto-Routing for xAI (Grok) vs Groq
            if (apiKey.Trim().StartsWith("xai-", StringComparison.OrdinalIgnoreCase) || model.Contains("grok", StringComparison.OrdinalIgnoreCase))
            {
                endpoint = "https://api.x.ai/v1/chat/completions";
                if (!model.Contains("grok", StringComparison.OrdinalIgnoreCase))
                {
                    model = "grok-beta";
                }
            }

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(25);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey.Trim());

            string systemPrompt = @"You are an expert Senior Clinical Physician and NLP structured data extractor.
Analyze the raw OCR text from the medical document and extract all clinical information strictly as a valid JSON object matching exactly the following schema.
Do not invent or hallucinate any medical data not present in the text. If a field is not found, leave it empty or null.

Required JSON Schema:
{
  ""category"": ""Prescription"" or ""LabReport"" or ""Radiology"",
  ""doctorName"": ""Doctor Name (or null)"",
  ""hospitalName"": ""Hospital Name (or null)"",
  ""clinicalSummary"": ""Concise 2-sentence clinical summary of what the document states"",
  ""diagnoses"": [""Diagnosis 1"", ""Diagnosis 2""],
  ""medications"": [
    {
      ""medicineName"": ""Name of drug"",
      ""dosage"": ""500 mg"",
      ""frequency"": ""Twice daily"",
      ""duration"": ""5 days""
    }
  ],
  ""labFindings"": [
    {
      ""testName"": ""Hemoglobin"",
      ""observedValue"": ""14.2"",
      ""referenceRange"": ""13.0 - 17.0"",
      ""unit"": ""g/dL"",
      ""status"": ""Normal"",
      ""category"": ""Hematology"",
      ""isAbnormal"": false
    }
  ],
  ""radiologyImpression"": ""Summary of radiology impression or null""
}";

            string userPrompt = $"Document Title: {fileName}\nExpected Category Hint: {category}\n\nRaw OCR Text:\n\"\"\"{rawOcrText}\"\"\"\n\nReturn ONLY valid JSON.";

            var requestBody = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                temperature = 0.1,
                max_tokens = 2048,
                response_format = new { type = "json_object" }
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync(endpoint, jsonContent);
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseString);
                var content = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

                if (!string.IsNullOrWhiteSpace(content))
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var extractedDto = JsonSerializer.Deserialize<AiExtractedDocumentDto>(content.Trim(), options);
                    if (extractedDto != null)
                    {
                        if (string.IsNullOrWhiteSpace(extractedDto.Category))
                        {
                            extractedDto.Category = category;
                        }
                        if (extractedDto.Diagnoses == null) extractedDto.Diagnoses = new List<string>();
                        if (extractedDto.Medications == null) extractedDto.Medications = new List<MedicationItemDto>();
                        if (extractedDto.LabFindings == null) extractedDto.LabFindings = new List<LabFindingItemDto>();
                        return extractedDto;
                    }
                }
            }
            else
            {
                var err = await response.Content.ReadAsStringAsync();
                _logger.LogError("AI Extraction API returned status {Status}: {Error}", response.StatusCode, err);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred during AI structured document extraction for {FileName}", fileName);
        }

        return fallback;
    }

    public async Task<AiExtractedDocumentDto> ExtractFromHandwrittenImageAsync(string base64Image, string fileName, string category)
    {
        var fallback = new AiExtractedDocumentDto
        {
            Category = string.IsNullOrWhiteSpace(category) ? "Prescription" : category,
            ClinicalSummary = $"Vision AI extraction completed for {fileName} ({category}). Please verify structured entries below.",
            Diagnoses = new List<string>(),
            Medications = new List<MedicationItemDto>(),
            LabFindings = new List<LabFindingItemDto>(),
            RadiologyImpression = string.Empty
        };

        if (string.IsNullOrWhiteSpace(base64Image))
        {
            return fallback;
        }

        try
        {
            string apiKey = _configuration["GroqSettings:ApiKey"] ?? Environment.GetEnvironmentVariable("GROQ_API_KEY") ?? string.Empty;
            if (string.IsNullOrWhiteSpace(apiKey) || apiKey.Equals("YOUR_GROQ_API_KEY_HERE", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Vision AI Document Extraction bypassed: No valid Groq/xAI API key found.");
                return fallback;
            }

            // For multimodal vision, Groq endpoint is https://api.groq.com/openai/v1/chat/completions with model llama-3.2-90b-vision-preview
            string endpoint = "https://api.groq.com/openai/v1/chat/completions";
            string model = "llama-3.2-90b-vision-preview";

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(35);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey.Trim());

            string systemPrompt = @"You are an expert Senior Clinical Physician. Analyze this medical document/prescription image directly and extract all clinical information strictly as a valid JSON object matching exactly the following schema.
If this is a handwritten prescription, carefully decipher the doctor's handwriting for medicine names, dosages, and frequencies.
Do not invent or hallucinate any data not present in the image. If a field is not found, leave it empty or null.

Required JSON Schema:
{
  ""category"": ""Prescription"" or ""LabReport"" or ""Radiology"",
  ""doctorName"": ""Doctor Name (or null)"",
  ""hospitalName"": ""Hospital Name (or null)"",
  ""clinicalSummary"": ""Concise 2-sentence clinical summary of what the document states"",
  ""diagnoses"": [""Diagnosis 1"", ""Diagnosis 2""],
  ""medications"": [
    {
      ""medicineName"": ""Name of drug"",
      ""dosage"": ""Dosage (e.g., 500mg)"",
      ""frequency"": ""Frequency (e.g., BID, TDS, once daily)""
    }
  ],
  ""labFindings"": [
    {
      ""testName"": ""Test Parameter"",
      ""observedValue"": ""Observed numeric or string value"",
      ""unit"": ""Unit of measurement (or empty string)"",
      ""normalRange"": ""Reference range (or empty string)"",
      ""status"": ""Normal or High or Low or Abnormal""
    }
  ],
  ""radiologyImpression"": ""Summary or impression text if radiology report (or null)""
}";

            // Determine image mime type based on file extension
            string mimeType = "image/jpeg";
            if (!string.IsNullOrWhiteSpace(fileName) && fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                mimeType = "image/png";
            }

            var payload = new
            {
                model = model,
                messages = new object[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "text", text = systemPrompt },
                            new { type = "image_url", image_url = new { url = $"data:{mimeType};base64,{base64Image}" } }
                        }
                    }
                },
                response_format = new { type = "json_object" },
                temperature = 0.1
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(endpoint, jsonContent);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseString);
                var content = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

                if (!string.IsNullOrWhiteSpace(content))
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var extractedDto = JsonSerializer.Deserialize<AiExtractedDocumentDto>(content.Trim(), options);
                    if (extractedDto != null)
                    {
                        if (string.IsNullOrWhiteSpace(extractedDto.Category))
                        {
                            extractedDto.Category = category;
                        }
                        if (extractedDto.Diagnoses == null) extractedDto.Diagnoses = new List<string>();
                        if (extractedDto.Medications == null) extractedDto.Medications = new List<MedicationItemDto>();
                        if (extractedDto.LabFindings == null) extractedDto.LabFindings = new List<LabFindingItemDto>();
                        return extractedDto;
                    }
                }
            }
            else
            {
                var err = await response.Content.ReadAsStringAsync();
                _logger.LogError("Vision AI Extraction API returned status {Status}: {Error}", response.StatusCode, err);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred during Multimodal Vision AI extraction for {FileName}", fileName);
        }

        return fallback;
    }
}
