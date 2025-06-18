using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Gerenciador.Dominio.Entidades;
using Gerenciador.Dominio.Repositorios.Interfaces;
using Gerenciador.Exceptions;

namespace Gerenciador.Infraestrutura.Repositorios.Implementacoes;

public class CarteiraRepository : ICarteiraRepository
{
    private readonly PostgreSqlDbContext _context;
    private readonly ILogger<CarteiraRepository> _logger;

    public CarteiraRepository(PostgreSqlDbContext context, ILogger<CarteiraRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Carteiras?> ObterPorIdAsync(int id)
    {
        try
        {
            return await _context.Carteiras
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter carteira por ID: {CarteiraId}", id);
            throw new DatabaseException($"Erro ao obter carteira por ID: {id}", ex);
        }
    }

    public async Task<Carteiras?> ObterPorUsuarioIdAsync(int usuarioId)
    {
        try
        {
            return await _context.Carteiras
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter carteira por usuário ID: {UsuarioId}", usuarioId);
            throw new DatabaseException($"Erro ao obter carteira do usuário ID: {usuarioId}", ex);
        }
    }

    public async Task<Carteiras> CriarAsync(Carteiras carteira)
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
                    _context.Carteiras.Add(carteira);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return carteira;
                }
                catch (DbUpdateException ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Erro ao criar carteira para usuário: {UsuarioId}", carteira.UsuarioId);

                    if (ex.InnerException?.Message.Contains("duplicate key") == true &&
                        ex.InnerException.Message.Contains("UsuarioId") == true)
                    {
                        throw new CarteiraException($"Já existe uma carteira para o usuário ID: {carteira.UsuarioId}", ex);
                    }

                    throw new DatabaseException("Erro ao criar carteira no banco de dados.", ex);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Erro ao criar carteira para usuário: {UsuarioId}", carteira.UsuarioId);
                    throw new DatabaseException("Erro ao criar carteira no banco de dados.", ex);
                }
            });
        }
        catch (CarteiraException)
        {
            // Apenas repassa a exceção
            throw;
        }
        catch (DatabaseException)
        {
            // Apenas repassa a exceção
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado ao criar carteira: {UsuarioId}", carteira.UsuarioId);
            throw new DatabaseException("Erro não esperado ao criar carteira.", ex);
        }
    }

    public async Task<Carteiras> AtualizarAsync(Carteiras carteira)
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
                    carteira.DataAtualizacao = DateTime.UtcNow;
                    _context.Carteiras.Update(carteira);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return carteira;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Erro de concorrência ao atualizar carteira: {CarteiraId}", carteira.Id);
                    throw new CarteiraException($"Erro de concorrência ao atualizar carteira. A carteira foi modificada por outro processo.", ex);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Erro ao atualizar carteira: {CarteiraId}", carteira.Id);
                    throw new DatabaseException($"Erro ao atualizar carteira ID: {carteira.Id}", ex);
                }
            });
        }
        catch (CarteiraException)
        {
            // Apenas repassa a exceção
            throw;
        }
        catch (DatabaseException)
        {
            // Apenas repassa a exceção
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado ao atualizar carteira: {CarteiraId}", carteira.Id);
            throw new DatabaseException("Erro não esperado ao atualizar carteira.", ex);
        }
    }
}