---
name: praeferenz-roo
description: >
  Development guide for PraeferenzRoO — EU Preferential Rules of Origin
  Calculation System. Use when adding features, fixing bugs, or extending
  the rule engine. Covers Clean Architecture rules, CQRS patterns, and
  the metadata-driven rule engine design.
---

## 1. Project Identity

The Preferential Rules of Origin (PRoO) Calculation System automates and enforces origin determination required under EU Free Trade Agreements (FTAs). Before this system, trade compliance officers performed origin calculations manually using spreadsheets and paper-based rule lookups — a process that caused legal exposure from manual errors in origin determination (retroactive duty assessments, fines, and loss of preferential status), operational inefficiency (each HS code lookup against treaty rules could take 30–90 minutes per product line per agreement), and an audit gap that made it impossible to demonstrate compliance to customs authorities during post-clearance audits.

The system is used by customs brokers, manufacturers, and trade compliance officers determining whether manufactured products qualify for preferential tariff rates under an FTA. The central question the system answers is: does this finished product, manufactured using a combination of originating and non-originating materials, qualify as originating in the country of production under the applicable trade agreement?

"Originating" in regulatory terms is determined by three fundamental rules. Wholly Obtained (WO) applies when the product is entirely produced in one country. Sufficient Processing (SP) applies when the product undergoes enough transformation, typically measured by Tariff Shift (Change of Tariff Heading or Subheading), Value Added threshold, or a combination. Minimal Operations are always insufficient regardless of processing level — specific listed operations (e.g., packing, simple assembly) can never confer origin.

The regulatory context spans Protocol 1 of each bilateral FTA (EU–Korea, EU–Canada CETA, EU–Japan EPA), Annex 22-03 of the Union Customs Code Delegated Regulation (UCC DA), and the REX Regulation (EU) 2015/2447 for registered exporters. The system handles the full scope of modern EU trade agreements: bilateral and multilateral agreements, cumulation provisions (bilateral, diagonal, full), tolerance (de minimis) rules, duty drawback restrictions, and proof of origin requirements (EUR.1, EUR-MED, Statement on Origin, REX), including period of validity rules for proofs and verification and administrative cooperation clauses.

---

## 2. Non-Negotiable Architecture Rules

**Dapper for reads, EF Core for writes.** Use Dapper `QueryAsync` / `QuerySingleOrDefaultAsync` for every SELECT. Use EF Core repositories for every INSERT, UPDATE, and soft DELETE. Never call a repository method for a SELECT.

**CQRS is mandatory.** Every feature requires: Command, CommandHandler, CommandValidator, Query, QueryHandler, DTO, and ResponseModel. No combined handlers. No shortcuts.

**No business logic in controllers.** Controllers call MediatR only — `await _mediator.Send(command, cancellationToken)`. No SQL, no validation logic, no EF Core calls in controllers.

**No magic strings — use typed constants.** Every role name, route, cache key, policy name, and SQL identifier must be a named constant. See `Shared/Constants/`.

**Pass CancellationToken everywhere.** Every public async method must accept `CancellationToken cancellationToken` as its last parameter and pass it to all downstream async calls.

**Audit trail on every entity.** Every entity inherits `AuditableEntity`. Fields required: `CreatedBy`, `UpdatedBy`, `DeletedBy`, `CreatedDate`, `ModifiedDate`, `IPAddress`, `Machine`. Never bypass the `AuditableEntityInterceptor`.

**Multi-tenant isolation on every query.** Every Dapper query and EF Core entity must include a `TenantId` filter. Every new entity must have a `TenantId` property. Zero cross-tenant queries permitted.

---

## 3. How to Add a New Backend Feature

**Step 1 — Create the domain entity**

In `src/Domain/Aggregates/[Feature]/`. Inherit from `AggregateRoot`. Private setters. Domain methods on the aggregate (not in handlers). No data annotations.

```csharp
public class MyEntity : AggregateRoot
{
    public string Name { get; private set; }
    public Guid TenantId { get; private set; }
    // domain methods here
}
```

**Step 2 — Define the repository interface**

In `src/Domain/Interfaces/`. Extends `IRepository<T>`. Write-side operations only — no SELECT methods.

```csharp
public interface IMyEntityRepository : IRepository<MyEntity>
{
    // domain-specific write operations only
}
```

**Step 3 — Write the Command + CommandHandler**

In `src/Application/Features/[Feature]/Commands/[VerbNoun]/`. Command is a `record`. Handler uses EF Core via repository.

```csharp
public record CreateMyEntityCommand(...) : IRequest<Result<CreateMyEntityResponse>>;

public class CreateMyEntityCommandHandler : IRequestHandler<CreateMyEntityCommand, Result<CreateMyEntityResponse>>
{
    public async Task<Result<CreateMyEntityResponse>> Handle(
        CreateMyEntityCommand request, CancellationToken cancellationToken) { ... }
}
```

**Step 4 — Write the FluentValidation Validator**

In the same folder as the Command. One `AbstractValidator<T>` per command/query. All required fields and business format rules validated here — not in the handler.

```csharp
public class CreateMyEntityCommandValidator : AbstractValidator<CreateMyEntityCommand>
{
    public CreateMyEntityCommandValidator() { RuleFor(x => x.Name).NotEmpty().MaximumLength(200); }
}
```

**Step 5 — Write the Query + QueryHandler**

In `src/Application/Features/[Feature]/Queries/[GetNoun]/`. QueryHandler uses Dapper via `IDbConnectionFactory`. SQL stored as a constant in `src/Persistence/Queries/`.

```csharp
public record GetMyEntityByIdQuery(Guid Id, Guid TenantId) : IRequest<Result<MyEntityDetailDto>>;

public class GetMyEntityByIdQueryHandler : IRequestHandler<GetMyEntityByIdQuery, Result<MyEntityDetailDto>>
{
    public async Task<Result<MyEntityDetailDto>> Handle(
        GetMyEntityByIdQuery request, CancellationToken cancellationToken)
    {
        using var conn = _connectionFactory.CreateConnection();
        var dto = await conn.QuerySingleOrDefaultAsync<MyEntityDetailDto>(
            MyEntityQueries.GetById, new { request.Id, request.TenantId });
        // ...
    }
}
```

**Step 6 — Write the EF Core configuration**

In `src/Persistence/Configurations/`. Implements `IEntityTypeConfiguration<T>`. No data annotations on domain entities. Snake_case column names.

```csharp
public class MyEntityConfiguration : IEntityTypeConfiguration<MyEntity>
{
    public void Configure(EntityTypeBuilder<MyEntity> builder)
    {
        builder.ToTable("my_entities");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200).HasColumnName("name");
    }
}
```

**Step 7 — Write the API Controller**

In `src/Api/Controllers/`. Inherit `ControllerBase`. One method = one MediatR dispatch. Return `ApiResponse<T>`.

```csharp
[ApiController]
[Route("api/[controller]")]
public class MyEntitiesController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CreateMyEntityResponse>>> Create(
        [FromBody] CreateMyEntityCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<CreateMyEntityResponse>.Ok(result.Value));
    }
}
```

**Step 8 — Write tests**

Unit tests for domain logic and CQRS handlers in `Tests/Domain.Tests/` or `Tests/Application.Tests/`. Integration tests for repository in `Tests/Persistence.Tests/`. Integration tests for the API endpoint in `Tests/Api.Tests/Integration/`. Coverage must exceed 80%.

---

## 4. How to Add a New Rule Type

**Step 1 — Create the evaluator class**

In `src/Infrastructure/RuleEngine/Rules/`. Implement `IRule`. If parameters are needed, also implement `IConfigurableRule`.

```csharp
public sealed class MyNewRule : IRule, IConfigurableRule
{
    private record MyNewRuleParameters(decimal SomeThreshold);
    private MyNewRuleParameters? _parameters;

    public void Configure(JsonDocument parameters)
    {
        _parameters = parameters.Deserialize<MyNewRuleParameters>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException("MyNewRule: parameters could not be deserialised.");
    }

    public Task<RuleResult> ExecuteAsync(RuleContext context)
    {
        if (_parameters is null) throw new InvalidOperationException("Configure() must be called first.");
        var calculatedValue = /* compute metric */;
        var passed = calculatedValue >= _parameters.SomeThreshold;
        return Task.FromResult(new RuleResult
        {
            RuleCode = "MY-NEW-001", RuleName = "My New Rule",
            IsApplicable = true, IsPassed = passed,
            CalculatedValue = calculatedValue, Threshold = _parameters.SomeThreshold,
            Status = passed ? RuleResultStatus.Passed : RuleResultStatus.Failed,
            Reason = passed ? $"Value {calculatedValue:F2} meets threshold." : $"Value {calculatedValue:F2} below threshold."
        });
    }
}
```

**Step 2 — Register in DI via AddKeyedScoped with a string key matching the DB rule_type value**

Open `src/Infrastructure/RuleEngine/ServiceCollectionExtensions.cs`. Add one line inside `AddRuleEvaluators()`. The key string is case-sensitive and must exactly match what will be stored in `rule_engine.rule_definitions.rule_type`.

```csharp
services.AddKeyedScoped<IRule, MyNewRule>("MyNewRule");
//                                        ^^^^^^^^^^ this string must match rule_type in DB
```

**Step 3 — Insert a rule_definitions row**

Write a migration SQL file in `src/Persistence/Migrations/`. The `rule_type` column value must match the DI key exactly — this is the most common cause of `RuleTypeNotFoundException`.

```sql
INSERT INTO rule_engine.rule_definitions
  (rule_name, rule_code, rule_category, rule_type, description, parameters,
   priority, execution_order, is_definitive, effective_date, created_by)
VALUES
  ('My New Rule', 'MY-NEW-001', 'ValueAdded', 'MyNewRule',
   'Description of what treaty article this implements.',
   '{"someThreshold": 35.0}'::jsonb,
   100, 4, FALSE, '2026-07-01', 'developer@company.com');
```

**Step 4 — Configure parameters JSONB**

The `parameters` JSONB must match the parameter record defined in Step 1. Validate parameters using the Rule Builder UI "Test" action before deploying to production. Document the parameter schema in `ESSENTIAL/rule-engine.md` Section 10.

**Step 5 — Write unit and integration tests**

Unit tests in `Tests/Infrastructure.Tests/` or `Tests/Application.Tests/Features/`. Required test methods: pass scenario, fail scenario, boundary case (value exactly at threshold), not-applicable skip scenario, and Configure-not-called throw scenario. Integration test must use TestContainers PostgreSQL, apply migrations, load the rule via the repository, execute the full pipeline, and assert `OriginCalculationResult.IsOriginating` and the specific `RuleResult` for the new rule.

**Step 6 — Update ESSENTIAL/rule-engine.md**

Add a subsection under Section 10 following the existing format (rule title, what it evaluates, parameters table, JSON parameters example, example RuleResult JSON). If a new rule category was introduced, update Section 11. Do not skip this step — undocumented rule types are a compliance risk.

---

## 5. Data Access Cheatsheet

| Operation | Correct Tool | Example |
|---|---|---|
| SELECT single row | Dapper `QuerySingleOrDefaultAsync` | In query handler via `IDbConnection` |
| SELECT list | Dapper `QueryAsync` | With `LIMIT`/`OFFSET` for pagination |
| SELECT count | Dapper `ExecuteScalarAsync<int>` | Separate count query |
| INSERT | EF Core via Repository | `await _repository.AddAsync(entity, ct)` |
| UPDATE | EF Core via Repository | `await _repository.UpdateAsync(entity, ct)` |
| DELETE (soft) | EF Core via Repository | Set `IsDeleted = true` — never physical delete |
| Raw aggregate/report | Dapper with hand-written SQL | In Dapper query class under `Persistence/Queries/` |
| Rule definitions lookup | Dapper (cached) | Via `IMemoryCache` with `CacheKeys.RuleDefinitions(tenantId, agreementId)` |

---

## 6. Frontend Patterns

The frontend follows a strict 3-layer pattern: API module, React Query hook, and Page/form component. Each layer has a single responsibility.

**Layer 1 — API module** (`src/features/[feature]/api/myFeatureApi.ts`)

Wraps Axios calls. Returns typed data. One file per feature.

```typescript
import { apiClient } from '@/lib/axios';
import type { MyEntityDto, CreateMyEntityRequest } from '../types/MyEntity';

export const myFeatureApi = {
    getById: (id: string) => apiClient.get<MyEntityDto>(`/my-entities/${id}`).then(r => r.data),
    create: (data: CreateMyEntityRequest) => apiClient.post<MyEntityDto>('/my-entities', data).then(r => r.data),
};
```

**Layer 2 — React Query hook** (`src/features/[feature]/hooks/useMyEntity.ts`)

Wraps TanStack Query. Exposes typed loading/error/data states. Never use `useEffect` + `useState` for API calls.

```typescript
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { myFeatureApi } from '../api/myFeatureApi';

export const useMyEntity = (id: string) =>
    useQuery({ queryKey: ['my-entities', id], queryFn: () => myFeatureApi.getById(id), enabled: !!id });

export const useCreateMyEntity = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: myFeatureApi.create,
        onSuccess: () => queryClient.invalidateQueries({ queryKey: ['my-entities'] }),
    });
};
```

**Layer 3 — Page/form component** (`src/features/[feature]/components/MyEntityForm.tsx`)

Uses shadcn/ui only. Wires React Hook Form + Zod via `zodResolver`. Named exports only. Handle loading and error states.

```typescript
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { myEntitySchema, type MyEntityFormValues } from '../schemas/myEntitySchema';
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import { useMyEntity } from '../hooks/useMyEntity';

export const MyEntityForm = ({ id }: { id: string }): JSX.Element => {
    const { data, isLoading, error } = useMyEntity(id);
    if (isLoading) return <Skeleton className="h-40 w-full" />;
    if (error) return <p className="text-destructive">Failed to load. Please try again.</p>;

    const form = useForm<MyEntityFormValues>({
        resolver: zodResolver(myEntitySchema),
        defaultValues: { name: data?.name ?? '' },
    });

    const onSubmit = (values: MyEntityFormValues) => { /* mutation.mutate(values) */ };

    return (
        <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
                <FormField control={form.control} name="name" render={({ field }) => (
                    <FormItem>
                        <FormLabel>Name</FormLabel>
                        <FormControl><Input {...field} /></FormControl>
                        <FormMessage />
                    </FormItem>
                )} />
                <Button type="submit">Save</Button>
            </form>
        </Form>
    );
};
```

**Zod schema** (`src/features/[feature]/schemas/myEntitySchema.ts`): one schema per form, co-located in the feature `schemas/` folder, mirrors the FluentValidation rules on the server.

---

## 7. File Location Map

> These paths are design-intent targets. Files are created incrementally as features are implemented. Not all paths exist in the repository at any given time. When creating a new file, use the canonical path for its layer — do not improvise alternate locations.

| Layer | Canonical Path |
|---|---|
| Domain entities | `src/Domain/Aggregates/[Feature]/` |
| Application commands | `src/Application/Features/[Feature]/Commands/` |
| Application queries | `src/Application/Features/[Feature]/Queries/` |
| Application DTOs | `src/Application/Features/[Feature]/DTOs/` |
| EF Core configurations | `src/Persistence/Configurations/` |
| Dapper query constants | `src/Persistence/Queries/` |
| Repositories (write) | `src/Persistence/Repositories/` |
| Rule engine evaluators | `src/Infrastructure/RuleEngine/Rules/` |
| API controllers | `src/Api/Controllers/` |
| Frontend API modules | `src/features/[feature]/api/` |
| Frontend hooks | `src/features/[feature]/hooks/` |
| Frontend components | `src/features/[feature]/components/` |
| Frontend Zod schemas | `src/features/[feature]/schemas/` |
| Unit tests | `Tests/Domain.Tests/` or `Tests/Application.Tests/` |
| Integration tests | `Tests/Api.Tests/Integration/` |
| Rule engine tests | `Tests/Application.Tests/Features/` or `Tests/Infrastructure.Tests/` |

---

## 8. Pre-Completion Checklist

- [ ] All SELECT queries use Dapper only — no EF Core reads
- [ ] All writes use EF Core — no Dapper for writes
- [ ] No business logic in controllers — only MediatR dispatch
- [ ] No magic strings — typed constants used throughout
- [ ] CancellationToken passed to every public async method
- [ ] FluentValidation validator exists for every new Command and Query
- [ ] No `any` in TypeScript — strict mode enabled and passing
- [ ] Zod schema exists for every new form, co-located in the feature `schemas/` folder
- [ ] shadcn/ui components used — no raw HTML interactive elements, no Material UI
- [ ] Unit tests added for new domain logic and CQRS handlers — coverage remains above 80%
- [ ] Integration test added for every new API endpoint
- [ ] No PII or secrets in logs — only user IDs and correlation IDs
- [ ] All new exceptions use the custom exception hierarchy (`NotFoundException`, `BusinessRuleException`, etc.)
- [ ] Folder structure follows the canonical paths in Section 7
- [ ] Commit messages follow Conventional Commits format (`feat`, `fix`, `docs`, `chore`, etc.)
- [ ] Rule engine: new evaluator DI key string matches `rule_type` value in migration SQL — verified by integration test
- [ ] Rule engine: `ESSENTIAL/rule-engine.md` Section 10 updated with new rule type documentation
