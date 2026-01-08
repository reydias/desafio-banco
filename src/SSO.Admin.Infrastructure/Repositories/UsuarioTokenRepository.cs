using SSO.Admin.Domain.Entities;
using SSO.Admin.Domain.Interfaces;
using SSO.Admin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SSO.Admin.Infrastructure.Repositories;

public class UsuarioTokenRepository : IUsuarioTokenRepository
{
    private readonly SSOAdminDbContext _context;

    public UsuarioTokenRepository(SSOAdminDbContext context)
    {
        _context = context;
    }

    public async Task<UsuarioToken?> ObterPorLoginAsync(string login, CancellationToken cancellationToken = default)
    {
        return await _context.UsuariosToken
            .FirstOrDefaultAsync(u => u.Login == login, cancellationToken);
    }

    public async Task<UsuarioToken?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.UsuariosToken
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<UsuarioToken>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await _context.UsuariosToken
            .ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(UsuarioToken usuarioToken, CancellationToken cancellationToken = default)
    {
        await _context.UsuariosToken.AddAsync(usuarioToken, cancellationToken);
    }

    public Task AtualizarAsync(UsuarioToken usuarioToken, CancellationToken cancellationToken = default)
    {
        _context.UsuariosToken.Update(usuarioToken);
        return Task.CompletedTask;
    }

    public Task RemoverAsync(UsuarioToken usuarioToken, CancellationToken cancellationToken = default)
    {
        _context.UsuariosToken.Remove(usuarioToken);
        return Task.CompletedTask;
    }
}



