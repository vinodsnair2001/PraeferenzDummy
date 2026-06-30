# Team Meeting — T03: EF Core DbContext + Initial Migration

**Date:** 2026-06-30  
**Branch:** `issue-3-ef-core-dbcontext-migration`  
**Depends on:** T02 (domain entities — branch `issue-2-domain-entities`, PR #30 open)

---

## 1. Requirement Summary

### Functional Requirements
- Create `ApplicationDbContext` in `src/PraeferenzRoO.Persistence/` inheriting `DbContext`
- Add one `IEntityTypeConfiguration<T>` class per entity, one file each, in `Persistence/Configurations/`
- Apply `UseSnakeCaseNamingConvention()` from `EFCore.NamingConventions`
- Create explicit named indexes (not auto-generated) per database standards
- Wire all FK relationships with named constraints and `ON DELETE RESTRICT`
- Apply `HasQueryFilter(x => !x.IsDeleted)` on every auditable entity configuration
- Create initial EF migration named `InitialCreate`
- Create `IDapperContext` interface (in Domain or Application) and `DapperContext` implementation (in Persistence) wrapping `NpgsqlConnection`
- Register all services in `Persistence/DependencyInjection.cs`

### Technical Requirements
- Packages: `Microsoft.EntityFrameworkCore`, `Npgsql.EntityFrameworkCore.PostgreSQL`, `EFCore.NamingConventions`, `Dapper`, `Npgsql`, `UuidNext`
- All entities are `AuditableEntity` or `AggregateRoot` (which extends `AuditableEntity`)
- `AuditableEntityInterceptor` (SaveChangesInterceptor) must auto-populate audit fields
- UUID v7 primary keys via `UuidNext`
- Optimistic concurrency via `UseXminAsConcurrencyToken()` on all aggregate roots
- Multi-tenant global query filters (`TenantId == currentTenantId`) on all entities
- Combined with soft-delete filter: `x => !x.IsDeleted && x.TenantId == _tenantService.CurrentTenantId`

### Non-Functional Requirements
- All constraint names must be explicit (no auto-generated names)
- Partial indexes for high-traffic tables (soft-delete `WHERE is_deleted = FALSE`)
- Forward-only migrations (empty `Down()` body throwing `NotSupportedException`)

### Assumptions
1. T02 entities (`FinishedProduct`, `Material`, `ProductMaterial`, `ProductRule`, `OriginCalculation`, `OriginCalculationDetail`, `TradeAgreement`, `HsCode`, `Country`, `User`) are the full T03 scope
2. `IDapperContext` is placed in the Application layer's `Common/Interfaces/` so it is accessible to query handlers without a Persistence reference
3. The `ITenantService` interface exists in the Application layer — confirmed by architecture handbook §7
4. The `ICurrentUserService` interface exists — confirmed by architecture handbook folder structure
5. `ProductRule` maps to the `public` schema (not `rule_engine`); `RuleDefinition` maps to `rule_engine` — but T02 has `ProductRule`, not `RuleDefinition`. This is clarified below.

### Missing Information
None identified. The issue brief is complete for the T03 scope.

---

## 2. Initial Opinions

### 🏛️ Praveen — Software Architect

The scope for T03 is clean and well-bounded. The Persistence layer is the correct home for `ApplicationDbContext` and all `IEntityTypeConfiguration<T>` files — keeping EF Core entirely out of Domain and Application is non-negotiable. My concern is the dual filter pattern: each entity needs **both** `IsDeleted == false` AND `TenantId == currentTenantId` as global query filters. EF Core only supports one `HasQueryFilter` call per entity. We must combine them into a single expression.

I also want to ensure we treat `IDapperContext` correctly. The Application layer must declare the interface (in `Common/Interfaces/`) so query handlers can receive it via DI without taking a dependency on the Persistence project. The Persistence project implements `DapperContext` and wires it up. This is the correct Clean Architecture boundary.

One architectural decision: `ProductRule` lives in the `public` schema alongside the other aggregate roots. But the architecture handbook shows a `rule_engine` schema for `RuleDefinition`. T02 did not create a `RuleDefinition` entity — that comes in T09/T10. For T03, `ProductRule` goes to `public`. This must be explicit in the entity configurations.

Finally, I want `AuditableEntityInterceptor` as a `SaveChangesInterceptor` (not a `SavingChangesInterceptor` synchronous override). The async override is preferred.

### 💻 Sreejith — Senior Programmer

The main implementation complexity is the number of configurations — 10 entities means 10 configuration files, plus the interceptor, plus the Dapper context. It is mechanical but must be done correctly.

Key edge cases I see:
- `ProductMaterial` is a join-table entity (linking `FinishedProduct` to `Material`). Its configuration must define a composite index and the two FKs explicitly.
- `OriginCalculationDetail` similarly requires careful FK wiring to `OriginCalculation`.
- `User.PasswordHash` and `RefreshTokenHash` must never appear in query logs. We should mark these columns with `HasConversion` or at minimum document that log masking is required (done in T05 JWT task).
- The `AuditableEntityInterceptor` needs `ICurrentUserService` and `IHttpContextAccessor` injected — both are scoped services. The interceptor must be registered as a scoped interceptor to avoid service lifetime issues.
- `UuidNext` is required for UUID v7 generation. We should create a single `NewId()` helper in Shared so all entity factories use it consistently.

The `Down()` method in migrations must throw `NotSupportedException` — I will add this explicitly, not leave it empty (empty `Down` is a silent no-op; throwing is an explicit safety net).

### 🎨 Sojiya — UI / UX Designer

T03 is entirely backend with no frontend surface. My contribution is limited to verifying that the column naming from EF Core's snake_case convention aligns with what future Dapper queries and the frontend API response DTOs will expect.

One note: the `users` table has `password_hash` and `refresh_token_hash` columns. If any future Dapper query returns a `User` projection to the frontend, these columns must be excluded from the query's SELECT list. I recommend adding a comment in `UserConfiguration.cs` marking these columns as sensitive, so future Dapper query authors know not to include them in projections.

Also, I want to confirm that column names for audit fields follow the database handbook's `_at` suffix convention for timestamps (e.g., `created_at`, `updated_at`, `deleted_at`) — but the `AuditableEntity` C# class uses `CreatedDate`, `ModifiedDate`, `DeletedDate`. There is a naming mismatch. The EF Core configuration must explicitly map these C# property names to the correct database column names.

### 📋 Vinod — Project Planner

T03 is on the critical path. Without it, T04 (cross-cutting infrastructure) cannot begin, and nothing above T03 can proceed. This must be delivered cleanly and without scope creep.

Scope boundary for T03:
- In scope: DbContext, entity configurations, DapperContext, AuditableEntityInterceptor, DI registration, `InitialCreate` migration
- Out of scope: repository implementations (T04+), seeding data (T23), CQRS handlers (T06+)

Risk: T02 (PR #30) is not yet merged. T03 should branch from the correct base but must pull in the T02 entity files. Since we are branching from `issue-25-claude-skill-developer-guide` (which does not have T02 merged yet), we need to either wait for PR #30 to merge or cherry-pick the entity files. We should coordinate the merge order.

Delivery estimate: 1 developer-day for configurations + interceptor + DI. Migration generation is automated. Tests (repository tests are in T04 scope) — for T03 itself, we write unit tests on the configuration builder (verifying index and constraint names).

---

## 3. Discussion Round 1 — Challenge Assumptions

**Praveen challenges Sojiya:** The `_at` vs `Date` naming mismatch you identified is real and important. In C# the property is `CreatedDate: DateTime`, but the database column convention is `created_at: TIMESTAMPTZ`. EF Core will auto-map `CreatedDate` → `created_date` via snake_case convention — not `created_at`. This breaks the database standard. We must use explicit `.HasColumnName("created_at")` in all configurations, or rename the C# properties to `CreatedAt` in T02.

**Sreejith responds:** Renaming in T02 is a breaking change since T02's PR (#30) is not yet merged. We should include the column name remap in T03 entity configurations rather than reopening the T02 PR. This is the lower-risk path. The interceptor uses the C# property names, so it remains unaffected.

**Praveen accepts:** Agreed. Each configuration must include explicit `.HasColumnName("created_at")` etc. for the nine audit fields. This must be done in the base configuration or extracted to an extension method to avoid copy-paste across 10 files.

**Vinod challenges Sreejith:** You mentioned `UuidNext` for UUID v7. But the existing `BaseEntity` uses `Guid.NewGuid()`. We cannot change that in T03 without touching Domain (which is not in T03 scope). Should we override in the EF Core configuration with `ValueGeneratedOnAdd()` and let the DB generate it, or is there a better path?

**Sreejith responds:** The database standard says "application always supplies the value" — we should not use `gen_random_uuid()` as the primary source. The correct answer is: add `UuidNext` to the Shared project and create a `UuidGenerator.NewId()` helper. Repositories (in T04) will use this when creating entities. For T03, we just configure EF Core with `ValueGeneratedNever()` on the PK (since the application provides it) and add `UuidNext` as a dependency. We document in T04 that all `new Entity()` instantiations must use `UuidGenerator.NewId()`.

**Praveen challenges Vinod on the T02 merge dependency:** We cannot generate the migration without the entity classes in the build. Since T03 branches from `issue-25-claude-skill-developer-guide` (which does not have T02 entities), we need to cherry-pick or rebase the entity files before the migration can be generated. The recommended approach is: merge PR #30 first, then create the T03 branch from the updated `issue-25-claude-skill-developer-guide`.

**Vinod accepts:** Correct. Action: merge PR #30 before implementing T03. If that is not possible (pending review), we can cherry-pick the T02 entity files for local development and rebase once #30 merges.

---

## 4. Discussion Round 2 — Refine the Design

### Database Changes
10 tables created by `InitialCreate` migration, all in `public` schema:
- `users`, `countries`, `hs_codes`, `trade_agreements`, `materials`, `finished_products`, `product_materials`, `product_rules`, `origin_calculations`, `origin_calculation_details`

### Column Name Mapping (audit fields)
All configurations must explicitly map:
| C# Property | DB Column | Type |
|---|---|---|
| `CreatedDate` | `created_at` | `TIMESTAMPTZ NOT NULL` |
| `ModifiedDate` | `updated_at` | `TIMESTAMPTZ` |
| `DeletedDate` | `deleted_at` | `TIMESTAMPTZ` |
| `CreatedBy` | `created_by` | `VARCHAR(256) NOT NULL` |
| `UpdatedBy` | `updated_by` | `VARCHAR(256)` |
| `DeletedBy` | `deleted_by` | `VARCHAR(256)` |
| `IsDeleted` | `is_deleted` | `BOOLEAN NOT NULL DEFAULT FALSE` |
| `IPAddress` | `ip_address` | `VARCHAR(45)` |
| `Machine` | `machine` | `VARCHAR(256)` |

This mapping should be extracted to a shared extension method `builder.ConfigureAuditColumns()` to avoid repetition across 10 configurations.

### Global Query Filter Pattern
Combined soft-delete + tenant filter on every entity:
```csharp
builder.HasQueryFilter(x => !x.IsDeleted && x.TenantId == _tenantService.CurrentTenantId);
```
`ApplicationDbContext` receives `ITenantService` via constructor injection.

### AuditableEntityInterceptor
- Registered as a scoped interceptor via `AddDbContext` options
- Receives `ICurrentUserService` and `IHttpContextAccessor`
- Populates: `CreatedBy`, `CreatedDate`, `IPAddress`, `Machine` on `Added`; `UpdatedBy`, `ModifiedDate`, `IPAddress`, `Machine` on `Modified`
- Does NOT handle soft-delete fields — those are set explicitly in application code

### IDapperContext Placement
```
Application/Common/Interfaces/IDapperContext.cs  ← interface
Persistence/Context/DapperContext.cs             ← implementation
```
Interface:
```csharp
public interface IDapperContext
{
    IDbConnection CreateConnection();
}
```

### Indexes Required (per issue brief + database standards)
| Table | Index | Type |
|---|---|---|
| `users` | `ix_users_email` on `email` | Standard (partial: `WHERE is_deleted = FALSE`) |
| `trade_agreements` | `ix_trade_agreements_code` on `code` | Standard (partial active) |
| `hs_codes` | `ix_hs_codes_code` on `code` | Standard (partial active) |
| `product_rules` | `ix_product_rules_rule_code` on `rule_code` | Standard |
| `product_rules` | `ix_product_rules_trade_agreement_hs_code` on `(trade_agreement_id, hs_code_id)` | Composite |
| All FK columns | `ix_{table}_{fk_column}` | Standard (FK mandatory index) |

### Concurrency
`UseXminAsConcurrencyToken()` applied on all `AggregateRoot` entities.

### DI Registration (`Persistence/DependencyInjection.cs`)
```csharp
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)
           .UseSnakeCaseNamingConvention()
           .AddInterceptors(sp.GetRequiredService<AuditableEntityInterceptor>()));
services.AddScoped<AuditableEntityInterceptor>();
services.AddScoped<IDapperContext, DapperContext>();
```

### Migration Generation (after implementation)
```bash
dotnet ef migrations add InitialCreate \
  --project src/PraeferenzRoO.Persistence \
  --startup-project src/PraeferenzRoO.Api
```
All `Down()` methods throw `NotSupportedException`.

---

## 5. Final Technical Decision

### Architecture
- `ApplicationDbContext` in `Persistence/Context/`
- 10 `IEntityTypeConfiguration<T>` files in `Persistence/Configurations/`
- `AuditableEntityInterceptor` in `Persistence/Interceptors/`
- `IDapperContext` in `Application/Common/Interfaces/`
- `DapperContext` in `Persistence/Context/`
- DI wired in `Persistence/DependencyInjection.cs`
- No new interfaces required in Domain
- `UuidNext` package added to Shared; `UuidGenerator.NewId()` helper created (used by T04 repositories)

### Backend
**EF Core entities and configurations (all in `Persistence/Configurations/`):**
- `CountryConfiguration`
- `HsCodeConfiguration`
- `TradeAgreementConfiguration`
- `ProductRuleConfiguration`
- `MaterialConfiguration`
- `FinishedProductConfiguration`
- `ProductMaterialConfiguration`
- `OriginCalculationConfiguration`
- `OriginCalculationDetailConfiguration`
- `UserConfiguration`

**Each configuration must:**
1. Call `builder.ToTable("{plural_snake}", "public")`
2. Configure PK with `ValueGeneratedNever()`
3. Map all FK relationships with explicit constraint names (`fk_{table}_{referenced_table}`) and `OnDelete(DeleteBehavior.Restrict)`
4. Map all FK columns to indexes (`ix_{table}_{column}`)
5. Call `builder.ConfigureAuditColumns()` (extension method)
6. Apply combined query filter
7. Apply `UseXminAsConcurrencyToken()` where applicable
8. Configure string lengths (`HasMaxLength`) on all `string` properties
9. Add partial index `WHERE is_deleted = FALSE` on columns with high-frequency WHERE predicates

**ApplicationDbContext:**
```csharp
public class ApplicationDbContext : DbContext
{
    private readonly ITenantService _tenantService;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantService tenantService) 
        : base(options) => _tenantService = tenantService;

    public DbSet<Country> Countries => Set<Country>();
    public DbSet<HsCode> HsCodes => Set<HsCode>();
    public DbSet<TradeAgreement> TradeAgreements => Set<TradeAgreement>();
    public DbSet<ProductRule> ProductRules => Set<ProductRule>();
    public DbSet<Material> Materials => Set<Material>();
    public DbSet<FinishedProduct> FinishedProducts => Set<FinishedProduct>();
    public DbSet<ProductMaterial> ProductMaterials => Set<ProductMaterial>();
    public DbSet<OriginCalculation> OriginCalculations => Set<OriginCalculation>();
    public DbSet<OriginCalculationDetail> OriginCalculationDetails => Set<OriginCalculationDetail>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
```

**API design:** None (T03 is persistence-only, no endpoints)

**Database changes:**
- 10 new tables in `public` schema
- All audit columns with explicit `created_at`, `updated_at` etc. column names
- Explicit named constraints on all PKs, FKs, unique constraints, and indexes
- Partial indexes on soft-delete predicates for high-traffic tables

**Dapper queries needed:** None in T03. `IDapperContext` is the interface only; query classes come in T06+.

### Frontend
None. T03 is backend-only.

### Security
- `User.PasswordHash` and `User.RefreshTokenHash` columns: marked in `UserConfiguration` with a comment warning against inclusion in Dapper projections
- `app_user` PostgreSQL role must not have DELETE privilege on any table — this is a database-level control enforced in T23 (seeding + Docker). T03 adds a reminder comment in `DependencyInjection.cs`
- Connection string from `IConfiguration` only, never hardcoded

### Testing
- **Unit tests** (`Persistence.Tests` or `Domain.Tests`):
  - Verify `AuditableEntityInterceptor` sets `CreatedBy`, `CreatedDate` on Added state
  - Verify interceptor sets `UpdatedBy`, `ModifiedDate` on Modified state
  - Verify interceptor does not overwrite `CreatedBy` on update
- **Configuration tests** (optional but recommended):
  - Use EF Core `ModelBuilder` to verify constraint names and index names match standards
- **Migration smoke test**: `dotnet ef migrations script` must produce valid SQL with no auto-generated constraint names

---

## 6. Risks

| Risk | Type | Likelihood | Impact | Mitigation |
|---|---|---|---|---|
| T02 entities not available (PR #30 unmerged) | Timeline | Medium | High | Merge PR #30 before starting T03 implementation; cherry-pick as fallback |
| Combined query filter breaks admin queries needing cross-tenant data | Technical | Low | Medium | Ensure `IgnoreQueryFilters()` is available and documented for admin handlers |
| `AuditableEntityInterceptor` scoped lifetime conflict with DbContext | Technical | Medium | High | Register interceptor as scoped; use `AddDbContext` with factory approach for interceptor resolution |
| Audit column name mismatch (C# `CreatedDate` vs DB `created_at`) | Technical | High | High | Use explicit `.HasColumnName("created_at")` in extension method; covered in code review |
| Migration snapshot drift if T02 entities differ from T03 entity classes | Technical | Low | Medium | Ensure migration is generated after T02 merge, not before |
| UUID v4 used instead of UUID v7 on entity creation | Technical | Medium | Low | `UuidGenerator.NewId()` helper in Shared; enforced in T04 repository implementations |

---

## 7. Deferred Items

| Item | Reason | Suggested Future Issue |
|---|---|---|
| Repository implementations (`ITradeAgreementRepository`, etc.) | Out of T03 scope; depends on DbContext being ready (T03 output) | T04 |
| Database seeding (country list, HS codes) | Separate concern; requires data files | T23 |
| `rule_engine` schema and `RuleDefinition` entity configuration | `RuleDefinition` entity not created in T02; planned for T09/T10 | T09 |
| Read replica / `ReadDbContext` for Dapper | Architecture handbook mentions it but it is not needed until query load justifies it | Post-MVP |
| PostgreSQL Row-Level Security (RLS) policies | Secondary safety net; application-level filters are the primary control for MVP | Post-MVP |
| `xmin` concurrency conflict handling in handlers | Catching `DbUpdateConcurrencyException` is T04 scope (repository implementations) | T04 |
| Rename C# audit properties from `CreatedDate` → `CreatedAt` | Would align C# with DB naming; deferred to avoid touching Domain in T03 | Future refactor |

---

## 8. Action Items

**Praveen**
- Review `ApplicationDbContext` design before implementation starts
- Confirm `ITenantService` constructor injection pattern in DbContext is acceptable (vs ambient context)
- Approve `ConfigureAuditColumns()` extension method approach as the reuse pattern

**Sreejith**
- Implement all 10 `IEntityTypeConfiguration<T>` files
- Implement `ApplicationDbContext` with `ApplyConfigurationsFromAssembly`
- Implement `AuditableEntityInterceptor`
- Implement `IDapperContext` interface in Application; `DapperContext` in Persistence
- Add `UuidNext` to Shared; create `UuidGenerator.NewId()`
- Write `ConfigureAuditColumns()` extension method in Persistence
- Generate `InitialCreate` migration (after T02 merge)
- Write unit tests for `AuditableEntityInterceptor`
- Register all services in `Persistence/DependencyInjection.cs`

**Sojiya**
- Add comment to `UserConfiguration.cs` marking `password_hash` and `refresh_token_hash` as sensitive columns — no Dapper projections should include them

**Vinod**
- Ensure PR #30 (T02) is merged before T03 implementation begins
- Update GitHub issue #3 with acceptance criteria (per this meeting)
- Store this decision summary in `PraeferenzBrain/decisions/T03-ef-core-dbcontext.md`

---

## Decision Summary — T03: EF Core DbContext + Initial Migration

### Agreed Architecture
- `ApplicationDbContext` in `Persistence/Context/` receives `ITenantService` via constructor
- 10 `IEntityTypeConfiguration<T>` files in `Persistence/Configurations/`, one per entity
- `AuditableEntityInterceptor` (scoped `SaveChangesInterceptor`) in `Persistence/Interceptors/`
- `IDapperContext` interface in `Application/Common/Interfaces/`; `DapperContext` implementation in `Persistence/Context/`
- `ConfigureAuditColumns()` EF Core extension method in Persistence to avoid repetition across 10 files
- `UuidGenerator.NewId()` helper in Shared using `UuidNext` (UUID v7)
- All wired in `Persistence/DependencyInjection.cs`

### Agreed Implementation Approach
**Backend:**
- `UseSnakeCaseNamingConvention()` from `EFCore.NamingConventions`; all audit field column names explicitly mapped via `HasColumnName("created_at")` etc.
- Combined global query filter per entity: `!x.IsDeleted && x.TenantId == _tenantService.CurrentTenantId`
- `UseXminAsConcurrencyToken()` on all aggregate roots
- Explicit named constraints on all PKs (`pk_{table}`), FKs (`fk_{table}_{referenced}`), indexes (`ix_{table}_{column(s)}`), unique indexes (`uix_...`), partial indexes (`pix_...`)
- `DOWN()` migration method throws `NotSupportedException`
- Migration generated only after T02 (PR #30) is merged

**Frontend:** None — T03 is persistence-only.

### Accepted Trade-offs
- C# audit property names (`CreatedDate`, `ModifiedDate`, `DeletedDate`) not renamed to match DB convention (`created_at` etc.) to avoid a Domain layer change in this task. Explicit `HasColumnName` in configurations handles the mapping.
- `ReadDbContext` deferred — a single `ApplicationDbContext` is sufficient for MVP.
- PostgreSQL RLS deferred — application-layer query filters are the primary tenant isolation control.

### Risks
- T02 merge dependency is the highest-impact risk — mitigated by merge-first workflow
- Interceptor scoped lifetime — mitigated by registering as scoped with factory resolution
- Column name mismatch — mitigated by `ConfigureAuditColumns()` extension method

### Deferred Items
- Repository implementations → T04
- `RuleDefinition`/`rule_engine` schema → T09
- DB seeding → T23
- C# audit property rename → future refactor
- RLS → post-MVP

### Action Plan
- **Praveen:** Approve architecture before implementation; confirm tenant injection pattern
- **Sreejith:** Full implementation — 10 configurations, DbContext, interceptor, DapperContext, extension method, UuidGenerator, DI registration, InitialCreate migration, unit tests
- **Sojiya:** Add sensitive-column comment to UserConfiguration
- **Vinod:** Merge T02 (PR #30) first; update issue #3 acceptance criteria
