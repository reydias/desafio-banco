using ConsolidadoDiario.Application.Commands;
using ConsolidadoDiario.Domain.Entities;
using ConsolidadoDiario.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ConsolidadoDiario.Application.Handlers;

public class ProcessarLancamentoEventHandler
{
    private readonly IConsolidadoRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ILogger<ProcessarLancamentoEventHandler> _logger;

    public ProcessarLancamentoEventHandler(
        IConsolidadoRepository repository,
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        ILogger<ProcessarLancamentoEventHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task HandleAsync(ProcessarLancamentoEventCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = command.Data.Date;
            var consolidado = await _repository.ObterPorDataEUsuarioAsync(data, command.UsuarioId, cancellationToken);
            var isNovo = consolidado == null;

            if (consolidado == null)
            {
                consolidado = new Domain.Entities.ConsolidadoDiario(command.UsuarioId, data);
                await _repository.AdicionarAsync(consolidado, cancellationToken);
            }

            // Adicionar o lançamento ao consolidado
            if (command.Tipo == "C") // Credito
            {
                consolidado.AdicionarCredito(command.Valor);
            }
            else if (command.Tipo == "D") // Debito
            {
                consolidado.AdicionarDebito(command.Valor);
            }

            // Se não for novo, precisa marcar como modificado para o EF Core
            if (!isNovo)
            {
                await _repository.AtualizarAsync(consolidado, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Invalidar cache - incluir UsuarioId na chave
            var cacheKey = $"consolidado:{command.UsuarioId}:{data:yyyy-MM-dd}";
            await _cacheService.RemoverAsync(cacheKey, cancellationToken);

            _logger.LogInformation("Lancamento processado: {LancamentoId} para data {Data}", command.LancamentoId, data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar lancamento: {LancamentoId}", command.LancamentoId);
            throw;
        }
    }
}

