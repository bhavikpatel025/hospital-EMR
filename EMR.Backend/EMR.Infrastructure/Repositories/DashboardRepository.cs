using EMR.Application.DTOs.Dashboard;
using EMR.Application.Interfaces;
using EMR.Domain.Enums;
using EMR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EMR.Infrastructure.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly AppDbContext _context;

        public DashboardRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardAnalyticsDto> GetAnalyticsAsync()
        {
            var result = new DashboardAnalyticsDto();

            // 1. Appointments by Status
            var statuses = await _context.Appointments
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var status in statuses)
            {
                result.AppointmentsByStatus.Add(new ChartDataDto
                {
                    Label = status.Status.ToString(),
                    Value = status.Count
                });
            }

            // 2. Patients by Gender
            var genders = await _context.Patients
                .GroupBy(p => p.Gender)
                .Select(g => new { Gender = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var gender in genders)
            {
                result.PatientsByGender.Add(new ChartDataDto
                {
                    Label = string.IsNullOrWhiteSpace(gender.Gender) ? "Unknown" : gender.Gender,
                    Value = gender.Count
                });
            }

            // 3. Patients by Age Group
            var patients = await _context.Patients.Select(p => p.Age).ToListAsync();
            
            result.PatientsByAgeGroup.Add(new ChartDataDto { Label = "0-18", Value = patients.Count(a => a <= 18) });
            result.PatientsByAgeGroup.Add(new ChartDataDto { Label = "19-35", Value = patients.Count(a => a >= 19 && a <= 35) });
            result.PatientsByAgeGroup.Add(new ChartDataDto { Label = "36-50", Value = patients.Count(a => a >= 36 && a <= 50) });
            result.PatientsByAgeGroup.Add(new ChartDataDto { Label = "51-65", Value = patients.Count(a => a >= 51 && a <= 65) });
            result.PatientsByAgeGroup.Add(new ChartDataDto { Label = "65+", Value = patients.Count(a => a > 65) });

            // 4. Appointments by Doctor
            var doctors = await _context.Appointments
                .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
                .GroupBy(a => a.Doctor.User.FullName)
                .Select(g => new { DoctorName = g.Key, ApptCount = g.Count() })
                .OrderByDescending(x => x.ApptCount)
                .Take(5)
                .ToListAsync();

            foreach (var doc in doctors)
            {
                result.AppointmentsByDoctor.Add(new ChartDataDto
                {
                    Label = $"Dr. {doc.DoctorName}",
                    Value = doc.ApptCount
                });
            }

            return result;
        }
    }
}
