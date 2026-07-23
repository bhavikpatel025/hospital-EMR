using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMR.Application.Interfaces;
using EMR.Domain.Entities;
using EMR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EMR.Infrastructure.Repositories
{
    public class JointAssessmentRepository : IJointAssessmentRepository
    {
        private readonly AppDbContext _context;

        public JointAssessmentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<JointAssessment> AddAsync(JointAssessment assessment)
        {
            _context.JointAssessments.Add(assessment);
            await _context.SaveChangesAsync();
            return assessment;
        }

        public async Task<List<JointAssessment>> GetByPatientIdAsync(int patientId)
        {
            return await _context.JointAssessments
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.AssessmentDate)
                .ToListAsync();
        }

        public async Task<JointAssessment?> GetLatestByPatientIdAsync(int patientId)
        {
            return await _context.JointAssessments
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.AssessmentDate)
                .FirstOrDefaultAsync();
        }
    }
}
