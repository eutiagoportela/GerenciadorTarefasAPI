using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using Gerenciador.Api.Extensions;
using Gerenciador.Api.Middleware;
using Gerenciador.Api.Scripts; 

var builder = WebApplication.CreateBuilder(args);

// ===== CONFIGURAR LOGGER =====
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// ===== REGISTRAR SERVIÇOS =====
builder.Services.AddControllers();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Mantém PascalCase
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); // Aceita enum como string também
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(config =>
{
    config.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Gerenciador de Carteiras API",
        Version = "v1",
        Description = "API para gerenciamento de carteiras digitais e transferências"
    });

    config.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = @"Cabeçalho de autorização JWT usando o esquema Bearer. Insira 'Bearer' [espaço] e, em seguida, seu token no campo de texto abaixo. Exemplo: 'Bearer 12345abcdef'.",
        In = ParameterLocation.Header,
        Scheme = "Bearer",
        Type = SecuritySchemeType.ApiKey
    });
    config.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

// DEPENDÊNCIAS DA APLICAÇÃO
Console.WriteLine("Registrando dependências da aplicação...");
builder.Services.AddApplicationDependencies(builder.Configuration, builder.Environment);
Console.WriteLine("Dependências registradas com sucesso!");

var app = builder.Build();

//  POPULAR BANCO DE DADOS
Console.WriteLine("🌱 Executando script de população do banco de dados...");
try
{
    await PopulateDb.SeedDatabaseIfNeeded(app.Services);
    Console.WriteLine("✅ Script de população executado com sucesso!");
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ Aviso: Erro ao executar script de população: {ex.Message}");
    // Não quebra a aplicação se falhar
}

// CONFIGURAR PIPELINE

// Adicionar middleware de tratamento de erros global
app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gerenciador de Carteiras API v1");
        c.RoutePrefix = "swagger";
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    });
}

// CORS AUTOMÁTICO BASEADO NO AMBIENTE
if (app.Environment.IsDevelopment())
{
    Console.WriteLine("Aplicando CORS para DESENVOLVIMENTO");
    app.UseCors("Development");
}
else
{
    Console.WriteLine("Aplicando CORS para PRODUÇÃO");
    app.UseCors("Production");
}

// Health Check (retorna JSON)
app.MapHealthChecks("/api/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            service = "GerenciadorAPI",
            timestamp = DateTime.UtcNow,
            environment = app.Environment.EnvironmentName
        };

        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    }
});

// ENDPOINT PARA POPULAR BANCO MANUALMENTE (DESENVOLVIMENTO)
if (app.Environment.IsDevelopment())
{
    app.MapPost("/api/seed", async (IServiceProvider serviceProvider) =>
    {
        try
        {
            await PopulateDb.ExecuteAsync(serviceProvider);
            return Results.Ok(new { message = "Banco populado com sucesso!", timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message, timestamp = DateTime.UtcNow });
        }
    }).WithTags("Development");
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

try
{
    Console.WriteLine($"🚀 Aplicação iniciada em modo: {app.Environment.EnvironmentName}");
    Console.WriteLine($"📊 Swagger UI: {(app.Environment.IsDevelopment() ? "https://localhost:5001/swagger" : "Não disponível em produção")}");
    Console.WriteLine($"❤️ Health Check: https://localhost:5001/api/health");

    if (app.Environment.IsDevelopment())
    {
        Console.WriteLine($"🌱 Seed Manual: POST https://localhost:5001/api/seed");
        Console.WriteLine($"🔑 Credenciais de teste: admin@teste.com / 123456");
    }

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Aplicação terminou inesperadamente: {ex.Message}");
}