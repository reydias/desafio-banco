using ConsolidadoDiario.Domain.Entities;
using FluentAssertions;
using Xunit;
using ConsolidadoEntity = ConsolidadoDiario.Domain.Entities.ConsolidadoDiario;

namespace ConsolidadoDiario.API.Tests.Domain.Entities;

public class ConsolidadoDiarioTests
{
    [Fact]
    public void Constructor_ComDadosValidos_DeveCriarConsolidado()
    {
        // Arrange & Act
        var usuarioId = Guid.NewGuid();
        var data = DateTime.UtcNow.Date;
        var consolidado = new ConsolidadoEntity(usuarioId, data);

        // Assert
        consolidado.Should().NotBeNull();
        consolidado.UsuarioId.Should().Be(usuarioId);
        consolidado.Data.Should().Be(data);
        consolidado.TotalCreditos.Should().Be(0);
        consolidado.TotalDebitos.Should().Be(0);
        consolidado.SaldoDiario.Should().Be(0);
        consolidado.QuantidadeLancamentos.Should().Be(0);
    }

    [Fact]
    public void AdicionarCredito_DeveIncrementarCreditos()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var consolidado = new ConsolidadoEntity(usuarioId, DateTime.UtcNow.Date);

        // Act
        consolidado.AdicionarCredito(100);
        consolidado.AdicionarCredito(50);

        // Assert
        consolidado.TotalCreditos.Should().Be(150);
        consolidado.QuantidadeLancamentos.Should().Be(2);
        consolidado.SaldoDiario.Should().Be(150);
    }

    [Fact]
    public void AdicionarDebito_DeveIncrementarDebitos()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var consolidado = new ConsolidadoEntity(usuarioId, DateTime.UtcNow.Date);

        // Act
        consolidado.AdicionarDebito(100);
        consolidado.AdicionarDebito(50);

        // Assert
        consolidado.TotalDebitos.Should().Be(150);
        consolidado.QuantidadeLancamentos.Should().Be(2);
        consolidado.SaldoDiario.Should().Be(-150);
    }

    [Fact]
    public void RecalcularSaldo_DeveCalcularSaldoCorretamente()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var consolidado = new ConsolidadoEntity(usuarioId, DateTime.UtcNow.Date);
        consolidado.AdicionarCredito(200);
        consolidado.AdicionarDebito(50);

        // Assert
        consolidado.SaldoDiario.Should().Be(150);
    }

    [Fact]
    public void Criar_ComDadosValidos_DeveCriarConsolidado()
    {
        // Arrange & Act
        var usuarioId = Guid.NewGuid();
        var consolidado = ConsolidadoEntity.Criar(usuarioId, DateTime.UtcNow.Date, 100, 50, 2);

        // Assert
        consolidado.Should().NotBeNull();
        consolidado.TotalCreditos.Should().Be(100);
        consolidado.TotalDebitos.Should().Be(50);
        consolidado.QuantidadeLancamentos.Should().Be(2);
        consolidado.SaldoDiario.Should().Be(50);
    }
}


