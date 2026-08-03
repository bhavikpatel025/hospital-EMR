using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;

    public AuthService(IUserRepository userRepository, IDoctorRepository doctorRepository, IPatientRepository patientRepository, IConfiguration configuration, IMapper mapper)
    {
        _userRepository = userRepository;
        _doctorRepository = doctorRepository;
        _patientRepository = patientRepository;
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