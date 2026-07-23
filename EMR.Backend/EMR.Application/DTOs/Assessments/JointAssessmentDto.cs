using System;

namespace EMR.Application.DTOs.Assessments
{
    public class JointAssessmentDto
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int? AppointmentId { get; set; }
        public DateTime AssessmentDate { get; set; }
        
        // JSON string of joint data (e.g., {"LeftShoulder": "Tender"})
        public string JointsDataJson { get; set; } = string.Empty;
        
        public string? Notes { get; set; }

        public int TotalTender { get; set; }
        public int TotalSwollen { get; set; }
        public int TotalBoth { get; set; }
        public int TotalLimited { get; set; }
        public int TotalNormal { get; set; }
        public int TotalJointsAssessed { get; set; }
    }
}
