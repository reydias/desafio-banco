namespace SSO.Admin.Application.DTOs;

public class TokenDTO
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}



