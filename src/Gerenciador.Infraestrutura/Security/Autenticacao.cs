using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Gerenciador.Infraestrutura.Security;

public static class Autenticacao
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment? environment = null)
    {
        //  OBTER CONFIGURAÇÕES COM FALLBACK PARA VARIÁVEIS DE AMBIENTE 
        var jwtKey = configuration["JwtSettings:Secret"]
                    ?? Environment.GetEnvironmentVariable("JWT_SECRET")
                    ?? throw new InvalidOperationException("JWT Key não configurada. Configure 'JwtSettings:Secret' no appsettings ou JWT_SECRET como variável de ambiente.");

        var jwtIssuer = configuration["JwtSettings:Issuer"]
                       ?? Environment.GetEnvironmentVariable("JWT_ISSUER")
                       ?? "GerenciadorAPI";

        var jwtAudience = configuration["JwtSettings:Audience"]
                         ?? Environment.GetEnvironmentVariable("JWT_AUDIENCE")
                         ?? "GerenciadorAPI";

        var jwtExpirationMinutes = configuration.GetValue<int>("JwtSettings:ExpirationInMinutes", 60);

        //  VALIDAÇÕES DE SEGURANÇA 
        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            throw new ArgumentNullException(nameof(jwtKey), "A chave secreta do JWT não pode estar vazia.");
        }

        if (jwtKey.Length < 32)
        {
            throw new ArgumentException("A chave secreta do JWT deve ter pelo menos 32 caracteres para segurança adequada.", nameof(jwtKey));
        }

        // LOGGING SEGURO SEM SERVICEPROVIDER TEMPORÁRIO
        Console.WriteLine("🔧 Configurando JWT Authentication");
        Console.WriteLine($"🔧 JWT Issuer: {jwtIssuer}");
        Console.WriteLine($"🔧 JWT Audience: {jwtAudience}");
        Console.WriteLine($"🔧 JWT Expiration: {jwtExpirationMinutes} minutos");
        Console.WriteLine($"🔧 JWT Key Length: {jwtKey.Length} caracteres");

        //  CONVERTER CHAVE PARA BYTES (UTF8 é MELHOR QUE ASCII) =====
        var key = Encoding.UTF8.GetBytes(jwtKey);

        //  CONFIGURAR AUTENTICAÇÃO JWT 
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            //  CONFIGURAÇÕES DE DESENVOLVIMENTO VS PRODUÇÃO 
            options.RequireHttpsMetadata = environment?.IsProduction() ?? true; // HTTPS obrigatório em produção
            options.SaveToken = true;

            //  PARÂMETROS DE VALIDAÇÃO DO TOKEN 
            options.TokenValidationParameters = new TokenValidationParameters
            {
                // Validar chave de assinatura
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),

                // Validar emissor
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,

                // Validar audiência
                ValidateAudience = true,
                ValidAudience = jwtAudience,

                // ✅ VALIDAR EXPIRAÇÃO (CRÍTICO PARA RETORNAR 401!)
                ValidateLifetime = true,

                // Tolerar diferença de tempo entre servidores (padrão: 5 min, definindo 0 para ser mais rigoroso)
                ClockSkew = TimeSpan.Zero,

                // Configurações adicionais de segurança
                RequireSignedTokens = true,

                // Definir qual claim será usado como NameIdentifier
                NameClaimType = "nameid",
                RoleClaimType = "role"
            };

            // EVENTOS PARA LOGGING (SEM SERVICEPROVIDER TEMPORÁRIO)
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    // USAR LOGGER DO CONTEXTO (SEGURO)
                    var logger = context.HttpContext.RequestServices.GetService<ILogger<object>>();
                    logger?.LogWarning("🔒 JWT Authentication failed: {Exception}", context.Exception.Message);

                    // GARANTIR QUE RETORNE 401 PARA TOKEN INVÁLIDO
                    Console.WriteLine($"🔒 JWT Authentication failed: {context.Exception.Message}");
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    // USAR LOGGER DO CONTEXTO (SEGURO)
                    var logger = context.HttpContext.RequestServices.GetService<ILogger<object>>();
                    var userId = context.Principal?.FindFirst("nameid")?.Value ?? "Unknown";
                    logger?.LogDebug("✅ JWT Token validated successfully for user: {UserId}", userId);

                    Console.WriteLine($"✅ JWT Token validated for user: {userId}");
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    // USAR LOGGER DO CONTEXTO (SEGURO)
                    var logger = context.HttpContext.RequestServices.GetService<ILogger<object>>();
                    logger?.LogWarning("🔒 JWT Authentication challenge: {Error} - {ErrorDescription}",
                                     context.Error, context.ErrorDescription);

                    Console.WriteLine($"🔒 JWT Challenge: {context.Error} - {context.ErrorDescription}");

                    //  GARANTIR QUE RETORNE 401
                    if (!context.Response.HasStarted)
                    {
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";

                        var response = new
                        {
                            Sucesso = false,
                            Mensagem = "Token de acesso inválido ou expirado"
                        };

                        return context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
                    }

                    return Task.CompletedTask;
                }
            };
        });

        //  REGISTRAR SERVIÇOS DE SEGURANÇA 
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        //  CONFIGURAR AUTORIZAÇÃO 
        services.AddAuthorization(options =>
        {
            // Política padrão: requer usuário autenticado
            options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        Console.WriteLine("✅ JWT Authentication configurado com sucesso");

        return services;
    }
}