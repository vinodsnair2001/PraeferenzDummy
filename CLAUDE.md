# Claude Code Instructions — PraeferenzDummy

## MANDATORY: Read Before Every Task

Before writing any code, modifying any file, creating any issue implementation, or responding to any engineering request, you MUST read the following files in this order:

1. [ESSENTIAL/architecture.md](ESSENTIAL/architecture.md)
2. [ESSENTIAL/coding-standards.md](ESSENTIAL/coding-standards.md)
3. [ESSENTIAL/security.md](ESSENTIAL/security.md)
4. [ESSENTIAL/stack.md](ESSENTIAL/stack.md)
5. [ESSENTIAL/testing.md](ESSENTIAL/testing.md)
6. [ESSENTIAL/database.md](ESSENTIAL/database.md)
7. [ESSENTIAL/ui-guidelines.md](ESSENTIAL/ui-guidelines.md)
8. [ESSENTIAL/rule-engine.md](ESSENTIAL/rule-engine.md)

After reading, confirm: "I have read all ESSENTIAL/ files and will follow the standards."

## Project

**Preferential Rules of Origin Calculation System** for EU Trade Agreements.

Determines whether a manufactured product qualifies as an Originating Product according to preferential rules of origin.

## Stack

**Backend:** ASP.NET Core 9 · C# · PostgreSQL 16 · EF Core 9 (writes only) · Dapper 2.x (reads only) · MediatR 12 · CQRS · FluentValidation 11 · AutoMapper 13 · Serilog · Swagger/OpenAPI

**Frontend:** React 19 · TypeScript · Vite 6 · shadcn/ui · Tailwind CSS 4 · React Hook Form · Zod · TanStack Query · Axios · Lucide Icons

## Solution Structure

```
/src
  Api            ← ASP.NET Core Web API (controllers, middleware, DI)
  Application    ← CQRS handlers, commands, queries, validators, DTOs
  Domain         ← Entities, value objects, interfaces, domain events
  Infrastructure ← External services, file storage, email, background jobs
  Persistence    ← EF Core DbContext, repositories (write-only), Dapper queries
  Shared         ← Cross-cutting utilities, constants, extensions
/Tests           ← Unit, integration, repository, rule engine, E2E tests
```

## Non-Negotiable Rules

| Rule | Enforcement |
|------|-------------|
| Clean Architecture | Api references Infrastructure + Persistence **only** in `Program.cs` for DI registration via extension methods — never in controllers or handlers |
| CQRS mandatory | Command + CommandHandler + Query + QueryHandler + Validator + DTO + ResponseModel per feature |
| Dapper = reads | ALL SELECT queries use Dapper |
| EF Core = writes | ALL Insert/Update/Delete use EF Core |
| Repositories = writes only | Never call a repository for SELECT |
| No business logic in controllers | Controllers only call MediatR |
| No SQL in controllers | SQL belongs in Dapper query constants |
| No static helper classes | Use extension methods or registered services |
| No magic strings | Use typed constants |
| async/await everywhere | CancellationToken on every async public method |
| Tests required | unit + integration + repository + CQRS + rule engine; coverage > 80% |
| Rule Engine rule | No hard-coded logic; all rules from DB; new rule = new IRule + DI + UI |
| Audit trail required | CreatedBy, UpdatedBy, DeletedBy, CreatedDate, ModifiedDate, IPAddress, Machine |
| Error envelope | `{ "success": false, "message": "...", "errors": [], "traceId": "" }` |
| RBAC roles | Admin, Operator, Viewer only |
| shadcn/ui only | Material UI is NOT approved |

## See Also

- [AGENTS.md](AGENTS.md) — Complete 26-rule contract for all AI agents
- [ESSENTIAL/architecture.md](ESSENTIAL/architecture.md) — System architecture
- [ESSENTIAL/rule-engine.md](ESSENTIAL/rule-engine.md) — Rule engine design
