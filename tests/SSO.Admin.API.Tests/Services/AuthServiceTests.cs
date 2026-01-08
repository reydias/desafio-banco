using BCrypt.Net;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SSO.Admin.Domain.Entities;
using SSO.Admin.Domain.Interfaces;
using SSO.Admin.Infrastructure.Services;
using Xunit;

namespace SSO.Admin.API.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUsuarioTokenRepository> _repositoryMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _repositoryMock = new Mock<IUsuarioTokenRepository>();
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<AuthService>>();

        _configurationMock.Setup(c => c["Jwt:SecretKey"]).Returns("YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!");
        _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns("SSOAPI");
        _configurationMock.Setup(c => c["Jwt:Audience"]).Returns("SSOAPI");
        _configurationMock.Setup(c => c["Jwt:ExpirationMinutes"]).Returns("60");

        _service = new AuthService(_repositoryMock.Object, _configurationMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GerarTokenAsync_ComCredenciaisValidas_DeveRetornarToken()
    {
        // Arrange
        var login = "admin";
        var senha = "123456";
        var senhaHash = BCrypt.Net.BCrypt.HashPassword(senha);
        var usuario = new UsuarioToken(login, senhaHash, "Admin", "admin@teste.com");

        _repositoryMock.Setup(r => r.ObterPorLoginAsync(login, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        // Act
        var result = await _service.GerarTokenAsync(login, senha);

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GerarTokenAsync_ComUsuarioInexistente_DeveLancarExcecao()
    {
        // Arrange
        var login = "admin";
        var senha = "123456";

        _repositoryMock.Setup(r => r.ObterPorLoginAsync(login, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UsuarioToken?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.GerarTokenAsync(login, senha));
    }

    [Fact]
    public async Task GerarTokenAsync_ComSenhaInvalida_DeveLancarExcecao()
    {
        // Arrange
        var login = "admin";
        var senha = "123456";
        var senhaHash = BCrypt.Net.BCrypt.HashPassword("senhaDiferente");
        var usuario = new UsuarioToken(login, senhaHash, "Admin", "admin@teste.com");

        _repositoryMock.Setup(r => r.ObterPorLoginAsync(login, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.GerarTokenAsync(login, senha));
    }

    [Fact]
    public async Task GerarTokenAsync_ComUsuarioInativo_DeveLancarExcecao()
    {
        // Arrange
        var login = "admin";
        var senha = "123456";
        var senhaHash = BCrypt.Net.BCrypt.HashPassword(senha);
        var usuario = new UsuarioToken(login, senhaHash, "Admin", "admin@teste.com");
        usuario.Desativar();

        _repositoryMock.Setup(r => r.ObterPorLoginAsync(login, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.GerarTokenAsync(login, senha));
    }
}


