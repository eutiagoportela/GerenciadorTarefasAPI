using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Gerenciador.Aplicacao.UseCases.Usuario.CriarUsuario;
using Gerenciador.Aplicacao.UseCases.Usuario.ObterUsuario;
using Gerenciador.Comunicacao.DTOs;
using Gerenciador.Comunicacao.Requests.Usuario;
using Gerenciador.Comunicacao.Responses.Usuario;
using Gerenciador.Exceptions;
using System.Security.Claims;

namespace Gerenciador.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuarioController : ControllerBase
{
    private readonly ICriarUsuarioUseCase _criarUsuarioUseCase;
    private readonly IObterUsuarioUseCase _obterUsuarioUseCase;
    private readonly ILogger<UsuarioController> _logger;

    public UsuarioController(
        ICriarUsuarioUseCase criarUsuarioUseCase,
        IObterUsuarioUseCase obterUsuarioUseCase,
        ILogger<UsuarioController> logger)
    {
        _criarUsuarioUseCase = criarUsuarioUseCase;
        _obterUsuarioUseCase = obterUsuarioUseCase;
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
    [AllowAnonymous]
    [ProducesResponseType(typeof(RespostaPadrao<UsuarioResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespostaPadrao<UsuarioResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespostaPadrao<UsuarioResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Criar([FromBody] CriarUsuarioRequest request)
    {
        _logger.LogInformation("Iniciando criação de usuário: {UsuarioEmail}", request.Email);

        try
        {
            var usuario = await _criarUsuarioUseCase.ExecuteAsync(request);

            _logger.LogInformation("Usuário {UsuarioId} criado com sucesso", usuario.Id);
            var resposta = RespostaPadrao<UsuarioResponse>.ComSucesso(usuario, "Usuário criado com sucesso");
            return Created($"/api/usuario/{usuario.Id}", resposta);
        }
        catch (EmailJaExisteException ex)
        {
            _logger.LogWarning("Tentativa de criar usuário com e-mail já existente: {UsuarioEmail}", request.Email);
            var resposta = RespostaPadrao<UsuarioResponse>.ComErro(ex.Message);
            return BadRequest(resposta);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Dados inválidos na criação de usuário: {ErrorMessage}", ex.Message);
            var resposta = RespostaPadrao<UsuarioResponse>.ComErro(ex.Message);
            return BadRequest(resposta);
        }
        catch (DatabaseException ex)
        {
            _logger.LogError(ex, "Erro de banco de dados ao criar usuário: {UsuarioEmail}", request.Email);
            var resposta = RespostaPadrao<UsuarioResponse>.ComErro("Erro no banco de dados: " + ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, resposta);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno ao criar usuário: {UsuarioEmail}", request.Email);
            var resposta = RespostaPadrao<UsuarioResponse>.ComErro("Erro interno do servidor: " + ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, resposta);
        }
    }

    [HttpGet("perfil")]
    [Authorize]
    [ProducesResponseType(typeof(RespostaPadrao<UsuarioResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaPadrao<UsuarioResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespostaPadrao<UsuarioResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ObterPerfil()
    {
        try
        {
            var usuarioId = ObterUsuarioId();
            _logger.LogInformation("Obtendo perfil do usuário {UsuarioId}", usuarioId);

            var usuario = await _obterUsuarioUseCase.ExecuteAsync(usuarioId);

            _logger.LogInformation("Perfil do usuário {UsuarioId} obtido com sucesso", usuarioId);
            var resposta = RespostaPadrao<UsuarioResponse>.ComSucesso(usuario);
            return Ok(resposta);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Acesso não autorizado ao tentar obter perfil: {Message}", ex.Message);
            var resposta = RespostaPadrao<UsuarioResponse>.ComErro("Acesso não autorizado");
            return Unauthorized(resposta);
        }
        catch (UsuarioNaoEncontradoException ex)
        {
            _logger.LogWarning("Usuário não encontrado ao obter perfil");
            var resposta = RespostaPadrao<UsuarioResponse>.ComErro(ex.Message);
            return NotFound(resposta);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno ao obter perfil do usuário");
            var resposta = RespostaPadrao<UsuarioResponse>.ComErro("Erro interno do servidor: " + ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, resposta);
        }
    }
}