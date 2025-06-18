using Gerenciador.Comunicacao.Requests.Usuario;
using Gerenciador.Comunicacao.Responses.Usuario;

namespace Gerenciador.Aplicacao.UseCases.Usuario.CriarUsuario;

public interface ICriarUsuarioUseCase
{
    Task<UsuarioResponse> ExecuteAsync(CriarUsuarioRequest request);
}