using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SSO.Admin.Application.Commands;
using SSO.Admin.Application.DTOs;
using SSO.Admin.Application.Handlers;
using SSO.Admin.Application.Queries;
using SSO.Admin.API.Controllers;
using SSO.Admin.Domain.Interfaces;
using Xunit;

namespace SSO.Admin.API.Tests.Controllers;

public class UsuariosTokenControllerTests
{
    private readonly Mock<IUsuarioTokenRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CriarUsuarioTokenHandler _criarHandler;
    private readonly AtualizarUsuarioTokenHandler _atualizarHandler;
    private readonly AtualizarSenhaHandler _atualizarSenhaHandler;
    private readonly ObterUsuarioTokenHandler _obterHandler;
    private readonly Mock<ILogger<UsuariosTokenController>> _loggerMock;
    private readonly UsuariosTokenController _controller;

    public UsuariosTokenControllerTests()
    {
        _repositoryMock = new Mock<IUsuarioTokenRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        
        var criarLoggerMock = new Mock<ILogger<CriarUsuarioTokenHandler>>();
        _criarHandler = new CriarUsuarioTokenHandler(_repositoryMock.Object, _unitOfWorkMock.Object, criarLoggerMock.Object);
        
        var atualizarLoggerMock = new Mock<ILogger<AtualizarUsuarioTokenHandler>>();
        _atualizarHandler = new AtualizarUsuarioTokenHandler(_repositoryMock.Object, _unitOfWorkMock.Object, atualizarLoggerMock.Object);
        
        var atualizarSenhaLoggerMock = new Mock<ILogger<AtualizarSenhaHandler>>();
        _atualizarSenhaHandler = new AtualizarSenhaHandler(_repositoryMock.Object, _unitOfWorkMock.Object, atualizarSenhaLoggerMock.Object);
        
        var obterLoggerMock = new Mock<ILogger<ObterUsuarioTokenHandler>>();
        _obterHandler = new ObterUsuarioTokenHandler(_repositoryMock.Object, obterLoggerMock.Object);
        
        _loggerMock = new Mock<ILogger<UsuariosTokenController>>();
        _controller = new UsuariosTokenController(
            _criarHandler,
            _atualizarHandler,
            _atualizarSenhaHandler,
            _obterHandler,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Criar_ComDadosValidos_DeveRetornar201()
    {
        // Arrange
        var dto = new CriarUsuarioTokenDTO
        {
            Login = "teste",
            Senha = "123456",
            Nome = "Teste",
            Email = "teste@teste.com"
        };

        _repositoryMock.Setup(r => r.ObterPorLoginAsync(dto.Login, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SSO.Admin.Domain.Entities.UsuarioToken?)null);
        _repositoryMock.Setup(r => r.AdicionarAsync(It.IsAny<SSO.Admin.Domain.Entities.UsuarioToken>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _controller.Criar(dto, CancellationToken.None);

        // Assert
        var createdAtResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdAtResult.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task ObterPorId_ComIdExistente_DeveRetornar200()
    {
        // Arrange
        var id = Guid.NewGuid();
        var usuario = new SSO.Admin.Domain.Entities.UsuarioToken("teste", "hash", "Teste", "teste@teste.com");

        _repositoryMock.Setup(r => r.ObterPorIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        // Act
        var result = await _controller.ObterPorId(id, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task ObterPorId_ComIdInexistente_DeveRetornar404()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repositoryMock.Setup(r => r.ObterPorIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SSO.Admin.Domain.Entities.UsuarioToken?)null);

        // Act
        var result = await _controller.ObterPorId(id, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }
}
