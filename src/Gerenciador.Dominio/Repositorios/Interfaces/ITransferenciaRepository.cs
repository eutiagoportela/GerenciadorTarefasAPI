using Gerenciador.Dominio.Entidades;

public interface ITransferenciaRepository
{
    Task<Transferencias?> ObterPorIdAsync(int id);
    Task<Transferencias> CriarAsync(Transferencias transferencia);
    Task<List<Transferencias>> ListarPorUsuarioAsync(int usuarioId);
    Task<List<Transferencias>> ListarPorUsuarioEPeriodoAsync(int usuarioId, DateTime dataInicio, DateTime dataFim);

    Task<(List<Transferencias>, int)> ListarPorUsuarioPaginadoAsync(int usuarioId, int pagina = 1, int tamanhoPagina = 10);
    Task<decimal> ObterSaldoTotalTransferenciasAsync(int usuarioId);
}