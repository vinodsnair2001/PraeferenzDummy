# Decision Summary — T01 Solution Scaffold

> **Issue:** [#1 — T01 Solution scaffold](https://github.com/vinodsnair2001/PraeferenzDummy/issues/1)
> **Branch:** `issue-1-solution-scaffold`
> **Meeting date:** 2026-06-27
> **Status:** Approved — ready for implementation

---

## Agreed Architecture

A 6-project Clean Architecture .NET 9 solution under `src/`, plus 3 skeleton test projects under `Tests/`.

### Dependency Matrix (authoritative)

| Project | References |
|---|---|
| `PraeferenzRoO.Shared` | _(none)_ |
| `PraeferenzRoO.Domain` | `PraeferenzRoO.Shared` |
| `PraeferenzRoO.Application` | `PraeferenzRoO.Domain`, `PraeferenzRoO.Shared` |
| `PraeferenzRoO.Infrastructure` | `PraeferenzRoO.Application`, `PraeferenzRoO.Domain`, `PraeferenzRoO.Shared` |
| `PraeferenzRoO.Persistence` | `PraeferenzRoO.Application`, `PraeferenzRoO.Domain`, `PraeferenzRoO.Shared` |
| `PraeferenzRoO.Api` | `PraeferenzRoO.Application`, `PraeferenzRoO.Infrastructure`, `PraeferenzRoO.Persistence`, `PraeferenzRoO.Shared` |

> **Rule:** Api references Infrastructure and Persistence **exclusively for DI registration** in `Program.cs` via extension methods. Controller and handler code must never import from Infrastructure or Persistence namespaces.

---

## Agreed Implementation Approach

### Files to Create

**Solution root:**
- `PraeferenzRoO.sln`
- `global.json` — SDK `9.0.*`, `rollForward: patch`
- `Directory.Build.props` — net9.0, LangVersion 13, Nullable enable, ImplicitUsings enable
- `Directory.Packages.props` — empty Central Package Management block
- `docker-compose.yml` — PostgreSQL 16-alpine with healthcheck, named volume `pg_data`
- `.env.example` — `DB_PASSWORD`, `POSTGRES_USER`, `POSTGRES_DB` keys (no values)
- `.gitignore` — dotnet template + `.env`, `.env.local`, `/logs/`, `TestResults/`, `coverage/`
- `README.md` — prerequisites, clone, env setup, build + run commands

**`src/` projects (classlib unless noted):**
- `PraeferenzRoO.Domain/`
- `PraeferenzRoO.Application/` + `Application/DependencyInjection.cs`
- `PraeferenzRoO.Infrastructure/` + `Infrastructure/DependencyInjection.cs`
- `PraeferenzRoO.Persistence/` + `Persistence/DependencyInjection.cs`
- `PraeferenzRoO.Shared/`
- `PraeferenzRoO.Api/` (webapi) + `Api/Program.cs` + `Api/appsettings.json` + `Api/appsettings.Development.json`

**`Tests/` projects (xunit, net9.0):**
- `PraeferenzRoO.Domain.Tests/` — one placeholder passing test
- `PraeferenzRoO.Application.Tests/` — one placeholder passing test
- `PraeferenzRoO.Api.Tests/` — one placeholder passing test

### Key Configuration Details

**`global.json`:**
```json
{
  "sdk": {
    "version": "9.0.0",
    "rollForward": "patch"
  }
}
```

**`Directory.Build.props`:**
```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <LangVersion>13</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
```

**`Program.cs` (minimal):**
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddHealthChecks();
var app = builder.Build();
app.MapHealthChecks("/health");
app.Run();
```

**`appsettings.json` sections:** ConnectionStrings (empty), Jwt (Issuer, Audience, AccessTokenExpiryMinutes, RefreshTokenExpiryDays), Serilog (MinimumLevel: Information), Cache, Localization, MultiTenancy.

**`appsettings.Development.json`:** Serilog MinimumLevel Debug + Console sink override.

**`docker-compose.yml`:** PostgreSQL 16-alpine only. No Redis (deferred to T04). Includes healthcheck (`pg_isready -U praeferenz`), named volume, `.env` file reference for `DB_PASSWORD`.

---

## Accepted Trade-offs

| Decision | Reason |
|---|---|
| `TreatWarningsAsErrors` NOT set | EF Core migration scaffolding produces warnings that would require constant suppression. Nullable enforcement is the primary strictness gate. |
| Api references Infrastructure + Persistence | Required for DI composition root in `Program.cs`. Documented to prevent misuse. |
| 3 test projects, not 5 | `Persistence.Tests` and `Infrastructure.Tests` deferred to T03/T04 when concrete code exists to test. |
| No Redis in docker-compose | Caching not needed until T04 cross-cutting infrastructure task. |

---

## Verification Gates

Before marking T01 complete, all of the following must pass:

```bash
dotnet build PraeferenzRoO.sln          # 0 errors
dotnet test Tests/                       # 3 passing, 0 failing
docker compose up -d postgres            # postgres container status: healthy
curl http://localhost:5000/health        # HTTP 200 "Healthy"
```

---

## Deferred Items

| Item | Target Issue |
|---|---|
| Redis service in docker-compose | T04 — cross-cutting infrastructure |
| `appsettings.Production.json` | T23 — DevOps |
| Multi-stage Dockerfile | T23 — DevOps |
| `Persistence.Tests` project | T03 — EF Core DbContext |
| `Infrastructure.Tests` project | T04 — cross-cutting infrastructure |
| ArchUnitNET architecture enforcement tests | T04 or standalone |
| pgAdmin optional docker service | `docker-compose.override.yml.example` |
| Hangfire schema | T11 — rule engine orchestrator |

---

## Risks

| Risk | Mitigation |
|---|---|
| SDK version drift across developer machines | `global.json` with `rollForward: patch` |
| Api importing Infrastructure directly in controllers | Documented rule + ArchUnitNET test at T04 |
| Package version conflicts across projects | `Directory.Packages.props` CPM created at T01 |
| `.env` with real secrets committed | `.gitignore` entry + `.env.example` pattern |

---

*Decision confirmed by: Praveen (Architect), Sreejith (Senior Dev), Sojiya (UX), Vinod (Planner)*
*Consumed by: praeferenz-orchestrator → backend-developer agent*
