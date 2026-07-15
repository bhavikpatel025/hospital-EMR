namespace EMR.Application.DTOs.Appointments;

public class AppointmentUpdateDto : AppointmentCreateDto
{
    [System.ComponentModel.DataAnnotations.Required]
    public int AppointmentId { get; set; }
}