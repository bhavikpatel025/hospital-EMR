namespace EMR.Application.DTOs.Patients
{
    public class TimelineEventDto
    {
        public string EventId { get; set; }
        public string EventType { get; set; } // "Appointment", "Prescription", "Assessment", "Document"
        public DateTime EventDate { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string AdditionalInfo { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
    }
}
