FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/ConsolidadoDiario.API/ConsolidadoDiario.API.csproj", "src/ConsolidadoDiario.API/"]
COPY ["src/ConsolidadoDiario.Application/ConsolidadoDiario.Application.csproj", "src/ConsolidadoDiario.Application/"]
COPY ["src/ConsolidadoDiario.Domain/ConsolidadoDiario.Domain.csproj", "src/ConsolidadoDiario.Domain/"]
COPY ["src/ConsolidadoDiario.Infrastructure/ConsolidadoDiario.Infrastructure.csproj", "src/ConsolidadoDiario.Infrastructure/"]
RUN dotnet restore "src/ConsolidadoDiario.API/ConsolidadoDiario.API.csproj"
COPY . .
WORKDIR "/src/src/ConsolidadoDiario.API"
RUN dotnet build "ConsolidadoDiario.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ConsolidadoDiario.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Mudar para root temporariamente para instalar openssl e copiar arquivos
USER root

# Instalar openssl para gerar certificado auto-assinado
RUN apt-get update && apt-get install -y openssl && rm -rf /var/lib/apt/lists/*

# Copiar arquivos publicados primeiro
COPY --from=publish /app/publish .

# Gerar certificado auto-assinado para HTTPS após copiar os arquivos
# Gerar chave privada e certificado, depois converter para PFX (formato que o Kestrel precisa)
RUN openssl req -x509 -newkey rsa:4096 -nodes \
    -keyout /app/https-dev.key \
    -out /app/https-dev.crt \
    -subj "/CN=localhost" \
    -days 365 && \
    openssl pkcs12 -export \
    -out /app/https-dev.pfx \
    -inkey /app/https-dev.key \
    -in /app/https-dev.crt \
    -passout pass: && \
    chmod 644 /app/https-dev.pfx && \
    chmod 644 /app/https-dev.crt && \
    chmod 644 /app/https-dev.key && \
    chown -R $APP_UID:$APP_UID /app

# Voltar para o usuário não-root
USER $APP_UID

EXPOSE 8080
EXPOSE 8081
ENTRYPOINT ["dotnet", "ConsolidadoDiario.API.dll"]




