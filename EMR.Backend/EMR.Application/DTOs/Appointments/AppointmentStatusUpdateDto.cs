using EMR.Domain.Enums;

namespace EMR.Application.DTOs.Appointments;

public class AppointmentStatusUpdateDto
{
    public int AppointmentId { get; set; }
    public AppointmentStatus Status { get; set; }
}