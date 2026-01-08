using BCrypt.Net;
using SSO.Admin.Application.Commands;
using SSO.Admin.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace SSO.Admin.Application.Handlers;

public class AtualizarSenhaHandler
{
    private readonly IUsuarioTokenRepository _usuarioTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AtualizarSenhaHandler> _logger;

    public AtualizarSenhaHandler(
        IUsuarioTokenRepository usuarioTokenRepository,
        IUnitOfWork unitOfWork,
        ILogger<AtualizarSenhaHandler> logger)
    {
        _usuarioTokenRepository = usuarioTokenRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task HandleAsync(AtualizarSenhaCommand command, CancellationToken cancellationToken = default)
    {
        var usuarioToken = await _usuarioTokenRepository.ObterPorIdAsync(command.Id, cancellationToken);
        if (usuarioToken == null)
        {
            throw new KeyNotFoundException("Usuário token não encontrado");
        }

        if (!BCrypt.Net.BCrypt.Verify(command.SenhaAtual, usuarioToken.SenhaHash))
        {
            throw new UnauthorizedAccessException("Senha atual incorreta");
        }

        var novaSenhaHash = BCrypt.Net.BCrypt.HashPassword(command.NovaSenha);
        usuarioToken.AtualizarSenha(novaSenhaHash);

        await _usuarioTokenRepository.AtualizarAsync(usuarioToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Senha atualizada para usuário token: {Id}", command.Id);
    }
}



