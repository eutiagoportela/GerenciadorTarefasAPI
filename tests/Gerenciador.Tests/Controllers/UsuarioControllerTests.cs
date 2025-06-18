using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Gerenciador.Api.Controllers;
using Gerenciador.Aplicacao.UseCases.Usuario.CriarUsuario;
using Gerenciador.Aplicacao.UseCases.Usuario.ObterUsuario;
using Gerenciador.Comunicacao.DTOs;
using Gerenciador.Comunicacao.Requests.Usuario;
using Gerenciador.Comunicacao.Responses.Usuario;
using Gerenciador.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Gerenciador.Tests.Controllers
{
    public class UsuarioControllerTests
    {
        private readonly Mock<ICriarUsuarioUseCase> _criarUsuarioUseCaseMock = new();
        private readonly Mock<IObterUsuarioUseCase> _obterUsuarioUseCaseMock = new();
        private readonly Mock<ILogger<UsuarioController>> _loggerMock = new();
        private readonly UsuarioController _controller;

        public UsuarioControllerTests()
        {
            _controller = new UsuarioController(
                _criarUsuarioUseCaseMock.Object,
                _obterUsuarioUseCaseMock.Object,
                _loggerMock.Object);
        }

        #region Criar

        [Fact]
        public async Task Criar_DeveRetornarCreated_QuandoSucesso()
        {
            var request = new CriarUsuarioRequest
            {
                Nome = "Tiago",
                Email = "teste@email.com",
                Senha = "12345678",
                ConfirmarSenha = "12345678"
            };

            var usuarioResponse = new UsuarioResponse
            {
                Id = 1,
                Nome = "Tiago",
                Email = "teste@email.com"
            };

            _criarUsuarioUseCaseMock
                .Setup(x => x.ExecuteAsync(request))
                .ReturnsAsync(usuarioResponse);

            var result = await _controller.Criar(request);

            var createdResult = Assert.IsType<CreatedResult>(result);
            var resposta = Assert.IsType<RespostaPadrao<UsuarioResponse>>(createdResult.Value);
            Assert.Equal("Usuário criado com sucesso", resposta.Mensagem);
            Assert.Equal(usuarioResponse.Id, resposta.Dados.Id);
        }

        [Fact]
        public async Task Criar_DeveRetornarBadRequest_QuandoEmailJaExisteException()
        {
            var request = new CriarUsuarioRequest { Email = "teste@email.com" };

            _criarUsuarioUseCaseMock
                .Setup(x => x.ExecuteAsync(request))
                .ThrowsAsync(new EmailJaExisteException("E-mail já cadastrado"));

            var result = await _controller.Criar(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var resposta = Assert.IsType<RespostaPadrao<UsuarioResponse>>(badRequest.Value);
            Assert.Equal("E-mail já cadastrado", resposta.Mensagem);
        }

        [Fact]
        public async Task Criar_DeveRetornarBadRequest_QuandoArgumentException()
        {
            var request = new CriarUsuarioRequest { Email = "teste@email.com" };

            _criarUsuarioUseCaseMock
                .Setup(x => x.ExecuteAsync(request))
                .ThrowsAsync(new ArgumentException("Dados inválidos"));

            var result = await _controller.Criar(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var resposta = Assert.IsType<RespostaPadrao<UsuarioResponse>>(badRequest.Value);
            Assert.Equal("Dados inválidos", resposta.Mensagem);
        }

        [Fact]
        public async Task Criar_DeveRetornarInternalServerError_QuandoDatabaseException()
        {
            var request = new CriarUsuarioRequest { Email = "teste@email.com" };

            _criarUsuarioUseCaseMock
                .Setup(x => x.ExecuteAsync(request))
                .ThrowsAsync(new DatabaseException("Erro de banco"));

            var result = await _controller.Criar(request);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);

            var resposta = Assert.IsType<RespostaPadrao<UsuarioResponse>>(objectResult.Value);
            Assert.StartsWith("Erro no banco de dados", resposta.Mensagem);
        }

        [Fact]
        public async Task Criar_DeveRetornarInternalServerError_QuandoExceptionGenerica()
        {
            var request = new CriarUsuarioRequest { Email = "teste@email.com" };

            _criarUsuarioUseCaseMock
                .Setup(x => x.ExecuteAsync(request))
                .ThrowsAsync(new Exception("Erro inesperado"));

            var result = await _controller.Criar(request);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);

            var resposta = Assert.IsType<RespostaPadrao<UsuarioResponse>>(objectResult.Value);
            Assert.StartsWith("Erro interno do servidor", resposta.Mensagem);
        }

        #endregion

        #region ObterPerfil

        private void MockUserWithId(int id)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, id.ToString())
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task ObterPerfil_DeveRetornarOk_QuandoSucesso()
        {
            int userId = 1;

            MockUserWithId(userId);

            var usuarioResponse = new UsuarioResponse
            {
                Id = userId,
                Nome = "Tiago",
                Email = "teste@email.com"
            };

            _obterUsuarioUseCaseMock
                .Setup(x => x.ExecuteAsync(userId))
                .ReturnsAsync(usuarioResponse);

            var result = await _controller.ObterPerfil();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var resposta = Assert.IsType<RespostaPadrao<UsuarioResponse>>(okResult.Value);
            Assert.Equal(usuarioResponse.Id, resposta.Dados.Id);
            Assert.Equal(usuarioResponse.Nome, resposta.Dados.Nome);
        }

        [Fact]
        public async Task ObterPerfil_DeveRetornarUnauthorized_QuandoTokenInvalido()
        {
            // Sem claim NameIdentifier ou inválido

            var user = new ClaimsPrincipal(new ClaimsIdentity()); // sem claims

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var result = await _controller.ObterPerfil();

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var resposta = Assert.IsType<RespostaPadrao<UsuarioResponse>>(unauthorizedResult.Value);
            Assert.Equal("Acesso não autorizado", resposta.Mensagem);
        }

        [Fact]
        public async Task ObterPerfil_DeveRetornarNotFound_QuandoUsuarioNaoEncontrado()
        {
            int userId = 1;
            MockUserWithId(userId);

            _obterUsuarioUseCaseMock
                .Setup(x => x.ExecuteAsync(userId))
                .ThrowsAsync(new UsuarioNaoEncontradoException("Usuário não encontrado"));

            var result = await _controller.ObterPerfil();

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var resposta = Assert.IsType<RespostaPadrao<UsuarioResponse>>(notFoundResult.Value);
            Assert.Equal("Usuário não encontrado", resposta.Mensagem);
        }

        [Fact]
        public async Task ObterPerfil_DeveRetornarInternalServerError_QuandoExceptionGenerica()
        {
            int userId = 1;
            MockUserWithId(userId);

            _obterUsuarioUseCaseMock
                .Setup(x => x.ExecuteAsync(userId))
                .ThrowsAsync(new Exception("Erro inesperado"));

            var result = await _controller.ObterPerfil();

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);

            var resposta = Assert.IsType<RespostaPadrao<UsuarioResponse>>(objectResult.Value);
            Assert.StartsWith("Erro interno do servidor", resposta.Mensagem);
        }

        #endregion
    }
}
