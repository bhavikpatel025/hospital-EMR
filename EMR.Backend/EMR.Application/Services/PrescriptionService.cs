using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMR.Application.DTOs.Prescriptions;
using EMR.Application.Interfaces;
using EMR.Domain.Entities;

namespace EMR.Application.Services
{
    public class PrescriptionService : IPrescriptionService
    {
        private readonly IPrescriptionRepository _repository;

        public PrescriptionService(IPrescriptionRepository repository)
        {
            _repository = repository;
        }

        public async Task<PrescriptionDto> GetPrescriptionByIdAsync(int id)
        {
            var prescription = await _repository.GetByIdAsync(id);
            return MapToDto(prescription);
        }

        public async Task<IEnumerable<PrescriptionDto>> GetPrescriptionsByPatientAsync(int patientId)
        {
            var prescriptions = await _repository.GetByPatientIdAsync(patientId);
            return prescriptions.Select(MapToDto);
        }

        public async Task<PrescriptionDto> GetPrescriptionByAppointmentAsync(int appointmentId)
        {
            var prescription = await _repository.GetByAppointmentIdAsync(appointmentId);
            return MapToDto(prescription);
        }

        public async Task<PrescriptionDto> CreatePrescriptionAsync(PrescriptionDto dto)
        {
            var prescription = new Prescription
            {
                PatientId = dto.PatientId,
                AppointmentId = dto.AppointmentId,
                ChiefComplaints = dto.ChiefComplaints,
                Diagnosis = dto.Diagnosis,
                Vitals = dto.Vitals,
                InvestigationsOrdered = dto.InvestigationsOrdered,
                Guidelines = dto.Guidelines,
                NextFollowUpDate = dto.NextFollowUpDate,
                CreatedAt = DateTime.UtcNow,
                Medications = dto.Medications.Select(m => new PrescribedMedication
                {
                    MedicineName = m.MedicineName,
                    Strength = m.Strength,
                    Dosage = m.Dosage,
                    Frequency = m.Frequency,
                    Instructions = m.Instructions,
                    Duration = m.Duration
                }).ToList()
            };

            var result = await _repository.AddAsync(prescription);
            return MapToDto(result);
        }

        private static PrescriptionDto MapToDto(Prescription? prescription)
        {
            if (prescription == null) return null;
            
            return new PrescriptionDto
            {
                PrescriptionId = prescription.PrescriptionId,
                PatientId = prescription.PatientId,
                AppointmentId = prescription.AppointmentId,
                ChiefComplaints = prescription.ChiefComplaints,
                Diagnosis = prescription.Diagnosis,
                Vitals = prescription.Vitals,
                InvestigationsOrdered = prescription.InvestigationsOrdered,
                Guidelines = prescription.Guidelines,
                NextFollowUpDate = prescription.NextFollowUpDate,
                CreatedAt = prescription.CreatedAt,
                Medications = prescription.Medications.Select(m => new PrescribedMedicationDto
                {
                    PrescribedMedicationId = m.PrescribedMedicationId,
                    PrescriptionId = m.PrescriptionId,
                    MedicineName = m.MedicineName,
                    Strength = m.Strength,
                    Dosage = m.Dosage,
                    Frequency = m.Frequency,
                    Instructions = m.Instructions,
                    Duration = m.Duration
                }).ToList()
            };
        }
    }
}
