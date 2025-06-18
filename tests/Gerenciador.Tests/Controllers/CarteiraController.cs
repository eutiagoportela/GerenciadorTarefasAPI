using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Gerenciador.Api.Controllers;
using Gerenciador.Aplicacao.UseCases.Carteira.AdicionarSaldo;
using Gerenciador.Aplicacao.UseCases.Carteira.ConsultarSaldo;
using Gerenciador.Comunicacao.DTOs;
using Gerenciador.Comunicacao.Requests.Carteira;
using Gerenciador.Comunicacao.Responses.Carteira;
using Gerenciador.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Gerenciador.Tests.Controllers
{
    public class CarteiraControllerTests
    {
        private readonly Mock<IConsultarSaldoUseCase> _consultarSaldoUseCaseMock = new();
        private readonly Mock<IAdicionarSaldoUseCase> _adicionarSaldoUseCaseMock = new();
        private readonly Mock<ILogger<CarteiraController>> _loggerMock = new();
        private readonly CarteiraController _controller;

        public CarteiraControllerTests()
        {
            _controller = new CarteiraController(
                _consultarSaldoUseCaseMock.Object,
                _adicionarSaldoUseCaseMock.Object,
                _loggerMock.Object);
        }

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

        #region ConsultarSaldo

        [Fact]
        public async Task ConsultarSaldo_DeveRetornarOk_QuandoSucesso()
        {
            int userId = 1;
            MockUserWithId(userId);

            var carteiraResponse = new CarteiraResponse
            {
                UsuarioId = userId,
                Saldo = 100m
            };

            _consultarSaldoUseCaseMock
                .Setup(x => x.ExecuteAsync(userId))
                .ReturnsAsync(carteiraResponse);

            var result = await _controller.ConsultarSaldo();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var resposta = Assert.IsType<RespostaPadrao<CarteiraResponse>>(okResult.Value);
            Assert.Equal(carteiraResponse.Saldo, resposta.Dados.Saldo);
        }

        [Fact]
        public async Task ConsultarSaldo_DeveRetornarUnauthorized_QuandoTokenInvalido()
        {
            // Sem claim de usuário
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
            };

            var result = await _controller.ConsultarSaldo();

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var resposta = Assert.IsType<RespostaPadrao<CarteiraResponse>>(unauthorizedResult.Value);
            Assert.Equal("Acesso não autorizado", resposta.Mensagem);
        }

        [Fact]
        public async Task ConsultarSaldo_DeveRetornarNotFound_QuandoUsuarioNaoEncontrado()
        {
            int userId = 1;
            MockUserWithId(userId);

            _consultarSaldoUseCaseMock
                .Setup(x => x.ExecuteAsync(userId))
                .ThrowsAsync(new UsuarioNaoEncontradoException("Usuário não encontrado"));

            var result = await _controller.ConsultarSaldo();

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var resposta = Assert.IsType<RespostaPadrao<CarteiraResponse>>(notFoundResult.Value);
            Assert.Equal("Usuário não encontrado", resposta.Mensagem);
        }

        [Fact]
        public async Task ConsultarSaldo_DeveRetornarInternalServerError_QuandoDatabaseException()
        {
            int userId = 1;
            MockUserWithId(userId);

            _consultarSaldoUseCaseMock
                .Setup(x => x.ExecuteAsync(userId))
                .ThrowsAsync(new DatabaseException("Erro no banco"));

            var result = await _controller.ConsultarSaldo();

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
            var resposta = Assert.IsType<RespostaPadrao<CarteiraResponse>>(objectResult.Value);
            Assert.StartsWith("Erro no banco de dados", resposta.Mensagem);
        }

        #endregion

        #region AdicionarSaldo

        [Fact]
        public async Task AdicionarSaldo_DeveRetornarOk_QuandoSucesso()
        {
            int userId = 1;
            MockUserWithId(userId);

            var request = new AdicionarSaldoRequest { Valor = 50m };

            var carteiraResponse = new CarteiraResponse
            {
                UsuarioId = userId,
                Saldo = 150m
            };

            _adicionarSaldoUseCaseMock
                .Setup(x => x.ExecuteAsync(userId, request))
                .ReturnsAsync(carteiraResponse);

            var result = await _controller.AdicionarSaldo(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var resposta = Assert.IsType<RespostaPadrao<CarteiraResponse>>(okResult.Value);
            Assert.Equal(150m, resposta.Dados.Saldo);
            Assert.Equal("Saldo adicionado com sucesso", resposta.Mensagem);
        }

        [Fact]
        public async Task AdicionarSaldo_DeveRetornarBadRequest_QuandoCarteiraException()
        {
            int userId = 1;
            MockUserWithId(userId);

            var request = new AdicionarSaldoRequest { Valor = 50m };

            _adicionarSaldoUseCaseMock
                .Setup(x => x.ExecuteAsync(userId, request))
                .ThrowsAsync(new CarteiraException("Erro na carteira"));

            var result = await _controller.AdicionarSaldo(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var resposta = Assert.IsType<RespostaPadrao<CarteiraResponse>>(badRequest.Value);
            Assert.Equal("Erro na carteira", resposta.Mensagem);
        }

        [Fact]
        public async Task AdicionarSaldo_DeveRetornarBadRequest_QuandoArgumentException()
        {
            int userId = 1;
            MockUserWithId(userId);

            var request = new AdicionarSaldoRequest { Valor = 50m };

            _adicionarSaldoUseCaseMock
                .Setup(x => x.ExecuteAsync(userId, request))
                .ThrowsAsync(new ArgumentException("Valor inválido"));

            var result = await _controller.AdicionarSaldo(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var resposta = Assert.IsType<RespostaPadrao<CarteiraResponse>>(badRequest.Value);
            Assert.Equal("Valor inválido", resposta.Mensagem);
        }

        [Fact]
        public async Task AdicionarSaldo_DeveRetornarUnauthorized_QuandoTokenInvalido()
        {
            var request = new AdicionarSaldoRequest { Valor = 50m };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
            };

            var result = await _controller.AdicionarSaldo(request);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            var resposta = Assert.IsType<RespostaPadrao<CarteiraResponse>>(unauthorized.Value);
            Assert.Equal("Acesso não autorizado", resposta.Mensagem);
        }

        [Fact]
        public async Task AdicionarSaldo_DeveRetornarInternalServerError_QuandoDatabaseException()
        {
            int userId = 1;
            MockUserWithId(userId);

            var request = new AdicionarSaldoRequest { Valor = 50m };

            _adicionarSaldoUseCaseMock
                .Setup(x => x.ExecuteAsync(userId, request))
                .ThrowsAsync(new DatabaseException("Erro no banco"));

            var result = await _controller.AdicionarSaldo(request);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);

            var resposta = Assert.IsType<RespostaPadrao<CarteiraResponse>>(objectResult.Value);
            Assert.StartsWith("Erro no banco de dados", resposta.Mensagem);
        }

        #endregion
    }
}
