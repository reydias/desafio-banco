using SSO.Admin.Application.DTOs;
using SSO.Admin.Application.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace SSO.Admin.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly LoginHandler _loginHandler;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        LoginHandler loginHandler,
        ILogger<AuthController> logger)
    {
        _loginHandler = loginHandler;
        _logger = logger;
    }

    /// <summary>
    /// Autentica um usuário token e retorna um token JWT
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TokenDTO>> Login([FromBody] LoginDTO loginDTO, CancellationToken cancellationToken)
    {
        try
        {
            var tokenDTO = await _loginHandler.HandleAsync(loginDTO, cancellationToken);
            return Ok(tokenDTO);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Tentativa de login falhou: {Login}", loginDTO.Login);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar login");
            return BadRequest(new { message = "Erro ao processar a requisição" });
        }
    }
}



