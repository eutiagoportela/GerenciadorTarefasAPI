using Gerenciador.Comunicacao.Requests.Transferencia;
using Gerenciador.Comunicacao.Responses.Transferencia;

namespace Gerenciador.Aplicacao.UseCases.Transferencia.CriarTransferencia;

public interface ICriarTransferenciaUseCase
{
    Task<TransferenciaResponse> ExecuteAsync(int remetenteId, CriarTransferenciaRequest request);
}