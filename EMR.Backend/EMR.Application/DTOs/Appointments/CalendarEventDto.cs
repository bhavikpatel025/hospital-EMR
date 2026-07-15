namespace EMR.Application.DTOs.Appointments;

public class CalendarEventDto
{
    public int AppointmentId { get; set; }
    public string Title { get; set; } = string.Empty;   // "Raj Patel - Dr. Meera Shah"
    public DateTime Start { get; set; }                 // FullCalendar ISO format expects this
    public DateTime End { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;   // Status-based color
}