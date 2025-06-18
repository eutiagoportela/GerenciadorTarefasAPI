using Gerenciador.Comunicacao.Responses.Transferencia;

namespace Gerenciador.Aplicacao.UseCases.Transferencia.ListarTransferencias;

public interface IListarTransferenciasUseCase
{
    Task<List<TransferenciaResponse>> ExecuteAsync(int usuarioId);
    Task<List<TransferenciaResponse>> ExecuteAsync(int usuarioId, DateTime dataInicio, DateTime dataFim);

    Task<(List<TransferenciaResponse>, int)> ExecutePaginadoAsync(int usuarioId, int pagina = 1, int tamanhoPagina = 10);
}