using System.Text.Json;
using FluentAssertions;
using Lancamentos.Application.Converters;
using Lancamentos.Domain.Entities;
using Xunit;

namespace Lancamentos.API.Tests.Converters;

public class TipoLancamentoJsonConverterTests
{
    private readonly JsonSerializerOptions _options;

    public TipoLancamentoJsonConverterTests()
    {
        _options = new JsonSerializerOptions
        {
            Converters = { new TipoLancamentoJsonConverter() }
        };
    }

    [Fact]
    public void Read_ComValorC_DeveRetornarCredito()
    {
        // Arrange
        var json = "\"C\"";

        // Act
        var result = JsonSerializer.Deserialize<TipoLancamento>(json, _options);

        // Assert
        result.Should().Be(TipoLancamento.Credito);
    }

    [Fact]
    public void Read_ComValorD_DeveRetornarDebito()
    {
        // Arrange
        var json = "\"D\"";

        // Act
        var result = JsonSerializer.Deserialize<TipoLancamento>(json, _options);

        // Assert
        result.Should().Be(TipoLancamento.Debito);
    }

    [Fact]
    public void Read_ComValorMinusculoC_DeveRetornarCredito()
    {
        // Arrange
        var json = "\"c\"";

        // Act
        var result = JsonSerializer.Deserialize<TipoLancamento>(json, _options);

        // Assert
        result.Should().Be(TipoLancamento.Credito);
    }

    [Fact]
    public void Read_ComValorInvalido_DeveLancarExcecao()
    {
        // Arrange
        var json = "\"X\"";

        // Act & Assert
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<TipoLancamento>(json, _options));
    }

    [Fact]
    public void Write_ComCredito_DeveSerializarComoC()
    {
        // Arrange
        var tipo = TipoLancamento.Credito;

        // Act
        var result = JsonSerializer.Serialize(tipo, _options);

        // Assert
        result.Should().Be("\"C\"");
    }

    [Fact]
    public void Write_ComDebito_DeveSerializarComoD()
    {
        // Arrange
        var tipo = TipoLancamento.Debito;

        // Act
        var result = JsonSerializer.Serialize(tipo, _options);

        // Assert
        result.Should().Be("\"D\"");
    }
}


