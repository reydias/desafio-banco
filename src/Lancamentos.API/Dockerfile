# Acesse https://aka.ms/customizecontainer para saber como personalizar seu contêiner de depuração e como o Visual Studio usa este Dockerfile para criar suas imagens para uma depuração mais rápida.

# Esta fase é usada durante a execução no VS no modo rápido (Padrão para a configuração de Depuração)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# Esta fase é usada para compilar o projeto de serviço
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Lancamentos.API/Lancamentos.API.csproj", "src/Lancamentos.API/"]
COPY ["src/Lancamentos.Application/Lancamentos.Application.csproj", "src/Lancamentos.Application/"]
COPY ["src/Lancamentos.Domain/Lancamentos.Domain.csproj", "src/Lancamentos.Domain/"]
COPY ["src/Lancamentos.Infrastructure/Lancamentos.Infrastructure.csproj", "src/Lancamentos.Infrastructure/"]
RUN dotnet restore "./src/Lancamentos.API/Lancamentos.API.csproj"
COPY . .
WORKDIR "/src/src/Lancamentos.API"
RUN dotnet build "./Lancamentos.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Esta fase é usada para publicar o projeto de serviço a ser copiado para a fase final
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Lancamentos.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Esta fase é usada na produção ou quando executada no VS no modo normal (padrão quando não está usando a configuração de Depuração)
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
ENTRYPOINT ["dotnet", "Lancamentos.API.dll"]


