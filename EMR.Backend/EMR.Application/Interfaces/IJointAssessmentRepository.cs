using System.Collections.Generic;
using System.Threading.Tasks;
using EMR.Domain.Entities;

namespace EMR.Application.Interfaces
{
    public interface IJointAssessmentRepository
    {
        Task<JointAssessment> AddAsync(JointAssessment assessment);
        Task<List<JointAssessment>> GetByPatientIdAsync(int patientId);
        Task<JointAssessment?> GetLatestByPatientIdAsync(int patientId);
    }
}
