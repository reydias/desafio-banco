using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SSO.Admin.Application.Handlers;
using SSO.Admin.Application.Queries;
using SSO.Admin.Domain.Entities;
using SSO.Admin.Domain.Interfaces;
using Xunit;

namespace SSO.Admin.API.Tests.Handlers;

public class ObterUsuarioTokenHandlerTests
{
    private readonly Mock<IUsuarioTokenRepository> _repositoryMock;
    private readonly Mock<ILogger<ObterUsuarioTokenHandler>> _loggerMock;
    private readonly ObterUsuarioTokenHandler _handler;

    public ObterUsuarioTokenHandlerTests()
    {
        _repositoryMock = new Mock<IUsuarioTokenRepository>();
        _loggerMock = new Mock<ILogger<ObterUsuarioTokenHandler>>();
        _handler = new ObterUsuarioTokenHandler(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ComIdExistente_DeveRetornarUsuario()
    {
        // Arrange
        var id = Guid.NewGuid();
        var usuario = new UsuarioToken("teste", "hash", "Teste", "teste@teste.com");

        _repositoryMock.Setup(r => r.ObterPorIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var query = new ObterUsuarioTokenQuery { Id = id };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result!.Login.Should().Be("teste");
        result.Nome.Should().Be("Teste");
    }

    [Fact]
    public async Task HandleAsync_ComIdInexistente_DeveRetornarNull()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repositoryMock.Setup(r => r.ObterPorIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UsuarioToken?)null);

        var query = new ObterUsuarioTokenQuery { Id = id };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_ObterTodos_DeveRetornarLista()
    {
        // Arrange
        var usuarios = new List<UsuarioToken>
        {
            new UsuarioToken("teste1", "hash1", "Teste 1", "teste1@teste.com"),
            new UsuarioToken("teste2", "hash2", "Teste 2", "teste2@teste.com")
        };

        _repositoryMock.Setup(r => r.ObterTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuarios);

        var query = new ObterTodosUsuariosTokenQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().HaveCount(2);
    }
}


