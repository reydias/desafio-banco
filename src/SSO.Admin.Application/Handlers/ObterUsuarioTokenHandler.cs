using SSO.Admin.Application.DTOs;
using SSO.Admin.Application.Queries;
using SSO.Admin.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace SSO.Admin.Application.Handlers;

public class ObterUsuarioTokenHandler
{
    private readonly IUsuarioTokenRepository _usuarioTokenRepository;
    private readonly ILogger<ObterUsuarioTokenHandler> _logger;

    public ObterUsuarioTokenHandler(
        IUsuarioTokenRepository usuarioTokenRepository,
        ILogger<ObterUsuarioTokenHandler> logger)
    {
        _usuarioTokenRepository = usuarioTokenRepository;
        _logger = logger;
    }

    public async Task<UsuarioTokenDTO?> HandleAsync(ObterUsuarioTokenQuery query, CancellationToken cancellationToken = default)
    {
        var usuarioToken = await _usuarioTokenRepository.ObterPorIdAsync(query.Id, cancellationToken);
        
        if (usuarioToken == null)
        {
            return null;
        }

        return new UsuarioTokenDTO
        {
            Id = usuarioToken.Id,
            Login = usuarioToken.Login,
            Nome = usuarioToken.Nome,
            Email = usuarioToken.Email,
            Ativo = usuarioToken.Ativo,
            DataCriacao = usuarioToken.DataCriacao,
            DataAtualizacao = usuarioToken.DataAtualizacao
        };
    }

    public async Task<IEnumerable<UsuarioTokenDTO>> HandleAsync(ObterTodosUsuariosTokenQuery query, CancellationToken cancellationToken = default)
    {
        var usuariosToken = await _usuarioTokenRepository.ObterTodosAsync(cancellationToken);

        return usuariosToken.Select(u => new UsuarioTokenDTO
        {
            Id = u.Id,
            Login = u.Login,
            Nome = u.Nome,
            Email = u.Email,
            Ativo = u.Ativo,
            DataCriacao = u.DataCriacao,
            DataAtualizacao = u.DataAtualizacao
        });
    }
}



