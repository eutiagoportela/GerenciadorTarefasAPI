using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Gerenciador.Dominio.Entidades;
using Gerenciador.Dominio.Repositorios.Interfaces;
using Gerenciador.Exceptions;

namespace Gerenciador.Infraestrutura.Repositorios.Implementacoes;

public class TransferenciaRepository : ITransferenciaRepository
{
    private readonly PostgreSqlDbContext _context;
    private readonly ILogger<TransferenciaRepository> _logger;

    public TransferenciaRepository(PostgreSqlDbContext context, ILogger<TransferenciaRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Transferencias?> ObterPorIdAsync(int id)
    {
        try
        {
            return await _context.Transferencias
                .Include(t => t.Remetente)
                .Include(t => t.Destinatario)
                .FirstOrDefaultAsync(t => t.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter transferência por ID: {TransferenciaId}", id);
            throw new DatabaseException($"Erro ao obter transferência por ID: {id}", ex);
        }
    }

    public async Task<Transferencias> CriarAsync(Transferencias transferencia)
    {
        // Usar a estratégia de execução correta
        var strategy = _context.Database.CreateExecutionStrategy();

        try
        {
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    _context.Transferencias.Add(transferencia);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return transferencia;
                }
                catch (DbUpdateException ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Erro ao criar transferência: {TransferenciaDetalhes}",
                        $"De: {transferencia.RemetenteId}, Para: {transferencia.DestinatarioId}, Valor: {transferencia.Valor}");

                    if (ex.InnerException?.Message.Contains("foreign key constraint") == true)
                    {
                        throw new DatabaseException("Erro ao criar transferência: Usuário remetente ou destinatário não encontrado.", ex);
                    }

                    throw new DatabaseException("Erro ao criar transferência no banco de dados.", ex);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Erro ao criar transferência: {TransferenciaDetalhes}",
                        $"De: {transferencia.RemetenteId}, Para: {transferencia.DestinatarioId}, Valor: {transferencia.Valor}");
                    throw new DatabaseException("Erro ao criar transferência no banco de dados.", ex);
                }
            });
        }
        catch (DatabaseException)
        {
            // Apenas repassa a exceção
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado ao criar transferência entre {RemetenteId} e {DestinatarioId}",
                transferencia.RemetenteId, transferencia.DestinatarioId);
            throw new DatabaseException("Erro não esperado ao criar transferência.", ex);
        }
    }

    public async Task<List<Transferencias>> ListarPorUsuarioAsync(int usuarioId)
    {
        try
        {
            return await _context.Transferencias
                .Include(t => t.Remetente)    
                .Include(t => t.Destinatario) 
                .Where(t => t.RemetenteId == usuarioId || t.DestinatarioId == usuarioId)
                .OrderByDescending(t => t.DataTransferencia)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar transferências do usuário: {UsuarioId}", usuarioId);
            throw new DatabaseException($"Erro ao listar transferências do usuário ID: {usuarioId}", ex);
        }
    }

    public async Task<List<Transferencias>> ListarPorUsuarioEPeriodoAsync(int usuarioId, DateTime dataInicio, DateTime dataFim)
    {
        try
        {
            return await _context.Transferencias
                .Include(t => t.Remetente)    
                .Include(t => t.Destinatario) 
                .Where(t =>
                    (t.RemetenteId == usuarioId || t.DestinatarioId == usuarioId) &&
                    t.DataTransferencia >= dataInicio &&
                    t.DataTransferencia <= dataFim)
                .OrderByDescending(t => t.DataTransferencia)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar transferências do usuário por período: {UsuarioId}, {DataInicio} a {DataFim}",
                usuarioId, dataInicio, dataFim);
            throw new DatabaseException($"Erro ao listar transferências do usuário ID: {usuarioId} no período especificado", ex);
        }
    }

    public async Task<(List<Transferencias>, int)> ListarPorUsuarioPaginadoAsync(
        int usuarioId,
        int pagina = 1,
        int tamanhoPagina = 10)
    {
        try
        {
            var query = _context.Transferencias
                .Include(t => t.Remetente)
                .Include(t => t.Destinatario)
                .Where(t => t.RemetenteId == usuarioId || t.DestinatarioId == usuarioId)
                .OrderByDescending(t => t.DataTransferencia);

            var totalCount = await query.CountAsync();

            var transferencias = await query
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToListAsync();

            return (transferencias, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar transferências paginadas do usuário: {UsuarioId}", usuarioId);
            throw new DatabaseException($"Erro ao listar transferências paginadas do usuário ID: {usuarioId}", ex);
        }
    }

    public async Task<decimal> ObterSaldoTotalTransferenciasAsync(int usuarioId)
    {
        try
        {
            var entradas = await _context.Transferencias
                .Where(t => t.DestinatarioId == usuarioId)
                .SumAsync(t => t.Valor);

            var saidas = await _context.Transferencias
                .Where(t => t.RemetenteId == usuarioId)
                .SumAsync(t => t.Valor);

            return entradas - saidas;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao calcular saldo total de transferências: {UsuarioId}", usuarioId);
            throw new DatabaseException($"Erro ao calcular saldo de transferências do usuário ID: {usuarioId}", ex);
        }
    }
}