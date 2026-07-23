using System.Collections.Generic;
using System.Threading.Tasks;
using EMR.Application.DTOs.Assessments;

namespace EMR.Application.Interfaces
{
    public interface IJointAssessmentService
    {
        Task<JointAssessmentDto> CreateAssessmentAsync(JointAssessmentDto assessmentDto);
        Task<List<JointAssessmentDto>> GetAssessmentsByPatientAsync(int patientId);
        Task<JointAssessmentDto?> GetLatestAssessmentByPatientAsync(int patientId);
    }
}
