# Order Management System

Order Management System is a .NET 10 clean-architecture solution for managing customers, products, orders, authentication, and AI-facing order operations through both a REST API and a Model Context Protocol (MCP) server.

The repository contains the domain model, application use cases, SQL Server persistence, Web API endpoints, MCP tools/resources/prompts, Bruno API requests, EF Core migrations, database utility scripts, and unit/integration tests.

## Overview

The system centers on order placement and lookup. It models customers, products, vouchers, users, orders, order items, shipping addresses, money, order status transitions, domain events, and an outbox table. The Web API exposes HTTP endpoints for authentication and order operations. The MCP server reuses the same application and infrastructure layers to expose order tools, resources, and prompts to MCP clients.

Startup behavior is repository-specific:

- `OrderManagement.WebAPI` starts a REST API, maps OpenAPI and Scalar docs, initializes the database, applies migrations in Development, and seeds data when the database is empty.
- `OrderManagement.McpServer` starts an MCP server with HTTP transport at `/mcp` and stdio transport for desktop/client integration.
- `OrderManagement.Infrastructure` wires SQL Server, ASP.NET Core Identity, repositories, SendGrid, Stripe, Redis/in-memory cache, external-service resilience policies, and EF Core migrations.

## Features

- Clean Architecture project layout: Domain, Application, Infrastructure, WebAPI, and MCP Server.
- CQRS-style application flow with MediatR commands, queries, validators, and pipeline behaviors.
- Order placement and order lookup through REST and MCP tools.
- Paginated order retrieval through MCP.
- ASP.NET Core Identity with seeded local admin account.
- JWT bearer authentication and authorization policy scaffolding.
- EF Core SQL Server persistence with migrations and design-time DbContext factory.
- Domain entities, value objects, specifications, repositories, domain events, and outbox messages.
- Database seeding for roles, admin user, customers, products, and draft orders.
- Serilog logging to console, rolling files, and Seq.
- Redis cache support with development-time in-memory cache option.
- External integrations for SendGrid email, Stripe payments, Azure Blob file storage abstraction, and inventory service abstraction.
- Bruno collection for local API requests.
- xUnit unit and integration tests with FluentAssertions, NSubstitute, Respawn, SQLite, and SQL Server Testcontainers.

## Architecture Overview

```text
HTTP clients / Bruno / Scalar
        |
        v
OrderManagement.WebAPI
  - Controllers
  - Middleware
  - Authentication and authorization
  - API versioning
        |
        v
OrderManagement.Application
  - MediatR commands and queries
  - DTOs and mapping extensions
  - Validators
  - Pipeline behaviors
        |
        v
OrderManagement.Domain
  - Entities and value objects
  - Domain services
  - Domain events
  - Specifications
        |
        v
OrderManagement.Infrastructure
  - EF Core DbContext and migrations
  - Repositories and Unit of Work
  - Identity, JWT, cache, email, payment, file storage

MCP clients
        |
        v
OrderManagement.McpServer
  - MCP tools, resources, prompts, and middleware
  - Reuses Application and Infrastructure services
```

Important application pipelines:

| Area | Implementation |
| --- | --- |
| Request/use-case dispatch | MediatR handlers in `src/OrderManagement.Application` |
| Validation | FluentValidation plus `ValidationBehavior` |
| Logging/performance | `LoggingBehavior` and `PerformanceBehavior` |
| Transactions | `TransactionBehavior` for commands implementing `ITransactionalCommand` |
| Persistence | EF Core `ApplicationDbContext` and repositories |
| API error responses | Global exception middleware and result-to-ProblemDetails extensions |
| MCP error responses | `McpErrorHandlingMiddleware` |

## Tech Stack

| Category | Technology |
| --- | --- |
| Runtime | .NET 10 (`net10.0`) |
| Web API | ASP.NET Core, Controllers, OpenAPI, Scalar |
| API versioning | `Asp.Versioning.Mvc`, `Asp.Versioning.Mvc.ApiExplorer` |
| Application layer | MediatR, FluentValidation, Dapper |
| Persistence | Entity Framework Core 10, SQL Server, Microsoft.Data.SqlClient |
| Identity/Auth | ASP.NET Core Identity, JWT Bearer |
| Logging | Serilog, console/file/Seq sinks |
| Caching | In-memory cache or Redis via StackExchange.Redis |
| External integrations | SendGrid, Stripe, Azure Blob storage abstraction, Polly HTTP resilience |
| MCP | ModelContextProtocol 1.3.0, HTTP transport, stdio transport |
| Testing | xUnit, FluentAssertions, NSubstitute, ASP.NET Core MVC Testing, Respawn, SQLite, Testcontainers.MsSql, coverlet |
| API client collection | Bruno |

## Project Structure

```text
.
|-- OrderManagement.slnx
|-- OrderManagementDb.bak
|-- README.md
|-- read-me.html
|-- bruno/
|   `-- OrderManagement.WebAPI/
|       |-- Auth/
|       |-- Docs/
|       |-- Orders/V1/
|       `-- environments/Local.bru
|-- docs/
|   |-- AddDefaultTokenProviders-Guide.md
|   `-- DTO-Mapping-Pattern.md
|-- src/
|   |-- OrderManagement.Domain/
|   |   |-- Common/
|   |   |-- Entities/
|   |   |-- Events/
|   |   |-- Repositories/
|   |   |-- Services/
|   |   |-- Specifications/
|   |   `-- ValueObjects/
|   |-- OrderManagement.Application/
|   |   |-- Auth/
|   |   |-- Common/
|   |   |-- Contracts/
|   |   `-- Orders/
|   |-- OrderManagement.Infrastructure/
|   |   |-- Auth/
|   |   |-- Caching/
|   |   |-- Email/
|   |   |-- FileStorage/
|   |   |-- Identity/
|   |   |-- Payment/
|   |   |-- Persistence/
|   |   |   |-- Configurations/
|   |   |   |-- Migrations/
|   |   |   |-- Outbox/
|   |   |   |-- Repositories/
|   |   |   |-- Scripts/
|   |   |   `-- Seeding/
|   |   |-- Resilience/
|   |   `-- Services/
|   |-- OrderManagement.WebAPI/
|   |   |-- Authorization/
|   |   |-- Controllers/
|   |   |-- Extensions/
|   |   |-- Middleware/
|   |   |-- Properties/
|   |   `-- Services/
|   `-- OrderManagement.McpServer/
|       |-- Middlewares/
|       |-- Model/
|       |-- OrderTools/
|       |-- Prompts/
|       |-- Resources/
|       `-- Services/
`-- tests/
    `-- OrderManagement.Tests/
        |-- Integration/
        `-- Unit/
```

Generated folders such as `bin/`, `obj/`, and `artifacts/` are present in the working tree but are not part of the source layout above.

## Prerequisites

- .NET 10 SDK.
- SQL Server reachable by the configured connection string.
- EF Core CLI tools for migration commands:

```powershell
dotnet tool install --global dotnet-ef
```

- Optional: Redis on `localhost:6379` when `Cache:UseInMemory` is `false`.
- Optional: Seq on `http://localhost:5341` for Serilog Seq sink.
- Optional: Bruno for running the checked-in API collection.
- Optional: Docker for tests that use `Testcontainers.MsSql`; Docker application deployment files are not detected from repository analysis.

## Installation

Restore and build the solution:

```powershell
dotnet restore .\OrderManagement.slnx
dotnet build .\OrderManagement.slnx
```

Run all tests:

```powershell
dotnet test .\OrderManagement.slnx
```

## Configuration

Primary configuration files:

| File | Purpose |
| --- | --- |
| `src/OrderManagement.WebAPI/appsettings.json` | Shared API settings for Serilog, SendGrid, Stripe, cache, JWT, and hosts |
| `src/OrderManagement.WebAPI/appsettings.Development.json` | Development SQL Server connection string and in-memory cache override |
| `src/OrderManagement.WebAPI/Properties/launchSettings.json` | Local API ports and Development environment |
| `src/OrderManagement.McpServer/appsettings.json` | MCP server settings copied to output |
| `src/OrderManagement.McpServer/appsettings.Development.json` | MCP development SQL Server connection string and cache override |
| `.vscode/mcp.json` | VS Code MCP client entry pointing to `http://localhost:5000/mcp` |
| `bruno/OrderManagement.WebAPI/environments/Local.bru` | Bruno local API variables |

Development API URLs from `launchSettings.json`:

| Profile | URL |
| --- | --- |
| HTTP | `http://localhost:5086` |
| HTTPS | `https://localhost:7086;http://localhost:5086` |
| Scalar docs | `http://localhost:5086/scalar/v1` |
| OpenAPI JSON | `http://localhost:5086/openapi/v1.json` |

MCP URL configured for VS Code:

```text
http://localhost:5000/mcp
```

## Environment Variables

The applications use standard .NET configuration binding and call `AddEnvironmentVariables()` in the MCP server. Environment variables can override JSON configuration using double underscores.

| Configuration key | Environment variable | Default detected value |
| --- | --- | --- |
| `ASPNETCORE_ENVIRONMENT` | `ASPNETCORE_ENVIRONMENT` | `Development` in Web API launch profiles |
| `ASPNETCORE_URLS` | `ASPNETCORE_URLS` | Not set in appsettings; fallback log text references `http://localhost:5000` |
| `ConnectionStrings:DefaultConnection` | `ConnectionStrings__DefaultConnection` | `Server=.;Database=OrderManagementDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true` |
| `Cache:ConnectionString` | `Cache__ConnectionString` | `localhost:6379` |
| `Cache:DefaultExpiryMinutes` | `Cache__DefaultExpiryMinutes` | `30` |
| `Cache:UseInMemory` | `Cache__UseInMemory` | `true` in Development, `false` in base appsettings |
| `JwtSettings:SecretKey` | `JwtSettings__SecretKey` | `your-very-long-secret-key-min-32-chars` |
| `JwtSettings:Issuer` | `JwtSettings__Issuer` | `OrderManagement.Api` |
| `JwtSettings:Audience` | `JwtSettings__Audience` | `OrderManagement.Client` |
| `JwtSettings:ExpiryMinutes` | `JwtSettings__ExpiryMinutes` | `60` |
| `SendGrid:ApiKey` | `SendGrid__ApiKey` | `REPLACE_WITH_USER_SECRETS` |
| `SendGrid:SenderEmail` | `SendGrid__SenderEmail` | `orders@tedu.com.vn` |
| `SendGrid:SenderName` | `SendGrid__SenderName` | `TEDU Order System` |
| `Stripe:SecretKey` | `Stripe__SecretKey` | `REPLACE_WITH_USER_SECRETS` |
| `Stripe:WebhookSecret` | `Stripe__WebhookSecret` | `REPLACE_WITH_USER_SECRETS` |
| `Serilog:WriteTo:2:Args:serverUrl` | `Serilog__WriteTo__2__Args__serverUrl` | `http://localhost:5341` |

For local secrets, prefer user secrets or environment variables instead of editing committed JSON:

```powershell
dotnet user-secrets set "SendGrid:ApiKey" "SG.xxxxx" --project .\src\OrderManagement.WebAPI
dotnet user-secrets set "Stripe:SecretKey" "sk_test_xxxxx" --project .\src\OrderManagement.WebAPI
dotnet user-secrets set "JwtSettings:SecretKey" "replace-with-a-long-local-secret" --project .\src\OrderManagement.WebAPI
```

## Database Setup

The repository uses SQL Server through EF Core. The development connection string targets a local SQL Server instance:

```text
Server=.;Database=OrderManagementDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```

Detected migration files:

- `20260420055454_InitialCreate`
- `20260420222804_AddDefaultValueToCustomerRowVersion`
- `20260421055602_AddIdentityTables`

Detected database scripts:

- `src/OrderManagement.Infrastructure/Persistence/Scripts/CleanData.sql`
- `src/OrderManagement.Infrastructure/Persistence/Scripts/migration_20042026.sql`
- `src/OrderManagement.Infrastructure/Persistence/Scripts/VerifySeeding.sql`

The Web API calls `InitialiseDatabaseAsync()` at startup. In Development it applies pending migrations, then runs `DatabaseSeeder`. The seeder skips when seed data already exists; otherwise it creates:

- Roles: `Admin`, `Manager`, `Staff`
- Admin user: `admin@orderms.local` / `Admin@123456`
- Sample customers
- Sample products
- Sample draft orders

The repository also contains `OrderManagementDb.bak`. Restore instructions for this backup are not detected from repository analysis.

Common EF Core commands:

```powershell
dotnet ef migrations add <MigrationName> `
  --project .\src\OrderManagement.Infrastructure `
  --startup-project .\src\OrderManagement.WebAPI `
  --output-dir Persistence\Migrations
```

```powershell
dotnet ef migrations list `
  --project .\src\OrderManagement.Infrastructure `
  --startup-project .\src\OrderManagement.WebAPI
```

```powershell
dotnet ef database update `
  --project .\src\OrderManagement.Infrastructure `
  --startup-project .\src\OrderManagement.WebAPI
```

```powershell
dotnet ef migrations script --idempotent `
  --output .\src\OrderManagement.Infrastructure\Persistence\Scripts\migration_$(Get-Date -Format 'yyyyMMdd').sql `
  --project .\src\OrderManagement.Infrastructure `
  --startup-project .\src\OrderManagement.WebAPI
```

## Running Locally

Run the Web API:

```powershell
dotnet run --project .\src\OrderManagement.WebAPI
```

Open:

- Scalar API reference: `http://localhost:5086/scalar/v1`
- OpenAPI JSON: `http://localhost:5086/openapi/v1.json`
- API base URL for Bruno: `http://localhost:5086`

Run the MCP server:

```powershell
dotnet run --project .\src\OrderManagement.McpServer
```

The MCP server maps:

- HTTP MCP endpoint: `/mcp`
- Stdio server transport for MCP clients that launch the executable directly
- Order resources: `oms://orders/{orderId}`, `oms://orders/list`
- System resource: `oms://system/info`
- Tools: `get_order`, `get_orders`, `place_order`
- Prompts: `summarize_draft_orders`, `draft_order_confirmation`

For a clean stdio MCP launch, build first and point clients at the generated executable instead of `dotnet run` if the client requires stdout to contain only JSON-RPC messages.

```powershell
dotnet build .\src\OrderManagement.McpServer\OrderManagement.McpServer.csproj
```

## Running with Docker

Docker application files are not detected from repository analysis. No `Dockerfile`, `docker-compose.yml`, or CI container build definition was found.

Tests reference `Testcontainers.MsSql`, so Docker may be required when running integration tests that start SQL Server containers.

## API Documentation

Detected REST endpoints:

| Method | Route | Purpose | Auth in code |
| --- | --- | --- | --- |
| `POST` | `/api/Auth/register` | Register a user | None detected |
| `POST` | `/api/Auth/login` | Login and receive token data | None detected |
| `POST` | `/api/v1/orders` | Place an order | `[Authorize]` is currently commented out |
| `GET` | `/api/v1/orders/{id}` | Get an order by GUID | `[Authorize]` and `MustOwnOrder` are currently commented out |

Bruno requests are available under `bruno/OrderManagement.WebAPI`:

- `Auth/Login.bru`
- `Auth/Register.bru`
- `Docs/OpenAPI V1.bru`
- `Docs/Scalar V1.bru`
- `Orders/V1/Place Order.bru`
- `Orders/V1/Get Order By Id.bru`

Local Bruno variables:

| Variable | Value detected |
| --- | --- |
| `baseUrl` | `http://localhost:5086` |
| `accessToken` | Empty until login script sets it |
| `customerId` | `00000000-0000-0000-0000-000000000000` |
| `productId` | `00000000-0000-0000-0000-000000000000` |
| `orderId` | `89AE1BF5-99C9-4571-912F-4E48D745512B` |

Sample login body from the Bruno collection:

```json
{
  "email": "admin@orderms.local",
  "password": "Admin@123456"
}
```

## Testing

Run the full test project:

```powershell
dotnet test .\tests\OrderManagement.Tests\OrderManagement.Tests.csproj
```

Run the solution tests:

```powershell
dotnet test .\OrderManagement.slnx
```

Test coverage in the repository includes:

- Unit tests for domain entities, value objects, pricing service, validators, mapping, application behaviors, order command handler, email service, repository behavior, and exception middleware.
- Integration tests for error handling and order placement.
- ASP.NET Core MVC test host support.
- SQLite and SQL Server Testcontainers packages for persistence-oriented tests.

## CI/CD

Not detected from repository analysis. No `.github/workflows`, Azure Pipelines, GitLab CI, TeamCity, Jenkins, or other CI/CD configuration files were found in the repository scan.

Recommended baseline pipeline for this repo:

```powershell
dotnet restore .\OrderManagement.slnx
dotnet build .\OrderManagement.slnx --configuration Release --no-restore
dotnet test .\OrderManagement.slnx --configuration Release --no-build
```

## Deployment

Application deployment scripts are not detected from repository analysis.

Production database guidance in the existing README favored generating and reviewing an idempotent SQL migration script before applying it manually:

```powershell
dotnet ef migrations script --idempotent `
  --output .\src\OrderManagement.Infrastructure\Persistence\Scripts\migration_$(Get-Date -Format 'yyyyMMdd').sql `
  --project .\src\OrderManagement.Infrastructure `
  --startup-project .\src\OrderManagement.WebAPI
```

Production configuration should provide real values for:

- `ConnectionStrings__DefaultConnection`
- `JwtSettings__SecretKey`
- `SendGrid__ApiKey`
- `Stripe__SecretKey`
- `Stripe__WebhookSecret`
- `Cache__ConnectionString` when Redis is used
- `ASPNETCORE_ENVIRONMENT`
- `ASPNETCORE_URLS` or hosting-platform equivalent

## Development Guidelines

- Keep domain rules in `OrderManagement.Domain`; do not move persistence or HTTP concerns into entities/value objects.
- Put use cases in `OrderManagement.Application` as MediatR commands/queries with validators when input validation is needed.
- Use mapping extensions in `Orders/Mappings` for DTO conversion patterns already present in the application layer.
- Keep infrastructure implementations behind application/domain interfaces.
- Add EF Core entity configuration under `OrderManagement.Infrastructure/Persistence/Configurations`.
- Add migrations from the Infrastructure project with WebAPI as the startup project.
- Prefer user secrets or environment variables for SendGrid, Stripe, JWT, and connection string overrides.
- Keep MCP tools/resources/prompts in `OrderManagement.McpServer` while reusing application handlers instead of duplicating business logic.
- Use the checked-in Bruno collection when validating API routes manually.
- Do not rely on Docker application deployment until Docker files are added.

## Roadmap

Not detected from repository analysis.

Potential next items suggested by current code comments and structure:

- Enable and enforce authorization on order endpoints.
- Implement or expose order cancellation/update routes.
- Add password reset/email confirmation token providers if required by product scope.
- Add CI/CD workflow configuration.
- Add Docker application packaging if containerized deployment is desired.
- Document restore/use of `OrderManagementDb.bak`.

## License

Not detected from repository analysis. No `LICENSE` file or license metadata was found.
