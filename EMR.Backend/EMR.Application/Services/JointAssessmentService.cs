using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMR.Application.DTOs.Assessments;
using EMR.Application.Interfaces;
using EMR.Domain.Entities;

namespace EMR.Application.Services
{
    public class JointAssessmentService : IJointAssessmentService
    {
        private readonly IJointAssessmentRepository _repository;

        public JointAssessmentService(IJointAssessmentRepository repository)
        {
            _repository = repository;
        }

        public async Task<JointAssessmentDto> CreateAssessmentAsync(JointAssessmentDto dto)
        {
            var assessment = new JointAssessment
            {
                PatientId = dto.PatientId,
                AppointmentId = dto.AppointmentId,
                AssessmentDate = dto.AssessmentDate,
                JointsDataJson = dto.JointsDataJson,
                Notes = dto.Notes,
                TotalTender = dto.TotalTender,
                TotalSwollen = dto.TotalSwollen,
                TotalBoth = dto.TotalBoth,
                TotalLimited = dto.TotalLimited,
                TotalNormal = dto.TotalNormal,
                TotalJointsAssessed = dto.TotalJointsAssessed
            };

            var savedAssessment = await _repository.AddAsync(assessment);

            dto.Id = savedAssessment.Id;
            return dto;
        }

        public async Task<List<JointAssessmentDto>> GetAssessmentsByPatientAsync(int patientId)
        {
            var assessments = await _repository.GetByPatientIdAsync(patientId);
            
            return assessments.Select(a => new JointAssessmentDto
            {
                Id = a.Id,
                PatientId = a.PatientId,
                AppointmentId = a.AppointmentId,
                AssessmentDate = a.AssessmentDate,
                JointsDataJson = a.JointsDataJson,
                Notes = a.Notes,
                TotalTender = a.TotalTender,
                TotalSwollen = a.TotalSwollen,
                TotalBoth = a.TotalBoth,
                TotalLimited = a.TotalLimited,
                TotalNormal = a.TotalNormal,
                TotalJointsAssessed = a.TotalJointsAssessed
            }).ToList();
        }

        public async Task<JointAssessmentDto?> GetLatestAssessmentByPatientAsync(int patientId)
        {
            var latest = await _repository.GetLatestByPatientIdAsync(patientId);

            if (latest == null) return null;

            return new JointAssessmentDto
            {
                Id = latest.Id,
                PatientId = latest.PatientId,
                AppointmentId = latest.AppointmentId,
                AssessmentDate = latest.AssessmentDate,
                JointsDataJson = latest.JointsDataJson,
                Notes = latest.Notes,
                TotalTender = latest.TotalTender,
                TotalSwollen = latest.TotalSwollen,
                TotalBoth = latest.TotalBoth,
                TotalLimited = latest.TotalLimited,
                TotalNormal = latest.TotalNormal,
                TotalJointsAssessed = latest.TotalJointsAssessed
            };
        }
    }
}
