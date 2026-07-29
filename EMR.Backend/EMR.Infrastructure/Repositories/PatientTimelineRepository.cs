using EMR.Application.DTOs.Patients;
using EMR.Application.Interfaces;
using EMR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EMR.Infrastructure.Repositories
{
    public class PatientTimelineRepository : IPatientTimelineRepository
    {
        private readonly AppDbContext _context;

        public PatientTimelineRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TimelineEventDto>> GetTimelineByPatientIdAsync(int patientId)
        {
            var timelineEvents = new List<TimelineEventDto>();

            // 1. Get Appointments
            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Where(a => a.PatientId == patientId)
                .ToListAsync();

            foreach (var appt in appointments)
            {
                timelineEvents.Add(new TimelineEventDto
                {
                    EventId = $"APT_{appt.AppointmentId}",
                    EventType = "Appointment",
                    EventDate = appt.AppointmentDate,
                    Title = $"Consultation with Dr. {(appt.Doctor?.User?.FullName ?? "Doctor")}",
                    Description = $"Status: {appt.Status}. Reason: {appt.Reason}",
                    Icon = "pi pi-calendar",
                    Color = "#3b82f6" // blue
                });
            }

            // 2. Get Prescriptions
            var prescriptions = await _context.Prescriptions
                .Include(p => p.Medications)
                .Where(p => p.PatientId == patientId)
                .ToListAsync();

            foreach (var rx in prescriptions)
            {
                var meds = rx.Medications?.Select(m => m.MedicineName).ToList() ?? new List<string>();
                timelineEvents.Add(new TimelineEventDto
                {
                    EventId = $"RX_{rx.PrescriptionId}",
                    EventType = "Prescription",
                    EventDate = rx.CreatedAt,
                    Title = "E-Prescription Issued",
                    Description = meds.Any() ? $"Prescribed: {string.Join(", ", meds)}" : "No medications prescribed.",
                    Icon = "pi pi-file-edit",
                    Color = "#ef4444" // red
                });
            }

            // 3. Get Joint Assessments
            var assessments = await _context.JointAssessments
                .Where(j => j.PatientId == patientId)
                .ToListAsync();

            foreach (var asst in assessments)
            {
                timelineEvents.Add(new TimelineEventDto
                {
                    EventId = $"AST_{asst.Id}",
                    EventType = "Assessment",
                    EventDate = asst.AssessmentDate,
                    Title = "Joint Assessment Completed",
                    Description = $"Tender: {asst.TotalTender}, Swollen: {asst.TotalSwollen}, Both: {asst.TotalBoth}, Limited: {asst.TotalLimited}",
                    Icon = "pi pi-user",
                    Color = "#14b8a6" // teal
                });
            }

            // 4. Get Documents (Labs / Radiology)
            var documents = await _context.PatientDocuments
                .Where(d => d.PatientId == patientId)
                .ToListAsync();

            foreach (var doc in documents)
            {
                timelineEvents.Add(new TimelineEventDto
                {
                    EventId = $"DOC_{doc.Id}",
                    EventType = doc.Category,
                    EventDate = doc.UploadedAt,
                    Title = $"{doc.Category} Document Uploaded",
                    Description = $"File: {doc.FileName}",
                    Icon = doc.Category == "LabReport" ? "pi pi-chart-line" : "pi pi-folder",
                    Color = doc.Category == "LabReport" ? "#22c55e" : "#f59e0b" // green or yellow
                });
            }

            // Sort all events by date descending
            return timelineEvents.OrderByDescending(e => e.EventDate);
        }
    }
}
