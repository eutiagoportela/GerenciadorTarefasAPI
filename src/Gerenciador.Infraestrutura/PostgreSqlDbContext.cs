using Microsoft.EntityFrameworkCore;
using Gerenciador.Dominio.Entidades;
using Gerenciador.Dominio.Enum;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Gerenciador.Infraestrutura;

public class PostgreSqlDbContext : DbContext
{
    public PostgreSqlDbContext(DbContextOptions<PostgreSqlDbContext> options) : base(options)
    {
    }

    public DbSet<Usuarios> Usuarios { get; set; }
    public DbSet<Carteiras> Carteiras { get; set; }
    public DbSet<Transferencias> Transferencias { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //  CONFIGURAÇÕES DAS ENTIDADES 
        modelBuilder.Entity<Usuarios>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Nome).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(150);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.SenhaHash).IsRequired();
            entity.Property(u => u.DataCriacao).IsRequired();
            entity.Property(u => u.DataAtualizacao).IsRequired();

            entity.HasOne(u => u.Carteira)
                  .WithOne(c => c.Usuario)
                  .HasForeignKey<Carteiras>(c => c.UsuarioId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Carteiras>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Saldo).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(c => c.DataCriacao).IsRequired();
            entity.Property(c => c.DataAtualizacao).IsRequired();
            entity.Property(c => c.UsuarioId).IsRequired();

            // Índices para performance
            entity.HasIndex(c => c.UsuarioId).IsUnique();
        });

        modelBuilder.Entity<Transferencias>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Valor).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(t => t.Descricao).HasMaxLength(200);
            entity.Property(t => t.Tipo).IsRequired();
            entity.Property(t => t.DataTransferencia).IsRequired();
            entity.Property(t => t.RemetenteId).IsRequired();
            entity.Property(t => t.DestinatarioId).IsRequired();

            entity.Property(t => t.Tipo)
                  .HasConversion<int>();

            // Relacionamentos
            entity.HasOne(t => t.Remetente)
                  .WithMany(u => u.TransferenciasEnviadas)
                  .HasForeignKey(t => t.RemetenteId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.Destinatario)
                  .WithMany(u => u.TransferenciasRecebidas)
                  .HasForeignKey(t => t.DestinatarioId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Índices para performance
            entity.HasIndex(t => t.RemetenteId);
            entity.HasIndex(t => t.DestinatarioId);
            entity.HasIndex(t => t.DataTransferencia);
            entity.HasIndex(t => new { t.RemetenteId, t.DataTransferencia });
            entity.HasIndex(t => new { t.DestinatarioId, t.DataTransferencia });
        });

        //  🌱 SEED DATA - USUÁRIOS DE TESTE 
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        //  USUÁRIO ADMINISTRADOR 
        modelBuilder.Entity<Usuarios>().HasData(
            new Usuarios
            {
                Id = 1,
                Nome = "Administrador",
                Email = "admin@teste.com",
                SenhaHash = BCrypt.Net.BCrypt.HashPassword("123456"), // Senha: 123456
                DataCriacao = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                DataAtualizacao = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        //  USUÁRIO DE TESTE 
        modelBuilder.Entity<Usuarios>().HasData(
            new Usuarios
            {
                Id = 2,
                Nome = "João Silva",
                Email = "joao@teste.com",
                SenhaHash = BCrypt.Net.BCrypt.HashPassword("123456"), // Senha: 123456
                DataCriacao = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                DataAtualizacao = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        //  CARTEIRAS INICIAIS 
        modelBuilder.Entity<Carteiras>().HasData(
            new Carteiras
            {
                Id = 1,
                UsuarioId = 1,
                Saldo = 1000,
                DataCriacao = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                DataAtualizacao = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Carteiras
            {
                Id = 2,
                UsuarioId = 2,
                Saldo = 500,
                DataCriacao = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                DataAtualizacao = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        //  TRANSFERÊNCIAS INICIAIS 
        modelBuilder.Entity<Transferencias>().HasData(
            new Transferencias
            {
                Id = 1,
                Valor = 200,
                Descricao = "Transferência inicial",
                Tipo = TipoTransferencia.Transferencia,
                RemetenteId = 1,
                DestinatarioId = 2,
                DataTransferencia = new DateTime(2024, 1, 2, 10, 0, 0, DateTimeKind.Utc)
            },
            new Transferencias
            {
                Id = 2,
                Valor = 100,
                Descricao = "Depósito inicial",
                Tipo = TipoTransferencia.Deposito,
                RemetenteId = 2,
                DestinatarioId = 2,
                DataTransferencia = new DateTime(2024, 1, 2, 11, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}