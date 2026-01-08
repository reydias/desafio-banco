using ConsolidadoDiario.Domain.Entities;
using ConsolidadoDiario.Domain.Interfaces;
using ConsolidadoDiario.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ConsolidadoEntity = ConsolidadoDiario.Domain.Entities.ConsolidadoDiario;

namespace ConsolidadoDiario.Infrastructure.Repositories;

public class ConsolidadoRepository : IConsolidadoRepository
{
    private readonly ConsolidadoDbContext _context;

    public ConsolidadoRepository(ConsolidadoDbContext context)
    {
        _context = context;
    }

    public async Task<ConsolidadoEntity?> ObterPorDataAsync(DateTime data, CancellationToken cancellationToken = default)
    {
        return await _context.Consolidados
            .FirstOrDefaultAsync(c => c.Data.Date == data.Date, cancellationToken);
    }

    public async Task<ConsolidadoEntity?> ObterPorDataEUsuarioAsync(DateTime data, Guid usuarioId, CancellationToken cancellationToken = default)
    {
        return await _context.Consolidados
            .FirstOrDefaultAsync(c => c.Data.Date == data.Date && c.UsuarioId == usuarioId, cancellationToken);
    }

    public async Task<ConsolidadoEntity> AdicionarAsync(ConsolidadoEntity consolidado, CancellationToken cancellationToken = default)
    {
        await _context.Consolidados.AddAsync(consolidado, cancellationToken);
        return consolidado;
    }

    public Task AtualizarAsync(ConsolidadoEntity consolidado, CancellationToken cancellationToken = default)
    {
        var entry = _context.Entry(consolidado);
        
        // Se a entidade não está sendo tracked, anexar e marcar como modificado
        if (entry.State == EntityState.Detached)
        {
            _context.Consolidados.Attach(consolidado);
            entry.State = EntityState.Modified;
        }
        else
        {
            // Se já está sendo tracked, apenas marcar como modificado
            entry.State = EntityState.Modified;
        }
        
        return Task.CompletedTask;
    }
}

