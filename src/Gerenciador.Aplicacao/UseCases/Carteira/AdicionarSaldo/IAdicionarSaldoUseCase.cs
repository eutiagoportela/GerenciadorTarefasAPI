using Gerenciador.Comunicacao.Requests.Carteira;
using Gerenciador.Comunicacao.Responses.Carteira;

namespace Gerenciador.Aplicacao.UseCases.Carteira.AdicionarSaldo;

public interface IAdicionarSaldoUseCase
{
    Task<CarteiraResponse> ExecuteAsync(int usuarioId, AdicionarSaldoRequest request);
}