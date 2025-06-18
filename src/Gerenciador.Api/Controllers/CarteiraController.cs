using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Gerenciador.Aplicacao.UseCases.Carteira.ConsultarSaldo;
using Gerenciador.Aplicacao.UseCases.Carteira.AdicionarSaldo;
using Gerenciador.Comunicacao.DTOs;
using Gerenciador.Comunicacao.Requests.Carteira;
using Gerenciador.Comunicacao.Responses.Carteira;
using Gerenciador.Exceptions;
using System.Security.Claims;

namespace Gerenciador.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CarteiraController : ControllerBase
{
    private readonly IConsultarSaldoUseCase _consultarSaldoUseCase;
    private readonly IAdicionarSaldoUseCase _adicionarSaldoUseCase;
    private readonly ILogger<CarteiraController> _logger;

    public CarteiraController(
        IConsultarSaldoUseCase consultarSaldoUseCase,
        IAdicionarSaldoUseCase adicionarSaldoUseCase,
        ILogger<CarteiraController> logger)
    {
        _consultarSaldoUseCase = consultarSaldoUseCase;
        _adicionarSaldoUseCase = adicionarSaldoUseCase;
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

    [HttpGet("saldo")]
    [ProducesResponseType(typeof(RespostaPadrao<CarteiraResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaPadrao<CarteiraResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespostaPadrao<CarteiraResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(RespostaPadrao<CarteiraResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ConsultarSaldo()
    {
        try
        {
            var usuarioId = ObterUsuarioId();
            _logger.LogInformation("Usuário {UsuarioId} consultando saldo da carteira", usuarioId);

            var carteira = await _consultarSaldoUseCase.ExecuteAsync(usuarioId);

            _logger.LogInformation("Saldo da carteira obtido com sucesso para usuário {UsuarioId}", usuarioId);
            var resposta = RespostaPadrao<CarteiraResponse>.ComSucesso(carteira);
            return Ok(resposta);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Acesso não autorizado ao consultar saldo: {Message}", ex.Message);
            var resposta = RespostaPadrao<CarteiraResponse>.ComErro("Acesso não autorizado");
            return Unauthorized(resposta);
        }
        catch (UsuarioNaoEncontradoException ex)
        {
            _logger.LogWarning("Usuário não encontrado ao consultar saldo");
            var resposta = RespostaPadrao<CarteiraResponse>.ComErro(ex.Message);
            return NotFound(resposta);
        }
        catch (CarteiraException ex)
        {
            _logger.LogWarning("Erro ao obter carteira: {ErrorMessage}", ex.Message);
            var resposta = RespostaPadrao<CarteiraResponse>.ComErro(ex.Message);
            return NotFound(resposta);
        }
        catch (DatabaseException ex)
        {
            _logger.LogError(ex, "Erro de banco de dados ao consultar saldo");
            var resposta = RespostaPadrao<CarteiraResponse>.ComErro("Erro no banco de dados: " + ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, resposta);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno ao consultar saldo");
            var resposta = RespostaPadrao<CarteiraResponse>.ComErro("Erro interno do servidor: " + ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, resposta);
        }
    }

    [HttpPost("adicionar-saldo")]
    [ProducesResponseType(typeof(RespostaPadrao<CarteiraResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaPadrao<CarteiraResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespostaPadrao<CarteiraResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespostaPadrao<CarteiraResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(RespostaPadrao<CarteiraResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AdicionarSaldo([FromBody] AdicionarSaldoRequest request)
    {
        try
        {
            var usuarioId = ObterUsuarioId();
            _logger.LogInformation("Usuário {UsuarioId} adicionando saldo de {Valor} à carteira", usuarioId, request.Valor);

            var carteira = await _adicionarSaldoUseCase.ExecuteAsync(usuarioId, request);

            _logger.LogInformation("Saldo adicionado com sucesso para usuário {UsuarioId}", usuarioId);
            var resposta = RespostaPadrao<CarteiraResponse>.ComSucesso(carteira, "Saldo adicionado com sucesso");
            return Ok(resposta);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Acesso não autorizado ao adicionar saldo: {Message}", ex.Message);
            var resposta = RespostaPadrao<CarteiraResponse>.ComErro("Acesso não autorizado");
            return Unauthorized(resposta);
        }
        catch (UsuarioNaoEncontradoException ex)
        {
            _logger.LogWarning("Usuário não encontrado ao adicionar saldo");
            var resposta = RespostaPadrao<CarteiraResponse>.ComErro(ex.Message);
            return NotFound(resposta);
        }
        catch (CarteiraException ex)
        {
            _logger.LogWarning("Erro ao adicionar saldo à carteira: {ErrorMessage}", ex.Message);
            var resposta = RespostaPadrao<CarteiraResponse>.ComErro(ex.Message);
            return BadRequest(resposta);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Dados inválidos ao adicionar saldo: {ErrorMessage}", ex.Message);
            var resposta = RespostaPadrao<CarteiraResponse>.ComErro(ex.Message);
            return BadRequest(resposta);
        }
        catch (DatabaseException ex)
        {
            _logger.LogError(ex, "Erro de banco de dados ao adicionar saldo");
            var resposta = RespostaPadrao<CarteiraResponse>.ComErro("Erro no banco de dados: " + ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, resposta);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno ao adicionar saldo");
            var resposta = RespostaPadrao<CarteiraResponse>.ComErro("Erro interno do servidor: " + ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, resposta);
        }
    }
}