namespace EMR.Application.DTOs.Auth;

public class TokenRefreshRequestDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
