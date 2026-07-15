using System;

namespace EMR.Domain.Entities;

public class PatientMedication
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public string? Duration { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Patient? Patient { get; set; }
}
