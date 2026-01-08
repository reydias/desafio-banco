using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using SSO.Admin.Domain.Entities;
using SSO.Admin.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace SSO.Admin.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioTokenRepository _usuarioTokenRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationMinutes;

    public AuthService(
        IUsuarioTokenRepository usuarioTokenRepository,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _usuarioTokenRepository = usuarioTokenRepository;
        _configuration = configuration;
        _logger = logger;
        _secretKey = _configuration["Jwt:SecretKey"] ?? "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!";
        _issuer = _configuration["Jwt:Issuer"] ?? "SSOAPI";
        _audience = _configuration["Jwt:Audience"] ?? "SSOAPI";
        _expirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60");
    }

    public async Task<string> GerarTokenAsync(string login, string senha, CancellationToken cancellationToken = default)
    {
        var usuarioToken = await _usuarioTokenRepository.ObterPorLoginAsync(login, cancellationToken);

        if (usuarioToken == null || !usuarioToken.Ativo)
        {
            throw new UnauthorizedAccessException("Login ou senha inválidos");
        }

        if (!BCrypt.Net.BCrypt.Verify(senha, usuarioToken.SenhaHash))
        {
            throw new UnauthorizedAccessException("Login ou senha inválidos");
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secretKey);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuarioToken.Id.ToString()),
            new Claim(ClaimTypes.Name, usuarioToken.Login),
            new Claim(ClaimTypes.Email, usuarioToken.Email),
            new Claim("Nome", usuarioToken.Nome)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_expirationMinutes),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}



