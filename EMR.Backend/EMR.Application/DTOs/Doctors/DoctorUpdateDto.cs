using System.ComponentModel.DataAnnotations;

namespace EMR.Application.DTOs.Doctors;

public class DoctorUpdateDto
{
    [Required]
    public int DoctorId { get; set; }

    [Required, MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public string Specialization { get; set; } = string.Empty;

    public string? Qualification { get; set; }

    [Range(0, 999999)]
    public decimal ConsultationFee { get; set; }

    [Range(0, 60)]
    public int ExperienceYears { get; set; }
}