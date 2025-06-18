using Microsoft.EntityFrameworkCore;
using Gerenciador.Aplicacao.AutoMapper;
using Gerenciador.Aplicacao.UseCases.Login.DoLogin;

//  USE CASES DE USUÁRIO 
using Gerenciador.Aplicacao.UseCases.Usuario.CriarUsuario;
using Gerenciador.Aplicacao.UseCases.Usuario.ObterUsuario;

//  USE CASES DE CARTEIRA 
using Gerenciador.Aplicacao.UseCases.Carteira.ConsultarSaldo;
using Gerenciador.Aplicacao.UseCases.Carteira.AdicionarSaldo;

//  USE CASES DE TRANSFERÊNCIA 
using Gerenciador.Aplicacao.UseCases.Transferencia.CriarTransferencia;
using Gerenciador.Aplicacao.UseCases.Transferencia.ListarTransferencias;

using Gerenciador.Dominio.Repositorios.Interfaces;
using Gerenciador.Infraestrutura;
using Gerenciador.Infraestrutura.Repositorios.Implementacoes;
using Gerenciador.Infraestrutura.Security;

namespace Gerenciador.Api.Extensions;

public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registra o banco de dados PostgreSQL
    /// </summary>
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPostgreSqlConfiguration(configuration);
        return services;
    }

    /// <summary>
    /// Registra o AutoMapper com validação
    /// </summary>
    public static IServiceCollection AddAutoMapperWithValidation(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile));

        // Validação do AutoMapper (recomendado em desenvolvimento)
        var mapperConfig = new AutoMapper.MapperConfiguration(cfg =>
            cfg.AddProfile<MappingProfile>());
        mapperConfig.AssertConfigurationIsValid();

        return services;
    }

    /// <summary>
    /// Registra todos os repositórios
    /// </summary>
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<ICarteiraRepository, CarteiraRepository>();
        services.AddScoped<ITransferenciaRepository, TransferenciaRepository>();

        return services;
    }

    /// <summary>
    /// Registra serviços de segurança e infraestrutura
    /// </summary>
    public static IServiceCollection AddSecurityServices(this IServiceCollection services)
    {
        // Registrar serviços de segurança
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        // Cache para otimização
        services.AddMemoryCache();

        return services;
    }

    /// <summary>
    /// Registra todos os Use Cases de Usuário
    /// </summary>
    public static IServiceCollection AddUsuarioUseCases(this IServiceCollection services)
    {
        services.AddScoped<ICriarUsuarioUseCase, CriarUsuarioUseCase>();
        services.AddScoped<IObterUsuarioUseCase, ObterUsuarioUseCase>();

        return services;
    }

    /// <summary>
    /// Registra todos os Use Cases de Carteira
    /// </summary>
    public static IServiceCollection AddCarteiraUseCases(this IServiceCollection services)
    {
        services.AddScoped<IConsultarSaldoUseCase, ConsultarSaldoUseCase>();
        services.AddScoped<IAdicionarSaldoUseCase, AdicionarSaldoUseCase>();

        return services;
    }

    /// <summary>
    /// Registra todos os Use Cases de Transferência
    /// </summary>
    public static IServiceCollection AddTransferenciaUseCases(this IServiceCollection services)
    {
        services.AddScoped<ICriarTransferenciaUseCase, CriarTransferenciaUseCase>();
        services.AddScoped<IListarTransferenciasUseCase, ListarTransferenciasUseCase>();

        return services;
    }

    /// <summary>
    /// Registra Use Cases de Autenticação
    /// </summary>
    public static IServiceCollection AddAuthUseCases(this IServiceCollection services)
    {
        services.AddScoped<IDoLoginUseCase, DoLoginUseCase>();

        return services;
    }

    /// <summary>
    /// Registra todos os Use Cases
    /// </summary>
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddUsuarioUseCases();
        services.AddCarteiraUseCases();
        services.AddTransferenciaUseCases();
        services.AddAuthUseCases();

        return services;
    }

    /// <summary>
    /// Configura CORS para desenvolvimento e produção
    /// </summary>
    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            //  DESENVOLVIMENTO 
            options.AddPolicy("Development", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });

            //  PRODUÇÃO 
            options.AddPolicy("Production", builder =>
            {
                builder.WithOrigins(
                    // Local development
                    "http://localhost:3000",    // React
                    "http://localhost:5173",    // Vite (React/Vue)
                    "http://localhost:8080",    // Vue CLI
                    "http://localhost:4200",    // Angular

                    // Production hosting
                    "https://carteiras-app.vercel.app",
                    "https://gerenciador-carteiras.netlify.app"
                )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
            });
        });

        return services;
    }

    /// <summary>
    /// Registra todas as dependências da aplicação
    /// </summary>
    public static IServiceCollection AddApplicationDependencies(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        //  1. PERSISTÊNCIA 
        services.AddDatabase(configuration);
        services.AddRepositories();

        //  2. MAPEAMENTO 
        services.AddAutoMapperWithValidation();

        //  3. SEGURANÇA 
        services.AddSecurityServices();
        services.AddJwtAuthentication(configuration, environment);

        //  4. REGRAS DE NEGÓCIO (USE CASES) 
        services.AddUseCases();

        //  5. CORS 
        services.AddCorsConfiguration();

        return services;
    }

    /// <summary>
    /// 🔧 HELPER - Validar se todas as dependências estão registradas
    /// </summary>
    public static void ValidateDependencies(this IServiceProvider serviceProvider)
    {
        try
        {
            //  TESTAR REPOSITÓRIOS 
            serviceProvider.GetRequiredService<IUsuarioRepository>();
            serviceProvider.GetRequiredService<ICarteiraRepository>();
            serviceProvider.GetRequiredService<ITransferenciaRepository>();

            //  TESTAR SEGURANÇA 
            serviceProvider.GetRequiredService<IPasswordHasher>();
            serviceProvider.GetRequiredService<IJwtTokenGenerator>();

            //  TESTAR USE CASES ESSENCIAIS 
            serviceProvider.GetRequiredService<ICriarUsuarioUseCase>();
            serviceProvider.GetRequiredService<IDoLoginUseCase>();
            serviceProvider.GetRequiredService<IConsultarSaldoUseCase>();
            serviceProvider.GetRequiredService<ICriarTransferenciaUseCase>();

            Console.WriteLine("✅ Todas as dependências estão registradas corretamente!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro na validação de dependências: {ex.Message}");
            throw;
        }
    }
}