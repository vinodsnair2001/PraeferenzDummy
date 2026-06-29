# Decision Summary — T02 Domain Entities

> **Issue:** [#2 — T02 Domain entities](https://github.com/vinodsnair2001/PraeferenzDummy/issues/2)
> **Branch:** `issue-2-domain-entities`
> **Meeting date:** 2026-06-29
> **Status:** Approved — ready for implementation

---

## Agreed Architecture

All 10 entities live exclusively in `src/PraeferenzRoO.Domain/`. Zero NuGet packages added to the Domain project.

### Class Hierarchy

```
BaseEntity (abstract)
└── AuditableEntity (abstract)  ← all 10 entities inherit this
    └── AggregateRoot (abstract) ← 8 aggregate roots inherit this
```

### Aggregate Roots (will have repositories in T03+)
`User`, `Country`, `TradeAgreement`, `HsCode`, `Material`, `FinishedProduct`, `ProductRule`, `OriginCalculation`

### Child Entities (no own repository — navigated through aggregate root)
`ProductMaterial` (child of `FinishedProduct`), `OriginCalculationDetail` (child of `OriginCalculation`)

---

## Folder Structure

```
src/PraeferenzRoO.Domain/
├── Common/
│   ├── BaseEntity.cs
│   ├── AuditableEntity.cs
│   ├── AggregateRoot.cs
│   └── DomainEvent.cs
├── Entities/
│   ├── User.cs
│   ├── Country.cs
│   ├── TradeAgreement.cs
│   ├── HsCode.cs
│   ├── Material.cs
│   ├── FinishedProduct.cs
│   ├── ProductMaterial.cs
│   ├── ProductRule.cs
│   ├── OriginCalculation.cs
│   └── OriginCalculationDetail.cs
└── Enums/
    ├── UserRole.cs
    ├── RuleType.cs
    ├── RuleCategory.cs
    └── CalculationStatus.cs
```

---

## Base Classes

### BaseEntity
```csharp
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
}
```

### AuditableEntity (authoritative — matches handbook)
```csharp
public abstract class AuditableEntity : BaseEntity
{
    public string CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public DateTime? DeletedDate { get; set; }
    public bool IsDeleted { get; set; }
    public string? IPAddress { get; set; }
    public string? Machine { get; set; }
}
```
> ⚠️ Field names are `CreatedDate` / `ModifiedDate` (NOT `CreatedAt` / `ModifiedAt`). The issue spec was wrong — use handbook names.

### AggregateRoot
```csharp
public abstract class AggregateRoot : AuditableEntity
{
    private readonly List<DomainEvent> _domainEvents = new();
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    protected void AddDomainEvent(DomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}
```

### DomainEvent
```csharp
public abstract record DomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
```

---

## Entity Specifications

All entities have `public Guid TenantId { get; set; }` — mandatory for multi-tenancy.

### User : AggregateRoot
```
Id, TenantId, Username (string), Email (string), PasswordHash (string),
RefreshTokenHash (string?), RefreshTokenExpiryDate (DateTime?),
Role (UserRole), IsActive (bool)
```
> ⚠️ Store `RefreshTokenHash` (BCrypt hash), NEVER the raw token. `PasswordHash` is BCrypt.

### Country : AggregateRoot
```
Id, TenantId, Name (string), IsoCode2 (string, max 2), IsoCode3 (string, max 3),
IsEuMember (bool), IsActive (bool)
```

### TradeAgreement : AggregateRoot
```
Id, TenantId, Name (string), Code (string), Description (string?),
EffectiveDate (DateOnly), ExpiryDate (DateOnly?), IsActive (bool)
```

### HsCode : AggregateRoot
```
Id, TenantId, Code (string), Description (string), Level (int),
ParentCode (string?), IsActive (bool)
Domain method: bool IsValidLevel() => Level is 2 or 4 or 6 or 8 or 10
```

### Material : AggregateRoot
```
Id, TenantId, Name (string), HsCodeValue (string), OriginCountryCode (string),
IsOriginating (bool), UnitCost (decimal), Currency (string, 3-char ISO 4217),
IsActive (bool)
```

### FinishedProduct : AggregateRoot
```
Id, TenantId, Name (string), HsCodeValue (string),
ExWorkPrice (decimal), Currency (string, 3-char ISO 4217), IsActive (bool)
Navigation: private List<ProductMaterial> _materials; public IReadOnlyCollection<ProductMaterial> Materials
Domain method: void AddMaterial(ProductMaterial material)
```

### ProductMaterial : AuditableEntity (child entity)
```
Id, TenantId, FinishedProductId (Guid), MaterialId (Guid),
Quantity (decimal), TotalCost (decimal)
```

### ProductRule : AggregateRoot
```
Id, TenantId, RuleName (string), RuleCode (string),
RuleCategory (RuleCategory), RuleType (RuleType),
Expression (string?), Condition (string?), ParametersJson (string?),
Priority (int), ExecutionOrder (int),
EffectiveDate (DateOnly), ExpiryDate (DateOnly?),
TradeAgreementId (Guid), CountryId (Guid?), HsCodeId (Guid?),
IsEnabled (bool), Version (int)
```
> `ParametersJson`: JSON blob, schema defined at T09/T11.

### OriginCalculation : AggregateRoot
```
Id, TenantId, FinishedProductId (Guid), TradeAgreementId (Guid), CountryId (Guid),
Status (CalculationStatus), IsOriginating (bool?),
DecisionSummary (string?), DecisionTreeJson (string?), CalculatedAt (DateTime?)
Navigation: private List<OriginCalculationDetail> _details; public IReadOnlyCollection<OriginCalculationDetail> Details
Domain method: void AddDetail(OriginCalculationDetail detail)
```
> `DecisionTreeJson`: JSON blob, schema defined at T11.

### OriginCalculationDetail : AuditableEntity (child entity)
```
Id, TenantId, OriginCalculationId (Guid),
RuleName (string), RuleType (RuleType), Passed (bool),
Message (string?), EvidenceJson (string?), ExecutionOrder (int)
```
> `EvidenceJson`: JSON blob, schema defined at T11.

---

## Enums

### UserRole
```csharp
public enum UserRole { Admin = 1, Operator = 2, Viewer = 3 }
```

### RuleType
```csharp
public enum RuleType
{
    TariffShift = 1, ValueAdded = 2, SpecificProcess = 3,
    WhollyObtained = 4, Cumulation = 5, Tolerance = 6, Combined = 7
}
```

### RuleCategory
```csharp
public enum RuleCategory { Mandatory = 1, Alternative = 2, Supplementary = 3 }
```

### CalculationStatus
```csharp
public enum CalculationStatus
{
    Pending = 1, InProgress = 2, Originating = 3, NonOriginating = 4, Error = 5
}
```

---

## Accepted Trade-offs

| Decision | Reason |
|---|---|
| Plain `string Currency` instead of `CurrencyCode` value object | Value objects deferred; 3-char validation via domain method or EF constraint |
| `User` as plain domain entity (no Identity coupling) | T05 handles Identity wiring; T02 gives the domain shape only |
| No concrete domain event records | No handlers exist yet; `DomainEvent` base + `AggregateRoot` infra is sufficient |
| Repository interfaces deferred | T03 is the correct home — interfaces belong alongside EF configurations |
| `int Level` not enum for HsCode | Enum too rigid for 8/10-digit extensions; validated via `IsValidLevel()` |

---

## Verification Gate

```bash
dotnet build PraeferenzRoO.sln   # 0 errors, 0 warnings
dotnet test Tests/               # existing 3 placeholder tests still pass
# Domain project .csproj must have zero <PackageReference> items
```

---

## Deferred Items

| Item | Target Issue |
|---|---|
| `HSCodeValue`, `CountryCode` value objects | T06 or standalone refactor |
| Concrete domain event records | T13 (`OriginCalculationCompletedEvent`) |
| Repository interface definitions | T03 |
| Domain service interfaces (`IRuleEngine`, etc.) | T09 |
| `DecisionTreeJson` / `EvidenceJson` schema | T11 |

---

## Unit Tests to Write (Domain.Tests)

- `HsCode_IsValidLevel_Returns_True_For_2_4_6_8_10`
- `HsCode_IsValidLevel_Returns_False_For_Invalid_Values` (0, 1, 3, 7, 11)
- `AggregateRoot_DomainEvents_Accumulate_And_Clear`
- `FinishedProduct_AddMaterial_Adds_To_Collection`
- `OriginCalculation_AddDetail_Adds_To_Collection`

---

*Decision confirmed by: Praveen (Architect), Sreejith (Senior Dev), Sojiya (UX), Vinod (Planner)*
*Consumed by: backend-developer agent*
