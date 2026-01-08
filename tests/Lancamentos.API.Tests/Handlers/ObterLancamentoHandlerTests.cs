using FluentAssertions;
using Lancamentos.Application.DTOs;
using Lancamentos.Application.Handlers;
using Lancamentos.Application.Queries;
using Lancamentos.Domain.Entities;
using Lancamentos.Domain.Interfaces;
using Moq;
using Xunit;

namespace Lancamentos.API.Tests.Handlers;

public class ObterLancamentoHandlerTests
{
    private readonly Mock<ILancamentoRepository> _repositoryMock;
    private readonly ObterLancamentoHandler _handler;

    public ObterLancamentoHandlerTests()
    {
        _repositoryMock = new Mock<ILancamentoRepository>();
        _handler = new ObterLancamentoHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ComIdExistente_DeveRetornarLancamento()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var lancamentoId = Guid.NewGuid();
        var lancamento = new Lancamento(usuarioId, DateTime.UtcNow, 100, TipoLancamento.Credito, "Teste");

        _repositoryMock.Setup(r => r.ObterPorIdEUsuarioAsync(lancamentoId, usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lancamento);

        var query = new ObterLancamentoQuery
        {
            UsuarioId = usuarioId,
            Id = lancamentoId
        };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(lancamento.Id);
        result.Valor.Should().Be(100);
    }

    [Fact]
    public async Task HandleAsync_ComIdInexistente_DeveRetornarNull()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var lancamentoId = Guid.NewGuid();

        _repositoryMock.Setup(r => r.ObterPorIdEUsuarioAsync(lancamentoId, usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Lancamento?)null);

        var query = new ObterLancamentoQuery
        {
            UsuarioId = usuarioId,
            Id = lancamentoId
        };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_ComIdNulo_DeveRetornarNull()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var query = new ObterLancamentoQuery
        {
            UsuarioId = usuarioId,
            Id = null
        };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HandleListAsync_ComData_DeveRetornarLancamentosPorData()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var data = DateTime.UtcNow.Date;
        var lancamentos = new List<Lancamento>
        {
            new Lancamento(usuarioId, data, 100, TipoLancamento.Credito, "Teste 1"),
            new Lancamento(usuarioId, data, 200, TipoLancamento.Debito, "Teste 2")
        };

        _repositoryMock.Setup(r => r.ObterPorDataEUsuarioAsync(data, usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lancamentos);

        var query = new ObterLancamentoQuery
        {
            UsuarioId = usuarioId,
            Data = data
        };

        // Act
        var result = await _handler.HandleListAsync(query);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleListAsync_SemData_DeveRetornarTodosLancamentos()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var lancamentos = new List<Lancamento>
        {
            new Lancamento(usuarioId, DateTime.UtcNow, 100, TipoLancamento.Credito, "Teste 1"),
            new Lancamento(usuarioId, DateTime.UtcNow, 200, TipoLancamento.Debito, "Teste 2")
        };

        _repositoryMock.Setup(r => r.ObterTodosPorUsuarioAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lancamentos);

        var query = new ObterLancamentoQuery
        {
            UsuarioId = usuarioId
        };

        // Act
        var result = await _handler.HandleListAsync(query);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleListAsync_ComTipo_DeveFiltrarPorTipo()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var lancamentos = new List<Lancamento>
        {
            new Lancamento(usuarioId, DateTime.UtcNow, 100, TipoLancamento.Credito, "Teste 1"),
            new Lancamento(usuarioId, DateTime.UtcNow, 200, TipoLancamento.Debito, "Teste 2"),
            new Lancamento(usuarioId, DateTime.UtcNow, 300, TipoLancamento.Credito, "Teste 3")
        };

        _repositoryMock.Setup(r => r.ObterTodosPorUsuarioAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lancamentos);

        var query = new ObterLancamentoQuery
        {
            UsuarioId = usuarioId,
            Tipo = TipoLancamento.Credito
        };

        // Act
        var result = await _handler.HandleListAsync(query);

        // Assert
        result.Should().HaveCount(2);
        result.All(r => r.Tipo == TipoLancamento.Credito).Should().BeTrue();
    }
}


