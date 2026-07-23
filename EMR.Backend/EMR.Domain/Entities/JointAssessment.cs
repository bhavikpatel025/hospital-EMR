using System;

namespace EMR.Domain.Entities
{
    public class JointAssessment
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int? AppointmentId { get; set; }
        public DateTime AssessmentDate { get; set; }
        
        // Storing the actual joints condition data as a JSON string to easily accommodate 40+ joints without creating 40 columns
        public string JointsDataJson { get; set; } = string.Empty;
        
        public string? Notes { get; set; }

        // Totals
        public int TotalTender { get; set; }
        public int TotalSwollen { get; set; }
        public int TotalBoth { get; set; }
        public int TotalLimited { get; set; }
        public int TotalNormal { get; set; }
        public int TotalJointsAssessed { get; set; }

        // Navigation property
        public Patient? Patient { get; set; }
    }
}
