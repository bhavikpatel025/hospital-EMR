using System;

namespace EMR.Domain.Entities;

public class PatientLabFinding
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string TestName { get; set; } = string.Empty;
    public string ObservedValue { get; set; } = string.Empty;
    public string ReferenceRange { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsAbnormal { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Patient? Patient { get; set; }
}
