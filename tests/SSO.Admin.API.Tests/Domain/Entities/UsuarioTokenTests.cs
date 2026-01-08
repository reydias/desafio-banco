using FluentAssertions;
using SSO.Admin.Domain.Entities;
using Xunit;

namespace SSO.Admin.API.Tests.Domain.Entities;

public class UsuarioTokenTests
{
    [Fact]
    public void Constructor_ComDadosValidos_DeveCriarUsuario()
    {
        // Arrange & Act
        var usuario = new UsuarioToken("teste", "hash", "Teste", "teste@teste.com");

        // Assert
        usuario.Should().NotBeNull();
        usuario.Login.Should().Be("teste");
        usuario.Nome.Should().Be("Teste");
        usuario.Email.Should().Be("teste@teste.com");
        usuario.Ativo.Should().BeTrue();
    }

    [Fact]
    public void Atualizar_ComDadosValidos_DeveAtualizarUsuario()
    {
        // Arrange
        var usuario = new UsuarioToken("teste", "hash", "Nome Antigo", "antigo@teste.com");

        // Act
        usuario.Atualizar("Nome Novo", "novo@teste.com");

        // Assert
        usuario.Nome.Should().Be("Nome Novo");
        usuario.Email.Should().Be("novo@teste.com");
        usuario.DataAtualizacao.Should().NotBeNull();
    }

    [Fact]
    public void AtualizarSenha_DeveAtualizarSenha()
    {
        // Arrange
        var usuario = new UsuarioToken("teste", "hashAntigo", "Teste", "teste@teste.com");

        // Act
        usuario.AtualizarSenha("hashNovo");

        // Assert
        usuario.SenhaHash.Should().Be("hashNovo");
        usuario.DataAtualizacao.Should().NotBeNull();
    }

    [Fact]
    public void Ativar_DeveAtivarUsuario()
    {
        // Arrange
        var usuario = new UsuarioToken("teste", "hash", "Teste", "teste@teste.com");
        usuario.Desativar();

        // Act
        usuario.Ativar();

        // Assert
        usuario.Ativo.Should().BeTrue();
    }

    [Fact]
    public void Desativar_DeveDesativarUsuario()
    {
        // Arrange
        var usuario = new UsuarioToken("teste", "hash", "Teste", "teste@teste.com");

        // Act
        usuario.Desativar();

        // Assert
        usuario.Ativo.Should().BeFalse();
    }
}


