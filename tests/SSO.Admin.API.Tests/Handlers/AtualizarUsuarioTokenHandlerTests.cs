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

public class AtualizarUsuarioTokenHandlerTests
{
    private readonly Mock<IUsuarioTokenRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<AtualizarUsuarioTokenHandler>> _loggerMock;
    private readonly AtualizarUsuarioTokenHandler _handler;

    public AtualizarUsuarioTokenHandlerTests()
    {
        _repositoryMock = new Mock<IUsuarioTokenRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<AtualizarUsuarioTokenHandler>>();
        _handler = new AtualizarUsuarioTokenHandler(_repositoryMock.Object, _unitOfWorkMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ComDadosValidos_DeveAtualizarUsuario()
    {
        // Arrange
        var id = Guid.NewGuid();
        var usuario = new UsuarioToken("teste", "hash", "Nome Antigo", "antigo@teste.com");
        var command = new AtualizarUsuarioTokenCommand
        {
            Id = id,
            Nome = "Nome Novo",
            Email = "novo@teste.com"
        };

        _repositoryMock.Setup(r => r.ObterPorIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _repositoryMock.Setup(r => r.AtualizarAsync(It.IsAny<UsuarioToken>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Nome.Should().Be("Nome Novo");
        result.Email.Should().Be("novo@teste.com");
        _repositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<UsuarioToken>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ComIdInexistente_DeveLancarExcecao()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new AtualizarUsuarioTokenCommand
        {
            Id = id,
            Nome = "Nome Novo",
            Email = "novo@teste.com"
        };

        _repositoryMock.Setup(r => r.ObterPorIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UsuarioToken?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.HandleAsync(command));
        _repositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<UsuarioToken>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}


