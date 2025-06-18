using Gerenciador.Comunicacao.Responses.Usuario;

namespace Gerenciador.Aplicacao.UseCases.Usuario.ObterUsuario;

public interface IObterUsuarioUseCase
{
    Task<UsuarioResponse> ExecuteAsync(int id);
}