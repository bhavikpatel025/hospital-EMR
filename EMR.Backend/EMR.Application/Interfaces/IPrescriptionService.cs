using System.Collections.Generic;
using System.Threading.Tasks;
using EMR.Application.DTOs.Prescriptions;

namespace EMR.Application.Interfaces
{
    public interface IPrescriptionService
    {
        Task<PrescriptionDto> GetPrescriptionByIdAsync(int id);
        Task<IEnumerable<PrescriptionDto>> GetPrescriptionsByPatientAsync(int patientId);
        Task<PrescriptionDto> GetPrescriptionByAppointmentAsync(int appointmentId);
        Task<PrescriptionDto> CreatePrescriptionAsync(PrescriptionDto dto);
    }
}
