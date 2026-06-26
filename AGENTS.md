# AGENTS.md — AI Agent Contract
## Preferential Rules of Origin Calculation System

This file is the **authoritative rules contract** for ALL AI coding agents working on this repository.
Read it in full before taking any action. Violations will be rejected in code review.

---

## MANDATORY PRE-TASK CHECKLIST

Before writing any code, modifying any file, creating any issue implementation, or responding to
any engineering request, you **MUST** read the following files **in this order**:

1. [`ESSENTIAL/architecture.md`](ESSENTIAL/architecture.md)
2. [`ESSENTIAL/coding-standards.md`](ESSENTIAL/coding-standards.md)
3. [`ESSENTIAL/security.md`](ESSENTIAL/security.md)
4. [`ESSENTIAL/stack.md`](ESSENTIAL/stack.md)
5. [`ESSENTIAL/testing.md`](ESSENTIAL/testing.md)
6. [`ESSENTIAL/database.md`](ESSENTIAL/database.md)
7. [`ESSENTIAL/ui-guidelines.md`](ESSENTIAL/ui-guidelines.md)
8. [`ESSENTIAL/rule-engine.md`](ESSENTIAL/rule-engine.md)

After reading, confirm: **"I have read all ESSENTIAL/ files and will follow the standards."**

Skipping this checklist is a contract violation. No exceptions.

---

## HARD RULES

These 26 rules are non-negotiable. Every rule applies to every task, every PR, every file.

### Architecture & Design

1. **Never violate Clean Architecture layer boundaries** — the Api layer cannot reference
   Infrastructure directly. Dependency direction: Api → Application → Domain;
   Persistence and Infrastructure depend on Application/Domain, never the reverse.

2. **CQRS is mandatory** — every feature must have: `Command`, `CommandHandler`, `Query`,
   `QueryHandler`, `Validator`, `DTO`, `ResponseModel`. No shortcuts. No combined handlers.

3. **Use Dapper for ALL SELECT queries** — this includes: Search Product, Search Rule,
   Search Agreement, Search Country, Search HS Codes, Search Calculations, Dashboard.
   No exceptions. Do not use EF Core for reads.

4. **Use EF Core for ALL writes (Insert, Update, Delete) only** — never use EF Core for
   SELECT queries. EF Core is strictly a write tool in this project.

5. **Repositories are ONLY for write operations** — never call a repository method to
   perform a SELECT. Read-side data access goes through Dapper query classes only.

6. **Never place business logic inside Controllers or Endpoints** — controllers call
   MediatR only. All logic lives in Application layer handlers.

7. **Never write SQL inside Controllers** — SQL belongs in Dapper query constants in the
   Persistence layer.

8. **No static helper classes** — use extension methods on interfaces, or register services
   in the DI container.

9. **No magic strings** — use typed constants. Every string literal that identifies a role,
   claim, policy, route, or configuration key must be a named constant.

### Quality & Testing

10. **Always create unit tests, integration tests, repository tests, calculation engine tests,
    CQRS tests** — code coverage must exceed 80%. No PR is mergeable below this threshold.

11. **Follow SOLID, DRY, KISS** — apply these principles in every implementation decision.
    Prefer simple, maintainable code over clever code.

### Validation

12. **Use FluentValidation for server-side validation; use Zod for frontend validation** —
    never duplicate validation logic between client and server. Each side owns its own
    validation; they may mirror rules but must not share code across the boundary.

### Security

13. **Always use parameterized queries** — never concatenate SQL strings. Every dynamic
    value in a Dapper query must be passed as a parameter.

14. **Never commit secrets, connection strings, or API keys** — use environment variables,
    user secrets (dev), or a secrets manager (production). The `.gitignore` must exclude
    all secret files.

### Async & Performance

15. **Always handle `CancellationToken` in every async public method** — pass the token
    through to all downstream calls including database queries and HTTP requests.

16. **Always paginate list queries** — no unbounded result sets. Every endpoint that returns
    a collection must accept `page` and `pageSize` parameters and return a paged envelope.

### Observability

17. **Use Serilog structured logging** — never use `Console.WriteLine`. Log the following
    categories: Errors, Warnings, Information, Performance, Unhandled Exceptions, Validation
    Errors, Database Errors, Request Execution Time. Sinks: Console + Rolling File.

18. **Every exception log must contain**: Timestamp, User, API endpoint, Stack Trace
    (internal logs only — never exposed to client), Correlation Id, Request Body,
    Response Code.

19. **Never return stack traces to the client** — use Global Exception Middleware. All
    error responses must use the envelope:
    ```json
    { "success": false, "message": "...", "errors": [], "traceId": "" }
    ```

### Access Control & Audit

20. **RBAC roles are `Admin`, `Operator`, `Viewer` only** — no other role may be introduced
    without explicit written approval. Hardcoding any other role is a violation.

21. **Audit trail is mandatory on every entity** — every persisted entity must carry:
    `CreatedBy`, `UpdatedBy`, `DeletedBy`, `CreatedDate`, `ModifiedDate`, `IPAddress`,
    `Machine`. Use a base entity or interceptor to enforce this automatically.

### Rule Engine

22. **The Rule Engine must NOT contain hard-coded business logic** — all rule configuration
    must come from the database. No rule thresholds, percentages, or criteria may be
    embedded in code.

23. **Adding a new rule type requires ONLY**: create an `IRule` implementor, register it in
    DI, and configure it via the Admin UI. No existing code may be modified to accommodate
    a new rule. If you find yourself modifying existing rule-engine files to add a rule,
    stop — the design is wrong.

### Frontend

24. **Frontend must use shadcn/ui** — Material UI is NOT approved for this project. Any
    component library other than shadcn/ui requires explicit written approval before use.

### Code Quality

25. **Generate production-quality code only** — no `TODO` comments, no placeholder
    implementations, no `throw new NotImplementedException()`, no stub methods left
    unfilled. If a feature is out of scope, exclude it entirely rather than leaving
    incomplete code.

26. **Update ESSENTIAL/ documentation when architectural decisions change** — if you make
    a decision that affects architecture, stack, security posture, or the rule engine
    design, you must update the relevant ESSENTIAL/ file in the same PR.

---

## PROHIBITED ACTIONS

The following actions are **strictly forbidden**. Performing any of them will result in
immediate PR rejection.

1. **Do not reference Infrastructure from the Api layer** — no `using` directives or
   constructor injections that cross this boundary.
2. **Do not use EF Core `DbSet` queries for SELECT** — no `.Where()`, `.ToList()`,
   `.FirstOrDefault()` on `DbSet` for read-side operations.
3. **Do not call repository methods for read operations** — repositories exist only
   for `Add`, `Update`, `Remove`, `SaveChanges`.
4. **Do not place any business logic in a Controller method** — the only permitted
   call in a controller action is `_mediator.Send(...)`.
5. **Do not concatenate SQL strings** — parameterized queries only, no string interpolation
   in SQL.
6. **Do not commit any secret, credential, or connection string** — not even for local dev.
7. **Do not use `Console.WriteLine` or `Debug.WriteLine` for logging** — use the injected
   `ILogger<T>` backed by Serilog.
8. **Do not expose exception details or stack traces to API consumers** — the Global
   Exception Middleware is the only permitted error-handling path.
9. **Do not introduce roles outside Admin / Operator / Viewer** — without written approval.
10. **Do not create an entity without an audit trail** — `CreatedBy`, `UpdatedBy`,
    `DeletedBy`, `CreatedDate`, `ModifiedDate`, `IPAddress`, `Machine` are mandatory.
11. **Do not hard-code rule thresholds, percentages, or criteria in code** — all rule
    logic must be database-driven.
12. **Do not use Material UI or any unapproved component library** — shadcn/ui only.
13. **Do not leave `TODO`, placeholder, or stub code** — production-quality only.
14. **Do not skip writing tests** — unit, integration, repository, CQRS, and rule engine
    tests are required; coverage must exceed 80%.
15. **Do not modify existing rule-engine handler files to add a new rule type** — new
    rules extend the system via `IRule` + DI registration + UI configuration only.
16. **Do not return unbounded list results** — every collection endpoint must paginate.
17. **Do not omit `CancellationToken` from async public methods** — pass it everywhere.
18. **Do not use static helper classes** — use extension methods or DI-registered services.
19. **Do not use magic strings** — all identifiers must be named constants.

---

## DECISION MAKING

When you are unsure about any implementation detail, architectural choice, rule interpretation,
or scope boundary:

**Stop. Ask. Do not guess.**

Implement only what is explicitly specified. If the specification is ambiguous, surface
the ambiguity and request clarification before writing code. An incorrect implementation
is worse than a delayed one.

Specifically, ask before:
- Introducing a new dependency or package not listed in `ESSENTIAL/stack.md`
- Deviating from any of the 26 Hard Rules above, for any reason
- Adding a role, claim, or permission not defined in the RBAC model
- Making an architectural decision that would require updating ESSENTIAL/ documentation
- Implementing a feature whose scope was not clearly defined in the issue

---

## SEE ALSO

- [`CLAUDE.md`](CLAUDE.md) — Claude Code-specific instructions
- [`ESSENTIAL/architecture.md`](ESSENTIAL/architecture.md) — System architecture and layer boundaries
- [`ESSENTIAL/rule-engine.md`](ESSENTIAL/rule-engine.md) — Rule engine design and extension model
- [`ESSENTIAL/stack.md`](ESSENTIAL/stack.md) — Approved technology stack
- [`ESSENTIAL/security.md`](ESSENTIAL/security.md) — Security requirements
