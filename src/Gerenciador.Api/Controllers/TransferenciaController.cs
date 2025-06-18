using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Gerenciador.Aplicacao.UseCases.Transferencia.CriarTransferencia;
using Gerenciador.Aplicacao.UseCases.Transferencia.ListarTransferencias;
using Gerenciador.Comunicacao.DTOs;
using Gerenciador.Comunicacao.Requests.Transferencia;
using Gerenciador.Comunicacao.Responses.Transferencia;
using Gerenciador.Exceptions;
using System.Security.Claims;

namespace Gerenciador.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransferenciaController : ControllerBase
{
    private readonly ICriarTransferenciaUseCase _criarTransferenciaUseCase;
    private readonly IListarTransferenciasUseCase _listarTransferenciasUseCase;
    private readonly ILogger<TransferenciaController> _logger;

    public TransferenciaController(
        ICriarTransferenciaUseCase criarTransferenciaUseCase,
        IListarTransferenciasUseCase listarTransferenciasUseCase,
        ILogger<TransferenciaController> logger)
    {
        _criarTransferenciaUseCase = criarTransferenciaUseCase;
        _listarTransferenciasUseCase = listarTransferenciasUseCase;
        _logger = logger;
    }

    private int ObterUsuarioId()
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(usuarioIdClaim) || !int.TryParse(usuarioIdClaim, out int userId))
        {
            throw new UnauthorizedAccessException("Token inválido ou usuário não identificado");
        }
        return userId;
    }

    [HttpPost]
    [ProducesResponseType(typeof(RespostaPadrao<TransferenciaResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespostaPadrao<TransferenciaResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespostaPadrao<TransferenciaResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespostaPadrao<TransferenciaResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(RespostaPadrao<TransferenciaResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Criar([FromBody] CriarTransferenciaRequest request)
    {
        try
        {
            var usuarioId = ObterUsuarioId();
            _logger.LogInformation("Usuário {UsuarioId} iniciando transferência para usuário {DestinatarioId} no valor de {Valor}",
                                usuarioId, request.DestinatarioId, request.Valor);

            var transferencia = await _criarTransferenciaUseCase.ExecuteAsync(usuarioId, request);

            _logger.LogInformation("Transferência {TransferenciaId} realizada com sucesso", transferencia.Id);
            var resposta = RespostaPadrao<TransferenciaResponse>.ComSucesso(transferencia, "Transferência realizada com sucesso");
            return Created($"/api/transferencia/{transferencia.Id}", resposta);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Acesso não autorizado ao realizar transferência: {Message}", ex.Message);
            var resposta = RespostaPadrao<TransferenciaResponse>.ComErro("Acesso não autorizado");
            return Unauthorized(resposta);
        }
        catch (UsuarioNaoEncontradoException ex)
        {
            _logger.LogWarning("Usuário não encontrado ao realizar transferência: {ErrorMessage}", ex.Message);
            var resposta = RespostaPadrao<TransferenciaResponse>.ComErro(ex.Message);
            return NotFound(resposta);
        }
        catch (CarteiraException ex)
        {
            _logger.LogWarning("Erro na carteira ao realizar transferência: {ErrorMessage}", ex.Message);
            var resposta = RespostaPadrao<TransferenciaResponse>.ComErro(ex.Message);
            return BadRequest(resposta);
        }
        catch (SaldoInsuficienteException ex)
        {
            _logger.LogWarning("Saldo insuficiente para realizar transferência: {Message}", ex.Message);
            var resposta = RespostaPadrao<TransferenciaResponse>.ComErro(ex.Message);
            return BadRequest(resposta);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Dados inválidos para transferência: {ErrorMessage}", ex.Message);
            var resposta = RespostaPadrao<TransferenciaResponse>.ComErro(ex.Message);
            return BadRequest(resposta);
        }
        catch (DatabaseException ex)
        {
            _logger.LogError(ex, "Erro de banco de dados ao realizar transferência");
            var resposta = RespostaPadrao<TransferenciaResponse>.ComErro("Erro no banco de dados: " + ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, resposta);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno ao realizar transferência");
            var resposta = RespostaPadrao<TransferenciaResponse>.ComErro("Erro interno do servidor: " + ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, resposta);
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(RespostaPadrao<List<TransferenciaResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaPadrao<List<TransferenciaResponse>>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespostaPadrao<List<TransferenciaResponse>>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(RespostaPadrao<List<TransferenciaResponse>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Listar()
    {
        try
        {
            var usuarioId = ObterUsuarioId();
            _logger.LogInformation("Usuário {UsuarioId} listando suas transferências", usuarioId);

            var transferencias = await _listarTransferenciasUseCase.ExecuteAsync(usuarioId);

            _logger.LogInformation("Listagem de transferências obtida com sucesso para usuário {UsuarioId}", usuarioId);
            var resposta = RespostaPadrao<List<TransferenciaResponse>>.ComSucesso(transferencias);
            return Ok(resposta);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Acesso não autorizado ao listar transferências: {Message}", ex.Message);
            var resposta = RespostaPadrao<List<TransferenciaResponse>>.ComErro("Acesso não autorizado");
            return Unauthorized(resposta);
        }
        catch (UsuarioNaoEncontradoException ex)
        {
            _logger.LogWarning("Usuário não encontrado ao listar transferências");
            var resposta = RespostaPadrao<List<TransferenciaResponse>>.ComErro(ex.Message);
            return NotFound(resposta);
        }
        catch (DatabaseException ex)
        {
            _logger.LogError(ex, "Erro de banco de dados ao listar transferências");
            var resposta = RespostaPadrao<List<TransferenciaResponse>>.ComErro("Erro no banco de dados: " + ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, resposta);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno ao listar transferências");
            var resposta = RespostaPadrao<List<TransferenciaResponse>>.ComErro("Erro interno do servidor: " + ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, resposta);
        }
    }

    [HttpGet("periodo")]
    [ProducesResponseType(typeof(RespostaPadrao<List<TransferenciaResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaPadrao<List<TransferenciaResponse>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespostaPadrao<List<TransferenciaResponse>>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespostaPadrao<List<TransferenciaResponse>>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(RespostaPadrao<List<TransferenciaResponse>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ListarPorPeriodo([FromQuery] DateTime dataInicio, [FromQuery] DateTime dataFim)
    {
        try
        {
            var usuarioId = ObterUsuarioId();
            _logger.LogInformation("Usuário {UsuarioId} listando transferências no período de {DataInicio} a {DataFim}",
                                usuarioId, dataInicio, dataFim);

            var transferencias = await _listarTransferenciasUseCase.ExecuteAsync(usuarioId, dataInicio, dataFim);

            _logger.LogInformation("Listagem de transferências por período obtida com sucesso para usuário {UsuarioId}", usuarioId);
            var resposta = RespostaPadrao<List<TransferenciaResponse>>.ComSucesso(transferencias);
            return Ok(resposta);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Acesso não autorizado ao listar transferências por período: {Message}", ex.Message);
            var resposta = RespostaPadrao<List<TransferenciaResponse>>.ComErro("Acesso não autorizado");
            return Unauthorized(resposta);
        }
        catch (UsuarioNaoEncontradoException ex)
        {
            _logger.LogWarning("Usuário não encontrado ao listar transferências por período");
            var resposta = RespostaPadrao<List<TransferenciaResponse>>.ComErro(ex.Message);
            return NotFound(resposta);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Datas inválidas para listar transferências por período: {ErrorMessage}", ex.Message);
            var resposta = RespostaPadrao<List<TransferenciaResponse>>.ComErro(ex.Message);
            return BadRequest(resposta);
        }
        catch (DatabaseException ex)
        {
            _logger.LogError(ex, "Erro de banco de dados ao listar transferências por período");
            var resposta = RespostaPadrao<List<TransferenciaResponse>>.ComErro("Erro no banco de dados: " + ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, resposta);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno ao listar transferências por período");
            var resposta = RespostaPadrao<List<TransferenciaResponse>>.ComErro("Erro interno do servidor: " + ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, resposta);
        }
    }
}