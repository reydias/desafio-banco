using Lancamentos.Domain.Entities;
using Lancamentos.Domain.Interfaces;
using Lancamentos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Lancamentos.Infrastructure.Repositories;

public class LancamentoRepository : ILancamentoRepository
{
    private readonly LancamentoDbContext _context;

    public LancamentoRepository(LancamentoDbContext context)
    {
        _context = context;
    }

    public async Task<Lancamento?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Lancamentos.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<Lancamento?> ObterPorIdEUsuarioAsync(Guid id, Guid usuarioId, CancellationToken cancellationToken = default)
    {
        return await _context.Lancamentos
            .FirstOrDefaultAsync(l => l.Id == id && l.UsuarioId == usuarioId, cancellationToken);
    }

    public async Task<IEnumerable<Lancamento>> ObterPorDataAsync(DateTime data, CancellationToken cancellationToken = default)
    {
        return await _context.Lancamentos
            .Where(l => l.Data.Date == data.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Lancamento>> ObterPorDataEUsuarioAsync(DateTime data, Guid usuarioId, CancellationToken cancellationToken = default)
    {
        return await _context.Lancamentos
            .Where(l => l.Data.Date == data.Date && l.UsuarioId == usuarioId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Lancamento>> ObterPorPeriodoAsync(DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default)
    {
        return await _context.Lancamentos
            .Where(l => l.Data >= dataInicio.Date && l.Data <= dataFim.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Lancamento>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Lancamentos.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Lancamento>> ObterTodosPorUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        return await _context.Lancamentos
            .Where(l => l.UsuarioId == usuarioId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Lancamento> AdicionarAsync(Lancamento lancamento, CancellationToken cancellationToken = default)
    {
        await _context.Lancamentos.AddAsync(lancamento, cancellationToken);
        return lancamento;
    }

    public Task AtualizarAsync(Lancamento lancamento, CancellationToken cancellationToken = default)
    {
        _context.Lancamentos.Update(lancamento);
        return Task.CompletedTask;
    }

    public async Task ExcluirAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var lancamento = await ObterPorIdAsync(id, cancellationToken);
        if (lancamento != null)
        {
            _context.Lancamentos.Remove(lancamento);
        }
    }
}

