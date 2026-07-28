using System.Collections.Generic;
using System.Threading.Tasks;
using EMR.Domain.Entities;

namespace EMR.Application.Interfaces
{
    public interface IPrescriptionRepository
    {
        Task<Prescription> GetByIdAsync(int id);
        Task<IEnumerable<Prescription>> GetByPatientIdAsync(int patientId);
        Task<Prescription> GetByAppointmentIdAsync(int appointmentId);
        Task<Prescription> AddAsync(Prescription prescription);
        Task UpdateAsync(Prescription prescription);
    }
}
