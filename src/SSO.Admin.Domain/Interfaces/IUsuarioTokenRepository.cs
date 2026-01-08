using SSO.Admin.Domain.Entities;

namespace SSO.Admin.Domain.Interfaces;

public interface IUsuarioTokenRepository
{
    Task<UsuarioToken?> ObterPorLoginAsync(string login, CancellationToken cancellationToken = default);
    Task<UsuarioToken?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<UsuarioToken>> ObterTodosAsync(CancellationToken cancellationToken = default);
    Task AdicionarAsync(UsuarioToken usuarioToken, CancellationToken cancellationToken = default);
    Task AtualizarAsync(UsuarioToken usuarioToken, CancellationToken cancellationToken = default);
    Task RemoverAsync(UsuarioToken usuarioToken, CancellationToken cancellationToken = default);
}



