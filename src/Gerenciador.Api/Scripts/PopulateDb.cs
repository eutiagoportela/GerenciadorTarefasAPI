using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Gerenciador.Dominio.Entidades;
using Gerenciador.Dominio.Enum;
using Gerenciador.Infraestrutura;

namespace Gerenciador.Api.Scripts;

/// <summary>
/// Script para popular o banco de dados com dados fictícios para demonstração
/// </summary>
public static class PopulateDb
{
    public static async Task ExecuteAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<object>>();

        try
        {
            logger.LogInformation("🌱 Iniciando população do banco de dados...");

            // Aplicar migrations pendentes
            await context.Database.MigrateAsync();

            // Verificar se já existe dados (evitar duplicação)
            if (await context.Usuarios.AnyAsync())
            {
                logger.LogInformation("⚠️ Banco já contém dados, pulando população");
                return;
            }

            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                //  1. CRIAR USUÁRIOS 
                var usuarios = new List<Usuarios>
                {
                    new Usuarios
                    {
                        Nome = "João Silva",
                        Email = "joao.silva@email.com",
                        SenhaHash = BCrypt.Net.BCrypt.HashPassword("senha123"),
                        DataCriacao = DateTime.UtcNow.AddDays(-30),
                        DataAtualizacao = DateTime.UtcNow.AddDays(-30)
                    },
                    new Usuarios
                    {
                        Nome = "Maria Santos",
                        Email = "maria.santos@email.com",
                        SenhaHash = BCrypt.Net.BCrypt.HashPassword("senha123"),
                        DataCriacao = DateTime.UtcNow.AddDays(-25),
                        DataAtualizacao = DateTime.UtcNow.AddDays(-25)
                    },
                    new Usuarios
                    {
                        Nome = "Pedro Oliveira",
                        Email = "pedro.oliveira@email.com",
                        SenhaHash = BCrypt.Net.BCrypt.HashPassword("senha123"),
                        DataCriacao = DateTime.UtcNow.AddDays(-20),
                        DataAtualizacao = DateTime.UtcNow.AddDays(-20)
                    },
                    new Usuarios
                    {
                        Nome = "Ana Costa",
                        Email = "ana.costa@email.com",
                        SenhaHash = BCrypt.Net.BCrypt.HashPassword("senha123"),
                        DataCriacao = DateTime.UtcNow.AddDays(-15),
                        DataAtualizacao = DateTime.UtcNow.AddDays(-15)
                    },
                    new Usuarios
                    {
                        Nome = "Carlos Ferreira",
                        Email = "carlos.ferreira@email.com",
                        SenhaHash = BCrypt.Net.BCrypt.HashPassword("senha123"),
                        DataCriacao = DateTime.UtcNow.AddDays(-10),
                        DataAtualizacao = DateTime.UtcNow.AddDays(-10)
                    }
                };

                context.Usuarios.AddRange(usuarios);
                await context.SaveChangesAsync();

                logger.LogInformation("✅ {Count} usuários criados", usuarios.Count);

                //  2. CRIAR CARTEIRAS 
                var carteiras = new List<Carteiras>
                {
                    new Carteiras
                    {
                        UsuarioId = usuarios[0].Id,
                        Saldo = 1500.50m,
                        DataCriacao = usuarios[0].DataCriacao,
                        DataAtualizacao = DateTime.UtcNow.AddDays(-5)
                    },
                    new Carteiras
                    {
                        UsuarioId = usuarios[1].Id,
                        Saldo = 2300.75m,
                        DataCriacao = usuarios[1].DataCriacao,
                        DataAtualizacao = DateTime.UtcNow.AddDays(-3)
                    },
                    new Carteiras
                    {
                        UsuarioId = usuarios[2].Id,
                        Saldo = 850.00m,
                        DataCriacao = usuarios[2].DataCriacao,
                        DataAtualizacao = DateTime.UtcNow.AddDays(-2)
                    },
                    new Carteiras
                    {
                        UsuarioId = usuarios[3].Id,
                        Saldo = 3200.25m,
                        DataCriacao = usuarios[3].DataCriacao,
                        DataAtualizacao = DateTime.UtcNow.AddDays(-1)
                    },
                    new Carteiras
                    {
                        UsuarioId = usuarios[4].Id,
                        Saldo = 750.00m,
                        DataCriacao = usuarios[4].DataCriacao,
                        DataAtualizacao = DateTime.UtcNow
                    }
                };

                context.Carteiras.AddRange(carteiras);
                await context.SaveChangesAsync();

                logger.LogInformation("✅ {Count} carteiras criadas", carteiras.Count);

                //  3. CRIAR TRANSFERÊNCIAS 
                var transferencias = new List<Transferencias>
                {
                    // Depósitos iniciais
                    new Transferencias
                    {
                        Valor = 1000.00m,
                        Descricao = "Depósito inicial",
                        Tipo = TipoTransferencia.Deposito,
                        RemetenteId = usuarios[0].Id,
                        DestinatarioId = usuarios[0].Id,
                        DataTransferencia = DateTime.UtcNow.AddDays(-30)
                    },
                    new Transferencias
                    {
                        Valor = 2000.00m,
                        Descricao = "Depósito inicial",
                        Tipo = TipoTransferencia.Deposito,
                        RemetenteId = usuarios[1].Id,
                        DestinatarioId = usuarios[1].Id,
                        DataTransferencia = DateTime.UtcNow.AddDays(-25)
                    },

                    // Transferências entre usuários
                    new Transferencias
                    {
                        Valor = 250.50m,
                        Descricao = "Pagamento de serviços",
                        Tipo = TipoTransferencia.Transferencia,
                        RemetenteId = usuarios[0].Id,
                        DestinatarioId = usuarios[1].Id,
                        DataTransferencia = DateTime.UtcNow.AddDays(-20)
                    },
                    new Transferencias
                    {
                        Valor = 500.00m,
                        Descricao = "Empréstimo",
                        Tipo = TipoTransferencia.Transferencia,
                        RemetenteId = usuarios[1].Id,
                        DestinatarioId = usuarios[2].Id,
                        DataTransferencia = DateTime.UtcNow.AddDays(-18)
                    },
                    new Transferencias
                    {
                        Valor = 100.25m,
                        Descricao = "Divisão da conta",
                        Tipo = TipoTransferencia.Transferencia,
                        RemetenteId = usuarios[2].Id,
                        DestinatarioId = usuarios[3].Id,
                        DataTransferencia = DateTime.UtcNow.AddDays(-15)
                    },

                    // Mais depósitos
                    new Transferencias
                    {
                        Valor = 300.00m,
                        Descricao = "Recarga da carteira",
                        Tipo = TipoTransferencia.Deposito,
                        RemetenteId = usuarios[2].Id,
                        DestinatarioId = usuarios[2].Id,
                        DataTransferencia = DateTime.UtcNow.AddDays(-12)
                    },
                    new Transferencias
                    {
                        Valor = 750.25m,
                        Descricao = "Transferência de negócio",
                        Tipo = TipoTransferencia.Transferencia,
                        RemetenteId = usuarios[3].Id,
                        DestinatarioId = usuarios[4].Id,
                        DataTransferencia = DateTime.UtcNow.AddDays(-10)
                    },

                    // Transferências recentes
                    new Transferencias
                    {
                        Valor = 150.00m,
                        Descricao = "Pagamento de almoço",
                        Tipo = TipoTransferencia.Transferencia,
                        RemetenteId = usuarios[4].Id,
                        DestinatarioId = usuarios[0].Id,
                        DataTransferencia = DateTime.UtcNow.AddDays(-5)
                    },
                    new Transferencias
                    {
                        Valor = 75.00m,
                        Descricao = "Contribuição para presente",
                        Tipo = TipoTransferencia.Transferencia,
                        RemetenteId = usuarios[0].Id,
                        DestinatarioId = usuarios[1].Id,
                        DataTransferencia = DateTime.UtcNow.AddDays(-3)
                    },
                    new Transferencias
                    {
                        Valor = 200.00m,
                        Descricao = "Reembolso",
                        Tipo = TipoTransferencia.Transferencia,
                        RemetenteId = usuarios[1].Id,
                        DestinatarioId = usuarios[0].Id,
                        DataTransferencia = DateTime.UtcNow.AddDays(-1)
                    }
                };

                context.Transferencias.AddRange(transferencias);
                await context.SaveChangesAsync();

                logger.LogInformation("✅ {Count} transferências criadas", transferencias.Count);

                //  COMMIT DA TRANSAÇÃO 
                await transaction.CommitAsync();

                logger.LogInformation("🎉 População do banco de dados concluída com sucesso!");
                logger.LogInformation("📊 Resumo:");
                logger.LogInformation("   - {UsuarioCount} usuários", usuarios.Count);
                logger.LogInformation("   - {CarteiraCount} carteiras", carteiras.Count);
                logger.LogInformation("   - {TransferenciaCount} transferências", transferencias.Count);
                logger.LogInformation("🔑 Credenciais de teste: email@email.com / senha123");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                logger.LogError(ex, "❌ Erro durante a população do banco de dados");
                throw;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Erro geral na população do banco de dados");
            throw;
        }
    }

    /// <summary>
    /// Método para ser chamado no Program.cs durante a inicialização
    /// </summary>
    public static async Task SeedDatabaseIfNeeded(IServiceProvider serviceProvider)
    {
        try
        {
            await ExecuteAsync(serviceProvider);
        }
        catch (Exception ex)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<object>>();
            logger.LogError(ex, "Falha ao popular o banco de dados");
        }
    }
}