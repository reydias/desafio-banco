using FluentAssertions;
using Lancamentos.Domain.Entities;
using Xunit;

namespace Lancamentos.API.Tests.Domain.Entities;

public class LancamentoTests
{
    [Fact]
    public void Constructor_ComDadosValidos_DeveCriarLancamento()
    {
        // Arrange & Act
        var usuarioId = Guid.NewGuid();
        var data = DateTime.UtcNow;
        var lancamento = new Lancamento(usuarioId, data, 100, TipoLancamento.Credito, "Teste");

        // Assert
        lancamento.Should().NotBeNull();
        lancamento.Valor.Should().Be(100);
        lancamento.Tipo.Should().Be(TipoLancamento.Credito);
        lancamento.Descricao.Should().Be("Teste");
        lancamento.UsuarioId.Should().Be(usuarioId);
    }

    [Fact]
    public void Atualizar_ComDadosValidos_DeveAtualizarLancamento()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var lancamento = new Lancamento(usuarioId, DateTime.UtcNow, 100, TipoLancamento.Credito, "Teste");

        // Act
        lancamento.Atualizar(200, TipoLancamento.Debito, "Teste Atualizado");

        // Assert
        lancamento.Valor.Should().Be(200);
        lancamento.Tipo.Should().Be(TipoLancamento.Debito);
        lancamento.Descricao.Should().Be("Teste Atualizado");
    }

    [Fact]
    public void Atualizar_ComDescricaoNula_DeveLancarExcecao()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var lancamento = new Lancamento(usuarioId, DateTime.UtcNow, 100, TipoLancamento.Credito, "Teste");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => lancamento.Atualizar(200, TipoLancamento.Debito, null!));
    }
}


