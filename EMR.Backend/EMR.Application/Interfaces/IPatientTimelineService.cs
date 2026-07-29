using EMR.Application.DTOs.Patients;

namespace EMR.Application.Interfaces
{
    public interface IPatientTimelineService
    {
        Task<IEnumerable<TimelineEventDto>> GetTimelineByPatientIdAsync(int patientId);
    }
}
