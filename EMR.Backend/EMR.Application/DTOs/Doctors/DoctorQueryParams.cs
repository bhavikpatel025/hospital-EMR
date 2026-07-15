namespace EMR.Application.DTOs.Doctors;

public class DoctorQueryParams
{
    public string? SearchTerm { get; set; }
    public string? Specialization { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "FullName";
    public bool SortDescending { get; set; } = false;
}