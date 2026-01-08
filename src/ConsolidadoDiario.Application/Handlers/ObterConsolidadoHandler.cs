using ConsolidadoDiario.Application.DTOs;
using ConsolidadoDiario.Application.Queries;
using ConsolidadoDiario.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ConsolidadoDiario.Application.Handlers;

public class ObterConsolidadoHandler
{
    private readonly IConsolidadoRepository _repository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<ObterConsolidadoHandler> _logger;

    public ObterConsolidadoHandler(
        IConsolidadoRepository repository,
        ICacheService cacheService,
        ILogger<ObterConsolidadoHandler> logger)
    {
        _repository = repository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<ConsolidadoDiarioDTO?> HandleAsync(ObterConsolidadoQuery query, CancellationToken cancellationToken = default)
    {
        var data = query.Data.Date;
        var cacheKey = $"consolidado:{query.UsuarioId}:{data:yyyy-MM-dd}";

        // Tentar obter do cache
        var cached = await _cacheService.ObterAsync<ConsolidadoDiarioDTO>(cacheKey, cancellationToken);
        if (cached != null)
        {
            _logger.LogInformation("Consolidado obtido do cache para usuario {UsuarioId} e data: {Data}", query.UsuarioId, data);
            return cached;
        }

        // Obter do banco de dados
        var consolidado = await _repository.ObterPorDataEUsuarioAsync(data, query.UsuarioId, cancellationToken);
        
        if (consolidado == null)
        {
            return null;
        }

        var dto = new ConsolidadoDiarioDTO
        {
            Data = consolidado.Data,
            TotalCreditos = consolidado.TotalCreditos,
            TotalDebitos = consolidado.TotalDebitos,
            SaldoDiario = consolidado.SaldoDiario,
            QuantidadeLancamentos = consolidado.QuantidadeLancamentos
        };

        // Armazenar no cache (5 minutos de expiração) - incluir UsuarioId na chave
        var cacheKeyComUsuario = $"consolidado:{query.UsuarioId}:{data:yyyy-MM-dd}";
        await _cacheService.DefinirAsync(cacheKeyComUsuario, dto, TimeSpan.FromMinutes(5), cancellationToken);

        return dto;
    }
}

