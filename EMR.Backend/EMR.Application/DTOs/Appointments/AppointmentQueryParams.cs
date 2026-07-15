using EMR.Domain.Enums;

namespace EMR.Application.DTOs.Appointments;

public class AppointmentQueryParams
{
    public string? SearchTerm { get; set; }  
    public int? DoctorId { get; set; }
    public AppointmentStatus? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "AppointmentDate";
    public bool SortDescending { get; set; } = false;
}