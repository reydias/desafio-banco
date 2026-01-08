using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SSO.Admin.Application.DTOs;
using SSO.Admin.Application.Handlers;
using SSO.Admin.API.Controllers;
using SSO.Admin.Domain.Interfaces;
using Xunit;

namespace SSO.Admin.API.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<LoginHandler>> _handlerLoggerMock;
    private readonly LoginHandler _handler;
    private readonly Mock<ILogger<AuthController>> _loggerMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _configurationMock = new Mock<IConfiguration>();
        _handlerLoggerMock = new Mock<ILogger<LoginHandler>>();
        
        var configurationSection = new Mock<IConfigurationSection>();
        configurationSection.Setup(c => c.Value).Returns("60");
        _configurationMock.Setup(c => c["Jwt:ExpirationMinutes"]).Returns("60");
        
        _handler = new LoginHandler(_authServiceMock.Object, _configurationMock.Object, _handlerLoggerMock.Object);
        _loggerMock = new Mock<ILogger<AuthController>>();
        _controller = new AuthController(_handler, _loggerMock.Object);
    }

    [Fact]
    public async Task Login_ComCredenciaisValidas_DeveRetornar200()
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
        var result = await _controller.Login(loginDTO, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeOfType<TokenDTO>();
    }

    [Fact]
    public async Task Login_ComCredenciaisInvalidas_DeveRetornar401()
    {
        // Arrange
        var loginDTO = new LoginDTO
        {
            Login = "admin",
            Senha = "senhaErrada"
        };

        _authServiceMock.Setup(a => a.GerarTokenAsync(loginDTO.Login, loginDTO.Senha, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Login ou senha inválidos"));

        // Act
        var result = await _controller.Login(loginDTO, CancellationToken.None);

        // Assert
        var unauthorizedResult = result.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorizedResult.StatusCode.Should().Be(401);
    }
}

