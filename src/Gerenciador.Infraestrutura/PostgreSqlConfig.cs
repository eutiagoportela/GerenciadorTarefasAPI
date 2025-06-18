using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Gerenciador.Dominio.Repositorios.Interfaces;
using Gerenciador.Infraestrutura.Repositorios.Implementacoes;

namespace Gerenciador.Infraestrutura;

public static class PostgreSqlConfig
{
    public static IServiceCollection AddPostgreSqlConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = GetConnectionString(configuration);

        // Configurar o contexto do Entity Framework
        services.AddDbContext<PostgreSqlDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                // Configurações adicionais para melhorar a estabilidade da conexão
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null);

                npgsqlOptions.CommandTimeout(30);
                npgsqlOptions.MigrationsAssembly("Gerenciador.Infraestrutura");
            });
        });

        // Registrar repositórios
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<ICarteiraRepository, CarteiraRepository>();
        services.AddScoped<ITransferenciaRepository, TransferenciaRepository>();

        return services;
    }

    private static string GetConnectionString(IConfiguration configuration)
    {
        // Registrar o logger para erros de conexão
        using var loggerFactory = LoggerFactory.Create(builder =>
            builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
        var logger = loggerFactory.CreateLogger("DatabaseConnection");

        try
        {
            // 1. Tentar connection string normal (Development)
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrEmpty(connectionString))
            {
                logger.LogInformation("Usando connection string do arquivo de configuração");
                return connectionString;
            }

            // 2. Se não tem, tentar DATABASE_URL (Heroku)
            connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
            if (!string.IsNullOrEmpty(connectionString) && connectionString.StartsWith("postgres://"))
            {
                logger.LogInformation("Usando connection string do Heroku (DATABASE_URL)");
                return ConvertHerokuConnectionString(connectionString);
            }

            // 3. Tentar construir da configuração individual
            connectionString = BuildConnectionString(configuration);
            if (!string.IsNullOrEmpty(connectionString))
            {
                logger.LogInformation("Usando connection string construída a partir de variáveis individuais");
                return connectionString;
            }

            // 4. Fallback para localhost (ambiente de desenvolvimento)
            logger.LogWarning("Nenhuma connection string configurada, usando configuração padrão para localhost");
            return "Host=localhost;Database=gerenciadordb;Username=postgres;Password=postgres;Port=5432;SSL Mode=Prefer;Trust Server Certificate=true";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao obter connection string");
            throw new InvalidOperationException("Não foi possível obter uma connection string válida. Verifique suas configurações.", ex);
        }
    }

    private static string BuildConnectionString(IConfiguration configuration)
    {
        var host = configuration["Postgres:Host"] ?? Environment.GetEnvironmentVariable("POSTGRES_HOST");
        var db = configuration["Postgres:Database"] ?? Environment.GetEnvironmentVariable("POSTGRES_DB");
        var username = configuration["Postgres:Username"] ?? Environment.GetEnvironmentVariable("POSTGRES_USER");
        var password = configuration["Postgres:Password"] ?? Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
        var port = configuration["Postgres:Port"] ?? Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";

        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(db) ||
            string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            return string.Empty;
        }

        return $"Host={host};Database={db};Username={username};Password={password};Port={port};SSL Mode=Prefer;Trust Server Certificate=true";
    }

    private static string ConvertHerokuConnectionString(string herokuConnectionString)
    {
        // postgres://user:password@host:port/database
        var uri = new Uri(herokuConnectionString);
        var userInfo = uri.UserInfo.Split(':');

        return $"Host={uri.Host};Database={uri.AbsolutePath.Trim('/')};Username={userInfo[0]};Password={userInfo[1]};Port={uri.Port};SSL Mode=Require;Trust Server Certificate=true";
    }
}