# Technology Stack Handbook
## Preferential Rules of Origin Calculation System — EU Trade Agreements

> **Scope:** This document is the authoritative reference for every technology approved for use in this project. Every developer, reviewer, and architect must consult this handbook before introducing, upgrading, or replacing any dependency. Deviations require a written Architecture Decision Record (ADR) and tech lead sign-off.

---

## Package Policy

> **No new NuGet package or npm package may be introduced without:**
> 1. Tech lead approval (written, in code review or issue comment)
> 2. A written rationale documented in an Architecture Decision Record (ADR) filed under `.superpowers/adr/`
> 3. A security review for any package that requires broad permissions, network access, or handles sensitive data (trade rule logic, HS codes, BOM data)

This policy applies equally to transitive dependency upgrades that change major versions.

---

## Table of Contents

### Backend
1. [ASP.NET Core 9 / .NET 9 / C# 13](#1-aspnet-core-9--net-9--c-13)
2. [Entity Framework Core 9 (Write path)](#2-entity-framework-core-9-write-path)
3. [Dapper 2.x (Read path)](#3-dapper-2x-read-path)
4. [PostgreSQL 16](#4-postgresql-16)
5. [MediatR 12](#5-mediatr-12)
6. [FluentValidation 11](#6-fluentvalidation-11)
7. [AutoMapper 13](#7-automapper-13)
8. [Serilog + Sinks](#8-serilog--sinks)
9. [ASP.NET Core Identity + JWT](#9-aspnet-core-identity--jwt)
10. [Swagger / OpenAPI 3.1 (Scalar UI)](#10-swagger--openapi-31-scalar-ui)
11. [IHostedService + Hangfire](#11-ihostedservice--hangfire)
12. [Health Checks](#12-health-checks)
13. [Global Exception Middleware](#13-global-exception-middleware)

### Frontend
14. [React 19](#14-react-19)
15. [TypeScript 5.x (strict mode)](#15-typescript-5x-strict-mode)
16. [Vite 6](#16-vite-6)
17. [shadcn/ui — AUTHORITATIVE UI Library](#17-shadcnui--authoritative-ui-library)
18. [Tailwind CSS 4](#18-tailwind-css-4)
19. [React Router v6](#19-react-router-v6)
20. [React Hook Form 7](#20-react-hook-form-7)
21. [Zod 3](#21-zod-3)
22. [TanStack Query (React Query) v5](#22-tanstack-query-react-query-v5)
23. [Axios 1.x](#23-axios-1x)
24. [Lucide React (Icons)](#24-lucide-react-icons)
25. [Framer Motion (Animations)](#25-framer-motion-animations)
26. [Recharts (Data Visualization)](#26-recharts-data-visualization)

### Testing
27. [xUnit 2](#27-xunit-2)
28. [NSubstitute / Moq 4](#28-nsubstitute--moq-4)
29. [TestContainers for .NET](#29-testcontainers-for-net)
30. [WebApplicationFactory](#30-webapplicationfactory)
31. [Playwright (E2E)](#31-playwright-e2e)
32. [Vitest + React Testing Library](#32-vitest--react-testing-library)
33. [BenchmarkDotNet](#33-benchmarkdotnet)
34. [Stryker.NET (Mutation Testing)](#34-strykernet-mutation-testing)

### DevOps
35. [Docker + Docker Compose](#35-docker--docker-compose)
36. [GitHub Actions (CI/CD)](#36-github-actions-cicd)
37. [Git + GitHub](#37-git--github)

---

## Backend

---

### 1. ASP.NET Core 9 / .NET 9 / C# 13

**Why selected**

ASP.NET Core 9 on .NET 9 delivers the performance headroom required for computationally intensive preferential origin calculations (multi-hop BOM traversal, HS code classification trees). The long-term support release cadence, strong EU-enterprise adoption, and mature ecosystem for trade-compliance domains made it the only serious candidate. C# 13 adds primary constructors and collection expressions that reduce boilerplate in CQRS handler classes. The platform's built-in dependency injection, middleware pipeline, and OpenAPI tooling eliminate entire layers of third-party infrastructure.

**When to use**

- All HTTP API controllers and minimal-API endpoints
- Application service layer (handlers, validators, command/query objects)
- Background worker hosts (`IHostedService`)
- All middleware (authentication, exception handling, correlation IDs)
- Shared kernel class libraries within the solution

**When NOT to use**

- Do not build a separate ASP.NET Core process to perform a task that belongs inside the existing host — consolidate workers into the single API host unless there is a proven scaling reason documented in an ADR.
- Do not use .NET Framework (4.x) libraries. Any dependency that only targets `net4x` is blocked.
- Do not target `net8.0` or earlier in new projects within this solution; every project file must target `net9.0`.

**Approved alternatives**

None. This is a fixed platform decision for the lifetime of this project.

**Version policy**

| Component | Pinned version |
|---|---|
| .NET SDK | `9.0.x` (latest patch of the 9.0 band, locked via `global.json`) |
| ASP.NET Core | `9.0.x` (ships with .NET 9 SDK) |
| C# language | `13` (set via `<LangVersion>13</LangVersion>` in `Directory.Build.props`) |

`global.json` must specify `"rollForward": "patch"` so CI always picks up security patches automatically while staying on the 9.0 minor.

**Upgrade strategy**

.NET 9 reaches end of support in May 2026. Migration to .NET 10 (LTS, November 2025) should be tracked in a dedicated issue. The upgrade path is:

1. Run `dotnet-outdated` and `dotnet-upgrade-assistant` on a feature branch.
2. Fix all breaking changes and deprecation warnings.
3. Update `global.json` and all `<TargetFramework>` entries.
4. Full CI green (unit + integration + E2E).
5. Merge with tech lead review.

---

### 2. Entity Framework Core 9 (Write path)

**Why selected**

EF Core 9 is the write-side ORM. It provides the change-tracking, migration tooling, and transaction management needed when persisting origin rules, BOM entries, supplier declarations, and calculation results. Using EF Core exclusively for writes — paired with Dapper for reads — implements the CQRS read/write split at the data access layer without introducing Event Sourcing complexity prematurely. EF Core migrations give the team a code-first schema evolution story that is reproducible and auditable via Git.

**When to use**

- All `INSERT`, `UPDATE`, `DELETE`, and `UPSERT` operations
- Schema migration generation (`dotnet ef migrations add`)
- Unit of Work / transaction boundary management for multi-aggregate writes
- Seeding reference data (HS code tables, agreement lists) in migrations

**When NOT to use**

- Never use EF Core for read queries that return data to the API. All reads go through Dapper (see §3). This is a hard architectural boundary.
- Do not use EF Core lazy loading. It is disabled globally. Eager-load explicitly or use projection.
- Do not call `SaveChanges()` inside a loop. Batch writes.
- Do not use EF Core raw SQL (`FromSqlRaw`) for reads — route those through Dapper.

**Approved alternatives**

None for the write path. The EF Core + Dapper split is a deliberate architectural decision; replacing one side requires an ADR.

**Version policy**

| Package | Pinned version |
|---|---|
| `Microsoft.EntityFrameworkCore` | `9.0.*` |
| `Microsoft.EntityFrameworkCore.Design` | `9.0.*` |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | `9.0.*` |

All three must stay in sync on the same minor. Pin in `Directory.Packages.props` (Central Package Management).

**Upgrade strategy**

EF Core major versions track .NET major versions. When upgrading to .NET 10, upgrade EF Core to 10.x in the same PR. Run the full migration test suite (`TestContainers` integration tests) after any EF Core version change.

---

### 3. Dapper 2.x (Read path)

**Why selected**

Dapper provides thin, high-performance SQL mapping for all read operations. Origin rule calculations involve complex multi-join queries across HS code trees, agreement tables, and BOM data that benefit from hand-crafted SQL rather than ORM-generated queries. Dapper's micro-ORM approach gives full control over query shape, enables indexed query plans, and produces predictable, debuggable SQL. It is the counterpart to EF Core in the CQRS split.

**When to use**

- All `SELECT` queries: list views, detail views, dashboard aggregates
- Stored procedure calls for complex reporting
- Bulk-read operations where EF Core's change tracking would add overhead
- Any query where you need explicit control over the SQL for performance or clarity

**When NOT to use**

- Never use Dapper for writes; all mutations go through EF Core.
- Do not use Dapper to bypass the repository interface contract — all Dapper calls live inside infrastructure read-repository implementations, not directly in handlers.
- Do not use Dapper's dynamic result type (`dynamic`) in production query methods; always map to a typed DTO or record.

**Approved alternatives**

None. The EF Core + Dapper split is fixed.

**Version policy**

| Package | Pinned version |
|---|---|
| `Dapper` | `2.1.*` |

Pin in `Directory.Packages.props`.

**Upgrade strategy**

Dapper 2.x is stable and rarely has breaking changes. Review release notes on each minor bump. Upgrade by updating the version constraint, running the integration test suite, and verifying query output matches expected results in at least one TestContainers test.

---

### 4. PostgreSQL 16

**Why selected**

PostgreSQL 16 is the relational database engine. The project's data model involves complex hierarchical structures (HS code trees, BOM multi-level assembly), temporal data (rule validity windows keyed to agreement annexes), and audit trails — all domains where PostgreSQL's support for JSONB, recursive CTEs, range types, and row-level security provides native, performant solutions without application-level workarounds. PostgreSQL is the standard for EU-compliant data processing applications requiring strong ACID guarantees.

**When to use**

- Primary persistence for all application data
- Recursive CTE queries for BOM traversal and HS code tree walks
- JSONB columns for flexible supplier declaration attachment data
- Date range types for agreement validity periods
- Full-text search for HS code description lookup

**When NOT to use**

- Do not use PostgreSQL's `LISTEN`/`NOTIFY` as a message bus — use the domain event pattern via MediatR instead.
- Do not store large binary files (PDFs, certificates) as `BYTEA` — reference an object store and store only the path/reference.
- Do not bypass the application layer to run ad-hoc `UPDATE` statements directly on the production database. All mutations go through the application.

**Approved alternatives**

None. The database engine is a fixed infrastructure decision.

**Version policy**

| Component | Pinned version |
|---|---|
| PostgreSQL server | `16.x` (Docker image: `postgres:16-alpine`) |
| `Npgsql` driver | `9.0.*` (tracks EF Core version) |

**Upgrade strategy**

PostgreSQL major versions are released annually. Evaluate PostgreSQL 17 after it reaches general availability. Upgrade requires:

1. Review of release notes for breaking changes in SQL behavior.
2. `pg_upgrade` dry-run on a staging database snapshot.
3. Full integration test suite pass.
4. Coordinated deployment with schema migration.

---

### 5. MediatR 12

**Why selected**

MediatR implements the mediator pattern to decouple HTTP controllers from application logic. Every user action maps to a Command or Query object dispatched through MediatR. This enforces the CQRS boundary, enables pipeline behaviors (validation, logging, performance measurement) to be applied uniformly, and makes handlers independently testable without needing a controller or HTTP context. MediatR 12 targets .NET 9 and removes the dependency on `System.Reflection` scanning at startup in favor of source-generator registration.

**When to use**

- Every Command (write intent) and Query (read intent) in the application layer
- Cross-cutting pipeline behaviors: `IPipelineBehavior<,>` for validation, logging, and transaction management
- Domain event publishing inside the same process (`INotification` / `INotificationHandler`)

**When NOT to use**

- Do not use MediatR as a replacement for a message broker. Cross-service or cross-process messaging requires a proper broker (not in scope for this project's current phase, but relevant if it grows).
- Do not create MediatR handlers that call other MediatR handlers — this creates hidden coupling and defeats the purpose of the pattern.
- Do not put infrastructure concerns (database connections, HTTP calls) directly inside a handler; inject repositories and services.

**Approved alternatives**

None. MediatR is the approved mediator implementation.

**Version policy**

| Package | Pinned version |
|---|---|
| `MediatR` | `12.*` |
| `MediatR.Extensions.Microsoft.DependencyInjection` | `12.*` (if separate) |

**Upgrade strategy**

MediatR follows semantic versioning. Minor upgrades: update, run unit tests, verify pipeline behaviors. Major upgrades: review the migration guide — MediatR major versions have historically involved registration API changes.

---

### 6. FluentValidation 11

**Why selected**

FluentValidation 11 provides a fluent, testable API for expressing business validation rules in the application layer. Origin rule validation involves domain-specific constraints (valid HS code format, agreement-specific tolerance thresholds, date range overlaps) that are cleaner to express and test as FluentValidation `AbstractValidator<T>` classes than as Data Annotations. FluentValidation integrates with the MediatR pipeline via a `ValidationBehavior<,>` so validation runs automatically before every command handler.

**When to use**

- All Command and Query input validation
- Business-rule assertions that belong in the application layer (not just format checks)
- Re-use of validator logic across multiple commands via composition

**When NOT to use**

- Do not use FluentValidation for API-level model binding — this is handled by ASP.NET Core's model binding. FluentValidation runs after binding, in the pipeline behavior.
- Do not duplicate frontend Zod schema rules in FluentValidation — the two serve different environments. Backend validation is authoritative; frontend validation is a UX convenience layer.
- Do not put persistence or expensive I/O in synchronous validators without using the async validation API.

**Approved alternatives**

None. FluentValidation is the approved validation library.

**Version policy**

| Package | Pinned version |
|---|---|
| `FluentValidation` | `11.*` |
| `FluentValidation.AspNetCore` | `11.*` |

**Upgrade strategy**

FluentValidation 11 is stable. Monitor for a major version (12+) announcement. The API surface is narrow enough that major upgrades are low risk; validate by running the unit tests for all validators after each upgrade.

---

### 7. AutoMapper 13

**Why selected**

AutoMapper 13 eliminates repetitive property-mapping code between domain entities, DTOs, and view models. In a project with dozens of origin-rule entity types and corresponding API response shapes, manual mapping is error-prone and creates maintenance burden. AutoMapper's profile-based configuration keeps mapping logic centralized and testable. AutoMapper 13 adds improved null handling and performance improvements over 12.x.

**When to use**

- Entity → DTO mapping in query handlers
- DTO → Command object mapping in API controllers
- Projection mapping with `ProjectTo<T>()` for Dapper result sets

**When NOT to use**

- Do not use AutoMapper for mappings that involve conditional business logic — write an explicit factory method or mapping service instead.
- Do not use AutoMapper across assembly boundaries where the mapping configuration cannot be centrally validated.
- Do not use `Mapper.Map<T>()` statically — always inject `IMapper` through the constructor.

**Approved alternatives**

`Mapster` is a permitted alternative if a team benchmark demonstrates a significant performance advantage for a specific hot path. Requires ADR.

**Version policy**

| Package | Pinned version |
|---|---|
| `AutoMapper` | `13.*` |
| `AutoMapper.Extensions.Microsoft.DependencyInjection` | `13.*` |

**Upgrade strategy**

AutoMapper major versions have historically changed the configuration API. On a major upgrade, review the migration guide and run the `AssertConfigurationIsValid()` test to catch unmapped members immediately.

---

### 8. Serilog + Sinks

**Why selected**

Serilog provides structured, contextual logging. In a trade-compliance system, log events must carry correlation IDs, user context, agreement identifiers, and HS codes as structured properties — not flat strings — so they can be queried and filtered in downstream log aggregation. Serilog's sink-based architecture allows the output target to change without touching application log statements.

**When to use**

- All application logging: request lifecycle, command/query execution, validation failures, calculation steps
- Enrichment with ambient properties: `CorrelationId`, `UserId`, `AgreementCode`
- Logging performance metrics from the MediatR pipeline behavior

**When NOT to use**

- Do not log sensitive trade data (full BOM details, supplier financial data) at `Debug` or `Information` level in production — log identifiers and status codes only.
- Do not use `Console.WriteLine` or `Debug.WriteLine` anywhere in application code.
- Do not create new sink packages without tech lead approval — sinks can introduce network dependencies.

**Approved sinks**

| Sink | Use case | Status |
|---|---|---|
| `Serilog.Sinks.Console` | Local development, container stdout | Approved |
| `Serilog.Sinks.File` (rolling) | Production file logging | Approved |
| `Serilog.Sinks.PostgreSQL` | DB-based audit trail log | Conditional — only if audit log query requirement confirmed |
| `Serilog.Sinks.Seq` | Structured log server for dev/staging | Approved for non-production |

**Version policy**

| Package | Pinned version |
|---|---|
| `Serilog` | `4.*` |
| `Serilog.AspNetCore` | `8.*` |
| `Serilog.Sinks.Console` | `6.*` |
| `Serilog.Sinks.File` | `6.*` |
| `Serilog.Sinks.PostgreSQL` | `4.*` (conditional) |

**Upgrade strategy**

Serilog core and sinks version independently. Update each sink separately and run the integration test that asserts log events reach the configured output.

---

### 9. ASP.NET Core Identity + JWT

**Why selected**

ASP.NET Core Identity provides a production-hardened user store with password hashing, account lockout, and role/claim management that maps directly onto the project's user types (customs officer, trade analyst, administrator). A custom JWT service (not a third-party library) generates and validates tokens so that token shape, expiry, and claim set are under full project control without a dependency on an external identity provider during development.

**When to use**

- User registration, login, password management
- Role-based authorization on all API endpoints (`[Authorize(Roles = "...")]`)
- JWT issuance on successful authentication
- Claim-based feature gating (e.g., access to specific trade agreement data)

**When NOT to use**

- Do not share the JWT signing key across environments — each environment (dev, staging, prod) must have its own secret managed via environment variable or secrets manager.
- Do not implement custom password hashing — use Identity's `IPasswordHasher<T>`.
- Do not store JWT tokens in `localStorage` on the frontend — use `httpOnly` cookies or the Authorization header from memory.
- Do not use third-party OAuth libraries without an ADR; the current scope does not require external identity federation.

**Approved alternatives**

If external identity federation (Azure AD, Keycloak) is required in a future phase, an ADR must be written before any `OpenIddict` or `IdentityServer` package is introduced.

**Version policy**

| Package | Pinned version |
|---|---|
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | `9.0.*` |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | `9.0.*` |
| `System.IdentityModel.Tokens.Jwt` | `8.*` |

**Upgrade strategy**

Identity packages track .NET major versions. Upgrade together with ASP.NET Core in the .NET 10 migration PR.

---

### 10. Swagger / OpenAPI 3.1 (Scalar UI)

**Why selected**

OpenAPI 3.1 documentation is generated from the ASP.NET Core endpoint metadata at runtime, providing a live contract for frontend developers and integration partners. Scalar is selected over the default Swagger UI because it renders OpenAPI 3.1 correctly (Swagger UI has known OpenAPI 3.1 compatibility gaps), provides a cleaner UX for testing endpoints, and supports dark mode — a practical advantage for developer experience.

**When to use**

- All API endpoints must be documented with XML summary comments and `[ProducesResponseType]` attributes.
- Scalar UI is available at `/scalar` in development and staging environments only.
- The raw OpenAPI JSON must be exportable at `/openapi/v1.json` for integration with external tools.

**When NOT to use**

- Do not expose Scalar UI or the OpenAPI JSON endpoint in the production environment — disable via `ASPNETCORE_ENVIRONMENT` check.
- Do not use Swashbuckle's extended filter pipeline to mutate the OpenAPI document in complex ways; keep the document generation simple.

**Approved alternatives**

`Swashbuckle.AspNetCore` is permitted as the OpenAPI generation library if `Microsoft.AspNetCore.OpenApi` (the .NET 9 built-in) proves insufficient, subject to ADR. Scalar UI is the approved rendering frontend.

**Version policy**

| Package | Pinned version |
|---|---|
| `Microsoft.AspNetCore.OpenApi` | `9.0.*` |
| `Scalar.AspNetCore` | `2.*` |

**Upgrade strategy**

Scalar follows its own release cadence. Update after reviewing the changelog for breaking changes in the UI configuration API. The OpenAPI generation package tracks .NET.

---

### 11. IHostedService + Hangfire

**Why selected**

Background processing is needed for long-running origin determination jobs (BOM traversal across thousands of components), scheduled rule-validity recalculations, and asynchronous notification dispatch. `IHostedService` is the standard .NET mechanism for hosting background work inside the same process with proper startup/shutdown lifecycle integration.

Hangfire is conditionally approved as an upgrade path if the job queue requires persistence (surviving restarts), retry logic, and a management dashboard — requirements that `IHostedService` with an in-memory queue cannot safely satisfy.

**When to use `IHostedService`**

- Periodic, low-stakes background tasks: cache warming, health ping, cleanup jobs
- Tasks that can be lost on restart without business consequence
- Simple queued work that does not need retry or visibility

**When to use Hangfire (conditional)**

- Persistent, durable job queues where a restart must not lose enqueued work
- Jobs that require retry with backoff (external API calls, notification dispatch)
- When a dashboard for monitoring job execution is a stakeholder requirement

**When NOT to use either**

- Do not use background jobs to bypass the CQRS pipeline for data writes — even background jobs must dispatch MediatR commands.
- Do not use `Thread.Sleep` in `IHostedService` implementations; use `Task.Delay` with the cancellation token.
- Do not activate Hangfire without first creating an ADR that justifies the additional infrastructure dependency (Hangfire requires its own schema tables in PostgreSQL).

**Approved alternatives**

Quartz.NET is a permitted alternative to Hangfire for complex scheduling (cron-based) scenarios. Requires ADR.

**Version policy**

| Package | Pinned version |
|---|---|
| `Hangfire.Core` | `1.8.*` (conditional) |
| `Hangfire.AspNetCore` | `1.8.*` (conditional) |
| `Hangfire.PostgreSql` | `1.20.*` (conditional) |

**Upgrade strategy**

`IHostedService` has no version (built-in). Hangfire: upgrade minor versions when security advisories are issued. Review Hangfire's own PostgreSQL schema migration notes before upgrading.

---

### 12. Health Checks

**Why selected**

ASP.NET Core's built-in health check framework exposes `/health` and `/health/ready` endpoints that Docker, Kubernetes, and load balancers use for liveness and readiness probes. For a trade-compliance system that integrates with PostgreSQL and potentially external tariff data services, health checks provide operational visibility at zero cost to the application stack.

**When to use**

- Database connectivity health check (Npgsql health check package)
- External service dependency checks (if tariff API integrations are added)
- Custom checks for rule-engine warm-up state

**When NOT to use**

- Do not expose health check detail (connection strings, stack traces) on the `/health/live` endpoint — return only `Healthy`/`Unhealthy` status in production.
- Do not put business logic inside health check implementations.

**Approved alternatives**

None needed; the built-in framework is sufficient.

**Version policy**

| Package | Pinned version |
|---|---|
| `AspNetCore.HealthChecks.NpgSql` | `8.*` |

Built-in health check middleware ships with ASP.NET Core 9 — no separate package needed.

**Upgrade strategy**

The `AspNetCore.HealthChecks.*` community packages follow their own versioning. Update when the .NET version changes or security issues are reported.

---

### 13. Global Exception Middleware

**Why selected**

A custom global exception middleware (not `UseExceptionHandler` with the default behavior) ensures that all unhandled exceptions are:

1. Logged with full context via Serilog
2. Mapped to RFC 7807 Problem Details responses
3. Sanitized so that internal stack traces are never returned to the client

This is a custom implementation — no third-party package — to maintain full control over the error response shape required by the API contract.

**When to use**

- Registered once in `Program.cs` as the outermost middleware in the pipeline
- Catches all unhandled `Exception` types and maps them to HTTP Problem Details

**When NOT to use**

- Do not catch exceptions inside individual handlers and swallow them silently — let the middleware handle logging and response shaping.
- Do not return different error shapes from different endpoints — all errors must flow through this middleware.

**Approved alternatives**

ASP.NET Core's built-in Problem Details service (`IProblemDetailsService`, added in .NET 7+) is an approved alternative if it meets the project's Problem Details format requirements. Evaluate before writing a fully custom middleware.

**Version policy**

Custom code — no package version. Tested as part of the API integration test suite.

**Upgrade strategy**

Review compatibility with each ASP.NET Core major version upgrade.

---

## Frontend

---

### 14. React 19

**Why selected**

React 19 introduces the Actions pattern, `useOptimistic`, `useFormStatus`, and the React Compiler (opt-in), which reduce the boilerplate needed for server-integrated form workflows — a primary interaction pattern in origin-rule entry screens. React 19 is the stable version aligned with the current React Hook Form 7 and TanStack Query v5 ecosystem.

**When to use**

- All user interface components
- Component composition for complex rule-entry forms, BOM hierarchy displays, and calculation result panels

**When NOT to use**

- Do not use React class components — all components must be function components with hooks.
- Do not use React's `createRef` / `forwardRef` patterns where the `ref` callback pattern or a context provides a cleaner solution.
- Do not use `ReactDOM.render()` — use `createRoot()`.

**Approved alternatives**

None. React is the fixed frontend framework.

**Version policy**

| Package | Pinned version |
|---|---|
| `react` | `19.x` |
| `react-dom` | `19.x` |
| `@types/react` | `19.x` |
| `@types/react-dom` | `19.x` |

**Upgrade strategy**

React follows semantic versioning strictly. Patch and minor updates: apply promptly. Major (React 20+): evaluate only after ecosystem dependencies (React Router, React Hook Form, TanStack Query) release compatible versions.

---

### 15. TypeScript 5.x (strict mode)

**Why selected**

TypeScript 5.x with full strict mode (`strict: true`) is mandatory for the frontend codebase. In a domain with complex data structures (HS code objects, BOM trees, agreement annexes), TypeScript's type system catches entire classes of bugs at compile time that would otherwise surface only during integration testing. Strict mode eliminates `any` escape hatches, enforces null safety, and produces self-documenting API boundaries.

**When to use**

- Every `.ts` and `.tsx` file in the frontend
- Type definitions for all API response shapes (auto-generated from OpenAPI spec where possible)
- Discriminated unions for calculation result states

**When NOT to use**

- Do not use `@ts-ignore` or `@ts-expect-error` except as a last resort for untyped third-party library integration, always with an explanatory comment.
- Do not use `any` — use `unknown` with type guards instead.
- Do not disable `strict` or individual strict checks in `tsconfig.json`.

**Approved alternatives**

None. TypeScript is required; JavaScript-only files are not permitted in the frontend source tree.

**Version policy**

| Package | Pinned version |
|---|---|
| `typescript` | `5.x` (latest stable minor) |

**Upgrade strategy**

TypeScript minor versions occasionally introduce new strict checks that surface previously hidden type errors. After upgrading, run `tsc --noEmit` and resolve all new errors before merging. Major versions (6.x): wait for Vite and TS-plugin ecosystem compatibility confirmation.

---

### 16. Vite 6

**Why selected**

Vite 6 provides sub-second hot module replacement during development and optimized production bundles via Rollup. Its native ES module development server eliminates the full-bundle rebuild cycle that made Webpack-based setups slow for large React/TypeScript projects. Vite 6 supports the React 19 JSX transform out of the box and has first-class TypeScript support without requiring a separate transpilation step.

**When to use**

- Development server: `vite dev`
- Production build: `vite build`
- Preview of production build: `vite preview`
- Plugin configuration for path aliases, environment variables, and test setup (via `vite.config.ts`)

**When NOT to use**

- Do not use Vite for server-side rendering without first evaluating the project's SSR requirements — this project is a client-side SPA, so SSR is not in scope.
- Do not add Vite plugins without ADR approval — each plugin adds to the build graph and can introduce supply-chain risk.

**Approved alternatives**

None. Vite is the approved build tool.

**Version policy**

| Package | Pinned version |
|---|---|
| `vite` | `6.x` |
| `@vitejs/plugin-react` | `4.x` |

**Upgrade strategy**

Vite patch updates: apply immediately. Vite minor/major: review the migration guide, test the full production build and dev server startup, then merge.

---

### 17. shadcn/ui — AUTHORITATIVE UI Library

> **CRITICAL DECISION NOTICE**
>
> The Additional Requirements document supersedes the main specification for frontend UI components. **shadcn/ui is the ONLY approved component library for this project. Material UI (MUI) is NOT approved for this project.** The main PDF's mention of Material UI DataGrid is overridden by the Additional Requirements PDF. Any developer who introduces `@mui/material`, `@mui/x-data-grid`, or any other Material UI package will have those dependencies rejected in code review.

**Why selected**

shadcn/ui is not a traditional component library — it is a collection of accessible, composable components built on Radix UI primitives and styled with Tailwind CSS 4. Components are copied into the project's source tree rather than consumed as a versioned package, giving the team full ownership of the markup and styles. This model:

- Eliminates version-lock conflicts between the component library and Tailwind
- Allows domain-specific customization (EU trade color palette, HS code display formatting) without fighting a library's style system
- Keeps bundle size minimal — only included components are shipped
- Provides WAI-ARIA compliance through Radix UI's battle-tested primitive behaviors

**When to use**

- All UI components: buttons, inputs, dialogs, tables, select dropdowns, tabs, tooltips, badges
- Data table displays of origin rules, BOM entries, and calculation results (use shadcn's `Table` component)
- Form input components paired with React Hook Form 7
- Modal dialogs and sheets for rule-editing workflows
- Navigation: sidebar, breadcrumbs, command palette

**When NOT to use**

- Do not install `@mui/material` or any Material UI package — see the critical notice above.
- Do not override Radix UI's accessibility attributes (`aria-*`, `role`, `data-*`) without understanding the impact on keyboard navigation.
- Do not import shadcn components from an npm package — they live in `src/components/ui/` in this repository.
- Do not create new shadcn-style components without following the existing naming and file structure conventions.

**Approved alternatives**

None. shadcn/ui is the only approved UI component system. Radix UI primitives (which power shadcn/ui) may be used directly for components not yet available in shadcn/ui, with tech lead approval.

**Version policy**

shadcn/ui components are copied into the source tree via the `shadcn` CLI. The "version" is the component source as of the copy date, tracked in Git. The underlying dependencies:

| Package | Pinned version |
|---|---|
| `@radix-ui/*` (various) | pinned per `package.json` |
| `class-variance-authority` | `0.7.*` |
| `clsx` | `2.*` |
| `tailwind-merge` | `2.*` |

**Upgrade strategy**

To update a shadcn/ui component: re-run `npx shadcn@latest add <component>` on a branch, review the diff carefully for behavior changes, and merge with review. Update Radix UI packages using `npm outdated` and reviewing the Radix changelog for accessibility or behavior changes.

---

### 18. Tailwind CSS 4

**Why selected**

Tailwind CSS 4 is the styling layer for the entire frontend. Its utility-first approach eliminates CSS naming conflicts, reduces dead CSS in production (via the Oxide engine's content scanning), and pairs directly with shadcn/ui which assumes Tailwind as the styling primitive. Tailwind 4 introduces a significant performance improvement in the build step via the Oxide engine and allows CSS-native configuration instead of `tailwind.config.js`.

**When to use**

- All component styling via utility classes
- Responsive breakpoints (`sm:`, `md:`, `lg:`)
- Dark mode via the `dark:` variant (if dark mode is a project requirement)
- Custom design tokens defined in the CSS `@theme` block

**When NOT to use**

- Do not write standalone `.css` files for component styling — all styles live in JSX className strings or `@apply` directives in global CSS only.
- Do not introduce CSS-in-JS libraries (`styled-components`, `emotion`) — they conflict with the Tailwind approach.
- Do not override Tailwind's utility classes with competing utility libraries.

**Approved alternatives**

None. Tailwind is required by shadcn/ui and is a fixed dependency.

**Version policy**

| Package | Pinned version |
|---|---|
| `tailwindcss` | `4.x` |
| `@tailwindcss/vite` | `4.x` |

**Upgrade strategy**

Tailwind 4 uses a new configuration format — do not migrate to a 4.x patch that changes the `@theme` syntax without reading the release notes. Coordinate Tailwind upgrades with shadcn/ui component updates.

---

### 19. React Router v6

**Why selected**

React Router v6 provides file-convention-optional, nested route configuration that matches the application's hierarchical navigation structure (agreement → annex → rule → BOM). Its `createBrowserRouter` API with data loaders aligns with TanStack Query's prefetching pattern for seamless, waterfall-free navigation.

**When to use**

- All client-side page navigation
- Nested layouts (authenticated shell, rule-detail drawer)
- Route-level code splitting with `React.lazy`
- URL-driven state for filters and pagination (search params)

**When NOT to use**

- Do not use React Router's built-in data loader functions to fetch data — use TanStack Query for all server data fetching. React Router loaders are a parallel pattern that conflicts with TanStack Query's caching layer.
- Do not use hash-based routing (`createHashRouter`) unless a deployment constraint requires it.

**Approved alternatives**

TanStack Router is a type-safe alternative that is compatible with TanStack Query. If the team decides to adopt it, an ADR is required.

**Version policy**

| Package | Pinned version |
|---|---|
| `react-router-dom` | `6.x` |

**Upgrade strategy**

React Router v7 introduces full-stack framework patterns — evaluate only if the project pivots to SSR. Monitor the migration guide before upgrading.

---

### 20. React Hook Form 7

**Why selected**

React Hook Form 7 provides performant, uncontrolled form state management. Origin rule entry forms are complex (conditional fields, nested BOM arrays, multi-step validation) and React Hook Form's uncontrolled approach avoids the re-render-on-keystroke problem that controlled form libraries create at scale. It integrates directly with Zod via `@hookform/resolvers` for schema-based validation.

**When to use**

- All user-facing forms: rule creation, BOM entry, supplier declaration, user management
- Multi-step wizard forms using `useFormContext`
- Dynamic field arrays using `useFieldArray` for BOM component lists

**When NOT to use**

- Do not use React Hook Form for display-only data tables — it is a form library, not a state manager.
- Do not mix React Hook Form with controlled `useState` for the same field — choose one approach per field.

**Approved alternatives**

None for form management.

**Version policy**

| Package | Pinned version |
|---|---|
| `react-hook-form` | `7.x` |
| `@hookform/resolvers` | `3.x` |

**Upgrade strategy**

RHF follows semantic versioning. Minor updates: apply and run form-related Vitest tests. Major updates: review resolver API changes carefully.

---

### 21. Zod 3

**Why selected**

Zod 3 provides TypeScript-first schema validation that generates both runtime validators and TypeScript types from a single schema definition. On the frontend, Zod schemas paired with React Hook Form's resolver ensure that form validation rules and TypeScript types are always in sync. Zod also validates API response shapes, providing a safety net against unexpected backend response changes.

**When to use**

- All form validation schemas (passed to `zodResolver` in React Hook Form)
- API response parsing in TanStack Query's `select` transforms
- Environment variable validation at startup (`z.object({...}).parse(import.meta.env)`)

**When NOT to use**

- Do not duplicate Zod schemas on the backend — the backend uses FluentValidation. The two systems serve different environments.
- Do not use Zod for runtime type-checking of large arrays in performance-sensitive paths without benchmarking — Zod validation is synchronous and can block on large datasets.

**Approved alternatives**

`valibot` is an approved alternative for bundle-size-sensitive contexts (it is significantly smaller than Zod). Requires ADR if introduced alongside or replacing Zod.

**Version policy**

| Package | Pinned version |
|---|---|
| `zod` | `3.x` |

**Upgrade strategy**

Zod 4 (in active development as of mid-2025) introduces breaking changes to the schema API. Do not upgrade to Zod 4 until `@hookform/resolvers` ships official Zod 4 support.

---

### 22. TanStack Query (React Query) v5

**Why selected**

TanStack Query v5 manages all server state: caching, background refetching, optimistic updates, and loading/error states. In a trade-compliance application, rules and BOM data change infrequently but must always be fresh when a calculation is initiated. TanStack Query's stale-while-revalidate model ensures UI responsiveness without stale-data risk. It eliminates `useEffect`-based data fetching patterns that are difficult to test and reason about.

**When to use**

- All API data fetching in React components (`useQuery`)
- Mutations with cache invalidation after writes (`useMutation`)
- Prefetching rule data on hover or on route entry
- Pagination and infinite scroll for large rule lists (`useInfiniteQuery`)

**When NOT to use**

- Do not store UI state (open/closed, selected tab) in TanStack Query — that belongs in `useState` or URL search params.
- Do not use TanStack Query as a global client-state store — it is a server-state library.

**Approved alternatives**

SWR is a permitted alternative for simple fetch-and-cache scenarios if TanStack Query's overhead is disproportionate. Requires ADR.

**Version policy**

| Package | Pinned version |
|---|---|
| `@tanstack/react-query` | `5.x` |
| `@tanstack/react-query-devtools` | `5.x` |

**Upgrade strategy**

TanStack Query v5 has a stable API. Monitor for v6 announcement. Upgrade minor versions promptly for bug fixes.

---

### 23. Axios 1.x

**Why selected**

Axios 1.x provides an HTTP client with request/response interceptors, which are used to attach the JWT Authorization header, handle 401 token-refresh flows, and normalize error responses before they reach TanStack Query. The interceptor pattern is cleaner than wrapping every `fetch` call with the same boilerplate.

**When to use**

- All API calls from TanStack Query's `queryFn` and `mutationFn`
- Central `axios.create()` instance configured with the base URL and default headers

**When NOT to use**

- Do not use Axios directly in components — always go through TanStack Query hooks.
- Do not create multiple Axios instances without a documented reason — maintain one instance per API target.

**Approved alternatives**

Native `fetch` with a thin wrapper is an approved alternative for cases where Axios's bundle size is a concern. Requires ADR.

**Version policy**

| Package | Pinned version |
|---|---|
| `axios` | `1.x` |

**Upgrade strategy**

Axios is stable. Update patch versions promptly. Review interceptor API compatibility on minor upgrades.

---

### 24. Lucide React (Icons)

**Why selected**

Lucide React is the **only approved icon library** for this project. It provides a consistent, MIT-licensed, tree-shakeable SVG icon set that aligns with shadcn/ui's icon usage conventions. Lucide icons are used in shadcn/ui's own documentation and component examples, ensuring visual consistency without style conflicts.

**When to use**

- All iconography: navigation icons, action buttons, status indicators, form labels
- Pass size and stroke props from the component's design token rather than hard-coding

**When NOT to use**

- Do not install `react-icons`, `heroicons`, `@mui/icons-material`, `font-awesome`, or any other icon library. Lucide React is the sole approved source.
- Do not use icon font approaches (CSS classes for icons) — use the SVG React component form only.

**Approved alternatives**

None. Lucide React is the sole approved icon library.

**Version policy**

| Package | Pinned version |
|---|---|
| `lucide-react` | `0.x` (latest stable) |

**Upgrade strategy**

Lucide adds icons and occasionally renames existing ones in minor releases. Run a global search for any renamed icon after upgrading. The Lucide changelog documents all icon rename/removal events.

---

### 25. Framer Motion (Animations)

**Why selected**

Framer Motion provides declarative, physics-based animations for page transitions and micro-interactions. In a business application, animations serve a functional purpose: they communicate state transitions (loading, success, error), guide user attention through multi-step rule-entry workflows, and reduce perceived latency on navigation.

**When to use**

- Page-level enter/exit transitions (route changes)
- Modal and sheet open/close animations
- Loading skeleton shimmer effects
- Accordion expand/collapse in BOM tree views
- Micro-interactions: button press feedback, icon state transitions

**When NOT to use**

- Do not add animation to every component — reserve Framer Motion for interactions where animation adds comprehension value, not decoration.
- Do not animate large lists with Framer Motion without using `AnimatePresence` with a keyed list and `layout` optimization — unoptimized list animations cause layout thrash.
- Do not add Framer Motion to components that are rendered hundreds of times in a table without performance profiling.

**Approved alternatives**

CSS transitions via Tailwind's `transition-*` utilities are the preferred first choice for simple hover/focus effects — only reach for Framer Motion when CSS alone is insufficient.

**Version policy**

| Package | Pinned version |
|---|---|
| `framer-motion` | `12.x` |

**Upgrade strategy**

Framer Motion major versions sometimes change the `motion` API. Review the migration guide and test all animated routes and components after upgrading.

---

### 26. Recharts (Data Visualization)

**Why selected**

Recharts is a React-native charting library built on D3 that provides composable, declarative chart components. The dashboard requires origin rule compliance rate charts, BOM value distribution visualizations, and agreement utilization trends. Recharts' SVG-based output is accessible, responsive, and customizable to the project's design tokens without overriding a separate charting theme system.

**When to use**

- Dashboard KPI charts: bar charts for rule applicability, line charts for calculation volume over time
- BOM value breakdown: pie or donut charts for material origin composition
- Agreement utilization: stacked bar charts for comparing rules across trade agreements

**When NOT to use**

- Do not use Recharts for maps or geographic visualizations — use a dedicated mapping library if geographic origin visualization is required (ADR required).
- Do not use Recharts in components that render inside a virtualized list — Recharts re-calculates responsive dimensions on each render and does not virtualize internal data points.

**Approved alternatives**

`Victory` is an approved alternative for more complex chart types not available in Recharts. Requires ADR.

**Version policy**

| Package | Pinned version |
|---|---|
| `recharts` | `2.x` |

**Upgrade strategy**

Recharts 3.x (in development) changes the tooltip API. Do not upgrade from 2.x until the project's chart components are audited against the 3.x migration guide.

---

## Testing

---

### 27. xUnit 2

**Why selected**

xUnit 2 is the standard .NET testing framework adopted by the ASP.NET Core and EF Core teams themselves. Its extensible fixture model (`IClassFixture<T>`, `ICollectionFixture<T>`) maps directly onto the TestContainers PostgreSQL fixture pattern. xUnit's parallel test execution model speeds up the integration test suite.

**When to use**

- All unit tests for domain logic, validators, command handlers, mapping profiles
- All integration tests using TestContainers and WebApplicationFactory
- Theory-driven tests for HS code validation edge cases and origin calculation rule coverage

**When NOT to use**

- Do not mix xUnit and NUnit in the same solution — pick one framework.
- Do not put integration tests in the same test project as unit tests — separate them into `*.UnitTests` and `*.IntegrationTests` projects to control which tests run in which CI stage.

**Approved alternatives**

None. xUnit 2 is the approved test framework.

**Version policy**

| Package | Pinned version |
|---|---|
| `xunit` | `2.9.*` |
| `xunit.runner.visualstudio` | `2.8.*` |
| `Microsoft.NET.Test.Sdk` | `17.*` |

**Upgrade strategy**

xUnit 3 is in preview. Do not upgrade until it reaches stable GA and the TestContainers and WebApplicationFactory integrations are confirmed compatible.

---

### 28. NSubstitute / Moq 4

**Why selected**

Both mocking libraries are approved; the team may standardize on one. NSubstitute is preferred for its cleaner syntax that avoids the `.Object` unwrapping pattern. Moq 4 is retained as an alternative because many developers are familiar with it and it has a larger community sample base.

**When to use**

- Mocking repository interfaces in unit tests for command/query handlers
- Mocking external service interfaces (tariff API clients, email services)

**When NOT to use**

- Do not mock entity classes or value objects — test them directly.
- Do not mock the database — use TestContainers for integration-level data access tests.
- Do not mix NSubstitute and Moq within the same test project — choose one per project.

**Approved alternatives**

Both NSubstitute and Moq 4 are approved. FakeItEasy is a further alternative if the team has a strong preference.

**Version policy**

| Package | Pinned version |
|---|---|
| `NSubstitute` | `5.x` |
| `Moq` | `4.20.*` |

**Upgrade strategy**

Moq 4.20 introduced the SponsorLink controversy — ensure the version pinned does not include telemetry packages. NSubstitute: upgrade minor versions freely.

---

### 29. TestContainers for .NET

**Why selected**

TestContainers launches a real PostgreSQL 16 Docker container during the integration test run. This ensures that EF Core migrations, Dapper queries, and data access patterns are tested against the actual database engine rather than an in-memory substitute. In-memory providers hide SQL-level bugs (function availability, case sensitivity, index behavior) that TestContainers catches.

**When to use**

- All integration tests that touch the database (repository tests, handler tests that exercise the full stack through to PostgreSQL)
- One shared `PostgreSqlContainer` fixture per test collection for performance

**When NOT to use**

- Do not use TestContainers in unit tests — the database fixture belongs only in integration test projects.
- Do not spin up a container per test class — use `ICollectionFixture<PostgresFixture>` to share the container across the test collection.

**Approved alternatives**

Respawn (for database state reset between tests) is an approved companion library.

**Version policy**

| Package | Pinned version |
|---|---|
| `Testcontainers.PostgreSql` | `3.x` |

**Upgrade strategy**

TestContainers follows its own release cadence. Upgrade when new PostgreSQL major versions require an updated image reference.

---

### 30. WebApplicationFactory

**Why selected**

`WebApplicationFactory<TProgram>` from `Microsoft.AspNetCore.Mvc.Testing` creates an in-process test host that exercises the full ASP.NET Core middleware pipeline — authentication, validation, routing, exception handling — without a real HTTP server. Integration tests using this factory confirm that controller bindings, authorization policies, and middleware behaviors work correctly end-to-end.

**When to use**

- API-level integration tests: assert HTTP status codes, response headers, and JSON body shapes for all endpoints
- Authentication/authorization integration tests: verify role-based access control
- Combined with TestContainers: replace the database connection string in the test host's configuration to point at the test container

**When NOT to use**

- Do not use WebApplicationFactory for unit tests — it is heavyweight and slow relative to testing a class directly.

**Approved alternatives**

None. `WebApplicationFactory` is the standard approach for in-process API testing.

**Version policy**

Ships with `Microsoft.AspNetCore.Mvc.Testing` `9.0.*`.

**Upgrade strategy**

Tracks ASP.NET Core — upgrade as part of the .NET major version migration.

---

### 31. Playwright (E2E)

**Why selected**

Playwright provides cross-browser end-to-end testing with a first-class .NET API. E2E tests validate full user workflows: logging in, entering an origin rule, running a calculation, and reviewing the result — scenarios that unit and integration tests cannot cover because they require the full frontend + backend stack.

**When to use**

- Critical path workflows: user login, rule creation, BOM entry, calculation submission, result export
- Regression tests for high-risk UI interactions
- Visual regression testing (screenshot comparison) for dashboard charts

**When NOT to use**

- Do not write Playwright tests for every UI state — focus on end-to-end flows, not component rendering (that belongs in Vitest + RTL).
- Do not run Playwright tests in every PR CI pipeline — run them nightly or on release branches to avoid CI cost.

**Approved alternatives**

Cypress is an approved alternative if the team has existing expertise. Requires ADR.

**Version policy**

| Package | Pinned version |
|---|---|
| `Microsoft.Playwright` | `1.x` |

**Upgrade strategy**

Playwright updates frequently with browser compatibility patches. Update patch versions in a dedicated PR and run the full E2E suite to confirm no regressions.

---

### 32. Vitest + React Testing Library

**Why selected**

Vitest is the native test runner for Vite-based projects. It shares the same Vite configuration (path aliases, environment variables, plugin transforms) as the production build, eliminating the configuration divergence that Jest with babel transforms creates. React Testing Library enforces testing from the user's perspective (queries by role, label, text) rather than implementation details.

**When to use**

- All frontend unit tests: custom hooks, utility functions, Zod schema validation
- Component tests: render a component and assert user-visible output via RTL queries
- Integration tests of form submission flows within a single component tree

**When NOT to use**

- Do not test implementation details: internal state, private methods, component internal refs.
- Do not test the same scenario in both Vitest and Playwright — Playwright owns full-stack E2E; Vitest owns frontend-only behavior.

**Approved alternatives**

None for the frontend test runner — Vitest is aligned with Vite.

**Version policy**

| Package | Pinned version |
|---|---|
| `vitest` | `2.x` |
| `@testing-library/react` | `16.x` |
| `@testing-library/user-event` | `14.x` |
| `@testing-library/jest-dom` | `6.x` |
| `jsdom` | `25.x` |

**Upgrade strategy**

Vitest tracks Vite — coordinate upgrades. React Testing Library: review the changelog for query API changes after each major upgrade.

---

### 33. BenchmarkDotNet

**Why selected**

BenchmarkDotNet provides statistically rigorous performance benchmarks for hot paths in the origin calculation engine. HS code tree traversal, BOM cost-roll-up calculations, and rule applicability checks all have performance characteristics that must be quantified before optimization decisions are made. BenchmarkDotNet eliminates the noise inherent in manual `Stopwatch` measurements.

**When to use**

- Benchmarking origin calculation algorithms before and after optimization
- Comparing alternative data structures for HS code trees
- Establishing performance baselines that CI can regression-test against

**When NOT to use**

- Do not run BenchmarkDotNet benchmarks in the main unit or integration test suite — they require `Release` build configuration and are slow by design.
- Do not use BenchmarkDotNet to justify premature optimization — only benchmark code that profiling has identified as a bottleneck.

**Approved alternatives**

None needed for micro-benchmarking.

**Version policy**

| Package | Pinned version |
|---|---|
| `BenchmarkDotNet` | `0.14.*` |

**Upgrade strategy**

BenchmarkDotNet is stable. Update when a new .NET target requires an updated version.

---

### 34. Stryker.NET (Mutation Testing)

**Why selected**

Stryker.NET validates the effectiveness of the test suite by introducing mutations (logical negations, boundary changes, operator swaps) into the production code and verifying that tests catch them. In a rules-calculation domain where off-by-one errors in tolerance thresholds or operator flips in applicability conditions have real regulatory consequences, mutation testing is a quality gate — not optional.

**When to use**

- Run against the domain layer and application layer test suites on a schedule (nightly or pre-release)
- Use the mutation score as a quality gate: a score below 80% for core calculation logic triggers a review

**When NOT to use**

- Do not run Stryker on every PR — it is slow. Run it on release branches and nightly.
- Do not apply Stryker to infrastructure layer code (database context, migration files) — focus mutation budget on domain logic.

**Approved alternatives**

None for .NET mutation testing.

**Version policy**

| Package | Pinned version |
|---|---|
| `dotnet-stryker` | `4.x` (dotnet tool) |

**Upgrade strategy**

Update the `dotnet-stryker` global tool version in `dotnet-tools.json` when a new version supports the current .NET SDK.

---

## DevOps

---

### 35. Docker + Docker Compose

**Why selected**

Docker ensures that every developer runs the same PostgreSQL 16 version and configuration as production, eliminating "works on my machine" database issues. Docker Compose orchestrates the full local development stack (API + frontend dev server + PostgreSQL + optional Seq log viewer) with a single `docker compose up` command.

**When to use**

- `docker compose up` for local development environment
- `Dockerfile` for the API and frontend builds used in CI and production deployment
- Multi-stage builds: `build` stage for compilation, `runtime` stage for the minimal production image

**When NOT to use**

- Do not store secrets (JWT signing keys, database passwords) in `docker-compose.yml` or `Dockerfile` — use `.env` files (gitignored) or Docker secrets for local development, and a secrets manager for production.
- Do not use `docker compose` in production — use the orchestrator (Kubernetes or a managed container service) for production deployment.

**Approved alternatives**

Podman is an approved alternative to Docker Desktop for developers who prefer a daemon-less container engine.

**Version policy**

| Component | Pinned version |
|---|---|
| PostgreSQL image | `postgres:16-alpine` |
| .NET SDK image | `mcr.microsoft.com/dotnet/sdk:9.0-alpine` |
| .NET runtime image | `mcr.microsoft.com/dotnet/aspnet:9.0-alpine` |
| Node image (frontend build) | `node:22-alpine` |

Docker Compose: use the version bundled with Docker Desktop (v2 compose file format). Do not use the legacy `docker-compose` v1 binary.

**Upgrade strategy**

Update base images in a dedicated maintenance PR. Test the full build pipeline after any base image change. Pin to digest hashes in production image references for reproducibility.

---

### 36. GitHub Actions (CI/CD)

**Why selected**

GitHub Actions is the CI/CD platform because the project's source repository is hosted on GitHub. It provides native integration with pull request checks, branch protection rules, and GitHub Packages for container registry hosting. The free tier covers the project's CI needs during development; the runner infrastructure requires no additional provisioning.

**When to use**

- Pull request validation: build, lint, unit tests, integration tests
- Main branch: full test suite including integration tests, Docker image build and push
- Release pipeline: version tagging, Docker image publishing, deployment to staging

**When NOT to use**

- Do not store secrets in workflow YAML files — use GitHub Actions Secrets (repository or organization level).
- Do not run Playwright E2E tests on every PR — run them on the main branch or on release branches.
- Do not use `actions/checkout@v2` or any action at a non-pinned version — pin all action versions to a SHA for supply-chain security.

**Approved alternatives**

None. GitHub Actions is the approved CI/CD platform for this project.

**Version policy**

Actions are pinned to their full commit SHA in workflow files. Reference the action version in the comment for human readability:

```yaml
- uses: actions/checkout@11bd71901bbe5b1630ceea73d27597364c9af683 # v4.2.2
```

**Upgrade strategy**

Update action SHAs in a dedicated maintenance PR. Use Dependabot (`dependabot.yml` with `package-ecosystem: github-actions`) to automate action version PRs.

---

### 37. Git + GitHub

**Why selected**

Git is the version control system; GitHub is the hosting and collaboration platform. All code, infrastructure definitions, ADRs, and documentation live in the GitHub repository. GitHub's pull request model with required reviews enforces the code review gate that the package policy depends on.

**When to use**

- Feature branches: `feature/<issue-number>-short-description`
- All changes via pull request — direct pushes to `main` are blocked by branch protection
- Conventional commit messages: `feat:`, `fix:`, `docs:`, `test:`, `chore:`, `refactor:`

**When NOT to use**

- Do not commit secrets, `.env` files, or generated binaries to the repository.
- Do not force-push to `main` or `develop` branches.
- Do not merge a PR without at least one approving review.

**Approved alternatives**

None. Git and GitHub are the fixed version control and collaboration platform.

**Version policy**

Git: use the latest stable version available on the developer's OS. No minimum version constraint, but Git 2.40+ is recommended for `--rebase-merges` support. GitHub: no version (SaaS).

**Upgrade strategy**

Update Git via the OS package manager. No action required for GitHub (SaaS).

---

## Quick Reference: Version Summary

| Layer | Technology | Pinned Version |
|---|---|---|
| Backend runtime | .NET / ASP.NET Core | `9.0.*` |
| Backend language | C# | `13` |
| ORM (writes) | Entity Framework Core | `9.0.*` |
| Data access (reads) | Dapper | `2.1.*` |
| Database | PostgreSQL | `16.x` |
| Mediator | MediatR | `12.*` |
| Validation | FluentValidation | `11.*` |
| Mapping | AutoMapper | `13.*` |
| Logging | Serilog | `4.*` |
| Auth | ASP.NET Core Identity + JwtBearer | `9.0.*` |
| API docs | Scalar.AspNetCore | `2.*` |
| Background jobs | IHostedService (built-in) / Hangfire | `1.8.*` |
| Unit testing | xUnit | `2.9.*` |
| Mocking | NSubstitute | `5.*` |
| DB test fixture | TestContainers.PostgreSql | `3.*` |
| API integration tests | WebApplicationFactory | `9.0.*` |
| E2E tests | Playwright (.NET) | `1.*` |
| Perf benchmarks | BenchmarkDotNet | `0.14.*` |
| Mutation testing | Stryker.NET | `4.*` |
| Frontend framework | React | `19.x` |
| Frontend language | TypeScript | `5.x` |
| Build tool | Vite | `6.x` |
| UI components | shadcn/ui (via source copy) | N/A (Git-tracked) |
| Styling | Tailwind CSS | `4.x` |
| Routing | React Router DOM | `6.x` |
| Forms | React Hook Form | `7.x` |
| Schema validation | Zod | `3.x` |
| Server state | TanStack Query | `5.x` |
| HTTP client | Axios | `1.x` |
| Icons | Lucide React | `0.x` |
| Animations | Framer Motion | `12.x` |
| Charts | Recharts | `2.x` |
| Frontend tests | Vitest + React Testing Library | `2.x` / `16.x` |
| Containerization | Docker (PostgreSQL image) | `postgres:16-alpine` |
| CI/CD | GitHub Actions | SHA-pinned |

---

*Last updated: 2026-06-26. Maintained by the tech lead. All deviations require an ADR filed in `.superpowers/adr/`.*
