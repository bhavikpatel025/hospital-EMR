using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMR.Application.Interfaces;
using EMR.Domain.Entities;
using EMR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EMR.Infrastructure.Repositories
{
    public class PrescriptionRepository : IPrescriptionRepository
    {
        private readonly AppDbContext _context;

        public PrescriptionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Prescription> GetByIdAsync(int id)
        {
            return await _context.Prescriptions
                .Include(p => p.Medications)
                .FirstOrDefaultAsync(p => p.PrescriptionId == id);
        }

        public async Task<IEnumerable<Prescription>> GetByPatientIdAsync(int patientId)
        {
            return await _context.Prescriptions
                .Include(p => p.Medications)
                .Where(p => p.PatientId == patientId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Prescription> GetByAppointmentIdAsync(int appointmentId)
        {
            return await _context.Prescriptions
                .Include(p => p.Medications)
                .FirstOrDefaultAsync(p => p.AppointmentId == appointmentId);
        }

        public async Task<Prescription> AddAsync(Prescription prescription)
        {
            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();
            return prescription;
        }

        public async Task UpdateAsync(Prescription prescription)
        {
            _context.Prescriptions.Update(prescription);
            await _context.SaveChangesAsync();
        }
    }
}
