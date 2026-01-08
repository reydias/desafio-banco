using System.Security.Claims;
using ConsolidadoDiario.Application.DTOs;
using ConsolidadoDiario.Application.Handlers;
using ConsolidadoDiario.Application.Queries;
using ConsolidadoDiario.API.Controllers;
using ConsolidadoDiario.API.Helpers;
using ConsolidadoDiario.Domain.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ConsolidadoDiario.API.Tests.Controllers;

public class ConsolidadoControllerTests
{
    private readonly Mock<IConsolidadoRepository> _repositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ILogger<ObterConsolidadoHandler>> _handlerLoggerMock;
    private readonly ObterConsolidadoHandler _handler;
    private readonly Mock<ILogger<ConsolidadoController>> _loggerMock;
    private readonly ConsolidadoController _controller;

    public ConsolidadoControllerTests()
    {
        _repositoryMock = new Mock<IConsolidadoRepository>();
        _cacheServiceMock = new Mock<ICacheService>();
        _handlerLoggerMock = new Mock<ILogger<ObterConsolidadoHandler>>();
        _handler = new ObterConsolidadoHandler(_repositoryMock.Object, _cacheServiceMock.Object, _handlerLoggerMock.Object);
        _loggerMock = new Mock<ILogger<ConsolidadoController>>();
        _controller = new ConsolidadoController(_handler, _loggerMock.Object);

        // Setup User claims
        var usuarioId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "Test");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };
    }

    [Fact]
    public async Task ObterConsolidado_ComDataExistente_DeveRetornar200()
    {
        // Arrange
        var usuarioId = ClaimsHelper.ObterUsuarioIdOuLancarExcecao(_controller.User);
        var data = DateTime.UtcNow.Date;
        var cacheKey = $"consolidado:{usuarioId}:{data:yyyy-MM-dd}";
        var dto = new ConsolidadoDiarioDTO
        {
            Data = data,
            TotalCreditos = 100,
            TotalDebitos = 50,
            SaldoDiario = 50,
            QuantidadeLancamentos = 2
        };

        _cacheServiceMock.Setup(c => c.ObterAsync<ConsolidadoDiarioDTO>(cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConsolidadoDiarioDTO?)null);
        
        var consolidado = new ConsolidadoDiario.Domain.Entities.ConsolidadoDiario(usuarioId, data);
        consolidado.AdicionarCredito(100);
        consolidado.AdicionarDebito(50);
        
        _repositoryMock.Setup(r => r.ObterPorDataEUsuarioAsync(data, usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(consolidado);
        _cacheServiceMock.Setup(c => c.DefinirAsync(It.IsAny<string>(), It.IsAny<ConsolidadoDiarioDTO>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.ObterConsolidado(data, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeOfType<ConsolidadoDiarioDTO>();
    }

    [Fact]
    public async Task ObterConsolidado_ComDataInexistente_DeveRetornar404()
    {
        // Arrange
        var usuarioId = ClaimsHelper.ObterUsuarioIdOuLancarExcecao(_controller.User);
        var data = DateTime.UtcNow.Date;
        var cacheKey = $"consolidado:{usuarioId}:{data:yyyy-MM-dd}";

        _cacheServiceMock.Setup(c => c.ObterAsync<ConsolidadoDiarioDTO>(cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConsolidadoDiarioDTO?)null);
        _repositoryMock.Setup(r => r.ObterPorDataEUsuarioAsync(data, usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConsolidadoDiario.Domain.Entities.ConsolidadoDiario?)null);

        // Act
        var result = await _controller.ObterConsolidado(data, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }
}
