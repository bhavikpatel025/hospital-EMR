using System;
using System.Text.Json.Serialization;

namespace EMR.Domain.Entities
{
    public class PrescribedMedication
    {
        public int PrescribedMedicationId { get; set; }
        
        public int PrescriptionId { get; set; }
        [JsonIgnore]
        public Prescription? Prescription { get; set; }

        public string MedicineName { get; set; } = string.Empty;
        
        // E.g., "250mg Tablet", "10ml Syrup"
        public string? Strength { get; set; } 
        
        // "1-0-1", "0-0-1", "1-1-1"
        public string? Dosage { get; set; } 
        
        // "Daily", "SOS", "Weekly"
        public string? Frequency { get; set; } 
        
        // "After food", "Before food"
        public string? Instructions { get; set; } 
        
        // "5 Days", "1 Month"
        public string? Duration { get; set; } 
    }
}
