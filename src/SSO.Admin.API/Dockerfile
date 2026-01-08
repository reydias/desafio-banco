FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/SSO.Admin.API/SSO.Admin.API.csproj", "src/SSO.Admin.API/"]
COPY ["src/SSO.Admin.Application/SSO.Admin.Application.csproj", "src/SSO.Admin.Application/"]
COPY ["src/SSO.Admin.Domain/SSO.Admin.Domain.csproj", "src/SSO.Admin.Domain/"]
COPY ["src/SSO.Admin.Infrastructure/SSO.Admin.Infrastructure.csproj", "src/SSO.Admin.Infrastructure/"]
RUN dotnet restore "src/SSO.Admin.API/SSO.Admin.API.csproj"
COPY . .
WORKDIR "/src/src/SSO.Admin.API"
RUN dotnet build "SSO.Admin.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SSO.Admin.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

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
ENTRYPOINT ["dotnet", "SSO.Admin.API.dll"]

