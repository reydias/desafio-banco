using BCrypt.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SSO.Admin.Application.Commands;
using SSO.Admin.Application.Handlers;
using SSO.Admin.Domain.Entities;
using SSO.Admin.Domain.Interfaces;
using Xunit;

namespace SSO.Admin.API.Tests.Handlers;

public class AtualizarSenhaHandlerTests
{
    private readonly Mock<IUsuarioTokenRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<AtualizarSenhaHandler>> _loggerMock;
    private readonly AtualizarSenhaHandler _handler;

    public AtualizarSenhaHandlerTests()
    {
        _repositoryMock = new Mock<IUsuarioTokenRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<AtualizarSenhaHandler>>();
        _handler = new AtualizarSenhaHandler(_repositoryMock.Object, _unitOfWorkMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ComSenhaValida_DeveAtualizarSenha()
    {
        // Arrange
        var id = Guid.NewGuid();
        var senhaAtual = "123456";
        var senhaHash = BCrypt.Net.BCrypt.HashPassword(senhaAtual);
        var usuario = new UsuarioToken("teste", senhaHash, "Teste", "teste@teste.com");
        
        var command = new AtualizarSenhaCommand
        {
            Id = id,
            SenhaAtual = senhaAtual,
            NovaSenha = "novaSenha123"
        };

        _repositoryMock.Setup(r => r.ObterPorIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _repositoryMock.Setup(r => r.AtualizarAsync(It.IsAny<UsuarioToken>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _repositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<UsuarioToken>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ComSenhaInvalida_DeveLancarExcecao()
    {
        // Arrange
        var id = Guid.NewGuid();
        var senhaHash = BCrypt.Net.BCrypt.HashPassword("senhaCorreta");
        var usuario = new UsuarioToken("teste", senhaHash, "Teste", "teste@teste.com");
        
        var command = new AtualizarSenhaCommand
        {
            Id = id,
            SenhaAtual = "senhaIncorreta",
            NovaSenha = "novaSenha123"
        };

        _repositoryMock.Setup(r => r.ObterPorIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _handler.HandleAsync(command));
        _repositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<UsuarioToken>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ComIdInexistente_DeveLancarExcecao()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new AtualizarSenhaCommand
        {
            Id = id,
            SenhaAtual = "123456",
            NovaSenha = "novaSenha123"
        };

        _repositoryMock.Setup(r => r.ObterPorIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UsuarioToken?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.HandleAsync(command));
    }
}


