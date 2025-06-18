using Gerenciador.Comunicacao.Responses.Carteira;

namespace Gerenciador.Aplicacao.UseCases.Carteira.ConsultarSaldo;

public interface IConsultarSaldoUseCase
{
    Task<CarteiraResponse> ExecuteAsync(int usuarioId);
}