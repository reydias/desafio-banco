using BCrypt.Net;
using SSO.Admin.Application.Commands;
using SSO.Admin.Application.DTOs;
using SSO.Admin.Domain.Entities;
using SSO.Admin.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace SSO.Admin.Application.Handlers;

public class CriarUsuarioTokenHandler
{
    private readonly IUsuarioTokenRepository _usuarioTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CriarUsuarioTokenHandler> _logger;

    public CriarUsuarioTokenHandler(
        IUsuarioTokenRepository usuarioTokenRepository,
        IUnitOfWork unitOfWork,
        ILogger<CriarUsuarioTokenHandler> logger)
    {
        _usuarioTokenRepository = usuarioTokenRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<UsuarioTokenDTO> HandleAsync(CriarUsuarioTokenCommand command, CancellationToken cancellationToken = default)
    {
        // Verificar se login já existe
        var usuarioExistente = await _usuarioTokenRepository.ObterPorLoginAsync(command.Login, cancellationToken);
        if (usuarioExistente != null)
        {
            throw new InvalidOperationException("Login já existe");
        }

        // Criptografar senha
        var senhaHash = BCrypt.Net.BCrypt.HashPassword(command.Senha);

        // Criar usuário
        var usuarioToken = new UsuarioToken(command.Login, senhaHash, command.Nome, command.Email);

        await _usuarioTokenRepository.AdicionarAsync(usuarioToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Usuário token criado: {Login}", command.Login);

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



