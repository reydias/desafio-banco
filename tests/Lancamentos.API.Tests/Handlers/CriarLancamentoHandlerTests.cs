using FluentAssertions;
using Lancamentos.Application.Commands;
using Lancamentos.Application.DTOs;
using Lancamentos.Domain.Entities;
using Lancamentos.Domain.Events;
using Lancamentos.Domain.Interfaces;
using Lancamentos.Application.Handlers;
using Moq;
using Xunit;

namespace Lancamentos.API.Tests.Handlers;

public class CriarLancamentoHandlerTests
{
    private readonly Mock<ILancamentoRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly CriarLancamentoHandler _handler;

    public CriarLancamentoHandlerTests()
    {
        _repositoryMock = new Mock<ILancamentoRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _handler = new CriarLancamentoHandler(_repositoryMock.Object, _unitOfWorkMock.Object, _eventPublisherMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ComDadosValidos_DeveCriarLancamento()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var command = new CriarLancamentoCommand
        {
            UsuarioId = usuarioId,
            Data = DateTime.UtcNow,
            Valor = 100.50m,
            Tipo = TipoLancamento.Credito,
            Descricao = "Teste"
        };

        _repositoryMock.Setup(r => r.AdicionarAsync(It.IsAny<Lancamento>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Lancamento l, CancellationToken ct) => l);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _eventPublisherMock.Setup(e => e.PublicarAsync(It.IsAny<LancamentoCriadoEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Valor.Should().Be(100.50m);
        result.Tipo.Should().Be(TipoLancamento.Credito);
        result.Descricao.Should().Be("Teste");
        _repositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Lancamento>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _eventPublisherMock.Verify(e => e.PublicarAsync(It.IsAny<LancamentoCriadoEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ComValorZero_DeveLancarExcecao()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var command = new CriarLancamentoCommand
        {
            UsuarioId = usuarioId,
            Data = DateTime.UtcNow,
            Valor = 0,
            Tipo = TipoLancamento.Credito,
            Descricao = "Teste"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _handler.HandleAsync(command));
        _repositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Lancamento>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ComValorNegativo_DeveLancarExcecao()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var command = new CriarLancamentoCommand
        {
            UsuarioId = usuarioId,
            Data = DateTime.UtcNow,
            Valor = -10,
            Tipo = TipoLancamento.Credito,
            Descricao = "Teste"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_ComDescricaoVazia_DeveLancarExcecao()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var command = new CriarLancamentoCommand
        {
            UsuarioId = usuarioId,
            Data = DateTime.UtcNow,
            Valor = 100,
            Tipo = TipoLancamento.Credito,
            Descricao = ""
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_ComDescricaoNula_DeveLancarExcecao()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var command = new CriarLancamentoCommand
        {
            UsuarioId = usuarioId,
            Data = DateTime.UtcNow,
            Valor = 100,
            Tipo = TipoLancamento.Credito,
            Descricao = null!
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_ComTipoDebito_DeveCriarLancamento()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var command = new CriarLancamentoCommand
        {
            UsuarioId = usuarioId,
            Data = DateTime.UtcNow,
            Valor = 50.25m,
            Tipo = TipoLancamento.Debito,
            Descricao = "Débito teste"
        };

        _repositoryMock.Setup(r => r.AdicionarAsync(It.IsAny<Lancamento>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Lancamento l, CancellationToken ct) => l);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _eventPublisherMock.Setup(e => e.PublicarAsync(It.IsAny<LancamentoCriadoEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Tipo.Should().Be(TipoLancamento.Debito);
    }
}


