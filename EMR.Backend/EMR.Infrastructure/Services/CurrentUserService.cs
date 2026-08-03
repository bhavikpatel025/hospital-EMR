using EMR.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace EMR.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out int userId) ? userId : null;
        }
    }

    public string? Role => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;

    public int? DoctorId
    {
        get
        {
            var doctorIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("DoctorId")?.Value;
            return int.TryParse(doctorIdClaim, out int doctorId) ? doctorId : null;
        }
    }

    public bool IsAdmin => Role == "Admin";
    public bool IsDoctor => Role == "Doctor";
    public bool IsReceptionist => Role == "Receptionist";
}
