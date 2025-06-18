using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Gerenciador.Dominio.Entidades;
using Gerenciador.Dominio.Repositorios.Interfaces;
using Gerenciador.Exceptions;

namespace Gerenciador.Infraestrutura.Repositorios.Implementacoes;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly PostgreSqlDbContext _context;
    private readonly ILogger<UsuarioRepository> _logger;

    public UsuarioRepository(PostgreSqlDbContext context, ILogger<UsuarioRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Usuarios?> ObterPorIdAsync(int id)
    {
        try
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter usuário por ID: {UsuarioId}", id);
            throw new DatabaseException($"Erro ao obter usuário por ID: {id}", ex);
        }
    }

    public async Task<Usuarios?> ObterPorEmailAsync(string email)
    {
        try
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter usuário por e-mail: {Email}", email);
            throw new DatabaseException($"Erro ao obter usuário por e-mail: {email}", ex);
        }
    }

    public async Task<Usuarios> CriarAsync(Usuarios usuario)
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
                    _context.Usuarios.Add(usuario);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return usuario;
                }
                catch (DbUpdateException ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Erro ao criar usuário: {UsuarioEmail}", usuario.Email);

                    if (ex.InnerException?.Message.Contains("duplicate key") == true &&
                        ex.InnerException.Message.Contains("Email") == true)
                    {
                        throw new EmailJaExisteException($"O e-mail {usuario.Email} já está cadastrado.", ex);
                    }

                    throw new DatabaseException("Erro ao criar usuário no banco de dados.", ex);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Erro ao criar usuário: {UsuarioEmail}", usuario.Email);
                    throw new DatabaseException("Erro ao criar usuário no banco de dados.", ex);
                }
            });
        }
        catch (EmailJaExisteException)
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
            _logger.LogError(ex, "Erro não tratado ao criar usuário: {UsuarioEmail}", usuario.Email);
            throw new DatabaseException("Erro não esperado ao criar usuário.", ex);
        }
    }

    public async Task<List<Usuarios>> ListarTodosAsync()
    {
        try
        {
            return await _context.Usuarios
                .OrderBy(u => u.Nome)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar todos os usuários");
            throw new DatabaseException("Erro ao listar usuários do banco de dados.", ex);
        }
    }

    public async Task<bool> EmailExisteAsync(string email)
    {
        try
        {
            return await _context.Usuarios
                .AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar se e-mail existe: {Email}", email);
            throw new DatabaseException($"Erro ao verificar se o e-mail {email} existe no banco de dados.", ex);
        }
    }
}