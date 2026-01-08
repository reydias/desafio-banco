using ConsolidadoDiario.Application.DTOs;
using ConsolidadoDiario.Application.Handlers;
using ConsolidadoDiario.Application.Queries;
using ConsolidadoDiario.Domain.Entities;
using ConsolidadoDiario.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ConsolidadoEntity = ConsolidadoDiario.Domain.Entities.ConsolidadoDiario;

namespace ConsolidadoDiario.API.Tests.Handlers;

public class ObterConsolidadoHandlerTests
{
    private readonly Mock<IConsolidadoRepository> _repositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ILogger<ObterConsolidadoHandler>> _loggerMock;
    private readonly ObterConsolidadoHandler _handler;

    public ObterConsolidadoHandlerTests()
    {
        _repositoryMock = new Mock<IConsolidadoRepository>();
        _cacheServiceMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<ObterConsolidadoHandler>>();
        _handler = new ObterConsolidadoHandler(_repositoryMock.Object, _cacheServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ComCache_DeveRetornarDoCache()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var data = DateTime.UtcNow.Date;
        var cacheKey = $"consolidado:{usuarioId}:{data:yyyy-MM-dd}";
        var cachedDTO = new ConsolidadoDiarioDTO
        {
            Data = data,
            TotalCreditos = 100,
            TotalDebitos = 50,
            SaldoDiario = 50,
            QuantidadeLancamentos = 2
        };

        _cacheServiceMock.Setup(c => c.ObterAsync<ConsolidadoDiarioDTO>(cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedDTO);

        var query = new ObterConsolidadoQuery
        {
            UsuarioId = usuarioId,
            Data = data
        };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result!.TotalCreditos.Should().Be(100);
        _repositoryMock.Verify(r => r.ObterPorDataEUsuarioAsync(It.IsAny<DateTime>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_SemCache_DeveRetornarDoBanco()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var data = DateTime.UtcNow.Date;
        var cacheKey = $"consolidado:{usuarioId}:{data:yyyy-MM-dd}";
        var consolidado = new ConsolidadoEntity(usuarioId, data);
        consolidado.AdicionarCredito(100);
        consolidado.AdicionarDebito(50);

        _cacheServiceMock.Setup(c => c.ObterAsync<ConsolidadoDiarioDTO>(cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConsolidadoDiarioDTO?)null);
        _repositoryMock.Setup(r => r.ObterPorDataEUsuarioAsync(data, usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(consolidado);
        _cacheServiceMock.Setup(c => c.DefinirAsync(It.IsAny<string>(), It.IsAny<ConsolidadoDiarioDTO>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var query = new ObterConsolidadoQuery
        {
            UsuarioId = usuarioId,
            Data = data
        };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result!.TotalCreditos.Should().Be(100);
        result.TotalDebitos.Should().Be(50);
        _cacheServiceMock.Verify(c => c.DefinirAsync(It.IsAny<string>(), It.IsAny<ConsolidadoDiarioDTO>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ComConsolidadoInexistente_DeveRetornarNull()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var data = DateTime.UtcNow.Date;
        var cacheKey = $"consolidado:{usuarioId}:{data:yyyy-MM-dd}";

        _cacheServiceMock.Setup(c => c.ObterAsync<ConsolidadoDiarioDTO>(cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConsolidadoDiarioDTO?)null);
        _repositoryMock.Setup(r => r.ObterPorDataEUsuarioAsync(data, usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConsolidadoEntity?)null);

        var query = new ObterConsolidadoQuery
        {
            UsuarioId = usuarioId,
            Data = data
        };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().BeNull();
    }
}

