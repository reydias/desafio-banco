using SSO.Admin.Application.Commands;
using SSO.Admin.Application.DTOs;
using SSO.Admin.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace SSO.Admin.Application.Handlers;

public class AtualizarUsuarioTokenHandler
{
    private readonly IUsuarioTokenRepository _usuarioTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AtualizarUsuarioTokenHandler> _logger;

    public AtualizarUsuarioTokenHandler(
        IUsuarioTokenRepository usuarioTokenRepository,
        IUnitOfWork unitOfWork,
        ILogger<AtualizarUsuarioTokenHandler> logger)
    {
        _usuarioTokenRepository = usuarioTokenRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<UsuarioTokenDTO> HandleAsync(AtualizarUsuarioTokenCommand command, CancellationToken cancellationToken = default)
    {
        var usuarioToken = await _usuarioTokenRepository.ObterPorIdAsync(command.Id, cancellationToken);
        if (usuarioToken == null)
        {
            throw new KeyNotFoundException("Usuário token não encontrado");
        }

        usuarioToken.Atualizar(command.Nome, command.Email);

        await _usuarioTokenRepository.AtualizarAsync(usuarioToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Usuário token atualizado: {Id}", command.Id);

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
}



