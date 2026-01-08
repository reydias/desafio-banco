using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SSO.Admin.Application.DTOs;
using SSO.Admin.Application.Handlers;
using SSO.Admin.Domain.Interfaces;
using Xunit;

namespace SSO.Admin.API.Tests.Handlers;

public class LoginHandlerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<LoginHandler>> _loggerMock;
    private readonly LoginHandler _handler;

    public LoginHandlerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<LoginHandler>>();

        var configurationSection = new Mock<IConfigurationSection>();
        configurationSection.Setup(c => c.Value).Returns("60");
        _configurationMock.Setup(c => c["Jwt:ExpirationMinutes"]).Returns("60");

        _handler = new LoginHandler(_authServiceMock.Object, _configurationMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ComCredenciaisValidas_DeveRetornarToken()
    {
        // Arrange
        var loginDTO = new LoginDTO
        {
            Login = "admin",
            Senha = "123456"
        };

        var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.test";
        _authServiceMock.Setup(a => a.GerarTokenAsync(loginDTO.Login, loginDTO.Senha, It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        // Act
        var result = await _handler.HandleAsync(loginDTO);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be(token);
        result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(60), TimeSpan.FromSeconds(5));
    }
}


