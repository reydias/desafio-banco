using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SSO.Admin.Application.Commands;
using SSO.Admin.Application.DTOs;
using SSO.Admin.Application.Handlers;
using SSO.Admin.Domain.Entities;
using SSO.Admin.Domain.Interfaces;
using Xunit;

namespace SSO.Admin.API.Tests.Handlers;

public class CriarUsuarioTokenHandlerTests
{
    private readonly Mock<IUsuarioTokenRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<CriarUsuarioTokenHandler>> _loggerMock;
    private readonly CriarUsuarioTokenHandler _handler;

    public CriarUsuarioTokenHandlerTests()
    {
        _repositoryMock = new Mock<IUsuarioTokenRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<CriarUsuarioTokenHandler>>();
        _handler = new CriarUsuarioTokenHandler(_repositoryMock.Object, _unitOfWorkMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ComDadosValidos_DeveCriarUsuario()
    {
        // Arrange
        var command = new CriarUsuarioTokenCommand
        {
            Login = "teste",
            Senha = "123456",
            Nome = "Teste",
            Email = "teste@teste.com"
        };

        _repositoryMock.Setup(r => r.ObterPorLoginAsync(command.Login, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UsuarioToken?)null);
        _repositoryMock.Setup(r => r.AdicionarAsync(It.IsAny<UsuarioToken>(), It.IsAny<CancellationToken>()))
            .Returns<UsuarioToken, CancellationToken>((u, ct) => Task.FromResult(u));
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Login.Should().Be("teste");
        result.Nome.Should().Be("Teste");
        result.Email.Should().Be("teste@teste.com");
        result.Ativo.Should().BeTrue();
        _repositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<UsuarioToken>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ComLoginExistente_DeveLancarExcecao()
    {
        // Arrange
        var command = new CriarUsuarioTokenCommand
        {
            Login = "teste",
            Senha = "123456",
            Nome = "Teste",
            Email = "teste@teste.com"
        };

        var usuarioExistente = new UsuarioToken("teste", "hash", "Teste", "teste@teste.com");
        _repositoryMock.Setup(r => r.ObterPorLoginAsync(command.Login, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuarioExistente);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));
        _repositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<UsuarioToken>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

