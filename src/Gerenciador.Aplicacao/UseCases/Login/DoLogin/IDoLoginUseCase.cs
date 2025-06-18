using Gerenciador.Comunicacao.Requests.Usuario;
using Gerenciador.Comunicacao.Responses.Usuario;

namespace Gerenciador.Aplicacao.UseCases.Login.DoLogin;

public interface IDoLoginUseCase
{
    Task<LoginResponse> ExecuteAsync(LoginRequest request);
}