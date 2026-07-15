using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMR.Application.DTOs.Patients
{
    public class PatientCreateDto
    {
        [Required(ErrorMessage = "Full name is required")]
        [MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Age is required")]
        [Range(0, 150, ErrorMessage = "Enter a valid age")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; } = string.Empty;

        public string? BloodGroup { get; set; }

        [Required(ErrorMessage = "Mobile number is required")]
        [Phone(ErrorMessage = "Invalid mobile number")]
        public string Mobile { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email")]
        public string? Email { get; set; }

        public string? Address { get; set; }
    }
}
