# CookTime Project Guidelines

Recipe management system: C# ASP.NET API + React SPA + PostgreSQL.

## Build and Test

```bash
./scripts/server     # Start all services (Docker Compose)
./scripts/test       # Run tests (requires server running)
./scripts/build      # Full build: frontend → wwwroot → dotnet build
```

Tests require the server to be healthy first—`./scripts/test` waits up to 30s for `/health`.

## Architecture

| Layer | Location | Notes |
|-------|----------|-------|
| API Routes | [src/CookTime/Routes/](src/CookTime/Routes/) | Use `RouteGroupBuilder.MapXRoutes()` extension pattern |
| Services | [src/CookTime/Services/](src/CookTime/Services/) | Singletons registered in Program.cs |
| DTOs | [src/CookTime/Models/Contracts/](src/CookTime/Models/Contracts/) | All API contracts live here |
| Database | [src/CookTime/Scripts/](src/CookTime/Scripts/) | PostgreSQL functions + migrations |
| Frontend | [src/CookTime/client-app/](src/CookTime/client-app/) | React + React Router 7 + Vite + TypeScript |

**Database access**: Uses PostgreSQL stored functions with JSONB parameters via `CookTimeDB.cs`. See [CookTimeDB.cs](src/CookTime/Services/CookTimeDB.cs) for the pattern.

## Migrations

- Migration files: `src/CookTime/Scripts/NNN_description.sql`
- Tracked in `cooktime.schema_migrations` table
- Auto-applied on startup via [Migrations.cs](src/CookTime/Services/Migrations.cs)
- When altering tables/functions, create a new migration script

## Testing

- Framework: MSTest with async tests
- Inherit from [TestBase](src/CookTimeTests/TestBase.cs) for isolated test users and cleanup
- Pattern: `[TestInitialize]` calls `InitializeAsync()`, `[TestCleanup]` calls `CleanupAsync()`

## Project Conventions

- **Namespaces**: Use `CookTime.*` (e.g., `CookTime.Services`, `CookTime.Models.Contracts`)
- **Docker rebuild**: Required when NuGet packages change
- **Environment**: Development uses Docker Compose; user secrets override appsettings

## Language-Specific Instructions

See scoped instruction files in [.github/instructions/](.github/instructions/):
- [csharp.instructions.md](.github/instructions/csharp.instructions.md) - C# style (braces, records, no regions)
- [database.instructions.md](.github/instructions/database.instructions.md) - SQL migration rules
- [frontend.instructions.md](.github/instructions/frontend.instructions.md) - React patterns
