using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SSO.Admin.Domain.Entities;
using SSO.Admin.Infrastructure.Data;
using SSO.Admin.Infrastructure.Repositories;
using Xunit;

namespace SSO.Admin.API.Tests.Repositories;

public class UsuarioTokenRepositoryTests : IDisposable
{
    private readonly SSOAdminDbContext _context;
    private readonly UsuarioTokenRepository _repository;

    public UsuarioTokenRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<SSOAdminDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new SSOAdminDbContext(options);
        _repository = new UsuarioTokenRepository(_context);
    }

    [Fact]
    public async Task AdicionarAsync_DeveAdicionarUsuario()
    {
        // Arrange
        var usuario = new UsuarioToken("teste", "hash", "Teste", "teste@teste.com");

        // Act
        await _repository.AdicionarAsync(usuario);
        await _context.SaveChangesAsync();

        // Assert
        var result = await _repository.ObterPorIdAsync(usuario.Id);
        result.Should().NotBeNull();
        result!.Login.Should().Be("teste");
    }

    [Fact]
    public async Task ObterPorLoginAsync_ComLoginExistente_DeveRetornarUsuario()
    {
        // Arrange
        var usuario = new UsuarioToken("teste", "hash", "Teste", "teste@teste.com");
        await _repository.AdicionarAsync(usuario);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ObterPorLoginAsync("teste");

        // Assert
        result.Should().NotBeNull();
        result!.Login.Should().Be("teste");
    }

    [Fact]
    public async Task ObterPorIdAsync_ComIdExistente_DeveRetornarUsuario()
    {
        // Arrange
        var usuario = new UsuarioToken("teste", "hash", "Teste", "teste@teste.com");
        await _repository.AdicionarAsync(usuario);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ObterPorIdAsync(usuario.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(usuario.Id);
    }

    [Fact]
    public async Task ObterTodosAsync_DeveRetornarTodosUsuarios()
    {
        // Arrange
        var usuario1 = new UsuarioToken("teste1", "hash1", "Teste 1", "teste1@teste.com");
        var usuario2 = new UsuarioToken("teste2", "hash2", "Teste 2", "teste2@teste.com");
        await _repository.AdicionarAsync(usuario1);
        await _repository.AdicionarAsync(usuario2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ObterTodosAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task AtualizarAsync_DeveAtualizarUsuario()
    {
        // Arrange
        var usuario = new UsuarioToken("teste", "hash", "Nome Antigo", "antigo@teste.com");
        await _repository.AdicionarAsync(usuario);
        await _context.SaveChangesAsync();

        usuario.Atualizar("Nome Novo", "novo@teste.com");

        // Act
        await _repository.AtualizarAsync(usuario);
        await _context.SaveChangesAsync();

        // Assert
        var result = await _repository.ObterPorIdAsync(usuario.Id);
        result.Should().NotBeNull();
        result!.Nome.Should().Be("Nome Novo");
        result.Email.Should().Be("novo@teste.com");
    }

    [Fact]
    public async Task RemoverAsync_DeveRemoverUsuario()
    {
        // Arrange
        var usuario = new UsuarioToken("teste", "hash", "Teste", "teste@teste.com");
        await _repository.AdicionarAsync(usuario);
        await _context.SaveChangesAsync();

        // Act
        await _repository.RemoverAsync(usuario);
        await _context.SaveChangesAsync();

        // Assert
        var result = await _repository.ObterPorIdAsync(usuario.Id);
        result.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}


