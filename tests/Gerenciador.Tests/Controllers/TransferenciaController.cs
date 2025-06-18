using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Gerenciador.Api.Controllers;
using Gerenciador.Aplicacao.UseCases.Transferencia.CriarTransferencia;
using Gerenciador.Aplicacao.UseCases.Transferencia.ListarTransferencias;
using Gerenciador.Comunicacao.DTOs;
using Gerenciador.Comunicacao.Requests.Transferencia;
using Gerenciador.Comunicacao.Responses.Transferencia;
using Gerenciador.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Gerenciador.Tests.Controllers
{
    public class TransferenciaControllerTests
    {
        private readonly Mock<ICriarTransferenciaUseCase> _criarTransferenciaUseCaseMock = new();
        private readonly Mock<IListarTransferenciasUseCase> _listarTransferenciasUseCaseMock = new();
        private readonly Mock<ILogger<TransferenciaController>> _loggerMock = new();
        private readonly TransferenciaController _controller;

        public TransferenciaControllerTests()
        {
            _controller = new TransferenciaController(
                _criarTransferenciaUseCaseMock.Object,
                _listarTransferenciasUseCaseMock.Object,
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

        #region Criar

        [Fact]
        public async Task Criar_DeveRetornarCreated_QuandoSucesso()
        {
            int userId = 1;
            MockUserWithId(userId);

            var request = new CriarTransferenciaRequest
            {
                DestinatarioId = 2,
                Valor = 100m
            };

            var transferenciaResponse = new TransferenciaResponse
            {
                Id = 10,
                Valor = 100m,
                DestinatarioId = 2,
                RemetenteId = userId,
                DataTransferencia = DateTime.UtcNow
            };

            _criarTransferenciaUseCaseMock
                .Setup(x => x.ExecuteAsync(userId, request))
                .ReturnsAsync(transferenciaResponse);

            var result = await _controller.Criar(request);

            var createdResult = Assert.IsType<CreatedResult>(result);
            var resposta = Assert.IsType<RespostaPadrao<TransferenciaResponse>>(createdResult.Value);
            Assert.Equal("Transferência realizada com sucesso", resposta.Mensagem);
            Assert.Equal(transferenciaResponse.Id, resposta.Dados.Id);
        }

        [Fact]
        public async Task Criar_DeveRetornarBadRequest_QuandoCarteiraException()
        {
            int userId = 1;
            MockUserWithId(userId);

            var request = new CriarTransferenciaRequest
            {
                DestinatarioId = 2,
                Valor = 100m
            };

            _criarTransferenciaUseCaseMock
                .Setup(x => x.ExecuteAsync(userId, request))
                .ThrowsAsync(new CarteiraException("Erro na carteira"));

            var result = await _controller.Criar(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var resposta = Assert.IsType<RespostaPadrao<TransferenciaResponse>>(badRequest.Value);
            Assert.Equal("Erro na carteira", resposta.Mensagem);
        }

        [Fact]
        public async Task Criar_DeveRetornarBadRequest_QuandoSaldoInsuficienteException()
        {
            int userId = 1;
            MockUserWithId(userId);

            var request = new CriarTransferenciaRequest
            {
                DestinatarioId = 2,
                Valor = 100m
            };

            _criarTransferenciaUseCaseMock
                .Setup(x => x.ExecuteAsync(userId, request))
                .ThrowsAsync(new SaldoInsuficienteException("Saldo insuficiente"));

            var result = await _controller.Criar(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var resposta = Assert.IsType<RespostaPadrao<TransferenciaResponse>>(badRequest.Value);
            Assert.Equal("Saldo insuficiente", resposta.Mensagem);
        }

        [Fact]
        public async Task Criar_DeveRetornarNotFound_QuandoUsuarioNaoEncontrado()
        {
            int userId = 1;
            MockUserWithId(userId);

            var request = new CriarTransferenciaRequest
            {
                DestinatarioId = 2,
                Valor = 100m
            };

            _criarTransferenciaUseCaseMock
                .Setup(x => x.ExecuteAsync(userId, request))
                .ThrowsAsync(new UsuarioNaoEncontradoException("Usuário não encontrado"));

            var result = await _controller.Criar(request);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var resposta = Assert.IsType<RespostaPadrao<TransferenciaResponse>>(notFoundResult.Value);
            Assert.Equal("Usuário não encontrado", resposta.Mensagem);
        }

        [Fact]
        public async Task Criar_DeveRetornarUnauthorized_QuandoTokenInvalido()
        {
            var request = new CriarTransferenciaRequest
            {
                DestinatarioId = 2,
                Valor = 100m
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
            };

            var result = await _controller.Criar(request);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var resposta = Assert.IsType<RespostaPadrao<TransferenciaResponse>>(unauthorizedResult.Value);
            Assert.Equal("Acesso não autorizado", resposta.Mensagem);
        }

        [Fact]
        public async Task Criar_DeveRetornarInternalServerError_QuandoExceptionGenerica()
        {
            int userId = 1;
            MockUserWithId(userId);

            var request = new CriarTransferenciaRequest
            {
                DestinatarioId = 2,
                Valor = 100m
            };

            _criarTransferenciaUseCaseMock
                .Setup(x => x.ExecuteAsync(userId, request))
                .ThrowsAsync(new Exception("Erro inesperado"));

            var result = await _controller.Criar(request);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);

            var resposta = Assert.IsType<RespostaPadrao<TransferenciaResponse>>(objectResult.Value);
            Assert.StartsWith("Erro interno do servidor", resposta.Mensagem);
        }

        #endregion

        #region Listar

        [Fact]
        public async Task Listar_DeveRetornarOk_QuandoSucesso()
        {
            int userId = 1;
            MockUserWithId(userId);

            var listaTransferencias = new List<TransferenciaResponse>
            {
                new TransferenciaResponse
                {
                    Id = 1,
                    RemetenteId = userId,
                    DestinatarioId = 2,
                    Valor = 50m,
                    DataTransferencia = DateTime.UtcNow.AddDays(-5),
                    RemetenteNome = "Usuário Remetente",
                    DestinatarioNome = "Usuário Destinatário"
                },
                new TransferenciaResponse
                {
                    Id = 2,
                    RemetenteId = userId,
                    DestinatarioId = 3,
                    Valor = 100m,
                    DataTransferencia = DateTime.UtcNow.AddDays(-2),
                    RemetenteNome = "Usuário Remetente",
                    DestinatarioNome = "Outro Destinatário"
                }
            };


            _listarTransferenciasUseCaseMock
                .Setup(x => x.ExecuteAsync(userId))
                .ReturnsAsync(listaTransferencias);

            var result = await _controller.Listar();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var resposta = Assert.IsType<RespostaPadrao<List<TransferenciaResponse>>>(okResult.Value);
            Assert.Equal(2, resposta.Dados.Count);
        }

        [Fact]
        public async Task Listar_DeveRetornarNotFound_QuandoUsuarioNaoEncontrado()
        {
            int userId = 1;
            MockUserWithId(userId);

            _listarTransferenciasUseCaseMock
                .Setup(x => x.ExecuteAsync(userId))
                .ThrowsAsync(new UsuarioNaoEncontradoException("Usuário não encontrado"));

            var result = await _controller.Listar();

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var resposta = Assert.IsType<RespostaPadrao<List<TransferenciaResponse>>>(notFoundResult.Value);
            Assert.Equal("Usuário não encontrado", resposta.Mensagem);
        }

        [Fact]
        public async Task Listar_DeveRetornarInternalServerError_QuandoDatabaseException()
        {
            int userId = 1;
            MockUserWithId(userId);

            _listarTransferenciasUseCaseMock
                .Setup(x => x.ExecuteAsync(userId))
                .ThrowsAsync(new DatabaseException("Erro no banco"));

            var result = await _controller.Listar();

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);

            var resposta = Assert.IsType<RespostaPadrao<List<TransferenciaResponse>>>(objectResult.Value);
            Assert.StartsWith("Erro no banco de dados", resposta.Mensagem);
        }

        #endregion

        #region ListarPorPeriodo

        [Fact]
        public async Task ListarPorPeriodo_DeveRetornarOk_QuandoSucesso()
        {
            int userId = 1;
            MockUserWithId(userId);

            var dataInicio = DateTime.UtcNow.AddDays(-7);
            var dataFim = DateTime.UtcNow;

            var listaTransferencias = new List<TransferenciaResponse>
            {
                new TransferenciaResponse
                {
                    Id = 1,
                    RemetenteId = userId,
                    DestinatarioId = 2,
                    Valor = 50m,
                    DataTransferencia = DateTime.UtcNow.AddDays(-5),
                    RemetenteNome = "Usuário Remetente",
                    DestinatarioNome = "Usuário Destinatário"
                },
                new TransferenciaResponse
                {
                    Id = 2,
                    RemetenteId = userId,
                    DestinatarioId = 3,
                    Valor = 100m,
                    DataTransferencia = DateTime.UtcNow.AddDays(-2),
                    RemetenteNome = "Usuário Remetente",
                    DestinatarioNome = "Outro Destinatário"
                }
            };


            _listarTransferenciasUseCaseMock
                .Setup(x => x.ExecuteAsync(userId, dataInicio, dataFim))
                .ReturnsAsync(listaTransferencias);

            var result = await _controller.ListarPorPeriodo(dataInicio, dataFim);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var resposta = Assert.IsType<RespostaPadrao<List<TransferenciaResponse>>>(okResult.Value);
            Assert.Equal(2, resposta.Dados.Count);
        }

        [Fact]
        public async Task ListarPorPeriodo_DeveRetornarBadRequest_QuandoDatasInvalidas()
        {
            int userId = 1;
            MockUserWithId(userId);

            var dataInicio = DateTime.UtcNow;
            var dataFim = DateTime.UtcNow.AddDays(-1); // fim antes do início

            _listarTransferenciasUseCaseMock
                .Setup(x => x.ExecuteAsync(userId, dataInicio, dataFim))
                .ThrowsAsync(new ArgumentException("Data fim deve ser maior que data início"));

            var result = await _controller.ListarPorPeriodo(dataInicio, dataFim);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var resposta = Assert.IsType<RespostaPadrao<List<TransferenciaResponse>>>(badRequest.Value);
            Assert.Equal("Data fim deve ser maior que data início", resposta.Mensagem);
        }

        #endregion
    }
}
