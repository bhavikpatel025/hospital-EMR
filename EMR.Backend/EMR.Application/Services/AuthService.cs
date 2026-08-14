using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using EMR.Application.DTOs.Auth;
using EMR.Application.Interfaces;
using EMR.Domain.Entities;
using EMR.Shared.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace EMR.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IDoctorRepository _doctorRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;

    public AuthService(
        IUserRepository userRepository, 
        IDoctorRepository doctorRepository, 
        IPatientRepository patientRepository,
        IEmailService emailService,
        IConfiguration configuration, 
        IMapper mapper)
    {
        _userRepository = userRepository;
        _doctorRepository = doctorRepository;
        _patientRepository = patientRepository;
        _emailService = emailService;
        _configuration = configuration;
        _mapper = mapper;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user is null || !user.IsActive)
            return null;

        if (!PasswordHasher.VerifyHash(request.Password, user.PasswordHash, user.PasswordSalt))
            return null;

        int? doctorId = null;
        if (user.Role.RoleName == "Doctor")
        {
            var doctor = await _doctorRepository.GetByUserIdAsync(user.UserId);
            doctorId = doctor?.DoctorId;
        }

        var (token, expiresAt) = GenerateJwtToken(user, doctorId);
        var refreshToken = GenerateRefreshToken();
        var refreshExpiresAt = DateTime.UtcNow.AddDays(30);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = refreshExpiresAt;
        await _userRepository.UpdateAsync(user);

        return new LoginResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt = refreshExpiresAt,
            User = _mapper.Map<UserDto>(user)
        };
    }

    public async Task<PatientLoginResponseDto?> PatientLoginAsync(PatientLoginRequestDto request)
    {
        // For development, we bypass actual OTP validation and just check if OTP is correct
        if (request.Otp != "123456")
            return null;

        var patient = await _patientRepository.GetByMobileAsync(request.Mobile);

        if (patient is null || !patient.IsActive)
            return null;

        var (token, expiresAt) = GeneratePatientJwtToken(patient);

        return new PatientLoginResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            PatientId = patient.PatientId,
            FullName = patient.FullName,
            Mobile = patient.Mobile
        };
    }

    public async Task<LoginResponseDto?> RefreshTokenAsync(TokenRefreshRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return null;

        var user = await _userRepository.GetByRefreshTokenAsync(request.RefreshToken);

        if (user is null || !user.IsActive || user.RefreshTokenExpiryTime is null || user.RefreshTokenExpiryTime < DateTime.UtcNow)
            return null;

        // Sliding window token rotation: Issue a brand new Access Token and a brand new Refresh Token
        int? doctorId = null;
        if (user.Role.RoleName == "Doctor")
        {
            var doctor = await _doctorRepository.GetByUserIdAsync(user.UserId);
            doctorId = doctor?.DoctorId;
        }

        var (token, expiresAt) = GenerateJwtToken(user, doctorId);
        var newRefreshToken = GenerateRefreshToken();
        var refreshExpiresAt = DateTime.UtcNow.AddDays(30);

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = refreshExpiresAt;
        await _userRepository.UpdateAsync(user);

        return new LoginResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            RefreshToken = newRefreshToken,
            RefreshTokenExpiresAt = refreshExpiresAt,
            User = _mapper.Map<UserDto>(user)
        };
    }

    public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null || !user.IsActive)
        {
            throw new KeyNotFoundException("User not found or account is inactive.");
        }

        if (!PasswordHasher.VerifyHash(request.CurrentPassword, user.PasswordHash, user.PasswordSalt))
        {
            throw new InvalidOperationException("Current password is incorrect.");
        }

        if (request.NewPassword == request.CurrentPassword)
        {
            throw new InvalidOperationException("New password cannot be the same as your current password.");
        }

        if (request.NewPassword != request.ConfirmNewPassword)
        {
            throw new InvalidOperationException("New password and confirmation password do not match.");
        }

        PasswordHasher.CreateHash(request.NewPassword, out var newHash, out var newSalt);
        user.PasswordHash = newHash;
        user.PasswordSalt = newSalt;

        await _userRepository.UpdateAsync(user);
        return true;
    }

    public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequestDto request, string clientBaseUrl)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant());
        if (user is null || !user.IsActive)
        {
            // Security best practice: don't disclose whether user exists
            return true;
        }

        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        var resetToken = Convert.ToHexString(tokenBytes).ToLowerInvariant();
        var expiry = DateTime.UtcNow.AddMinutes(15);

        user.PasswordResetToken = resetToken;
        user.ResetTokenExpiry = expiry;
        await _userRepository.UpdateAsync(user);

        var baseUrl = string.IsNullOrWhiteSpace(clientBaseUrl) ? "http://localhost:4200" : clientBaseUrl.TrimEnd('/');
        var resetLink = $"{baseUrl}/reset-password?token={resetToken}&email={Uri.EscapeDataString(user.Email)}";

        await _emailService.SendPasswordResetEmailAsync(user.Email, user.FullName, resetLink);
        return true;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        if (request.NewPassword != request.ConfirmNewPassword)
        {
            throw new InvalidOperationException("New password and confirmation password do not match.");
        }

        var user = await _userRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant());
        if (user is null || !user.IsActive)
        {
            throw new InvalidOperationException("Invalid password reset request.");
        }

        if (string.IsNullOrWhiteSpace(user.PasswordResetToken) ||
            !string.Equals(user.PasswordResetToken, request.Token, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Invalid or expired password reset token.");
        }

        if (!user.ResetTokenExpiry.HasValue || user.ResetTokenExpiry.Value < DateTime.UtcNow)
        {
            throw new InvalidOperationException("Password reset link has expired. Please request a new one.");
        }

        PasswordHasher.CreateHash(request.NewPassword, out var newHash, out var newSalt);
        user.PasswordHash = newHash;
        user.PasswordSalt = newSalt;
        user.PasswordResetToken = null;
        user.ResetTokenExpiry = null;

        await _userRepository.UpdateAsync(user);
        return true;
    }

    private static string GenerateRefreshToken()
    {
        return Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
    }

    private (string Token, DateTime ExpiresAt) GenerateJwtToken(User user, int? doctorId = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.RoleName)
        };

        if (doctorId.HasValue)
        {
            claims.Add(new Claim("DoctorId", doctorId.Value.ToString()));
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        
        // Enterprise standard: Short-lived Access Token (15 minutes) rotated silently via 30-day Refresh Token
        var expiresAt = DateTime.UtcNow.AddMinutes(15);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    private (string Token, DateTime ExpiresAt) GeneratePatientJwtToken(Patient patient)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, patient.PatientId.ToString()),
            new(ClaimTypes.Name, patient.FullName),
            new(ClaimTypes.MobilePhone, patient.Mobile),
            new(ClaimTypes.Role, "Patient")
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        
        var expiresAt = DateTime.UtcNow.AddHours(24);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}