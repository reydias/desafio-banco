using ConsolidadoDiario.Application.DTOs;
using ConsolidadoDiario.Application.Handlers;
using ConsolidadoDiario.Application.Queries;
using ConsolidadoDiario.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConsolidadoDiario.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConsolidadoController : ControllerBase
{
    private readonly ObterConsolidadoHandler _obterHandler;
    private readonly ILogger<ConsolidadoController> _logger;

    public ConsolidadoController(
        ObterConsolidadoHandler obterHandler,
        ILogger<ConsolidadoController> logger)
    {
        _obterHandler = obterHandler;
        _logger = logger;
    }

    [HttpGet("{data}")]
    [ProducesResponseType(typeof(ConsolidadoDiarioDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConsolidadoDiarioDTO>> ObterConsolidado(DateTime data, CancellationToken cancellationToken)
    {
        try
        {
            var usuarioId = ClaimsHelper.ObterUsuarioIdOuLancarExcecao(User);
            
            var query = new ObterConsolidadoQuery 
            { 
                UsuarioId = usuarioId,
                Data = data 
            };
            var resultado = await _obterHandler.HandleAsync(query, cancellationToken);

            if (resultado == null)
                return NotFound(new { message = $"Consolidado não encontrado para a data {data:yyyy-MM-dd}" });

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting consolidado for date: {Data}", data);
            return StatusCode(500, new { error = "An error occurred while getting the consolidado" });
        }
    }
}

