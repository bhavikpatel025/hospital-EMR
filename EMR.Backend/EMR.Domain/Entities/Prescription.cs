using System;
using System.Collections.Generic;

namespace EMR.Domain.Entities
{
    public class Prescription
    {
        public int PrescriptionId { get; set; }
        
        public int PatientId { get; set; }
        public Patient? Patient { get; set; }
        
        public int? AppointmentId { get; set; }
        public Appointment? Appointment { get; set; }

        public string? ChiefComplaints { get; set; }
        public string? Diagnosis { get; set; }
        public string? Vitals { get; set; } 
        
        public string? InvestigationsOrdered { get; set; }
        
        public string? Guidelines { get; set; }
        
        public string? NextFollowUpDate { get; set; }

        public ICollection<PrescribedMedication> Medications { get; set; } = new List<PrescribedMedication>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
