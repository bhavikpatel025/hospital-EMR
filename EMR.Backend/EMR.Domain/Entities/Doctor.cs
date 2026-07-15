using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMR.Domain.Entities
{
    public class Doctor
    {
        public int DoctorId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public string Specialization { get; set; } = string.Empty;
        public string? Qualification { get; set; }
        public decimal ConsultationFee { get; set; }
        public int ExperienceYears { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
