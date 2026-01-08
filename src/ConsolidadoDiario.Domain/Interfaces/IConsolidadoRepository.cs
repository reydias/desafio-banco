namespace ConsolidadoDiario.Domain.Interfaces;

public interface IConsolidadoRepository
{
    Task<Domain.Entities.ConsolidadoDiario?> ObterPorDataAsync(DateTime data, CancellationToken cancellationToken = default);
    Task<Domain.Entities.ConsolidadoDiario?> ObterPorDataEUsuarioAsync(DateTime data, Guid usuarioId, CancellationToken cancellationToken = default);
    Task<Domain.Entities.ConsolidadoDiario> AdicionarAsync(Domain.Entities.ConsolidadoDiario consolidado, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Domain.Entities.ConsolidadoDiario consolidado, CancellationToken cancellationToken = default);
}

