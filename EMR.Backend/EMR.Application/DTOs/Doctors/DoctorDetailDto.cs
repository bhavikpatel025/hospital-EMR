namespace EMR.Application.DTOs.Doctors;

public class DoctorDetailDto
{
    public int DoctorId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string? Qualification { get; set; }
    public decimal ConsultationFee { get; set; }
    public int ExperienceYears { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}