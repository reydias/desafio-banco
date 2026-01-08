using System.Security.Claims;
using FluentAssertions;
using Lancamentos.Application.Commands;
using Lancamentos.Application.DTOs;
using Lancamentos.Application.Handlers;
using Lancamentos.Application.Queries;
using Lancamentos.Domain.Entities;
using Lancamentos.Domain.Events;
using Lancamentos.Domain.Interfaces;
using Lancamentos.API.Controllers;
using Lancamentos.API.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Lancamentos.API.Tests.Controllers;

public class LancamentosControllerTests
{
    private readonly Mock<ILancamentoRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly CriarLancamentoHandler _criarHandler;
    private readonly ObterLancamentoHandler _obterHandler;
    private readonly Mock<ILogger<LancamentosController>> _loggerMock;
    private readonly LancamentosController _controller;

    public LancamentosControllerTests()
    {
        _repositoryMock = new Mock<ILancamentoRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _criarHandler = new CriarLancamentoHandler(_repositoryMock.Object, _unitOfWorkMock.Object, _eventPublisherMock.Object);
        _obterHandler = new ObterLancamentoHandler(_repositoryMock.Object);
        _loggerMock = new Mock<ILogger<LancamentosController>>();
        _controller = new LancamentosController(_criarHandler, _obterHandler, _loggerMock.Object);
        
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
    public async Task Criar_ComDadosValidos_DeveRetornar201()
    {
        // Arrange
        var usuarioId = ClaimsHelper.ObterUsuarioIdOuLancarExcecao(_controller.User);
        var dto = new CriarLancamentoDTO
        {
            Data = DateTime.UtcNow,
            Valor = 100,
            Tipo = TipoLancamento.Credito,
            Descricao = "Teste"
        };

        _repositoryMock.Setup(r => r.AdicionarAsync(It.IsAny<Lancamento>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Lancamento l, CancellationToken ct) => l);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _eventPublisherMock.Setup(e => e.PublicarAsync(It.IsAny<LancamentoCriadoEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Criar(dto, CancellationToken.None);

        // Assert
        var createdAtResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdAtResult.StatusCode.Should().Be(201);
        createdAtResult.Value.Should().BeOfType<LancamentoDTO>();
    }

    [Fact]
    public async Task Criar_ComDadosInvalidos_DeveRetornar400()
    {
        // Arrange
        var dto = new CriarLancamentoDTO
        {
            Data = DateTime.UtcNow,
            Valor = 0,
            Tipo = TipoLancamento.Credito,
            Descricao = "Teste"
        };

        // Act
        var result = await _controller.Criar(dto, CancellationToken.None);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task ObterPorId_ComIdExistente_DeveRetornar200()
    {
        // Arrange
        var usuarioId = ClaimsHelper.ObterUsuarioIdOuLancarExcecao(_controller.User);
        var id = Guid.NewGuid();
        var lancamento = new Lancamento(usuarioId, DateTime.UtcNow, 100, TipoLancamento.Credito, "Teste");

        _repositoryMock.Setup(r => r.ObterPorIdEUsuarioAsync(id, usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lancamento);

        // Act
        var result = await _controller.ObterPorId(id, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeOfType<LancamentoDTO>();
    }

    [Fact]
    public async Task ObterPorId_ComIdInexistente_DeveRetornar404()
    {
        // Arrange
        var usuarioId = ClaimsHelper.ObterUsuarioIdOuLancarExcecao(_controller.User);
        var id = Guid.NewGuid();

        _repositoryMock.Setup(r => r.ObterPorIdEUsuarioAsync(id, usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Lancamento?)null);

        // Act
        var result = await _controller.ObterPorId(id, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Listar_DeveRetornar200()
    {
        // Arrange
        var usuarioId = ClaimsHelper.ObterUsuarioIdOuLancarExcecao(_controller.User);
        var lancamentos = new List<Lancamento>
        {
            new Lancamento(usuarioId, DateTime.UtcNow, 100, TipoLancamento.Credito, "Teste 1")
        };

        _repositoryMock.Setup(r => r.ObterTodosPorUsuarioAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lancamentos);

        // Act
        var result = await _controller.Listar(null, null, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }
}
