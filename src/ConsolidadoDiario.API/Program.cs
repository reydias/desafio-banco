using System.Text;
using ConsolidadoDiario.Application.Handlers;
using ConsolidadoDiario.Domain.Interfaces;
using ConsolidadoDiario.Infrastructure.Cache;
using ConsolidadoDiario.Infrastructure.Data;
using ConsolidadoDiario.Infrastructure.Messaging;
using ConsolidadoDiario.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

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
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo 
    { 
        Title = "Consolidado Diário API", 
        Version = "v1",
        Description = "API para consulta de consolidado diário"
    });
});

// Database
builder.Services.AddDbContext<ConsolidadoDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var connectionString = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
    return ConnectionMultiplexer.Connect(connectionString);
});

// Repositories
builder.Services.AddScoped<IConsolidadoRepository, ConsolidadoRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Cache Service
builder.Services.AddSingleton<ICacheService, RedisCacheService>();

// Handlers
builder.Services.AddScoped<ObterConsolidadoHandler>();
builder.Services.AddScoped<ProcessarLancamentoEventHandler>();

// RabbitMQ Consumer (Background Service)
builder.Services.AddHostedService<RabbitMQConsumer>();

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

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "swagger/{documentName}/swagger.json";
    });
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Consolidado Diário API v1");
        c.RoutePrefix = "swagger";
        // Configurar para usar o protocolo atual (HTTP ou HTTPS)
        c.ConfigObject.AdditionalItems["validatorUrl"] = null;
    });
}

// CORS deve vir antes de Authentication
app.UseCors("AllowAll");

// Authentication e Authorization devem vir antes do UseHttpsRedirection para evitar problemas com redirects
app.UseAuthentication();
app.UseAuthorization();

// HTTPS redirection habilitado - HTTPS está configurado no Docker
app.UseHttpsRedirection();
app.MapControllers();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ConsolidadoDbContext>();
    context.Database.EnsureCreated();
}

app.Run();
