using ConsolidadoDiario.Application.Commands;
using ConsolidadoDiario.Application.Handlers;
using ConsolidadoDiario.Domain.Entities;
using ConsolidadoDiario.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ConsolidadoEntity = ConsolidadoDiario.Domain.Entities.ConsolidadoDiario;

namespace ConsolidadoDiario.API.Tests.Handlers;

public class ProcessarLancamentoEventHandlerTests
{
    private readonly Mock<IConsolidadoRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ILogger<ProcessarLancamentoEventHandler>> _loggerMock;
    private readonly ProcessarLancamentoEventHandler _handler;

    public ProcessarLancamentoEventHandlerTests()
    {
        _repositoryMock = new Mock<IConsolidadoRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _cacheServiceMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<ProcessarLancamentoEventHandler>>();
        _handler = new ProcessarLancamentoEventHandler(_repositoryMock.Object, _unitOfWorkMock.Object, _cacheServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ComConsolidadoNovoECredito_DeveCriarConsolidado()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var data = DateTime.UtcNow.Date;
        var command = new ProcessarLancamentoEventCommand
        {
            LancamentoId = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Data = data,
            Valor = 100,
            Tipo = "C",
            DataCriacao = DateTime.UtcNow
        };

        _repositoryMock.Setup(r => r.ObterPorDataEUsuarioAsync(data, usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConsolidadoEntity?)null);
        _repositoryMock.Setup(r => r.AdicionarAsync(It.IsAny<ConsolidadoEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConsolidadoEntity c, CancellationToken ct) => c);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _cacheServiceMock.Setup(c => c.RemoverAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _repositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<ConsolidadoEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoverAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ComConsolidadoExistenteEDebito_DeveAtualizarConsolidado()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var data = DateTime.UtcNow.Date;
        var consolidado = new ConsolidadoEntity(usuarioId, data);
        consolidado.AdicionarCredito(100);

        var command = new ProcessarLancamentoEventCommand
        {
            LancamentoId = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Data = data,
            Valor = 50,
            Tipo = "D",
            DataCriacao = DateTime.UtcNow
        };

        _repositoryMock.Setup(r => r.ObterPorDataEUsuarioAsync(data, usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(consolidado);
        _repositoryMock.Setup(r => r.AtualizarAsync(It.IsAny<ConsolidadoEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _cacheServiceMock.Setup(c => c.RemoverAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _repositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<ConsolidadoEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

