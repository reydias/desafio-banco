using System.Text;
using System.Text.Json;
using AspNetCoreRateLimit;
using Lancamentos.Application.Converters;
using Lancamentos.Application.Handlers;
using Lancamentos.Domain.Interfaces;
using Lancamentos.Infrastructure.Data;
using Lancamentos.Infrastructure.Messaging;
using Lancamentos.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel for HTTPS only when running in Docker (ASPNETCORE_URLS is set)
// When running locally, let the launchSettings.json control the ports
var urls = builder.Configuration["ASPNETCORE_URLS"];
if (!string.IsNullOrEmpty(urls) && urls.Contains("8080"))
{
    // Running in Docker - configure Kestrel explicitly
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(8080); // HTTP
        options.ListenAnyIP(8081, listenOptions =>
        {
            // HTTPS com certificado auto-assinado gerado no Dockerfile (formato PFX)
            var pfxPath = "/app/https-dev.pfx";
            
            if (System.IO.File.Exists(pfxPath))
            {
                // Usar certificado PFX gerado no Dockerfile (sem senha)
                listenOptions.UseHttps(pfxPath);
            }
            else
            {
                // Se certificado não existir, não configurar HTTPS nesta porta
                // A aplicação funcionará apenas com HTTP na porta 8080
                return;
            }
        });
    });
}

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new TipoLancamentoJsonConverter());
    });
builder.Services.AddEndpointsApiExplorer();

// Swagger com suporte a JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo 
    { 
        Title = "Lancamentos API", 
        Version = "v1",
        Description = "API para gerenciamento de lançamentos financeiros"
    });
});

// Database
builder.Services.AddDbContext<LancamentoDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    
// Remover Usuarios do contexto - agora está no SSO.Admin

// Repositories
builder.Services.AddScoped<ILancamentoRepository, LancamentoRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Event Publisher
builder.Services.AddSingleton<IEventPublisher, RabbitMQEventPublisher>();

// Handlers
builder.Services.AddScoped<CriarLancamentoHandler>();
builder.Services.AddScoped<ObterLancamentoHandler>();

// JWT Authentication - Usar as mesmas configurações do SSO para validação
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] ?? "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "SSOAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "SSOAPI";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(5) // Tolerância de 5 minutos para diferenças de relógio
    };
});

builder.Services.AddAuthorization();

// Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// CORS - Configuração mais restritiva
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "*" };
        
        if (allowedOrigins.Contains("*"))
        {
            // Não pode usar AllowCredentials() com AllowAnyOrigin()
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        else
        {
            // Quando especifica origens, pode usar AllowCredentials()
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
    });
});

var app = builder.Build();

// Log das URLs configuradas
var logger = app.Services.GetRequiredService<ILogger<Program>>();
var configuredUrls = app.Configuration["ASPNETCORE_URLS"] ?? "não configurado";
logger.LogInformation("ASPNETCORE_URLS: {Urls}", configuredUrls);

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "swagger/{documentName}/swagger.json";
    });
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Lancamentos API v1");
        c.RoutePrefix = "swagger";
        // Configurar para usar o protocolo atual (HTTP ou HTTPS)
        c.ConfigObject.AdditionalItems["validatorUrl"] = null;
    });
}

// Rate Limiting Middleware
app.UseIpRateLimiting();

// CORS deve vir antes de Authentication
app.UseCors("AllowSpecificOrigins");

// Authentication e Authorization devem vir antes do UseHttpsRedirection para evitar problemas com redirects
app.UseAuthentication();
app.UseAuthorization();

// HTTPS redirection habilitado - HTTPS está configurado no Docker
app.UseHttpsRedirection();

app.MapControllers();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LancamentoDbContext>();
    context.Database.EnsureCreated();
}

app.Run();
