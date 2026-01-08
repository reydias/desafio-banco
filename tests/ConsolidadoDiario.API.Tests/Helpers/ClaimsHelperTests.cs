using System.Security.Claims;
using ConsolidadoDiario.API.Helpers;
using FluentAssertions;
using Xunit;

namespace ConsolidadoDiario.API.Tests.Helpers;

public class ClaimsHelperTests
{
    [Fact]
    public void ObterUsuarioId_ComClaimValido_DeveRetornarGuid()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = ClaimsHelper.ObterUsuarioId(principal);

        // Assert
        result.Should().Be(usuarioId);
    }

    [Fact]
    public void ObterUsuarioIdOuLancarExcecao_ComClaimValido_DeveRetornarGuid()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = ClaimsHelper.ObterUsuarioIdOuLancarExcecao(principal);

        // Assert
        result.Should().Be(usuarioId);
    }

    [Fact]
    public void ObterUsuarioIdOuLancarExcecao_SemClaim_DeveLancarExcecao()
    {
        // Arrange
        var identity = new ClaimsIdentity();
        var principal = new ClaimsPrincipal(identity);

        // Act & Assert
        Assert.Throws<UnauthorizedAccessException>(() => ClaimsHelper.ObterUsuarioIdOuLancarExcecao(principal));
    }
}


