using System;

namespace EMR.Domain.Entities;

public class PatientRadiologyNote
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string ImpressionText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Patient? Patient { get; set; }
}
