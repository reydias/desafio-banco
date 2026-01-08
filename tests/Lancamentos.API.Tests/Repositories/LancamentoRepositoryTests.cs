using FluentAssertions;
using Lancamentos.Domain.Entities;
using Lancamentos.Infrastructure.Data;
using Lancamentos.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Lancamentos.API.Tests.Repositories;

public class LancamentoRepositoryTests : IDisposable
{
    private readonly LancamentoDbContext _context;
    private readonly LancamentoRepository _repository;

    public LancamentoRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<LancamentoDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new LancamentoDbContext(options);
        _repository = new LancamentoRepository(_context);
    }

    [Fact]
    public async Task AdicionarAsync_DeveAdicionarLancamento()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var lancamento = new Lancamento(usuarioId, DateTime.UtcNow, 100, TipoLancamento.Credito, "Teste");

        // Act
        var result = await _repository.AdicionarAsync(lancamento);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(lancamento.Id);
    }

    [Fact]
    public async Task ObterPorIdEUsuarioAsync_ComIdExistente_DeveRetornarLancamento()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var lancamento = new Lancamento(usuarioId, DateTime.UtcNow, 100, TipoLancamento.Credito, "Teste");
        await _repository.AdicionarAsync(lancamento);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ObterPorIdEUsuarioAsync(lancamento.Id, usuarioId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(lancamento.Id);
    }

    [Fact]
    public async Task ObterPorIdEUsuarioAsync_ComUsuarioDiferente_DeveRetornarNull()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var outroUsuarioId = Guid.NewGuid();
        var lancamento = new Lancamento(usuarioId, DateTime.UtcNow, 100, TipoLancamento.Credito, "Teste");
        await _repository.AdicionarAsync(lancamento);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ObterPorIdEUsuarioAsync(lancamento.Id, outroUsuarioId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ObterPorDataEUsuarioAsync_DeveRetornarLancamentosPorData()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var data = DateTime.UtcNow.Date;
        var lancamento1 = new Lancamento(usuarioId, data, 100, TipoLancamento.Credito, "Teste 1");
        var lancamento2 = new Lancamento(usuarioId, data, 200, TipoLancamento.Debito, "Teste 2");
        await _repository.AdicionarAsync(lancamento1);
        await _repository.AdicionarAsync(lancamento2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ObterPorDataEUsuarioAsync(data, usuarioId);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task ObterTodosPorUsuarioAsync_DeveRetornarTodosLancamentosDoUsuario()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var outroUsuarioId = Guid.NewGuid();
        var lancamento1 = new Lancamento(usuarioId, DateTime.UtcNow, 100, TipoLancamento.Credito, "Teste 1");
        var lancamento2 = new Lancamento(usuarioId, DateTime.UtcNow, 200, TipoLancamento.Debito, "Teste 2");
        var lancamento3 = new Lancamento(outroUsuarioId, DateTime.UtcNow, 300, TipoLancamento.Credito, "Teste 3");
        await _repository.AdicionarAsync(lancamento1);
        await _repository.AdicionarAsync(lancamento2);
        await _repository.AdicionarAsync(lancamento3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ObterTodosPorUsuarioAsync(usuarioId);

        // Assert
        result.Should().HaveCount(2);
        result.All(r => r.UsuarioId == usuarioId).Should().BeTrue();
    }

    [Fact]
    public async Task AtualizarAsync_DeveAtualizarLancamento()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var lancamento = new Lancamento(usuarioId, DateTime.UtcNow, 100, TipoLancamento.Credito, "Teste");
        await _repository.AdicionarAsync(lancamento);
        await _context.SaveChangesAsync();

        lancamento.Atualizar(200, TipoLancamento.Debito, "Teste Atualizado");

        // Act
        await _repository.AtualizarAsync(lancamento);
        await _context.SaveChangesAsync();

        // Assert
        var result = await _repository.ObterPorIdEUsuarioAsync(lancamento.Id, usuarioId);
        result.Should().NotBeNull();
        result!.Valor.Should().Be(200);
        result.Tipo.Should().Be(TipoLancamento.Debito);
    }

    [Fact]
    public async Task ExcluirAsync_DeveRemoverLancamento()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var lancamento = new Lancamento(usuarioId, DateTime.UtcNow, 100, TipoLancamento.Credito, "Teste");
        await _repository.AdicionarAsync(lancamento);
        await _context.SaveChangesAsync();

        // Act
        await _repository.ExcluirAsync(lancamento.Id);
        await _context.SaveChangesAsync();

        // Assert
        var result = await _repository.ObterPorIdEUsuarioAsync(lancamento.Id, usuarioId);
        result.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}


