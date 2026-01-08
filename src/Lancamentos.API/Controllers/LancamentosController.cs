using Lancamentos.Application.Commands;
using Lancamentos.Application.DTOs;
using Lancamentos.Application.Handlers;
using Lancamentos.Application.Queries;
using Lancamentos.Domain.Entities;
using Lancamentos.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lancamentos.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LancamentosController : ControllerBase
{
    private readonly CriarLancamentoHandler _criarHandler;
    private readonly ObterLancamentoHandler _obterHandler;
    private readonly ILogger<LancamentosController> _logger;

    public LancamentosController(
        CriarLancamentoHandler criarHandler,
        ObterLancamentoHandler obterHandler,
        ILogger<LancamentosController> logger)
    {
        _criarHandler = criarHandler;
        _obterHandler = obterHandler;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(LancamentoDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LancamentoDTO>> Criar([FromBody] CriarLancamentoDTO dto, CancellationToken cancellationToken)
    {
        try
        {
            var usuarioId = ClaimsHelper.ObterUsuarioIdOuLancarExcecao(User);
            
            var command = new CriarLancamentoCommand
            {
                UsuarioId = usuarioId,
                Data = dto.Data,
                Valor = dto.Valor,
                Tipo = dto.Tipo,
                Descricao = dto.Descricao
            };

            var resultado = await _criarHandler.HandleAsync(command, cancellationToken);
            return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Id }, resultado);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request data");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating lancamento");
            return StatusCode(500, new { error = "An error occurred while creating the lancamento" });
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(LancamentoDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LancamentoDTO>> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var usuarioId = ClaimsHelper.ObterUsuarioIdOuLancarExcecao(User);
        
        var query = new ObterLancamentoQuery 
        { 
            UsuarioId = usuarioId,
            Id = id 
        };
        var resultado = await _obterHandler.HandleAsync(query, cancellationToken);

        if (resultado == null)
            return NotFound();

        return Ok(resultado);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<LancamentoDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LancamentoDTO>>> Listar(
        [FromQuery] DateTime? data,
        [FromQuery] TipoLancamento? tipo,
        CancellationToken cancellationToken)
    {
        var usuarioId = ClaimsHelper.ObterUsuarioIdOuLancarExcecao(User);
        
        var query = new ObterLancamentoQuery
        {
            UsuarioId = usuarioId,
            Data = data,
            Tipo = tipo
        };

        var resultados = await _obterHandler.HandleListAsync(query, cancellationToken);
        return Ok(resultados);
    }
}

