using System;
using System.ComponentModel.DataAnnotations;

namespace EMR.Application.DTOs.Appointments;

public class PublicAppointmentCreateDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, Phone, MaxLength(20)]
    public string Mobile { get; set; } = string.Empty;

    [Required]
    public int DoctorId { get; set; }

    [Required]
    public DateTime AppointmentDate { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    public string? Reason { get; set; }
}
