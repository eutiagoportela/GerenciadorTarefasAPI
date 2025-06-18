using System.Net;
using System.Text.Json;
using Gerenciador.Comunicacao.DTOs;
using Gerenciador.Exceptions;

namespace Gerenciador.Api.Middleware;

/// <summary>
/// Middleware para tratamento centralizado de exceções na aplicação
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ErrorHandlingMiddleware(
        RequestDelegate next,
        ILogger<ErrorHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        HttpStatusCode statusCode;
        string message;
        List<string> errors = new List<string>();
        string logMessage = $"Erro não tratado: {exception.Message}";

        // Determinar o código de status e mensagem baseado no tipo da exceção
        switch (exception)
        {
            case UsuarioNaoEncontradoException:
                statusCode = HttpStatusCode.NotFound;
                message = exception.Message;
                _logger.LogWarning(exception, "Usuário não encontrado: {Message}", exception.Message);
                break;

            case EmailJaExisteException:
                statusCode = HttpStatusCode.BadRequest;
                message = exception.Message;
                _logger.LogWarning(exception, "Email já existe: {Message}", exception.Message);
                break;

            case AcessoNegadoException:
                statusCode = HttpStatusCode.Forbidden;
                message = exception.Message;
                _logger.LogWarning(exception, "Acesso negado: {Message}", exception.Message);
                break;

            case SaldoInsuficienteException:
                statusCode = HttpStatusCode.BadRequest;
                message = exception.Message;
                _logger.LogWarning(exception, "Saldo insuficiente: {Message}", exception.Message);
                break;

            case CarteiraException:
                statusCode = HttpStatusCode.BadRequest;
                message = exception.Message;
                _logger.LogWarning(exception, "Erro na carteira: {Message}", exception.Message);
                break;

            case DatabaseException dbEx:
                statusCode = HttpStatusCode.InternalServerError;

                // Em desenvolvimento, mostrar detalhes do erro de BD
                if (_environment.IsDevelopment())
                {
                    message = dbEx.Message;
                    if (dbEx.SqlErrorCode != null)
                    {
                        errors.Add($"Código de erro SQL: {dbEx.SqlErrorCode}");
                    }
                    if (exception.InnerException != null)
                    {
                        errors.Add(exception.InnerException.Message);
                    }
                }
                else
                {
                    message = dbEx.GetFriendlyMessage();
                }

                _logger.LogError(exception, "Erro de banco de dados: {Message}, SqlCode: {SqlCode}",
                    dbEx.Message, dbEx.SqlErrorCode);
                break;

            case ArgumentException:
                statusCode = HttpStatusCode.BadRequest;
                message = exception.Message;
                _logger.LogWarning(exception, "Argumento inválido: {Message}", exception.Message);
                break;

            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Unauthorized;
                message = "Não autorizado. Faça login novamente.";
                _logger.LogWarning(exception, "Acesso não autorizado: {Message}", exception.Message);
                break;

            default:
                statusCode = HttpStatusCode.InternalServerError;
                message = "Ocorreu um erro interno no servidor.";

                // Em desenvolvimento, mostrar mais detalhes
                if (_environment.IsDevelopment())
                {
                    message = exception.Message;
                    if (exception.InnerException != null)
                    {
                        errors.Add(exception.InnerException.Message);
                    }
                }

                _logger.LogError(exception, "Erro não tratado: {Message}", exception.Message);
                break;
        }

        // Montar resposta de erro
        var response = RespostaPadrao.ComErro(message, errors);

        // Configurar resposta HTTP
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        // Serializar resposta
        var json = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(json);
    }
}