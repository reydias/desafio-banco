using System.Text;
using SSO.Admin.Application.Handlers;
using SSO.Admin.Domain.Interfaces;
using SSO.Admin.Infrastructure.Data;
using SSO.Admin.Infrastructure.Repositories;
using SSO.Admin.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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

// Swagger com suporte a JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo 
    { 
        Title = "SSO Admin API", 
        Version = "v1",
        Description = "API para gerenciamento de usuários e tokens de autenticação"
    });

    // Definição de segurança para JWT Bearer
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando o esquema Bearer. Exemplo: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.ParameterLocation.Header,
        Type = Microsoft.OpenApi.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

});

// Database
builder.Services.AddDbContext<SSOAdminDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)));

// Repositories
builder.Services.AddScoped<IUsuarioTokenRepository, UsuarioTokenRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();

// Handlers
builder.Services.AddScoped<CriarUsuarioTokenHandler>();
builder.Services.AddScoped<AtualizarUsuarioTokenHandler>();
builder.Services.AddScoped<AtualizarSenhaHandler>();
builder.Services.AddScoped<ObterUsuarioTokenHandler>();
builder.Services.AddScoped<LoginHandler>();

// JWT Authentication
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
    
    // Habilitar logs para debug
    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(context.Exception, "Falha na autenticação JWT");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Token JWT validado com sucesso para o usuário: {User}", context.Principal?.Identity?.Name);
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("Desafio de autenticação JWT. Erro: {Error}, Descrição: {ErrorDescription}", 
                context.Error, context.ErrorDescription);
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// CORS
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

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "swagger/{documentName}/swagger.json";
    });
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SSO Admin API v1");
        c.RoutePrefix = "swagger";
        // Configurar para usar o protocolo atual (HTTP ou HTTPS)
        c.ConfigObject.AdditionalItems["validatorUrl"] = null;
    });
}

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
    var context = scope.ServiceProvider.GetRequiredService<SSOAdminDbContext>();
    context.Database.EnsureCreated();
}

app.Run();
