namespace EMR.Application.DTOs.Appointments;

public class AppointmentRescheduleDto
{
    public int AppointmentId { get; set; }
    public DateTime NewDate { get; set; }
    public TimeSpan NewStartTime { get; set; }
    public TimeSpan NewEndTime { get; set; }
}