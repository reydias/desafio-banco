using System.Security.Claims;
using FluentAssertions;
using Lancamentos.API.Helpers;
using Xunit;

namespace Lancamentos.API.Tests.Helpers;

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
    public void ObterUsuarioId_ComClaimInvalido_DeveRetornarNull()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "invalid-guid")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = ClaimsHelper.ObterUsuarioId(principal);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ObterUsuarioId_SemClaim_DeveRetornarNull()
    {
        // Arrange
        var identity = new ClaimsIdentity();
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = ClaimsHelper.ObterUsuarioId(principal);

        // Assert
        result.Should().BeNull();
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
    public void ObterUsuarioIdOuLancarExcecao_ComClaimInvalido_DeveLancarExcecao()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "invalid-guid")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        // Act & Assert
        Assert.Throws<UnauthorizedAccessException>(() => ClaimsHelper.ObterUsuarioIdOuLancarExcecao(principal));
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


