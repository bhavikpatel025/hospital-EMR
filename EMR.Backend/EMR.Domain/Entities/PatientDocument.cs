using System;

namespace EMR.Domain.Entities;

public class PatientDocument
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string Category { get; set; } = string.Empty; // Prescription, LabReport, Radiology
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string RawTextSummary { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public virtual Patient? Patient { get; set; }
}
