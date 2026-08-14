using EMR.Application.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMR.Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
        Task<PatientLoginResponseDto?> PatientLoginAsync(PatientLoginRequestDto request);
        Task<LoginResponseDto?> RefreshTokenAsync(TokenRefreshRequestDto request);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto request);
        Task<bool> ForgotPasswordAsync(ForgotPasswordRequestDto request, string clientBaseUrl);
        Task<bool> ResetPasswordAsync(ResetPasswordRequestDto request);
    }
}
