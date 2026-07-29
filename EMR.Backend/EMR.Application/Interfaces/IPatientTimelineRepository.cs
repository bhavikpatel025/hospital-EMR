using EMR.Application.DTOs.Patients;

namespace EMR.Application.Interfaces
{
    public interface IPatientTimelineRepository
    {
        Task<IEnumerable<TimelineEventDto>> GetTimelineByPatientIdAsync(int patientId);
    }
}
