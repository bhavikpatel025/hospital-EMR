using System;

namespace EMR.Application.DTOs.Documents;

public class LabFindingItemDto
{
    public string TestName { get; set; } = string.Empty;
    public string ObservedValue { get; set; } = string.Empty;
    public string ReferenceRange { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsAbnormal { get; set; }
}
