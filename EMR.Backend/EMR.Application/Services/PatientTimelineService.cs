using EMR.Application.DTOs.Patients;
using EMR.Application.Interfaces;

namespace EMR.Application.Services
{
    public class PatientTimelineService : IPatientTimelineService
    {
        private readonly IPatientTimelineRepository _repository;

        public PatientTimelineService(IPatientTimelineRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<TimelineEventDto>> GetTimelineByPatientIdAsync(int patientId)
        {
            return await _repository.GetTimelineByPatientIdAsync(patientId);
        }
    }
}


