# Desafio Banco - Sistema de Controle de Fluxo de Caixa

## 📋 Índice

- [Descrição](#descrição)
- [Pré-requisitos](#pré-requisitos)
- [Tecnologias](#tecnologias)
- [Arquitetura](#arquitetura)
- [Como Executar](#como-executar)
- [Configuração Docker](#configuração-docker)
- [Autenticação e Autorização](#autenticação-e-autorização)
- [Testes](#testes)
- [Desenvolvimento](#desenvolvimento)
- [Troubleshooting](#troubleshooting)

---

## Descrição

Sistema desenvolvido em .NET Core para controle de fluxo de caixa diário, incluindo:
- **Serviço de Lançamentos**: Gerencia créditos e débitos diários
- **Serviço de Consolidado Diário**: Gera relatório consolidado do saldo diário
- **Serviço SSO Admin**: Gerencia usuários e autenticação JWT

---
## Pré-requisitos

- Docker Desktop instalado e rodando
- .NET SDK 10.0 (opcional, apenas para desenvolvimento local)
- WSL 2 (Windows) ou Docker Engine (Linux/Mac)
- Pelo menos 4GB de RAM disponível para Docker

---
## Tecnologias

### Backend
- **.NET 10.0** - Framework principal
- **ASP.NET Core** - Framework web
- **Entity Framework Core 10.0.1** - ORM
- **JWT Bearer Authentication** - Autenticação
- **BCrypt.Net** - Criptografia de senhas
- **AspNetCoreRateLimit** - Rate limiting

### Infraestrutura
- **SQL Server 2022** - Banco de dados relacional
- **RabbitMQ 3** - Message broker
- **Redis 7** - Cache em memória
- **Docker & Docker Compose** - Containerização

### Ferramentas de Desenvolvimento
- **Swagger/OpenAPI** - Documentação de API
- **Visual Studio 2022** - IDE
- **Git** - Controle de versão

---

## Arquitetura

O projeto utiliza **Clean Architecture** com os seguintes padrões:
- **Domain-Driven Design (DDD)**
- **CQRS** (Command Query Responsibility Segregation)
- **Repository Pattern**
- **Unit of Work Pattern**
- **Event-Driven Architecture**

### Diagrama de Arquitetura

<img width="878" height="570" alt="image" src="https://github.com/user-attachments/assets/5d4ea970-ecfc-4ca2-b226-7511428d5625" />

### Componentes Principais

<img width="601" height="837" alt="image" src="https://github.com/user-attachments/assets/8592ad67-9fe4-4135-be6a-a169b5db747e" />

#### 1. SSO Admin API

**Responsabilidade:** Gerenciamento de usuários e autenticação JWT

**Funcionalidades:**
- CRUD de usuários
- Autenticação e geração de tokens JWT
- Validação de tokens
- Criptografia de senhas com BCrypt

**Banco de Dados:** SSOAdminDB (SQL Server)

#### 2. Lancamentos API

**Responsabilidade:** Gerenciamento de lançamentos financeiros (créditos e débitos)

**Funcionalidades:**
- Criar lançamentos
- Consultar lançamentos
- Publicar eventos de lançamento criado
- Autenticação JWT obrigatória
- Rate Limiting

**Banco de Dados:** LancamentosDB (SQL Server)

**Eventos Publicados:**
- `LancamentoCriadoEvent` → RabbitMQ

#### 3. Consolidado Diário API

**Responsabilidade:** Consulta de consolidado diário de saldo

**Funcionalidades:**
- Consultar consolidado por data
- Cache em Redis para performance
- Consumo de eventos do RabbitMQ
- Atualização automática via eventos
- Autenticação JWT obrigatória

**Banco de Dados:** ConsolidadoDB (SQL Server)

**Cache:** Redis (TTL configurável)

**Background Service:** RabbitMQ Consumer (processa eventos de lançamento)

## Padrões Arquiteturais

### 1. Clean Architecture

O sistema é organizado em camadas concêntricas:

- **Domain (Núcleo):** Entidades, interfaces e regras de negócio puras
- **Application:** Casos de uso, handlers, DTOs e lógica de aplicação
- **Infrastructure:** Implementações concretas (repositórios, banco de dados, mensageria)
- **API (Presentation):** Controllers, configuração e endpoints HTTP

**Benefícios:**
- Independência de frameworks
- Testabilidade
- Manutenibilidade
- Flexibilidade para mudanças

### 2. Domain-Driven Design (DDD)

- **Entidades:** Representam objetos de negócio com identidade única
- **Value Objects:** Objetos imutáveis sem identidade
- **Aggregates:** Agrupamento de entidades relacionadas
- **Domain Events:** Eventos que representam algo importante no domínio

### 3. CQRS (Command Query Responsibility Segregation)

**Commands (Escrita):**
- `CriarLancamentoCommand`
- `CriarUsuarioTokenCommand`
- `AtualizarUsuarioTokenCommand`

**Queries (Leitura):**
- `ObterLancamentoQuery`
- `ObterConsolidadoQuery`
- `ObterUsuarioTokenQuery`

**Benefícios:**
- Separação clara de responsabilidades
- Otimização independente de leitura e escrita
- Escalabilidade

### 4. Repository Pattern

Abstração da camada de acesso a dados:

```csharp
public interface ILancamentoRepository
{
    Task<Lancamento> GetByIdAsync(int id);
    Task AddAsync(Lancamento lancamento);
    Task<IEnumerable<Lancamento>> GetAllAsync();
}
```

**Benefícios:**
- Testabilidade (mock de repositórios)
- Flexibilidade para trocar implementação
- Isolamento da lógica de negócio

### 5. Unit of Work Pattern

Gerencia transações e garante consistência:

```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
```

### 6. Event-Driven Architecture

Comunicação assíncrona entre serviços:

- **Event Publisher:** Publica eventos no RabbitMQ
- **Event Consumer:** Consome eventos e processa (Background Service)
- **Event Store:** RabbitMQ como message broker

**Fluxo:**
1. Lancamentos API cria lançamento
2. Publica `LancamentoCriadoEvent` no RabbitMQ
3. Consolidado Diário API consome evento
4. Atualiza consolidado e cache

---

### Decisões Arquiteturais (ADRs)

#### ADR-001: Microserviços vs Monolito

**Decisão:** Arquitetura de microserviços

**Contexto:** Sistema precisa escalar independentemente e ter serviços desacoplados

**Consequências:**
- ✅ Escalabilidade independente
- ✅ Desenvolvimento paralelo
- ✅ Tecnologias independentes
- ❌ Complexidade de deployment
- ❌ Necessidade de comunicação entre serviços

#### ADR-002: Comunicação Síncrona vs Assíncrona

**Decisão:** Híbrida - Síncrona para APIs REST, Assíncrona para eventos

**Contexto:** 
- APIs REST para operações que precisam de resposta imediata
- Eventos assíncronos para processamento em background

**Consequências:**
- ✅ Performance melhorada (processamento assíncrono)
- ✅ Desacoplamento entre serviços
- ✅ Resiliência (eventos podem ser reprocessados)
- ❌ Complexidade adicional (message broker)

#### ADR-003: Cache Strategy

**Decisão:** Cache em Redis para consultas de consolidado

**Contexto:** Consultas de consolidado são frequentes e podem ser custosas

**Consequências:**
- ✅ Performance melhorada (50 req/s)
- ✅ Redução de carga no banco de dados
- ❌ Complexidade de invalidação de cache
- ❌ Consistência eventual

#### ADR-004: Autenticação Centralizada

**Decisão:** SSO Admin API centraliza autenticação, outras APIs validam tokens

**Contexto:** Múltiplas APIs precisam de autenticação

**Consequências:**
- ✅ Gerenciamento centralizado de usuários
- ✅ Tokens JWT stateless
- ✅ Escalabilidade (sem sessão no servidor)
- ❌ Necessidade de compartilhar secret key

#### ADR-005: Clean Architecture

**Decisão:** Aplicar Clean Architecture em todos os serviços

**Contexto:** Manutenibilidade e testabilidade são prioridades

**Consequências:**
- ✅ Código testável
- ✅ Baixo acoplamento
- ✅ Alta coesão
- ❌ Mais camadas (complexidade inicial)

---

## Escalabilidade

### Horizontal Scaling
- APIs podem ser escaladas independentemente
- Stateless design (JWT tokens)
- Cache compartilhado (Redis)

### Performance
- Cache em Redis para consultas frequentes
- Processamento assíncrono de eventos
- Connection pooling no Entity Framework

### Monitoramento
- Logs estruturados
- Health checks (a implementar)
- Métricas de performance (a implementar)

---

## Como Executar

### Opção 1: Tudo no Docker (Recomendado para Produção/Teste)

**Usando PowerShell diretamente:**
```powershell
docker-compose up -d
```

Isso irá iniciar:
- **SQL Server** na porta 1433
- **RabbitMQ** nas portas 5672 (AMQP) e 15672 (Management UI)
- **Redis** na porta 6379
- **SSO Admin API** nas portas 5002 (HTTP) e 7248 (HTTPS)
- **API de Lançamentos** nas portas 5000 (HTTP) e 5003 (HTTPS)
- **API de Consolidado Diário** nas portas 5001 (HTTP) e 5004 (HTTPS)

**Acesse as APIs:**

**HTTP:**
- **SSO Admin API**: http://localhost:5002/swagger
- **Lancamentos API**: http://localhost:5000/swagger
- **Consolidado Diário API**: http://localhost:5001/swagger

**HTTPS (com certificado auto-assinado):**
- **SSO Admin API**: https://localhost:7248/swagger
- **Lancamentos API**: https://localhost:5003/swagger
- **Consolidado Diário API**: https://localhost:5004/swagger

**Nota:** Ao acessar via HTTPS, o navegador mostrará um aviso de certificado auto-assinado. Isso é esperado em desenvolvimento. Clique em "Avançado" e depois em "Continuar para localhost".

**RabbitMQ Management**: http://localhost:15672 (guest/guest)

### Opção 2: Desenvolvimento com Debug no Visual Studio (Recomendado)

Para desenvolvimento com debug completo, breakpoints e hot reload:

1. **Iniciar apenas infraestrutura:**

**Usando PowerShell:**
```powershell
docker-compose up -d sqlserver rabbitmq redis
```

2. **Parar containers das APIs (se estiverem rodando):**
```powershell
docker-compose stop sso.admin.api lancamentos.api consolidadodiario.api
```

3. **No Visual Studio:**
   - Abra a solução `DesafioBanco.slnx`
   - Clique com botão direito na **Solution** → **Properties**
   - Em **Startup Project**, selecione **Multiple startup projects**
   - Configure:
     - `SSO.Admin.API` → **Start** (opcional, apenas se precisar testar autenticação)
     - `Lancamentos.API` → **Start**
     - `ConsolidadoDiario.API` → **Start**
   - Pressione **F5** ou clique em **Start**

As APIs rodarão localmente, permitindo debug completo, enquanto os serviços de infraestrutura (SQL Server, RabbitMQ, Redis) rodam no Docker.

**⚠️ Se der erro de porta em uso:** Execute `stop-apis-only.bat` para parar apenas os containers das APIs.

---

## Configuração Docker

### Visão Geral

O projeto utiliza Docker Compose para orquestrar todos os serviços necessários, incluindo:
- SQL Server (banco de dados)
- RabbitMQ (mensageria)
- Redis (cache)
- SSO Admin API (autenticação e gerenciamento de usuários)
- Lancamentos API
- Consolidado Diário API

### Portas Utilizadas

**APIs HTTP:**
- **5002** - SSO Admin API
- **5000** - API Lancamentos
- **5001** - API Consolidado Diário

**APIs HTTPS:**
- **7248** - SSO Admin API
- **5003** - API Lancamentos
- **5004** - API Consolidado Diário

**Infraestrutura:**
- **1433** - SQL Server
- **5672** - RabbitMQ AMQP
- **15672** - RabbitMQ Management UI
- **6379** - Redis

Se alguma porta estiver em uso, você pode alterá-la no arquivo `docker-compose.yml`.

### Configuração HTTPS

O projeto está configurado para usar HTTPS no Docker com certificados auto-assinados. Os certificados são gerados automaticamente durante o build das imagens Docker usando OpenSSL.

**Características dos certificados:**
- Gerados automaticamente no Dockerfile
- Formato PFX (PKCS#12)
- Válidos por 365 dias
- Auto-assinados (apenas para desenvolvimento)

**Nota:** Ao acessar via HTTPS, o navegador mostrará um aviso de certificado não confiável. Isso é esperado em desenvolvimento. Para continuar, clique em "Avançado" e depois em "Continuar para localhost".

**Para produção:** Substitua os certificados auto-assinados por certificados válidos emitidos por uma autoridade certificadora (CA).

### Comandos Úteis

#### Verificar Status dos Containers

   ```powershell
docker-compose ps
   ```

#### Ver Logs

   ```powershell
# Todos os serviços
docker-compose logs -f

# Serviço específico
docker-compose logs -f sso.admin.api
docker-compose logs -f lancamentos.api
docker-compose logs -f consolidadodiario.api
docker-compose logs -f sqlserver
docker-compose logs -f rabbitmq
docker-compose logs -f redis
```

#### Parar Serviços

```powershell
# Parar todos os serviços
docker-compose down

# Parar apenas as APIs (manter infraestrutura)
docker-compose stop sso.admin.api lancamentos.api consolidadodiario.api
```

#### Rebuild das Imagens

```powershell
# Rebuild completo (sem cache)
docker-compose build --no-cache

# Rebuild e iniciar
docker-compose up -d --build
```

#### Limpar Tudo

   ```powershell
# Parar e remover containers, redes e volumes
docker-compose down -v

# Remover imagens também
docker-compose down -v --rmi all
```

### Variáveis de Ambiente

As variáveis de ambiente são configuradas no `docker-compose.yml`:

- `ASPNETCORE_ENVIRONMENT=Development`
- `ASPNETCORE_URLS=http://+:8080;https://+:8081`
- Connection strings para SQL Server, RabbitMQ e Redis
- Configurações JWT para autenticação

### Volumes Persistentes

Os seguintes volumes são criados para persistir dados:

- `sqlserver_data` - Dados do SQL Server
- `rabbitmq_data` - Dados do RabbitMQ
- `redis_data` - Dados do Redis

Para limpar todos os dados: `docker-compose down -v`

---

## Autenticação e Autorização

### Visão Geral

Foi implementado um sistema completo de autenticação e autorização usando JWT (JSON Web Tokens) Bearer Token. A implementação segue as melhores práticas de segurança e inclui:

- Autenticação JWT
- Criptografia de senhas com BCrypt
- Rate Limiting
- CORS configurável
- CRUD completo de usuários
- Endpoint de login

### Estrutura Implementada

#### Domain Layer

- **`UsuarioToken`**: Entidade de domínio para usuários
- **`IUsuarioTokenRepository`**: Interface do repositório
- **`IAuthService`**: Interface do serviço de autenticação

#### Application Layer

**DTOs:**
- `LoginDTO`: Dados de entrada para login
- `TokenDTO`: Resposta com token JWT
- `UsuarioTokenDTO`: DTO para retorno de dados do usuário
- `CriarUsuarioTokenDTO`: DTO para criação de usuário
- `AtualizarUsuarioTokenDTO`: DTO para atualização de usuário
- `AtualizarSenhaDTO`: DTO para atualização de senha

**Commands:**
- `CriarUsuarioTokenCommand`
- `AtualizarUsuarioTokenCommand`
- `AtualizarSenhaCommand`

**Queries:**
- `ObterUsuarioTokenQuery`

**Handlers:**
- `LoginHandler`: Processa autenticação e gera token JWT
- `CriarUsuarioTokenHandler`: Cria novos usuários
- `AtualizarUsuarioTokenHandler`: Atualiza dados do usuário
- `AtualizarSenhaHandler`: Atualiza senha do usuário
- `ObterUsuarioTokenHandler`: Retorna dados de usuário(s)

#### Infrastructure Layer

- **`UsuarioTokenRepository`**: Implementação do repositório de usuários
- **`AuthService`**: Implementação do serviço de autenticação e geração de tokens JWT
- **`SSOAdminDbContext`**: Contexto do Entity Framework Core

#### API Layer

- **`AuthController`**: Endpoint de autenticação
  - `POST /api/auth/login`: Realiza login e retorna token JWT

- **`UsuariosController`**: CRUD de usuários (protegido com `[Authorize]`)
  - `POST /api/usuarios`: Cria um novo usuário
  - `GET /api/usuarios/{id}`: Obtém usuário por ID
  - `PUT /api/usuarios/{id}`: Atualiza dados do usuário
  - `PUT /api/usuarios/{id}/senha`: Atualiza senha do usuário

### Configuração

#### appsettings.json

```json
{
  "Jwt": {
    "SecretKey": "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLongForProductionUse!",
    "Issuer": "SSOAPI",
    "Audience": "SSOAPI",
    "ExpirationMinutes": "60"
  },
  "Cors": {
    "AllowedOrigins": [ "*" ]
  }
}
```

#### Program.cs

O `Program.cs` foi atualizado para incluir:

1. **JWT Authentication**: Configurado com validação de token
2. **Swagger com JWT**: Suporte a Bearer Token no Swagger UI
3. **CORS**: Configuração flexível de origens permitidas
4. **Authorization Middleware**: Proteção de endpoints

### Segurança

#### Senhas

- Senhas são criptografadas usando **BCrypt** antes de serem armazenadas no banco de dados
- Validação de senha durante login usando BCrypt.Verify

#### JWT

- Tokens JWT são assinados usando HMAC-SHA256
- Tokens incluem claims: NameIdentifier, Name, Email, Nome
- Expiração configurável (padrão: 60 minutos)
- Validação completa: Issuer, Audience, Lifetime, Signature

#### CORS

- Configurável via `appsettings.json`
- Por padrão permite todas as origens (configurar adequadamente para produção)

### Como Usar

#### 1. Como Criar o Primeiro Usuário (Gerar Hash de Senha)

Este documento explica como gerar o hash BCrypt para uma senha que será inserida diretamente no banco de dados.

##### Passo 1: Compilar e Executar

```powershell
cd tools
dotnet run --project GerarHashSenha.csproj
```

Ou para uma senha específica:

```powershell
dotnet run --project GerarHashSenha.csproj minhaSenha123
```

##### Passo 2: Copiar o Hash Gerado

O script irá exibir o hash BCrypt gerado. Copie o hash e use-o no SQL abaixo.

##### Passo 3: Inserir no Banco de Dados

**Para atualizar a senha de um usuário existente:**

```sql
UPDATE UsuariosToken 
SET SenhaHash = '$2a$11$[hash_gerado_pelo_script]' 
WHERE Login = 'seu_login';
```

**Para inserir um novo usuário:**

```sql
INSERT INTO UsuariosToken (Id, Login, SenhaHash, Nome, Email, Ativo, DataCriacao)
VALUES (
    NEWID(), 
    'admin', 
    '$2a$11$[hash_gerado_pelo_script]', 
    'Administrador', 
    'admin@exemplo.com', 
    1, 
    GETUTCDATE()
);
```

##### Importante

- O hash gerado é único a cada execução (BCrypt inclui um salt aleatório)
- Sempre use o hash completo, incluindo o prefixo `$2a$11$`
- Nunca armazene senhas em texto plano no banco de dados
- Use este método apenas para configuração inicial ou reset de senha

#### 2. Criar um Usuário (Requer autenticação)

```http
POST /api/usuarios
Authorization: Bearer {token}
Content-Type: application/json

{
  "login": "usuario1",
  "senha": "senha123",
  "nome": "Usuário Teste",
  "email": "usuario@teste.com"
}
```

#### 2. Login

```http
POST /api/auth/login
Content-Type: application/json

{
  "login": "usuario1",
  "senha": "senha123"
}
```

**Resposta:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-01-01T12:00:00Z"
}
```

#### 3. Usar o Token

Inclua o token no header `Authorization` de todas as requisições protegidas:

```http
GET /api/lancamentos
Authorization: Bearer {token}
```

### Endpoints Públicos vs Protegidos

#### Públicos (sem autenticação)
- `POST /api/auth/login`

#### Protegidos (requerem `Authorization: Bearer {token}`)
- Todos os endpoints em `/api/lancamentos/*`
- Todos os endpoints em `/api/usuarios/*`
- Todos os endpoints em `/api/consolidado/*`

### Considerações de Produção

1. **Secret Key**: Altere a `SecretKey` do JWT para uma chave forte e segura (mínimo 32 caracteres)
2. **CORS**: Configure origens específicas em vez de `*`
3. **HTTPS**: Sempre use HTTPS em produção
4. **Refresh Tokens**: Considere implementar refresh tokens para melhor UX
5. **Roles/Permissions**: Considere adicionar roles e permissões se necessário
6. **Audit Log**: Considere adicionar logs de auditoria para ações críticas

---

## Endpoints Principais

### SSO Admin API (Porta 5002 HTTP / 7248 HTTPS)

- `POST /api/usuarios` - Criar novo usuário
- `POST /api/auth/login` - Fazer login e obter token JWT
- `GET /api/usuarios/{id}` - Obter usuário por ID
- `PUT /api/usuarios/{id}` - Atualizar usuário
- `PUT /api/usuarios/{id}/senha` - Atualizar senha do usuário

### Lancamentos API (Porta 5000 HTTP / 5003 HTTPS)

- `POST /api/lancamentos` - Criar um novo lançamento (requer autenticação JWT)
- `GET /api/lancamentos/{id}` - Obter lançamento por ID (requer autenticação JWT)

### Consolidado Diário API (Porta 5001 HTTP / 5004 HTTPS)

- `GET /api/consolidado/{data}` - Obter consolidado por data (requer autenticação JWT)

---

## Desenvolvimento

### Aplicar Migrations

```bash
dotnet ef database update --project ../Lancamentos.Infrastructure
```

### Configurações

#### Connection Strings

As connection strings estão configuradas no `docker-compose.yml` através de variáveis de ambiente:

- **SQL Server**: `Server=sqlserver,1433;Database={DatabaseName};User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;`
- **RabbitMQ**: `amqp://guest:guest@rabbitmq:5672`
- **Redis**: `redis:6379`

#### Variáveis de Ambiente

No ambiente Docker, as configurações são sobrescritas pelas variáveis de ambiente definidas no `docker-compose.yml`.

---

## Testes

```bash
# Executar todos os testes
dotnet test

# Executar testes de um projeto específico
dotnet test tests/Lancamentos.API.Tests
dotnet test tests/ConsolidadoDiario.API.Tests
dotnet test tests/SSO.Admin.API.Tests
```

---

## Troubleshooting

### Erro de conexão com SQL Server

- Verifique se o container do SQL Server está rodando: `docker-compose ps`
- Verifique os logs: `docker-compose logs sqlserver`
- Aguarde alguns segundos após iniciar o container para o SQL Server estar pronto (pode levar até 60 segundos)

### Erro de conexão com RabbitMQ

- Verifique se o container está saudável: `docker-compose ps`
- Acesse o management UI: http://localhost:15672
- Aguarde o RabbitMQ inicializar completamente (pode levar 30-60 segundos)

### Portas em uso

Se as portas estiverem em uso, você pode alterar no `docker-compose.yml`:
- 5002 → SSO Admin API (HTTP)
- 7248 → SSO Admin API (HTTPS)
- 5000 → API Lancamentos (HTTP)
- 5003 → API Lancamentos (HTTPS)
- 5001 → API Consolidado Diário (HTTP)
- 5004 → API Consolidado Diário (HTTPS)
- 1433 → SQL Server
- 5672 → RabbitMQ AMQP
- 15672 → RabbitMQ Management
- 6379 → Redis

### Certificados HTTPS

O projeto está configurado para usar HTTPS no Docker com certificados auto-assinados gerados automaticamente durante o build das imagens. Os certificados são válidos por 365 dias e são gerados usando OpenSSL nos Dockerfiles.

**Nota:** Certificados auto-assinados são adequados apenas para desenvolvimento. Para produção, use certificados válidos emitidos por uma autoridade certificadora.

**Se você receber erro sobre certificado HTTPS não encontrado:**

1. Reconstrua as imagens: `docker-compose up -d --build`
2. Verifique os logs: `docker-compose logs lancamentos.api`
3. Verifique se o certificado foi gerado: `docker exec lancamentos_api ls -la /app/https-dev.pfx`

### Docker Desktop não está rodando

Se você recebeu o erro:
```
unable to get image 'desafiobanco-lancamentos.api': error during connect: Get "http://%2F%2F.%2Fpipe%2FdockerDesktopLinuxEngine/v1.51/images/desafiobanco-lancamentos.api/json": open //./pipe/dockerDesktopLinuxEngine: The system cannot find the file specified.
```

**Solução:**
1. Inicie o Docker Desktop
2. Aguarde até que o ícone do Docker na bandeja do sistema fique verde
3. Verifique se está rodando: `docker ps`
4. Execute novamente: `docker-compose up -d`

### Erro ao acessar Swagger via HTTPS

**Solução:**
1. Certifique-se de usar `https://` e não `http://`
2. Aceite o aviso de certificado auto-assinado no navegador
3. Verifique se a porta HTTPS está correta (7248, 5003, 5004)
4. Verifique os logs: `docker-compose logs sso.admin.api`

### APIs não conseguem conectar ao SQL Server

**Solução:**
1. Verifique se o SQL Server está healthy: `docker-compose ps`
2. Aguarde o SQL Server estar completamente pronto (pode levar até 60 segundos)
3. Verifique a connection string no `docker-compose.yml`
4. Verifique os logs da API: `docker-compose logs lancamentos.api`

---

## Próximos Passos

### Melhorias Futuras
1. **API Gateway** - Centralizar roteamento e autenticação
2. **Service Discovery** - Descoberta automática de serviços
3. **Circuit Breaker** - Resiliência em chamadas entre serviços
4. **Distributed Tracing** - Rastreamento de requisições
5. **Health Checks** - Monitoramento de saúde dos serviços
6. **Métricas** - Prometheus/Grafana para métricas
7. **Logs Centralizados** - ElasticSearch
8. **Front** - Página para o acesso as funcionalidades

### Testes
- Testes unitários para handlers e repositories
- Testes de integração para APIs
- Testes de performance para consolidado (50 req/s)
- Testes de carga

---

