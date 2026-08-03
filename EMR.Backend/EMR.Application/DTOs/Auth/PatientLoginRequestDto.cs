using System.ComponentModel.DataAnnotations;

namespace EMR.Application.DTOs.Auth
{
    public class PatientLoginRequestDto
    {
        [Required(ErrorMessage = "Mobile number is required")]
        public string Mobile { get; set; } = string.Empty;

        [Required(ErrorMessage = "OTP is required")]
        public string Otp { get; set; } = string.Empty;
    }
}
