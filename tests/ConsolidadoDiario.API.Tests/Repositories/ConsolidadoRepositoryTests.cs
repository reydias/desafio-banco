using ConsolidadoDiario.Domain.Entities;
using ConsolidadoDiario.Infrastructure.Data;
using ConsolidadoDiario.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using ConsolidadoEntity = ConsolidadoDiario.Domain.Entities.ConsolidadoDiario;

namespace ConsolidadoDiario.API.Tests.Repositories;

public class ConsolidadoRepositoryTests : IDisposable
{
    private readonly ConsolidadoDbContext _context;
    private readonly ConsolidadoRepository _repository;

    public ConsolidadoRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ConsolidadoDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ConsolidadoDbContext(options);
        _repository = new ConsolidadoRepository(_context);
    }

    [Fact]
    public async Task AdicionarAsync_DeveAdicionarConsolidado()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var consolidado = new ConsolidadoEntity(usuarioId, DateTime.UtcNow.Date);

        // Act
        var result = await _repository.AdicionarAsync(consolidado);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(consolidado.Id);
    }

    [Fact]
    public async Task ObterPorDataEUsuarioAsync_ComDataExistente_DeveRetornarConsolidado()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var data = DateTime.UtcNow.Date;
        var consolidado = new ConsolidadoEntity(usuarioId, data);
        consolidado.AdicionarCredito(100);
        await _repository.AdicionarAsync(consolidado);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ObterPorDataEUsuarioAsync(data, usuarioId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(consolidado.Id);
    }

    [Fact]
    public async Task ObterPorDataEUsuarioAsync_ComUsuarioDiferente_DeveRetornarNull()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var outroUsuarioId = Guid.NewGuid();
        var data = DateTime.UtcNow.Date;
        var consolidado = new ConsolidadoEntity(usuarioId, data);
        await _repository.AdicionarAsync(consolidado);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ObterPorDataEUsuarioAsync(data, outroUsuarioId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AtualizarAsync_DeveAtualizarConsolidado()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var data = DateTime.UtcNow.Date;
        var consolidado = new ConsolidadoEntity(usuarioId, data);
        consolidado.AdicionarCredito(100);
        await _repository.AdicionarAsync(consolidado);
        await _context.SaveChangesAsync();

        consolidado.AdicionarDebito(50);

        // Act
        await _repository.AtualizarAsync(consolidado);
        await _context.SaveChangesAsync();

        // Assert
        var result = await _repository.ObterPorDataEUsuarioAsync(data, usuarioId);
        result.Should().NotBeNull();
        result!.TotalDebitos.Should().Be(50);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
