# Coding Standards Handbook

**Project:** Preferential Rules of Origin Calculation System (PraeferenzRoO)
**Applies to:** All backend (ASP.NET Core 9 / C#) and frontend (React 19 / TypeScript) code
**Effective:** 2026-06-26
**Status:** Mandatory — no exceptions without explicit written approval

---

## Table of Contents

1. [General Standards](#1-general-standards)
2. [C# / .NET Standards](#2-c--net-standards)
3. [CQRS Standards](#3-cqrs-standards)
4. [MediatR Standards](#4-mediatr-standards)
5. [Dapper Standards](#5-dapper-standards)
6. [EF Core Standards](#6-ef-core-standards)
7. [FluentValidation Standards](#7-fluentvalidation-standards)
8. [AutoMapper Standards](#8-automapper-standards)
9. [Dependency Injection Standards](#9-dependency-injection-standards)
10. [React Standards](#10-react-standards)
11. [TypeScript Standards](#11-typescript-standards)
12. [shadcn/ui Standards](#12-shadcnui-standards)
13. [Tailwind CSS Standards](#13-tailwind-css-standards)
14. [Zod + React Hook Form Standards](#14-zod--react-hook-form-standards)
15. [Logging Standards](#15-logging-standards)
16. [Exception Handling Standards](#16-exception-handling-standards)
17. [Naming Conventions](#17-naming-conventions)
18. [Folder Structure](#18-folder-structure)
19. [Performance Standards](#19-performance-standards)
20. [Async/Await Standards](#20-asyncawait-standards)
21. [API Response Standards](#21-api-response-standards)
22. [Git Standards](#22-git-standards)
23. [Pull Request Standards](#23-pull-request-standards)

---

## 1. General Standards

### 1.1 File Length and Responsibility

- Maximum file length: **400 lines** (excluding generated or migration files)
- One class / one interface / one component per file — no exceptions
- A file that grows beyond 300 lines is a signal that a class has too many responsibilities; refactor before it reaches 400
- Every class, method, and component must have a **single, clearly named responsibility**

### 1.2 Principles

The following principles are **non-negotiable** in every line of code produced for this project:

| Principle | Meaning in this project |
|-----------|------------------------|
| **SOLID** | Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion — enforced through code review |
| **DRY** | Never copy-paste logic; extract shared logic into extension methods, base classes, or shared services |
| **KISS** | Prefer the simplest implementation that correctly solves the problem; complexity must be justified |

### 1.3 Absolute Rules (No Exceptions)

The following rules are absolute and will cause a pull request to be **rejected**:

- **"ALL SELECT queries must use Dapper"** — no EF Core `.ToList()`, `.FirstOrDefault()`, or `.Where()` for reads
- **"ALL Insert/Update/Delete must use Entity Framework Core"** — Repositories ONLY for writes
- **"Never use repositories for SELECT"** — query handlers call Dapper directly via `IDbConnection`
- **"No business logic inside controllers"** — controllers are thin; they dispatch and return
- **"No SQL inside controllers"** — SQL belongs in query handlers or Dapper query classes
- **"No static helper classes"** — use extension methods on interfaces, or injectable services
- **"No magic strings — use constants"** — all literal strings used in code logic must be named constants
- **"Use async/await everywhere; support CancellationToken"** — every public async method must accept `CancellationToken cancellationToken`

### 1.4 Magic Numbers and Magic Strings

**BAD:**
```csharp
if (productCode.Length > 10)
    throw new InvalidOperationException("invalid_product");
```

**GOOD:**
```csharp
public static class ProductConstants
{
    public const int MaxCodeLength = 10;
    public const string InvalidProductMessage = "Product code exceeds maximum allowed length.";
}

if (productCode.Length > ProductConstants.MaxCodeLength)
    throw new InvalidOperationException(ProductConstants.InvalidProductMessage);
```

---

## 2. C# / .NET Standards

### 2.1 Naming

| Construct | Convention | Example |
|-----------|-----------|---------|
| Class | PascalCase | `CreateOriginDeclarationCommand` |
| Interface | `I` prefix + PascalCase | `IOriginCalculationService` |
| Private field | `_camelCase` | `_mediator` |
| Public property | PascalCase | `ProductCode` |
| Method | PascalCase | `CalculatePreferentialOrigin` |
| Parameter | camelCase | `cancellationToken` |
| Local variable | camelCase | `tariffCode` |
| Constant | PascalCase or ALL_CAPS (pick one per project — use PascalCase) | `MaxCodeLength` |
| Enum | PascalCase type, PascalCase members | `OriginStatus.Qualifying` |
| Generic type parameter | `T` or descriptive `TEntity` | `TResult` |

### 2.2 Record vs Class vs Struct

- **Records** — use for immutable DTOs, Commands, Queries, and Responses
- **Classes** — use for domain entities that have identity and mutable state
- **Structs** — use only for small value types representing measurements or coordinates; do not use structs for domain concepts

```csharp
// Commands and Queries are records
public record CreateRulesOfOriginCommand(
    string ProductCode,
    string TariffCode,
    string CountryOfOrigin) : IRequest<CreateRulesOfOriginResponse>;

// Domain entities are classes
public class RulesOfOriginDeclaration
{
    public Guid Id { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    // ... domain methods
}
```

### 2.3 Nullable Reference Types

- Nullable reference types are **enabled project-wide** (`<Nullable>enable</Nullable>`)
- Never suppress with `!` unless you have documented proof the value cannot be null at that point
- Prefer `string?` over `string` for optional values; use guard clauses to exit early

```csharp
// BAD
public string GetCountryName(string? code) => _lookup[code!];

// GOOD
public string GetCountryName(string? code)
{
    if (string.IsNullOrWhiteSpace(code))
        throw new ArgumentNullException(nameof(code));
    return _lookup[code];
}
```

### 2.4 Pattern Matching

Use C# pattern matching where it simplifies branching logic:

```csharp
// GOOD — switch expression
var label = originStatus switch
{
    OriginStatus.Qualifying => "Preferential",
    OriginStatus.NonQualifying => "Non-Preferential",
    OriginStatus.Pending => "Under Review",
    _ => throw new ArgumentOutOfRangeException(nameof(originStatus))
};
```

### 2.5 Expression-Bodied Members

Use expression-bodied members only when the expression fits on **one line** and is immediately readable:

```csharp
// GOOD
public string FullDescription => $"{ProductCode} — {TariffCode}";

// BAD — too complex for expression body; use a block
public string Summary => _items.Where(i => i.IsActive).Select(i => i.Code).First();
```

### 2.6 Async/Await Rules

- Every public async method **must** accept `CancellationToken cancellationToken` as its last parameter
- Never use `async void` — use `async Task` or `async Task<T>`
- Never call `.Result` or `.Wait()` on a Task — use `await`
- Do not use `ConfigureAwait(false)` in the Application layer (it is acceptable only in library code)

**BAD:**
```csharp
public async Task<List<ProductDto>> GetProductsAsync()
{
    return await _dbConnection.QueryAsync<ProductDto>(Sql.Products.GetAll).ConfigureAwait(false);
}
```

**GOOD:**
```csharp
public async Task<List<ProductDto>> GetProductsAsync(CancellationToken cancellationToken)
{
    return (await _dbConnection.QueryAsync<ProductDto>(
        new CommandDefinition(Sql.Products.GetAll, cancellationToken: cancellationToken))).ToList();
}
```

### 2.7 String Handling

- Use `string.IsNullOrWhiteSpace()` for empty checks — never `== ""`
- Use interpolated strings `$""` for human-readable output; use `string.Concat` or `StringBuilder` inside hot loops
- Use verbatim strings `@""` for multi-line SQL in SQL constant classes

---

## 3. CQRS Standards

### 3.1 Principle

Every feature — no matter how small — requires its own set of CQRS artifacts. There is no shortcut.

**Required artifacts per feature:**

| Artifact | Naming | Location |
|----------|--------|----------|
| Command | `VerbNounCommand` | `Application/Features/[Feature]/Commands/` |
| Command Handler | `VerbNounCommandHandler` | `Application/Features/[Feature]/Commands/` |
| Command Validator | `VerbNounCommandValidator` | `Application/Features/[Feature]/Commands/` |
| Query | `GetNounQuery` / `GetNounListQuery` | `Application/Features/[Feature]/Queries/` |
| Query Handler | `GetNounQueryHandler` / `GetNounListQueryHandler` | `Application/Features/[Feature]/Queries/` |
| Response DTO | `VerbNounResponse` / `NounDto` | `Application/Features/[Feature]/Dtos/` |

### 3.2 Command Example

```csharp
// Command — record for immutability
public record CreateOriginDeclarationCommand(
    string ProductCode,
    string TariffCode,
    string CountryOfOrigin,
    decimal ValueAddedPercentage) : IRequest<CreateOriginDeclarationResponse>;

// Handler — EF Core for writes
public sealed class CreateOriginDeclarationCommandHandler
    : IRequestHandler<CreateOriginDeclarationCommand, CreateOriginDeclarationResponse>
{
    private readonly IApplicationDbContext _context;

    public CreateOriginDeclarationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CreateOriginDeclarationResponse> Handle(
        CreateOriginDeclarationCommand request,
        CancellationToken cancellationToken)
    {
        var declaration = new OriginDeclaration
        {
            Id = Guid.NewGuid(),
            ProductCode = request.ProductCode,
            TariffCode = request.TariffCode,
            CountryOfOrigin = request.CountryOfOrigin,
            ValueAddedPercentage = request.ValueAddedPercentage,
            CreatedAt = DateTime.UtcNow
        };

        _context.OriginDeclarations.Add(declaration);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateOriginDeclarationResponse(declaration.Id, declaration.ProductCode);
    }
}
```

### 3.3 Query Example

```csharp
// Query — record
public record GetOriginDeclarationQuery(Guid DeclarationId) : IRequest<OriginDeclarationDto>;

// Handler — Dapper for reads (MANDATORY)
public sealed class GetOriginDeclarationQueryHandler
    : IRequestHandler<GetOriginDeclarationQuery, OriginDeclarationDto>
{
    private readonly IDbConnection _dbConnection;

    public GetOriginDeclarationQueryHandler(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<OriginDeclarationDto> Handle(
        GetOriginDeclarationQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _dbConnection.QuerySingleOrDefaultAsync<OriginDeclarationDto>(
            new CommandDefinition(
                Sql.OriginDeclarations.GetById,
                new { DeclarationId = request.DeclarationId },
                cancellationToken: cancellationToken));

        if (result is null)
            throw new NotFoundException(nameof(OriginDeclaration), request.DeclarationId);

        return result;
    }
}
```

### 3.4 Response DTOs

- DTOs are **records** — they carry data and have no behaviour
- Never expose domain entity classes through the API
- Use `NounDto` suffix for nested objects; use `VerbNounResponse` for top-level command responses

```csharp
public record CreateOriginDeclarationResponse(Guid Id, string ProductCode);

public record OriginDeclarationDto
{
    public Guid Id { get; init; }
    public string ProductCode { get; init; } = string.Empty;
    public string TariffCode { get; init; } = string.Empty;
    public string CountryOfOrigin { get; init; } = string.Empty;
    public decimal ValueAddedPercentage { get; init; }
    public DateTime CreatedAt { get; init; }
}
```

---

## 4. MediatR Standards

### 4.1 Pipeline Behaviors

Cross-cutting concerns are implemented as MediatR `IPipelineBehavior<TRequest, TResponse>`. They must **never** be placed inside handlers.

Required pipeline behaviors (registered in order):

1. `LoggingBehavior<TRequest, TResponse>` — logs request start/end and elapsed time
2. `ValidationBehavior<TRequest, TResponse>` — runs all FluentValidation validators
3. `PerformanceBehavior<TRequest, TResponse>` — warns when a handler exceeds threshold

### 4.2 Good vs Bad: Cross-Cutting Concerns

**BAD — validation inside handler:**
```csharp
public async Task<CreateOriginDeclarationResponse> Handle(
    CreateOriginDeclarationCommand request,
    CancellationToken cancellationToken)
{
    // BAD: manual validation in handler
    if (string.IsNullOrWhiteSpace(request.ProductCode))
        throw new ArgumentException("ProductCode is required");

    if (request.ValueAddedPercentage < 0 || request.ValueAddedPercentage > 100)
        throw new ArgumentException("Invalid percentage");

    // ... actual handler logic
}
```

**GOOD — validation in pipeline behavior:**
```csharp
// Validator (FluentValidation)
public class CreateOriginDeclarationCommandValidator
    : AbstractValidator<CreateOriginDeclarationCommand>
{
    public CreateOriginDeclarationCommandValidator()
    {
        RuleFor(x => x.ProductCode)
            .NotEmpty()
            .MaximumLength(ProductConstants.MaxCodeLength);

        RuleFor(x => x.ValueAddedPercentage)
            .InclusiveBetween(0, 100);
    }
}

// Pipeline behavior runs validator automatically
public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count > 0)
            throw new ValidationException(failures);

        return await next();
    }
}
```

### 4.3 Notification Handlers

Use MediatR notifications for domain events (fire-and-forget side effects):

```csharp
public record OriginDeclarationCreatedNotification(Guid DeclarationId) : INotification;

public class SendOriginDeclarationEmailHandler
    : INotificationHandler<OriginDeclarationCreatedNotification>
{
    private readonly IEmailService _emailService;

    public SendOriginDeclarationEmailHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task Handle(
        OriginDeclarationCreatedNotification notification,
        CancellationToken cancellationToken)
    {
        await _emailService.SendConfirmationAsync(notification.DeclarationId, cancellationToken);
    }
}
```

---

## 5. Dapper Standards

### 5.1 The Cardinal Rule

**"ALL SELECT queries must use Dapper"** — this applies to every read operation without exception.

EF Core's `.ToList()`, `.FirstOrDefault()`, `.Where()`, `.Include()`, and any other read-producing method are **forbidden** in query handlers and read services.

### 5.2 SQL Constants

All SQL must be stored as constants in a static class named `Sql`, organised by entity:

```csharp
public static class Sql
{
    public static class OriginDeclarations
    {
        public const string GetById = @"
            SELECT
                od.id AS Id,
                od.product_code AS ProductCode,
                od.tariff_code AS TariffCode,
                od.country_of_origin AS CountryOfOrigin,
                od.value_added_percentage AS ValueAddedPercentage,
                od.created_at AS CreatedAt
            FROM origin_declarations od
            WHERE od.id = @DeclarationId
              AND od.deleted_at IS NULL";

        public const string GetListPaged = @"
            SELECT
                od.id AS Id,
                od.product_code AS ProductCode,
                od.tariff_code AS TariffCode,
                od.country_of_origin AS CountryOfOrigin
            FROM origin_declarations od
            WHERE od.deleted_at IS NULL
            ORDER BY od.created_at DESC
            LIMIT @PageSize OFFSET @Offset";

        public const string GetCount = @"
            SELECT COUNT(*)
            FROM origin_declarations
            WHERE deleted_at IS NULL";
    }
}
```

### 5.3 Good vs Bad: Dapper vs Repository for SELECT

**BAD — using a repository (or EF Core) for reads:**
```csharp
// BAD: repositories are for writes only
public class GetOriginDeclarationQueryHandler
    : IRequestHandler<GetOriginDeclarationQuery, OriginDeclarationDto>
{
    private readonly IOriginDeclarationRepository _repository;

    public async Task<OriginDeclarationDto> Handle(
        GetOriginDeclarationQuery request,
        CancellationToken cancellationToken)
    {
        // BAD: repository/EF Core used for SELECT
        var entity = await _repository.GetByIdAsync(request.DeclarationId);
        return _mapper.Map<OriginDeclarationDto>(entity);
    }
}
```

**GOOD — Dapper directly in query handler:**
```csharp
public class GetOriginDeclarationQueryHandler
    : IRequestHandler<GetOriginDeclarationQuery, OriginDeclarationDto>
{
    private readonly IDbConnection _dbConnection;

    public GetOriginDeclarationQueryHandler(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<OriginDeclarationDto> Handle(
        GetOriginDeclarationQuery request,
        CancellationToken cancellationToken)
    {
        // GOOD: Dapper with parameterized query and CancellationToken
        var result = await _dbConnection.QuerySingleOrDefaultAsync<OriginDeclarationDto>(
            new CommandDefinition(
                Sql.OriginDeclarations.GetById,
                new { request.DeclarationId },
                cancellationToken: cancellationToken));

        return result ?? throw new NotFoundException(nameof(OriginDeclaration), request.DeclarationId);
    }
}
```

### 5.4 Good vs Bad: Parameterized vs Concatenated SQL

**BAD — SQL injection risk:**
```csharp
// BAD: string concatenation in SQL is forbidden
var sql = "SELECT * FROM products WHERE code = '" + productCode + "'";
var result = await _connection.QueryAsync<ProductDto>(sql);
```

**GOOD — parameterized query:**
```csharp
// GOOD: parameters passed as anonymous object; SQL stored as constant
var result = await _connection.QueryAsync<ProductDto>(
    new CommandDefinition(
        Sql.Products.GetByCode,
        new { ProductCode = productCode },
        cancellationToken: cancellationToken));
```

### 5.5 Paged Queries

All list queries must be paginated. Use `LIMIT` / `OFFSET` with typed parameters:

```csharp
var parameters = new
{
    PageSize = request.PageSize,
    Offset = (request.Page - 1) * request.PageSize
};

var items = await _dbConnection.QueryAsync<ProductDto>(
    new CommandDefinition(Sql.Products.GetListPaged, parameters, cancellationToken: cancellationToken));

var total = await _dbConnection.ExecuteScalarAsync<int>(
    new CommandDefinition(Sql.Products.GetCount, cancellationToken: cancellationToken));
```

---

## 6. EF Core Standards

### 6.1 The Cardinal Rule

**"ALL Insert/Update/Delete must use Entity Framework Core"** — Dapper is forbidden for writes.

Repositories are the write gateway and exist solely to wrap EF Core operations on domain entities.

### 6.2 Fluent Configuration

Domain entity classes must have **zero** data annotations (`[Required]`, `[MaxLength]`, etc.). All mapping is done via fluent API in configuration classes:

```csharp
// BAD — data annotations on domain entity
public class OriginDeclaration
{
    [Required]
    [MaxLength(20)]
    public string ProductCode { get; set; } = string.Empty;
}

// GOOD — fluent configuration in a separate class
public class OriginDeclarationConfiguration
    : IEntityTypeConfiguration<OriginDeclaration>
{
    public void Configure(EntityTypeBuilder<OriginDeclaration> builder)
    {
        builder.ToTable("origin_declarations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductCode)
            .IsRequired()
            .HasMaxLength(ProductConstants.MaxCodeLength)
            .HasColumnName("product_code");

        builder.Property(x => x.TariffCode)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnName("tariff_code");

        // Shadow properties for audit fields
        builder.Property<DateTime>("CreatedAt")
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property<DateTime?>("DeletedAt")
            .HasColumnName("deleted_at");
    }
}
```

### 6.3 Owned Entities and Value Objects

Domain value objects are mapped as owned entities:

```csharp
builder.OwnsOne(x => x.OriginCriteria, oc =>
{
    oc.Property(x => x.MinimumValueAdded)
        .HasColumnName("min_value_added")
        .HasPrecision(5, 2);

    oc.Property(x => x.CriteriaCode)
        .HasColumnName("criteria_code")
        .HasMaxLength(5);
});
```

### 6.4 Audit Fields via Shadow Properties

Audit fields (`created_at`, `updated_at`, `deleted_at`) are managed as shadow properties and set in `SaveChangesAsync` override — never set manually in handlers:

```csharp
public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    foreach (var entry in ChangeTracker.Entries<BaseEntity>())
    {
        switch (entry.State)
        {
            case EntityState.Added:
                entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
                break;
            case EntityState.Modified:
                entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
                break;
        }
    }

    return await base.SaveChangesAsync(cancellationToken);
}
```

---

## 7. FluentValidation Standards

### 7.1 One Validator Per Command/Query

Every Command and Query that carries user input must have exactly one `AbstractValidator<T>`:

```csharp
public class CreateOriginDeclarationCommandValidator
    : AbstractValidator<CreateOriginDeclarationCommand>
{
    public CreateOriginDeclarationCommandValidator()
    {
        RuleFor(x => x.ProductCode)
            .NotEmpty().WithMessage(ValidationMessages.ProductCodeRequired)
            .MaximumLength(ProductConstants.MaxCodeLength)
                .WithMessage(ValidationMessages.ProductCodeTooLong);

        RuleFor(x => x.TariffCode)
            .NotEmpty().WithMessage(ValidationMessages.TariffCodeRequired)
            .Matches(ValidationConstants.TariffCodePattern)
                .WithMessage(ValidationMessages.TariffCodeInvalidFormat);

        RuleFor(x => x.ValueAddedPercentage)
            .InclusiveBetween(0, 100)
                .WithMessage(ValidationMessages.PercentageMustBeBetween0And100);
    }
}
```

### 7.2 Good vs Bad: FluentValidation vs Data Annotations

**BAD — Data Annotations (forbidden for commands/domain):**
```csharp
public class CreateOriginDeclarationRequest
{
    [Required]
    [StringLength(10)]
    public string ProductCode { get; set; } = string.Empty;

    [Range(0, 100)]
    public decimal ValueAddedPercentage { get; set; }
}
```

**GOOD — FluentValidation:**
```csharp
public class CreateOriginDeclarationCommandValidator
    : AbstractValidator<CreateOriginDeclarationCommand>
{
    public CreateOriginDeclarationCommandValidator()
    {
        RuleFor(x => x.ProductCode)
            .NotEmpty()
            .MaximumLength(ProductConstants.MaxCodeLength);

        RuleFor(x => x.ValueAddedPercentage)
            .InclusiveBetween(0, 100);
    }
}
```

### 7.3 Validation Message Constants

All validation messages are stored as constants — never write literal validation strings inline:

```csharp
public static class ValidationMessages
{
    public const string ProductCodeRequired = "Product code is required.";
    public const string ProductCodeTooLong = "Product code must not exceed 10 characters.";
    public const string TariffCodeRequired = "Tariff code is required.";
    public const string TariffCodeInvalidFormat = "Tariff code must be in XXXX.XX format.";
    public const string PercentageMustBeBetween0And100 = "Value added percentage must be between 0 and 100.";
}
```

### 7.4 No Duplication with Frontend

Server-side FluentValidation and frontend Zod schemas validate the **same contract** but must not duplicate code. The FluentValidation rules are the authority; Zod mirrors them for UX purposes only. If a rule changes, both must be updated.

---

## 8. AutoMapper Standards

### 8.1 Profile Classes

Each feature area has its own AutoMapper `Profile` class — never place all mappings in a single global profile:

```csharp
public class OriginDeclarationMappingProfile : Profile
{
    public OriginDeclarationMappingProfile()
    {
        CreateMap<OriginDeclaration, OriginDeclarationDto>();
        CreateMap<CreateOriginDeclarationCommand, OriginDeclaration>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
    }
}
```

### 8.2 No Map Inside Domain

AutoMapper is an **infrastructure/application concern**. Domain entity classes must not reference or use AutoMapper. Map at the boundary between Application layer and response construction.

### 8.3 ProjectTo for Dapper

Since all reads use Dapper (not EF Core's `IQueryable`), `ProjectTo` is not applicable in this project. AutoMapper is used **only** for command-to-entity mapping in write handlers.

---

## 9. Dependency Injection Standards

### 9.1 Lifetime Rules

| Lifetime | Use when | Examples |
|----------|---------|---------|
| **Scoped** | Per HTTP request, involves DbContext | Repositories, DbContext, application services |
| **Transient** | Lightweight, stateless, cheap to create | Validators, small factories |
| **Singleton** | Application-wide state, thread-safe, expensive to create | Configuration readers, in-memory caches, IHttpClientFactory |

Do not inject a Scoped service into a Singleton — this causes a captive dependency and runtime errors.

### 9.2 Good vs Bad: Constructor Injection vs Service Locator

**BAD — Service Locator (forbidden):**
```csharp
public class OriginCalculationService
{
    public async Task<decimal> CalculateAsync(string productCode, CancellationToken cancellationToken)
    {
        // BAD: resolving dependencies at runtime via service locator
        var dbConnection = ServiceLocator.Current.GetInstance<IDbConnection>();
        var logger = ServiceLocator.Current.GetInstance<ILogger<OriginCalculationService>>();
        // ...
    }
}
```

**GOOD — Constructor injection:**
```csharp
public class OriginCalculationService : IOriginCalculationService
{
    private readonly IDbConnection _dbConnection;
    private readonly ILogger<OriginCalculationService> _logger;

    public OriginCalculationService(
        IDbConnection dbConnection,
        ILogger<OriginCalculationService> logger)
    {
        _dbConnection = dbConnection;
        _logger = logger;
    }

    public async Task<decimal> CalculateAsync(string productCode, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Calculating origin for {ProductCode}", productCode);
        // ...
    }
}
```

### 9.3 Extension Methods for Registration

Register services using extension methods, not inside `Program.cs` directly:

```csharp
public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));

        return services;
    }
}
```

---

## 10. React Standards

### 10.1 Functional Components Only

Class components are **forbidden**. Every React component is a functional component using hooks.

```tsx
// BAD — class component
class ProductList extends React.Component<Props, State> {
    render() { return <div />; }
}

// GOOD — functional component
const ProductList = ({ products }: ProductListProps): JSX.Element => {
    return <div>{/* ... */}</div>;
};
```

### 10.2 One Component Per File

- Each file exports exactly one component
- The filename matches the component name exactly: `ProductList.tsx` exports `ProductList`
- Use **named exports**, not default exports

```tsx
// BAD — default export
export default function ProductList() { ... }

// GOOD — named export
export const ProductList = (): JSX.Element => { ... };
```

### 10.3 Custom Hooks

Extract stateful logic into custom hooks prefixed with `use`:

```tsx
// hooks/useOriginDeclaration.ts
export const useOriginDeclaration = (id: string) => {
    return useQuery({
        queryKey: [QueryKeys.OriginDeclaration, id],
        queryFn: () => originDeclarationApi.getById(id),
        enabled: !!id,
    });
};
```

### 10.4 File Organization

```
src/features/origin-declarations/
  components/
    OriginDeclarationForm.tsx
    OriginDeclarationList.tsx
    OriginDeclarationDetail.tsx
  hooks/
    useOriginDeclaration.ts
    useOriginDeclarationList.ts
  schemas/
    originDeclarationSchema.ts
  api/
    originDeclarationApi.ts
  types/
    OriginDeclaration.ts
  index.ts
```

---

## 11. TypeScript Standards

### 11.1 Strict Mode

`tsconfig.json` must include:

```json
{
  "compilerOptions": {
    "strict": true,
    "noImplicitAny": true,
    "strictNullChecks": true,
    "noUnusedLocals": true,
    "noUnusedParameters": true
  }
}
```

### 11.2 No `any`

Using `any` is forbidden. Use `unknown` when the type is genuinely unknown, then narrow with type guards:

```typescript
// BAD
const parseResponse = (data: any) => data.id;

// GOOD
const parseResponse = (data: unknown): string => {
    if (typeof data === 'object' && data !== null && 'id' in data) {
        return String((data as { id: unknown }).id);
    }
    throw new Error('Invalid response shape');
};
```

### 11.3 Interface vs Type

- Use `interface` for object shapes (API responses, component props, domain models)
- Use `type` for unions, intersections, or computed types
- Interfaces are prefixed with `I` only for contracts that represent injectable services; domain models use plain PascalCase

```typescript
// API response type — no I prefix
interface OriginDeclarationDto {
    id: string;
    productCode: string;
    tariffCode: string;
    countryOfOrigin: string;
    valueAddedPercentage: number;
    createdAt: string;
}

// Union type — use type alias
type OriginStatus = 'Qualifying' | 'NonQualifying' | 'Pending';
```

### 11.4 Enums for Fixed Sets

```typescript
// GOOD — use const enum for fixed domain sets
export const enum OriginCriteriaCode {
    WholllyObtained = 'A',
    SufficientProcessing = 'B',
    CumulationApplied = 'C',
}
```

### 11.5 Naming

| Construct | Convention | Example |
|-----------|-----------|---------|
| Component | PascalCase | `OriginDeclarationForm` |
| Interface (domain) | PascalCase | `OriginDeclarationDto` |
| Interface (service) | `I` prefix | `IOriginDeclarationService` |
| Variable | camelCase | `declarationId` |
| Constant | camelCase or SCREAMING_SNAKE | `maxCodeLength` / `MAX_CODE_LENGTH` |
| Function | camelCase | `calculateOrigin` |
| Custom hook | `use` prefix + camelCase | `useOriginDeclarationList` |
| File (component) | PascalCase.tsx | `OriginDeclarationForm.tsx` |
| File (hook/util) | camelCase.ts | `useOriginDeclaration.ts` |

---

## 12. shadcn/ui Standards

### 12.1 shadcn Supersedes All Custom UI

This project uses **shadcn/ui** — Material UI, Ant Design, or any other component library is **forbidden**. Do not import from `@mui`, `antd`, or similar packages.

### 12.2 Use shadcn Primitives

Always reach for the shadcn component before writing custom HTML:

**BAD — raw HTML button:**
```tsx
<button
    className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700"
    onClick={handleSubmit}
>
    Submit Declaration
</button>
```

**GOOD — shadcn Button:**
```tsx
import { Button } from '@/components/ui/button';

<Button variant="default" onClick={handleSubmit}>
    Submit Declaration
</Button>
```

### 12.3 Extend, Don't Override

When a shadcn component needs customisation, extend it via `className` and `cn()` — never modify the generated shadcn source files in `components/ui/`:

```tsx
import { cn } from '@/lib/utils';
import { Button } from '@/components/ui/button';

interface PrimaryActionButtonProps {
    label: string;
    isLoading?: boolean;
    onClick: () => void;
}

export const PrimaryActionButton = ({
    label,
    isLoading = false,
    onClick,
}: PrimaryActionButtonProps): JSX.Element => (
    <Button
        className={cn('w-full', isLoading && 'opacity-50 cursor-not-allowed')}
        disabled={isLoading}
        onClick={onClick}
    >
        {isLoading ? 'Processing...' : label}
    </Button>
);
```

### 12.4 Consistent Imports

Shadcn components are imported from `@/components/ui/[component]`. Never use relative paths to reach UI components from feature folders:

```tsx
// BAD
import { Button } from '../../../components/ui/button';

// GOOD
import { Button } from '@/components/ui/button';
```

---

## 13. Tailwind CSS Standards

### 13.1 Utility Classes Only

- No custom CSS files for component styling — use Tailwind utility classes
- Global styles in `index.css` are limited to CSS variables (shadcn theme tokens) and `@tailwind` directives
- Custom CSS is permitted **only** for animations or browser-specific hacks that Tailwind cannot express

### 13.2 Conditional Classes with `cn()`

Use the `cn()` helper (from `@/lib/utils`) for all conditional or merged class strings — never string concatenation:

```tsx
// BAD
<div className={'base-class ' + (isActive ? 'active' : '') + ' ' + extraClass}>

// GOOD
<div className={cn('base-class', isActive && 'active', extraClass)}>
```

### 13.3 No Magic Numbers in Tailwind

Use design tokens or consistent Tailwind scale values — do not use arbitrary values (e.g., `w-[347px]`) unless absolutely necessary:

```tsx
// BAD
<div className="w-[347px] h-[89px] mt-[13px]">

// GOOD
<div className="w-full max-w-md h-20 mt-3">
```

---

## 14. Zod + React Hook Form Standards

### 14.1 One Zod Schema Per Form Feature

Each form has exactly one Zod schema, co-located in the feature's `schemas/` directory:

```typescript
// features/origin-declarations/schemas/originDeclarationSchema.ts
import { z } from 'zod';

export const originDeclarationSchema = z.object({
    productCode: z
        .string()
        .min(1, 'Product code is required')
        .max(10, 'Product code must not exceed 10 characters'),
    tariffCode: z
        .string()
        .min(1, 'Tariff code is required')
        .regex(/^\d{4}\.\d{2}$/, 'Tariff code must be in XXXX.XX format'),
    countryOfOrigin: z
        .string()
        .min(2, 'Country is required')
        .max(2, 'Use ISO 2-letter country code'),
    valueAddedPercentage: z
        .number()
        .min(0, 'Percentage must be at least 0')
        .max(100, 'Percentage must not exceed 100'),
});

export type OriginDeclarationFormValues = z.infer<typeof originDeclarationSchema>;
```

### 14.2 useForm with zodResolver

**BAD — manual validation in component:**
```tsx
const [errors, setErrors] = useState<Record<string, string>>({});

const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const newErrors: Record<string, string> = {};
    if (!productCode) newErrors.productCode = 'Required';
    if (valueAdded < 0 || valueAdded > 100) newErrors.valueAdded = 'Invalid range';
    setErrors(newErrors);
    if (Object.keys(newErrors).length === 0) {
        // submit
    }
};
```

**GOOD — Zod schema + React Hook Form:**
```tsx
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { originDeclarationSchema, type OriginDeclarationFormValues } from '../schemas/originDeclarationSchema';
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';

export const OriginDeclarationForm = (): JSX.Element => {
    const form = useForm<OriginDeclarationFormValues>({
        resolver: zodResolver(originDeclarationSchema),
        defaultValues: {
            productCode: '',
            tariffCode: '',
            countryOfOrigin: '',
            valueAddedPercentage: 0,
        },
    });

    const onSubmit = (values: OriginDeclarationFormValues) => {
        // values are fully typed and validated
        createDeclarationMutation.mutate(values);
    };

    return (
        <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
                <FormField
                    control={form.control}
                    name="productCode"
                    render={({ field }) => (
                        <FormItem>
                            <FormLabel>Product Code</FormLabel>
                            <FormControl>
                                <Input placeholder="e.g. PROD001" {...field} />
                            </FormControl>
                            <FormMessage />
                        </FormItem>
                    )}
                />
                <Button type="submit">Submit Declaration</Button>
            </form>
        </Form>
    );
};
```

### 14.3 No Validation Duplication

Zod schemas on the frontend mirror FluentValidation rules on the backend. They are **not** shared code — they are independently maintained mirrors. When a rule changes on the server, the corresponding Zod schema must be updated in the same pull request.

---

## 15. Logging Standards

### 15.1 Structured Logging with Serilog

All logging uses Serilog with structured (message template) syntax — never string interpolation in log calls:

```csharp
// BAD — string interpolation loses structure
_logger.LogInformation($"Processing declaration {declarationId} for product {productCode}");

// GOOD — structured logging with named properties
_logger.LogInformation(
    "Processing declaration {DeclarationId} for product {ProductCode}",
    declarationId,
    productCode);
```

### 15.2 Log Levels

| Level | When to use |
|-------|------------|
| `Trace` | Extremely verbose; disabled in production |
| `Debug` | Developer diagnostics; disabled in production |
| `Information` | Normal application flow milestones (request start/end, significant state changes) |
| `Warning` | Recoverable problems, unexpected-but-handled conditions |
| `Error` | Operation failed; requires attention; always include the exception object |
| `Critical` | Application is crashing or in an unrecoverable state |

### 15.3 What NOT to Log

The following data must **never** appear in log output:

- Passwords, API keys, secrets, or tokens
- Full credit card numbers or bank account details
- Personal Identifiable Information (PII): names, email addresses, national IDs
- Raw request/response bodies (log summaries instead)
- SQL query results that may contain sensitive data

### 15.4 Correlation

Every log entry in a request context includes `TraceId` via the Serilog middleware — do not manually add correlation IDs to individual log calls.

---

## 16. Exception Handling Standards

### 16.1 Custom Exception Hierarchy

```csharp
// Base application exception
public abstract class ApplicationException : Exception
{
    protected ApplicationException(string message) : base(message) { }
    protected ApplicationException(string message, Exception innerException)
        : base(message, innerException) { }
}

// Entity not found
public class NotFoundException : ApplicationException
{
    public NotFoundException(string entityName, object key)
        : base($"{entityName} with key '{key}' was not found.") { }
}

// Business rule violation
public class BusinessRuleException : ApplicationException
{
    public BusinessRuleException(string message) : base(message) { }
}

// Forbidden action
public class ForbiddenAccessException : ApplicationException
{
    public ForbiddenAccessException() : base("You do not have permission to perform this action.") { }
}
```

### 16.2 Rules

- **Never swallow exceptions** — do not catch and ignore without re-throwing or logging
- **Always include the inner exception** when wrapping: `new ApplicationException("message", ex)`
- **Never throw `Exception`** directly — always use a typed custom exception
- Global exception handling is done in middleware (`ExceptionHandlingMiddleware`) — do not try/catch in every handler

```csharp
// BAD — swallowing exception
try
{
    await _context.SaveChangesAsync(cancellationToken);
}
catch (Exception)
{
    // silently swallowed — FORBIDDEN
}

// BAD — catching and not including inner exception
catch (DbException ex)
{
    throw new ApplicationException("Database error");  // loses stack trace
}

// GOOD — preserve inner exception
catch (DbException ex)
{
    _logger.LogError(ex, "Failed to save origin declaration {DeclarationId}", declaration.Id);
    throw new ApplicationException("Failed to save origin declaration.", ex);
}
```

---

## 17. Naming Conventions

### 17.1 C# and TypeScript Side-by-Side

| Construct | C# Convention | TypeScript Convention |
|-----------|--------------|----------------------|
| Class / Component | `PascalCase` | `PascalCase` |
| Interface | `IPascalCase` | `PascalCase` (domain) / `IPascalCase` (service) |
| Method / Function | `PascalCase` | `camelCase` |
| Private field | `_camelCase` | — (no class fields; use hooks) |
| Variable | `camelCase` | `camelCase` |
| Constant | `PascalCase` | `SCREAMING_SNAKE_CASE` or `camelCase` |
| Enum | `PascalCase.Member` | `PascalCase.Member` (const enum) |
| File (class) | `ClassName.cs` | `ComponentName.tsx` |
| File (interface) | `IInterfaceName.cs` | — |
| File (hook) | — | `useSomething.ts` |
| Command | `VerbNounCommand` | — |
| Query | `GetNounQuery` / `GetNounListQuery` | — |
| Handler | `VerbNounCommandHandler` / `GetNounQueryHandler` | — |
| Response DTO | `VerbNounResponse` / `NounDto` | `NounDto` (TypeScript interface) |
| Validator | `VerbNounCommandValidator` | Zod schema: `nounSchema` |

### 17.2 Branch Naming

| Prefix | Purpose | Example |
|--------|---------|---------|
| `feat/` | New feature | `feat/origin-declaration-api` |
| `fix/` | Bug fix | `fix/tariff-code-validation` |
| `docs/` | Documentation only | `docs/add-coding-standards` |
| `chore/` | Tooling, dependency updates, refactors | `chore/update-efcore-9` |
| `test/` | Test additions or fixes | `test/origin-calculation-unit-tests` |

Branch names are kebab-case, lowercase, descriptive but concise (maximum 50 characters).

### 17.3 Commit Message Format (Conventional Commits)

```
<type>(<scope>): <short summary>

[optional body]

[optional footer]
```

Types: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`, `perf`

Examples:
```
feat(origin-declarations): add EF Core write handler
fix(validation): correct tariff code regex pattern
docs(standards): add coding standards handbook
chore(deps): upgrade MediatR to 12.3.0
test(origin-calc): add unit tests for qualifying criteria
```

---

## 18. Folder Structure

### 18.1 Backend (Solution Root)

```
PraeferenzRoO.sln
│
├── src/
│   ├── Domain/
│   │   ├── Entities/
│   │   │   ├── OriginDeclaration.cs
│   │   │   └── TariffCode.cs
│   │   ├── ValueObjects/
│   │   │   └── OriginCriteria.cs
│   │   ├── Enums/
│   │   │   └── OriginStatus.cs
│   │   ├── Events/
│   │   │   └── OriginDeclarationCreatedEvent.cs
│   │   └── Exceptions/
│   │       ├── NotFoundException.cs
│   │       └── BusinessRuleException.cs
│   │
│   ├── Application/
│   │   ├── Common/
│   │   │   ├── Behaviours/
│   │   │   │   ├── LoggingBehavior.cs
│   │   │   │   ├── ValidationBehavior.cs
│   │   │   │   └── PerformanceBehavior.cs
│   │   │   ├── Exceptions/
│   │   │   │   └── ValidationException.cs
│   │   │   ├── Interfaces/
│   │   │   │   ├── IApplicationDbContext.cs
│   │   │   │   └── ICurrentUserService.cs
│   │   │   └── Models/
│   │   │       ├── ApiResponse.cs
│   │   │       └── PaginatedResult.cs
│   │   │
│   │   ├── Features/
│   │   │   └── OriginDeclarations/
│   │   │       ├── Commands/
│   │   │       │   ├── CreateOriginDeclaration/
│   │   │       │   │   ├── CreateOriginDeclarationCommand.cs
│   │   │       │   │   ├── CreateOriginDeclarationCommandHandler.cs
│   │   │       │   │   ├── CreateOriginDeclarationCommandValidator.cs
│   │   │       │   │   └── CreateOriginDeclarationResponse.cs
│   │   │       │   └── UpdateOriginDeclaration/
│   │   │       │       ├── UpdateOriginDeclarationCommand.cs
│   │   │       │       ├── UpdateOriginDeclarationCommandHandler.cs
│   │   │       │       └── UpdateOriginDeclarationCommandValidator.cs
│   │   │       ├── Queries/
│   │   │       │   ├── GetOriginDeclaration/
│   │   │       │   │   ├── GetOriginDeclarationQuery.cs
│   │   │       │   │   └── GetOriginDeclarationQueryHandler.cs
│   │   │       │   └── GetOriginDeclarationList/
│   │   │       │       ├── GetOriginDeclarationListQuery.cs
│   │   │       │       └── GetOriginDeclarationListQueryHandler.cs
│   │   │       └── Dtos/
│   │   │           └── OriginDeclarationDto.cs
│   │   │
│   │   └── DependencyInjection.cs
│   │
│   ├── Infrastructure/
│   │   ├── Persistence/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── Configurations/
│   │   │   │   └── OriginDeclarationConfiguration.cs
│   │   │   ├── Repositories/
│   │   │   │   └── OriginDeclarationRepository.cs
│   │   │   └── Migrations/
│   │   ├── Sql/
│   │   │   └── Sql.cs
│   │   ├── Services/
│   │   │   └── CurrentUserService.cs
│   │   └── DependencyInjection.cs
│   │
│   └── WebApi/
│       ├── Controllers/
│       │   └── OriginDeclarationsController.cs
│       ├── Middleware/
│       │   └── ExceptionHandlingMiddleware.cs
│       ├── Extensions/
│       │   └── WebApplicationExtensions.cs
│       └── Program.cs
│
└── tests/
    ├── Domain.UnitTests/
    ├── Application.UnitTests/
    │   └── Features/
    │       └── OriginDeclarations/
    │           ├── Commands/
    │           └── Queries/
    └── Application.IntegrationTests/
```

### 18.2 Frontend (`frontend/src/`)

```
src/
├── app/
│   ├── App.tsx
│   ├── routes.tsx
│   └── queryClient.ts
│
├── components/
│   └── ui/               ← shadcn generated — do not modify
│       ├── button.tsx
│       ├── form.tsx
│       ├── input.tsx
│       └── ...
│
├── features/
│   └── origin-declarations/
│       ├── components/
│       │   ├── OriginDeclarationForm.tsx
│       │   ├── OriginDeclarationList.tsx
│       │   └── OriginDeclarationDetail.tsx
│       ├── hooks/
│       │   ├── useOriginDeclaration.ts
│       │   └── useOriginDeclarationList.ts
│       ├── schemas/
│       │   └── originDeclarationSchema.ts
│       ├── api/
│       │   └── originDeclarationApi.ts
│       ├── types/
│       │   └── OriginDeclaration.ts
│       └── index.ts
│
├── lib/
│   ├── utils.ts          ← cn() and shared utilities
│   ├── axios.ts          ← Axios instance config
│   └── constants.ts      ← App-wide constants
│
├── types/
│   └── api.ts            ← ApiResponse<T>, PaginatedResult<T>
│
└── main.tsx
```

---

## 19. Performance Standards

### 19.1 No N+1 Queries

Never load a collection and then query for each item in a loop. Use SQL JOINs or a single Dapper multi-query:

```csharp
// BAD — N+1
var declarations = await GetAllDeclarationsAsync(cancellationToken);
foreach (var declaration in declarations)
{
    // BAD: one query per item
    declaration.Criteria = await GetCriteriaAsync(declaration.Id, cancellationToken);
}

// GOOD — single JOIN in SQL constant
public const string GetListWithCriteria = @"
    SELECT
        od.id AS Id,
        od.product_code AS ProductCode,
        oc.criteria_code AS CriteriaCode,
        oc.min_value_added AS MinValueAdded
    FROM origin_declarations od
    LEFT JOIN origin_criteria oc ON oc.declaration_id = od.id
    WHERE od.deleted_at IS NULL
    ORDER BY od.created_at DESC
    LIMIT @PageSize OFFSET @Offset";
```

### 19.2 AsNoTracking on EF Core Reads

EF Core is used for writes only. But if any EF Core read is ever necessary (e.g., checking existence before write), always use `AsNoTracking()`:

```csharp
// The ONLY acceptable EF Core read — existence check before write
var exists = await _context.OriginDeclarations
    .AsNoTracking()
    .AnyAsync(x => x.ProductCode == request.ProductCode, cancellationToken);
```

### 19.3 Pagination Required on All List Queries

Every query that returns a list **must** accept and apply page/pageSize parameters. There are no unbounded list queries.

```csharp
public record GetOriginDeclarationListQuery(int Page = 1, int PageSize = 20)
    : IRequest<PaginatedResult<OriginDeclarationDto>>;
```

Default page size is 20. Maximum page size is 100. If `pageSize > 100`, clamp to 100 in the validator.

### 19.4 Frontend Performance

- Use TanStack Query for all server state — never `useEffect` + `useState` for API calls
- Set appropriate `staleTime` to avoid over-fetching
- Use React's `useMemo` / `useCallback` only when a genuine performance issue is measured — do not pre-optimise

---

## 20. Async/Await Standards

### 20.1 Rules

- Every public async method accepts `CancellationToken cancellationToken` as its last parameter
- Pass `cancellationToken` to all downstream async calls — never discard it
- Never use `async void` — the only exception is event handlers in legacy code (not applicable here)
- Never block on async code with `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()`
- Prefer `Task.WhenAll()` for parallel independent async operations

### 20.2 Good vs Bad: CancellationToken

**BAD — no CancellationToken:**
```csharp
public async Task<List<OriginDeclarationDto>> GetAllAsync()
{
    return (await _dbConnection.QueryAsync<OriginDeclarationDto>(
        Sql.OriginDeclarations.GetList)).ToList();
}
```

**GOOD — CancellationToken threaded through:**
```csharp
public async Task<List<OriginDeclarationDto>> GetAllAsync(CancellationToken cancellationToken)
{
    return (await _dbConnection.QueryAsync<OriginDeclarationDto>(
        new CommandDefinition(
            Sql.OriginDeclarations.GetList,
            cancellationToken: cancellationToken))).ToList();
}
```

### 20.3 ConfigureAwait Policy

- **Application layer:** do not use `ConfigureAwait(false)` — the synchronization context matters for MediatR pipelines
- **Infrastructure layer:** `ConfigureAwait(false)` is acceptable in pure infrastructure adapters (database adapters, HTTP clients)
- **Library/NuGet packages:** always use `ConfigureAwait(false)`

---

## 21. API Response Standards

### 21.1 Envelope Pattern

All API responses use a consistent envelope. Never return a naked DTO or a bare list.

**Success (single item):**
```json
{
    "success": true,
    "data": {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "productCode": "PROD001",
        "tariffCode": "6204.62",
        "countryOfOrigin": "DE",
        "valueAddedPercentage": 45.5
    },
    "message": "Origin declaration retrieved successfully.",
    "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01"
}
```

**Success (paginated list):**
```json
{
    "success": true,
    "data": [ ... ],
    "message": "Origin declarations retrieved successfully.",
    "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01",
    "pagination": {
        "page": 1,
        "pageSize": 20,
        "total": 100,
        "totalPages": 5
    }
}
```

**Error:**
```json
{
    "success": false,
    "message": "Validation failed.",
    "errors": [
        { "field": "productCode", "message": "Product code is required." },
        { "field": "tariffCode", "message": "Tariff code must be in XXXX.XX format." }
    ],
    "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01"
}
```

### 21.2 C# Response Models

```csharp
public sealed class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string Message { get; init; } = string.Empty;
    public string TraceId { get; init; } = string.Empty;
    public IEnumerable<ApiError>? Errors { get; init; }
    public PaginationMeta? Pagination { get; init; }

    public static ApiResponse<T> Ok(T data, string message = "Success") =>
        new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> Fail(string message, IEnumerable<ApiError>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors };
}

public sealed record ApiError(string Field, string Message);

public sealed record PaginationMeta(int Page, int PageSize, int Total, int TotalPages);
```

### 21.3 TypeScript API Response Types

```typescript
// src/types/api.ts

export interface ApiResponse<T> {
    success: boolean;
    data?: T;
    message: string;
    traceId: string;
    errors?: ApiError[];
    pagination?: PaginationMeta;
}

export interface ApiError {
    field: string;
    message: string;
}

export interface PaginationMeta {
    page: number;
    pageSize: number;
    total: number;
    totalPages: number;
}

export interface PaginatedResult<T> {
    items: T[];
    pagination: PaginationMeta;
}
```

### 21.4 Controller Pattern

Controllers are thin — they dispatch to MediatR and wrap the response. Zero business logic, zero SQL.

```csharp
// BAD — business logic in controller
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateOriginDeclarationRequest request)
{
    // BAD: validation in controller
    if (string.IsNullOrEmpty(request.ProductCode))
        return BadRequest("Product code required");

    // BAD: EF Core or Dapper called directly in controller
    _context.Declarations.Add(new OriginDeclaration { ... });
    await _context.SaveChangesAsync();

    return Ok(new { id = declaration.Id });
}

// GOOD — thin controller
[HttpPost]
public async Task<ActionResult<ApiResponse<CreateOriginDeclarationResponse>>> Create(
    [FromBody] CreateOriginDeclarationCommand command,
    CancellationToken cancellationToken)
{
    var result = await _mediator.Send(command, cancellationToken);
    return Ok(ApiResponse<CreateOriginDeclarationResponse>.Ok(result, "Declaration created successfully."));
}
```

---

## 22. Git Standards

### 22.1 Branch Naming

| Prefix | Purpose | Example |
|--------|---------|---------|
| `feat/` | New feature or capability | `feat/origin-calculation-engine` |
| `fix/` | Bug fix | `fix/null-reference-in-query-handler` |
| `docs/` | Documentation only | `docs/add-api-response-standards` |
| `chore/` | Tooling, dependencies, CI/CD | `chore/upgrade-net9` |
| `test/` | Test only changes | `test/integration-tests-origin-api` |
| `refactor/` | Code refactoring without feature change | `refactor/extract-validation-behavior` |

Rules:
- Branches are created from `main` (or the designated sprint branch)
- Branch names are all lowercase, hyphen-separated
- Never commit directly to `main` or `develop`

### 22.2 Commit Messages

Follow Conventional Commits specification (https://www.conventionalcommits.org/):

```
<type>(<optional scope>): <imperative summary — max 72 chars>

<optional body — explain WHY not WHAT, wrap at 72 chars>

<optional footer — breaking changes, issue refs>
```

Valid types: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`, `perf`, `ci`

Examples:
```
feat(origin-declarations): add CQRS handler for create declaration
fix(validation): prevent empty tariff code from passing validator
docs(standards): add coding standards handbook
test(origin-calc): add unit tests for WO qualifying criteria
chore(deps): upgrade Dapper to 2.1.35
perf(queries): add index on product_code column
refactor(handlers): extract validation into pipeline behavior
```

Breaking change footer:
```
feat(api)!: change pagination response format

BREAKING CHANGE: `totalCount` renamed to `total` in PaginationMeta.
All clients must update their response parsing.
```

### 22.3 Commit Hygiene

- Each commit represents a single logical change
- Do not mix feature work and formatting changes in the same commit
- Do not commit commented-out code
- Run `dotnet build` and `npm run build` before committing — broken builds are not committed

---

## 23. Pull Request Standards

### 23.1 PR Checklist (Author)

Before requesting review, the author must confirm **all** of the following:

- [ ] All SELECT queries use Dapper only — no EF Core reads
- [ ] All writes use EF Core — no Dapper for writes
- [ ] No business logic in controllers — only MediatR dispatch
- [ ] No magic strings — constants used throughout
- [ ] CancellationToken passed to every async call
- [ ] FluentValidation validator exists for every new Command/Query
- [ ] No `any` in TypeScript — strict mode passes
- [ ] Zod schema exists for every new form
- [ ] shadcn/ui components used — no raw HTML interactive elements
- [ ] Unit tests added for new logic — coverage remains above 80%
- [ ] Integration test added for new API endpoint (if applicable)
- [ ] No PII or secrets in logs
- [ ] All new exceptions use the custom exception hierarchy
- [ ] Folder structure follows the conventions in Section 18
- [ ] Commit messages follow Conventional Commits format
- [ ] PR title is in Conventional Commits format
- [ ] Build passes: `dotnet build` / `npm run build`
- [ ] All existing tests pass: `dotnet test` / `npm test`

### 23.2 PR Description Template

```markdown
## Summary
<!-- One paragraph describing what this PR does and why -->

## Changes
- 
- 
- 

## Testing
<!-- How was this tested? Unit tests? Manual? Integration? -->

## Checklist
- [ ] Dapper for reads, EF Core for writes
- [ ] No magic strings
- [ ] CancellationToken everywhere
- [ ] FluentValidation + Zod schemas in sync
- [ ] Coverage > 80%
- [ ] No PII in logs
```

### 23.3 Review Requirements

- Minimum **1 approving review** from a team member before merge
- PRs that fail CI (build, lint, tests) must not be merged
- The author resolves all reviewer comments — do not dismiss reviews unilaterally
- Squash-merge is preferred to keep `main` history clean (one commit per feature)

### 23.4 Test Coverage

- Unit test coverage must exceed **80%** for the Application layer
- New command handlers, query handlers, and domain logic require accompanying unit tests
- Integration tests are required for new API endpoints
- Tests follow the Arrange-Act-Assert pattern with descriptive names:

```csharp
[Fact]
public async Task Handle_WhenProductCodeIsEmpty_ThrowsValidationException()
{
    // Arrange
    var command = new CreateOriginDeclarationCommand(
        ProductCode: string.Empty,
        TariffCode: "6204.62",
        CountryOfOrigin: "DE",
        ValueAddedPercentage: 45m);

    // Act
    var act = async () => await _handler.Handle(command, CancellationToken.None);

    // Assert
    await act.Should().ThrowAsync<ValidationException>()
        .WithMessage("*ProductCode*");
}
```

---

*End of Coding Standards Handbook — version 1.0, effective 2026-06-26*
*All sections are mandatory. Non-compliance must be resolved before merge.*
