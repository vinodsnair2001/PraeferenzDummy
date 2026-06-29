# PraeferenzRoO — Preferential Rules of Origin Calculation System

Determines whether a manufactured product qualifies as an Originating Product under EU preferential trade agreement rules of origin.

## Prerequisites

- [.NET SDK 9.0.300+](https://dotnet.microsoft.com/download/dotnet/9)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for PostgreSQL)
- [Git](https://git-scm.com/)

## Quick Start

### 1. Clone and configure environment

```bash
git clone https://github.com/vinodsnair2001/PraeferenzDummy.git
cd PraeferenzDummy
cp .env.example .env
# Edit .env and set DB_PASSWORD to a secure value
```

### 2. Start the database

```bash
docker compose up -d db
# Wait for the container to become healthy
docker compose ps
```

### 3. Build the solution

```bash
dotnet build PraeferenzRoO.sln
```

### 4. Run the API

```bash
cd src/PraeferenzRoO.Api
dotnet run
```

### 5. Verify health endpoint

```bash
curl http://localhost:5000/health
# Expected: Healthy
```

## Running Tests

```bash
dotnet test Tests/
```

## Verification Gates

All of the following must pass before a feature branch is merged:

```bash
dotnet build PraeferenzRoO.sln   # 0 errors
dotnet test Tests/               # all tests green
docker compose ps                # db: healthy
curl http://localhost:5000/health # HTTP 200 Healthy
```

## Solution Structure

```
/src
  PraeferenzRoO.Shared         -- Cross-cutting utilities, constants, extensions
  PraeferenzRoO.Domain         -- Entities, value objects, domain events, interfaces
  PraeferenzRoO.Application    -- CQRS handlers, commands, queries, validators, DTOs
  PraeferenzRoO.Infrastructure -- Email, file storage, background jobs
  PraeferenzRoO.Persistence    -- EF Core DbContext, repositories, Dapper queries
  PraeferenzRoO.Api            -- ASP.NET Core Web API, controllers, middleware

/Tests
  PraeferenzRoO.Domain.Tests
  PraeferenzRoO.Application.Tests
  PraeferenzRoO.Api.Tests
```

## Architecture

Clean Architecture with CQRS. See [ESSENTIAL/architecture.md](ESSENTIAL/architecture.md) for the full specification.

Dependency rule: `Api → Application → Domain`. Infrastructure and Persistence depend on Application/Domain. Api references Infrastructure and Persistence only for DI composition in `Program.cs`.
