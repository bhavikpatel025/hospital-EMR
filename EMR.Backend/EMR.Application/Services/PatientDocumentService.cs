using System;
using System.Text;
using System.Threading.Tasks;
using EMR.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace EMR.Application.Services;

public class PatientDocumentService : IPatientDocumentService
{
    private readonly IPatientDocumentRepository _repository;
    private readonly IPrescriptionRepository _prescriptionRepository;
    private readonly IAiDocumentExtractionService _aiService;
    private readonly ILogger<PatientDocumentService> _logger;

    public PatientDocumentService(
        IPatientDocumentRepository repository,
        IPrescriptionRepository prescriptionRepository,
        IAiDocumentExtractionService aiService,
        ILogger<PatientDocumentService> logger)
    {
        _repository = repository;
        _prescriptionRepository = prescriptionRepository;
        _aiService = aiService;
        _logger = logger;
    }

    public async Task<(string explanation, bool cached)> ExplainDocumentAsync(int documentId, string language, string type = "DOC")
    {
        if (type.Equals("RX", StringComparison.OrdinalIgnoreCase))
        {
            return await ExplainPrescriptionAsync(documentId, language);
        }

        var document = await _repository.GetByIdAsync(documentId);
        
        if (document == null)
            throw new Exception("Document not found");

        if (string.IsNullOrWhiteSpace(document.RawTextSummary))
            throw new Exception("No extracted text found in this document to explain.");

        // Check cache based on language
        string cachedExplanation = "";
        switch (language.ToLower())
        {
            case "hindi":
                cachedExplanation = document.AiExplanationHindi;
                break;
            case "gujarati":
                cachedExplanation = document.AiExplanationGujarati;
                break;
            default:
                cachedExplanation = document.AiExplanationEnglish;
                break;
        }

        if (!string.IsNullOrWhiteSpace(cachedExplanation))
        {
            return (cachedExplanation, true);
        }

        // Not cached, call AI
        string explanation = await _aiService.ExplainDocumentAsync(document.RawTextSummary, language);

        // Save back to cache
        switch (language.ToLower())
        {
            case "hindi":
                document.AiExplanationHindi = explanation;
                break;
            case "gujarati":
                document.AiExplanationGujarati = explanation;
                break;
            default:
                document.AiExplanationEnglish = explanation;
                break;
        }

        await _repository.UpdateAsync(document);

        return (explanation, false);
    }

    private async Task<(string explanation, bool cached)> ExplainPrescriptionAsync(int prescriptionId, string language)
    {
        var rx = await _prescriptionRepository.GetByIdAsync(prescriptionId);
        if (rx == null)
            throw new Exception("Prescription not found");

        string cachedExplanation = "";
        switch (language.ToLower())
        {
            case "hindi":
                cachedExplanation = rx.AiExplanationHindi;
                break;
            case "gujarati":
                cachedExplanation = rx.AiExplanationGujarati;
                break;
            default:
                cachedExplanation = rx.AiExplanationEnglish;
                break;
        }

        if (!string.IsNullOrWhiteSpace(cachedExplanation))
            return (cachedExplanation, true);

        // Build a raw text summary from the prescription object
        var sb = new StringBuilder();
        sb.AppendLine($"Diagnosis: {rx.Diagnosis}");
        sb.AppendLine($"Chief Complaints: {rx.ChiefComplaints}");
        if (rx.Medications != null)
        {
            sb.AppendLine("Medications:");
            foreach (var med in rx.Medications)
            {
                sb.AppendLine($"- {med.MedicineName} ({med.Dosage}, {med.Frequency} for {med.Duration})");
            }
        }
        sb.AppendLine($"Advice/Guidelines: {rx.Guidelines}");
        
        string rawText = sb.ToString();

        string explanation = await _aiService.ExplainDocumentAsync(rawText, language);

        switch (language.ToLower())
        {
            case "hindi":
                rx.AiExplanationHindi = explanation;
                break;
            case "gujarati":
                rx.AiExplanationGujarati = explanation;
                break;
            default:
                rx.AiExplanationEnglish = explanation;
                break;
        }

        await _prescriptionRepository.UpdateAsync(rx);

        return (explanation, false);
    }
}
