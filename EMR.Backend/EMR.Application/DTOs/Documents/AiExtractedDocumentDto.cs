using System.Collections.Generic;

namespace EMR.Application.DTOs.Documents;

public class AiExtractedDocumentDto
{
    public string Category { get; set; } = string.Empty;
    public string? DoctorName { get; set; }
    public string? HospitalName { get; set; }
    public string? ClinicalSummary { get; set; }
    public List<string> Diagnoses { get; set; } = new List<string>();
    public List<MedicationItemDto> Medications { get; set; } = new List<MedicationItemDto>();
    public List<LabFindingItemDto> LabFindings { get; set; } = new List<LabFindingItemDto>();
    public string? RadiologyImpression { get; set; }
}
