namespace EMR.Application.DTOs.Doctors;

public class DoctorListDto
{
    public int DoctorId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public decimal ConsultationFee { get; set; }
    public bool IsActive { get; set; }
}