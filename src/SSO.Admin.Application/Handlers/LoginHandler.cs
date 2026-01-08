using SSO.Admin.Application.DTOs;
using SSO.Admin.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SSO.Admin.Application.Handlers;

public class LoginHandler
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(
        IAuthService authService,
        IConfiguration configuration,
        ILogger<LoginHandler> logger)
    {
        _authService = authService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<TokenDTO> HandleAsync(LoginDTO loginDTO, CancellationToken cancellationToken = default)
    {
        var token = await _authService.GerarTokenAsync(loginDTO.Login, loginDTO.Senha, cancellationToken);

        var expirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60");
        var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);

        _logger.LogInformation("Login realizado: {Login}", loginDTO.Login);

        return new TokenDTO
        {
            Token = token,
            ExpiresAt = expiresAt
        };
    }
}



