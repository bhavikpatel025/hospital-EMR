namespace EMR.Application.Interfaces;

public interface ICurrentUserService
{
    int? UserId { get; }
    string? Role { get; }
    int? DoctorId { get; }
    bool IsAdmin { get; }
    bool IsDoctor { get; }
    bool IsReceptionist { get; }
}
