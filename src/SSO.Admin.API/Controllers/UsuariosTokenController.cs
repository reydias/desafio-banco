using SSO.Admin.Application.Commands;
using SSO.Admin.Application.DTOs;
using SSO.Admin.Application.Handlers;
using SSO.Admin.Application.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SSO.Admin.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsuariosTokenController : ControllerBase
{
    private readonly CriarUsuarioTokenHandler _criarHandler;
    private readonly AtualizarUsuarioTokenHandler _atualizarHandler;
    private readonly AtualizarSenhaHandler _atualizarSenhaHandler;
    private readonly ObterUsuarioTokenHandler _obterHandler;
    private readonly ILogger<UsuariosTokenController> _logger;

    public UsuariosTokenController(
        CriarUsuarioTokenHandler criarHandler,
        AtualizarUsuarioTokenHandler atualizarHandler,
        AtualizarSenhaHandler atualizarSenhaHandler,
        ObterUsuarioTokenHandler obterHandler,
        ILogger<UsuariosTokenController> logger)
    {
        _criarHandler = criarHandler;
        _atualizarHandler = atualizarHandler;
        _atualizarSenhaHandler = atualizarSenhaHandler;
        _obterHandler = obterHandler;
        _logger = logger;
    }

    /// <summary>
    /// Cria um novo usuário token
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(UsuarioTokenDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UsuarioTokenDTO>> Criar([FromBody] CriarUsuarioTokenDTO dto, CancellationToken cancellationToken)
    {
        try
        {
            var command = new CriarUsuarioTokenCommand
            {
                Login = dto.Login,
                Senha = dto.Senha,
                Nome = dto.Nome,
                Email = dto.Email
            };

            var resultado = await _criarHandler.HandleAsync(command, cancellationToken);
            return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Id }, resultado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar usuário token");
            return BadRequest(new { message = "Erro ao criar usuário token" });
        }
    }

    /// <summary>
    /// Obtém um usuário token por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UsuarioTokenDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioTokenDTO>> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var query = new ObterUsuarioTokenQuery { Id = id };
        var resultado = await _obterHandler.HandleAsync(query, cancellationToken);

        if (resultado == null)
        {
            return NotFound(new { message = "Usuário token não encontrado" });
        }

        return Ok(resultado);
    }

    /// <summary>
    /// Obtém todos os usuários token
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UsuarioTokenDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UsuarioTokenDTO>>> ObterTodos(CancellationToken cancellationToken)
    {
        var query = new ObterTodosUsuariosTokenQuery();
        var resultado = await _obterHandler.HandleAsync(query, cancellationToken);
        return Ok(resultado);
    }

    /// <summary>
    /// Atualiza um usuário token
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UsuarioTokenDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UsuarioTokenDTO>> Atualizar(Guid id, [FromBody] AtualizarUsuarioTokenDTO dto, CancellationToken cancellationToken)
    {
        try
        {
            var command = new AtualizarUsuarioTokenCommand
            {
                Id = id,
                Nome = dto.Nome,
                Email = dto.Email
            };

            var resultado = await _atualizarHandler.HandleAsync(command, cancellationToken);
            return Ok(resultado);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar usuário token");
            return BadRequest(new { message = "Erro ao atualizar usuário token" });
        }
    }

    /// <summary>
    /// Atualiza a senha de um usuário token
    /// </summary>
    [HttpPut("{id}/senha")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> AtualizarSenha(Guid id, [FromBody] AtualizarSenhaDTO dto, CancellationToken cancellationToken)
    {
        try
        {
            var command = new AtualizarSenhaCommand
            {
                Id = id,
                SenhaAtual = dto.SenhaAtual,
                NovaSenha = dto.NovaSenha
            };

            await _atualizarSenhaHandler.HandleAsync(command, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar senha");
            return BadRequest(new { message = "Erro ao atualizar senha" });
        }
    }
}



