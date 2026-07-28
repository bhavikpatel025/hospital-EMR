using System;
using System.Collections.Generic;

namespace EMR.Application.DTOs.Prescriptions
{
    public class PrescriptionDto
    {
        public int PrescriptionId { get; set; }
        public int PatientId { get; set; }
        public int? AppointmentId { get; set; }
        
        public string? ChiefComplaints { get; set; }
        public string? Diagnosis { get; set; }
        public string? Vitals { get; set; }
        
        public string? InvestigationsOrdered { get; set; }
        public string? Guidelines { get; set; }
        
        public string? NextFollowUpDate { get; set; }

        public List<PrescribedMedicationDto> Medications { get; set; } = new();

        public DateTime CreatedAt { get; set; }
    }

    public class PrescribedMedicationDto
    {
        public int PrescribedMedicationId { get; set; }
        public int PrescriptionId { get; set; }
        
        public string MedicineName { get; set; } = string.Empty;
        public string? Strength { get; set; }
        public string? Dosage { get; set; }
        public string? Frequency { get; set; }
        public string? Instructions { get; set; }
        public string? Duration { get; set; }
    }
}
