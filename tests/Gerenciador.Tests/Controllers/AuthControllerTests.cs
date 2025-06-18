using System;
using System.Threading.Tasks;
using Gerenciador.Api.Controllers;
using Gerenciador.Aplicacao.UseCases.Login.DoLogin;
using Gerenciador.Comunicacao.Requests.Usuario;
using Gerenciador.Comunicacao.Responses.Usuario;
using Gerenciador.Comunicacao.DTOs;
using Gerenciador.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Gerenciador.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly AuthController _controller;
        private readonly Mock<IDoLoginUseCase> _doLoginUseCaseMock = new();
        private readonly Mock<ILogger<AuthController>> _loggerMock = new();

        public AuthControllerTests()
        {
            _controller = new AuthController(_doLoginUseCaseMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task LoginAsync_DeveRetornarOk_QuandoCredenciaisForemValidas()
        {
            // Arrange
            var request = new LoginRequest { Email = "teste@email.com", Senha = "12345678" };

            var loginResponse = new LoginResponse
            {
                Token = "token_exemplo",
                ExpiracaoToken = DateTime.UtcNow.AddHours(1),
                Usuario = new UsuarioResponse
                {
                    Id = new Random().Next(1, 10000), // Gera um int aleatório
                    Nome = "Tiago",
                    Email = "teste@email.com"
                }
            };

            _doLoginUseCaseMock
                .Setup(x => x.ExecuteAsync(request))
                .ReturnsAsync(loginResponse);

            // Act
            var result = await _controller.LoginAsync(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var resposta = Assert.IsType<RespostaPadrao<LoginResponse>>(okResult.Value);
            Assert.Equal("Login realizado com sucesso", resposta.Mensagem);
            Assert.NotNull(resposta.Dados);
            Assert.Equal("Tiago", resposta.Dados.Usuario.Nome);
            Assert.Equal("teste@email.com", resposta.Dados.Usuario.Email);
        }


        [Fact]
        public async Task LoginAsync_DeveRetornarBadRequest_QuandoArgumentExceptionForLancada()
        {
            // Arrange
            var request = new LoginRequest { Email = "invalido", Senha = "123" };

            _doLoginUseCaseMock
                .Setup(x => x.ExecuteAsync(request))
                .ThrowsAsync(new ArgumentException("E-mail inválido"));

            // Act
            var result = await _controller.LoginAsync(request);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var resposta = Assert.IsType<RespostaPadrao<LoginResponse>>(badRequest.Value);
            Assert.Equal("E-mail inválido", resposta.Mensagem);
        }

        [Fact]
        public async Task LoginAsync_DeveRetornarUnauthorized_QuandoUsuarioNaoForEncontrado()
        {
            // Arrange
            var request = new LoginRequest { Email = "naoexiste@email.com", Senha = "123456" };

            _doLoginUseCaseMock
                .Setup(x => x.ExecuteAsync(request))
                .ThrowsAsync(new UsuarioNaoEncontradoException());

            // Act
            var result = await _controller.LoginAsync(request);

            // Assert
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            var resposta = Assert.IsType<RespostaPadrao<LoginResponse>>(unauthorized.Value);
            Assert.Equal("Credenciais inválidas.", resposta.Mensagem);
        }

        [Fact]
        public async Task LoginAsync_DeveRetornarErroInterno_QuandoExceptionGenericaForLancada()
        {
            // Arrange
            var request = new LoginRequest { Email = "teste@email.com", Senha = "123456" };

            _doLoginUseCaseMock
                .Setup(x => x.ExecuteAsync(request))
                .ThrowsAsync(new Exception("Erro inesperado"));

            // Act
            var result = await _controller.LoginAsync(request);

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, errorResult.StatusCode);

            var resposta = Assert.IsType<RespostaPadrao<LoginResponse>>(errorResult.Value);
            Assert.Contains("Erro interno do servidor", resposta.Mensagem);
        }
    }
}
