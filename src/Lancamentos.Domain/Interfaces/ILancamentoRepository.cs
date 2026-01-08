using Lancamentos.Domain.Entities;

namespace Lancamentos.Domain.Interfaces;

public interface ILancamentoRepository
{
    Task<Lancamento?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Lancamento?> ObterPorIdEUsuarioAsync(Guid id, Guid usuarioId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Lancamento>> ObterPorDataAsync(DateTime data, CancellationToken cancellationToken = default);
    Task<IEnumerable<Lancamento>> ObterPorDataEUsuarioAsync(DateTime data, Guid usuarioId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Lancamento>> ObterPorPeriodoAsync(DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default);
    Task<IEnumerable<Lancamento>> ObterTodosAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Lancamento>> ObterTodosPorUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task<Lancamento> AdicionarAsync(Lancamento lancamento, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Lancamento lancamento, CancellationToken cancellationToken = default);
    Task ExcluirAsync(Guid id, CancellationToken cancellationToken = default);
}

