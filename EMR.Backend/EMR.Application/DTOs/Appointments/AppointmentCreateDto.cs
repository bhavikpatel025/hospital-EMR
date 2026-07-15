using System.ComponentModel.DataAnnotations;

namespace EMR.Application.DTOs.Appointments;

public class AppointmentCreateDto
{
    [Required]
    public int PatientId { get; set; }

    [Required]
    public int DoctorId { get; set; }

    [Required]
    public DateTime AppointmentDate { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    public string? Reason { get; set; }
    public string? Notes { get; set; }
}