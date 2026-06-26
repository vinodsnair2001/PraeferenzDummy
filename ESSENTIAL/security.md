# Security Handbook — Preferential Rules of Origin Calculation System

> Version: 1.0 | Last Updated: 2026-06-26 | Classification: Internal Engineering Reference — Confidential

---

## Table of Contents

1. [OWASP Top 10 Mitigations](#1-owasp-top-10-mitigations)
2. [Authentication — JWT](#2-authentication--jwt)
3. [Refresh Token Strategy](#3-refresh-token-strategy)
4. [Password Policies](#4-password-policies)
5. [RBAC — Role Definitions and Permission Matrix](#5-rbac--role-definitions-and-permission-matrix)
6. [Claims-Based Authorization](#6-claims-based-authorization)
7. [Rate Limiting](#7-rate-limiting)
8. [HTTPS and HSTS](#8-https-and-hsts)
9. [Security Headers](#9-security-headers)
10. [Input Validation](#10-input-validation)
11. [Output Encoding](#11-output-encoding)
12. [SQL Injection Prevention](#12-sql-injection-prevention)
13. [XSS Prevention](#13-xss-prevention)
14. [CSRF Protection](#14-csrf-protection)
15. [SSRF Prevention](#15-ssrf-prevention)
16. [Secrets Management](#16-secrets-management)
17. [Logging Security](#17-logging-security)
18. [Audit Logging](#18-audit-logging)
19. [File Upload Security](#19-file-upload-security)
20. [Encryption](#20-encryption)
21. [PII Protection](#21-pii-protection)
22. [GDPR Considerations](#22-gdpr-considerations)
23. [Backup Strategy](#23-backup-strategy)
24. [Container Security](#24-container-security)
25. [API Security](#25-api-security)
26. [Pre-Deployment Security Checklist](#26-pre-deployment-security-checklist)

---

## 1. OWASP Top 10 Mitigations

This section maps each OWASP Top 10 (2021) risk to its specific countermeasure within the PRoO system stack (ASP.NET Core 9, PostgreSQL, React 19, Dapper, EF Core).

### A01 — Broken Access Control

**Risk:** Users access trade agreement data, origin calculation results, or rule engine configuration belonging to other tenants or beyond their role.

**Mitigations:**
- RBAC enforced at the API layer via ASP.NET Core policy-based authorization (`[Authorize(Policy = "RequireOperator")]`).
- Multi-tenant isolation enforced at the **data access layer**: every Dapper query and EF Core query includes a `WHERE tenant_id = @TenantId` clause injected by a tenant-scoped repository base class. Direct table access without a tenant filter is a code review failure.
- Authorization checks occur server-side on every request; client-side UI visibility changes are cosmetic only and never the security gate.
- Resource-based authorization (e.g., "can this user modify this specific trade agreement") implemented via `IAuthorizationHandler` when ownership rules exceed role-level checks.

### A02 — Cryptographic Failures

**Risk:** Sensitive data (PII, JWT signing keys, database credentials) transmitted or stored without adequate encryption.

**Mitigations:**
- TLS 1.2+ enforced for all HTTP traffic in production; plaintext HTTP connections are rejected or redirected.
- JWT tokens signed with RS256 (asymmetric); the private key never leaves the server and is stored in Azure Key Vault in cloud deployments.
- Passwords stored using Argon2id (via ASP.NET Core Identity with a custom password hasher). MD5 and SHA-1 are prohibited.
- PII columns (user email, company VAT number) encrypted at rest using PostgreSQL's `pgcrypto` extension with AES-256.
- Database backups encrypted with AES-256 before being written to storage.

### A03 — Injection

**Risk:** Malicious SQL, command, or expression injection through HS code lookups, product description fields, or rule configuration inputs.

**Mitigations:**
- All Dapper queries use parameterized form exclusively. String-concatenated SQL is prohibited (see Section 12).
- EF Core parameterizes all LINQ queries automatically; raw SQL via `FromSqlRaw` is forbidden without a security review exception.
- FluentValidation rejects inputs that do not conform to expected formats before they reach the data layer.
- OS-level command execution is not used anywhere in the application.

### A04 — Insecure Design

**Risk:** Architectural shortcuts that cannot be patched at runtime (e.g., tenant data sharing by design, flat permission model).

**Mitigations:**
- Tenant isolation is a first-class architectural concern, not a retrofit filter. The `ITenantContext` abstraction is injected into every repository.
- The rule engine uses a domain-model representation of origin rules rather than evaluating arbitrary user-supplied expressions.
- Security requirements are reviewed at the design phase of every feature using the STRIDE threat model before implementation begins.

### A05 — Security Misconfiguration

**Risk:** Debug pages exposed in production, default credentials active, overly permissive CORS, unnecessary ports open.

**Mitigations:**
- Developer exception page is enabled only when `ASPNETCORE_ENVIRONMENT=Development`. Structured JSON error envelopes are returned in all other environments.
- CORS policy explicitly lists allowed origins; wildcard `*` is forbidden in production configuration.
- All default framework endpoints (`/swagger`, `/health`) are gated behind environment checks or authentication in production.
- Docker images run as a non-root user; no debug tools included in production images.

### A06 — Vulnerable and Outdated Components

**Risk:** NuGet or npm packages with known CVEs in the dependency tree.

**Mitigations:**
- `dotnet list package --vulnerable` runs in CI on every pull request.
- `npm audit` runs in CI on every pull request; builds fail on high/critical findings.
- Dependabot (GitHub) is configured for both NuGet and npm ecosystems.
- Base Docker images are pinned to specific digest hashes and updated on a scheduled pipeline.

### A07 — Identification and Authentication Failures

**Risk:** Brute-force attacks on login, weak token handling, session fixation.

**Mitigations:**
- Rate limiting on `/api/auth/login` and `/api/auth/refresh` endpoints (see Section 7).
- Account lockout after 5 failed login attempts within 10 minutes; unlock via email link only.
- JWT access tokens have a 15-minute lifetime. Refresh tokens rotate on use and expire after 7 days of inactivity.
- Refresh tokens stored in HttpOnly, Secure, SameSite=Strict cookies to prevent JavaScript access.

### A08 — Software and Data Integrity Failures

**Risk:** Tampered NuGet packages, pipeline code injection, unsigned container images.

**Mitigations:**
- NuGet package lock files (`packages.lock.json`) committed to source control; CI verifies lock file integrity.
- GitHub Actions workflows pin third-party actions to full commit SHAs, not mutable tags.
- Container images are signed using Docker Content Trust (Notary) before deployment.

### A09 — Security Logging and Monitoring Failures

**Risk:** Attacks proceed undetected because audit events are missing or unstructured.

**Mitigations:**
- Every API exception is logged with: Timestamp, User, API endpoint, Stack Trace (internal log only), Correlation Id, Request Body (sanitized), Response Code.
- All write operations produce audit log entries (see Section 18).
- Structured JSON logs shipped to a centralized SIEM (Seq in development, Azure Monitor / Elastic in production).
- Alerting rules fire on repeated 401/403 responses from the same IP within a time window.

### A10 — Server-Side Request Forgery (SSRF)

**Risk:** An attacker manipulates a URL parameter to cause the server to make requests to internal services or cloud metadata endpoints.

**Mitigations:**
- Outbound HTTP calls restricted to an explicit allowlist of external endpoints (EU Customs tariff APIs, external trade databases).
- The `HttpClient` factory is pre-configured with base addresses; free-form URL construction from user input is prohibited.
- Cloud metadata endpoints (e.g., `http://169.254.169.254`) are blocked at the network egress level via firewall rules (see Section 15).

---

## 2. Authentication — JWT

### Token Structure

JWT tokens follow the standard three-part structure: `header.payload.signature`

**Header:**
```json
{
  "alg": "RS256",
  "typ": "JWT",
  "kid": "<key-id-from-jwks>"
}
```

**Payload (claims):**
```json
{
  "sub": "<user-uuid>",
  "name": "Jane Smith",
  "email": "jane.smith@tenant.com",
  "role": "Operator",
  "tenant_id": "<tenant-uuid>",
  "permissions": ["trade_agreements:read", "trade_agreements:write"],
  "iat": 1750000000,
  "exp": 1750000900,
  "jti": "<unique-token-id>"
}
```

**Signature:** RS256 — RSA-SHA256 using a 2048-bit private key. The public key is published at `/.well-known/jwks.json` for validation by downstream services.

### Signing Algorithm

RS256 (asymmetric) is mandatory. HS256 (symmetric shared secret) is **prohibited** because it requires sharing the signing key with every service that validates tokens, creating a secret-sprawl risk. With RS256, any service can validate tokens using only the public key.

### Token Lifetime Policy

| Token Type     | Lifetime       | Storage              |
|----------------|----------------|----------------------|
| Access Token   | 15 minutes     | Memory (React state) |
| Refresh Token  | 7 days (sliding) | HttpOnly cookie    |
| Email verification token | 24 hours | One-time URL     |
| Password reset token | 1 hour | One-time URL      |

Access tokens are **never** stored in `localStorage` or `sessionStorage` to prevent XSS-based token theft.

### Token Validation Checklist (Server-Side)

Every inbound token must pass all of the following checks in `JwtBearerOptions`:

1. Signature verified against the current public key (or key identified by `kid` from JWKS).
2. `exp` claim is in the future (clock skew tolerance: 30 seconds).
3. `iss` claim matches the configured issuer.
4. `aud` claim matches the API's registered audience.
5. `tenant_id` claim matches the tenant derived from the request host or header.
6. `jti` is not present in the token revocation list (checked against Redis cache).

---

## 3. Refresh Token Strategy

### Rotation Policy

Every call to `/api/auth/refresh` **rotates** the refresh token: the old token is immediately invalidated and a new token is issued. This limits the blast radius of a stolen refresh token to the window between theft and the next legitimate use.

```
Client → POST /api/auth/refresh (sends refresh cookie)
Server → validates token, checks revocation list
Server → issues new access token + new refresh token
Server → marks old refresh token as revoked in DB
Client → stores new access token in memory
Browser → receives new refresh token cookie (HttpOnly, Secure, SameSite=Strict)
```

### Revocation List

Revoked `jti` values (JWT ID) are stored in a PostgreSQL `revoked_tokens` table and cached in Redis with a TTL equal to the token's remaining lifetime. On validation, the cache is checked first; the database is the authoritative fallback.

Tokens are revoked on:
- User logout (client calls `/api/auth/logout`).
- Password change (all existing tokens for the user are revoked).
- Admin-initiated user suspension.
- Detection of concurrent use of the same refresh token (replay attack indicator — all sessions for that user are terminated).

### Cookie Configuration

```csharp
options.Cookie.HttpOnly = true;
options.Cookie.Secure = true;
options.Cookie.SameSite = SameSiteMode.Strict;
options.Cookie.Path = "/api/auth/refresh";  // scope cookie to refresh endpoint only
options.Cookie.MaxAge = TimeSpan.FromDays(7);
```

---

## 4. Password Policies

### Minimum Requirements

| Rule | Value |
|------|-------|
| Minimum length | 12 characters |
| Uppercase letters | At least 1 |
| Lowercase letters | At least 1 |
| Digits | At least 1 |
| Special characters (`!@#$%^&*`) | At least 1 |
| Maximum length | 128 characters |
| Common password check | Blocked (top 10,000 list) |
| Username in password | Blocked |

### Hashing Algorithm

**Argon2id** is the required password hashing algorithm. Parameters:

```
Memory cost: 65536 KB (64 MB)
Time cost (iterations): 3
Parallelism: 4
Hash length: 32 bytes
Salt: 16 bytes (cryptographically random, unique per password)
```

**Prohibited algorithms:** MD5, SHA-1, SHA-256 (without salt/iterations), bcrypt below cost factor 10. These are never acceptable for password storage regardless of whether they are combined with a salt.

### ASP.NET Core Identity Integration

```csharp
services.Configure<PasswordHasherOptions>(options =>
{
    options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3;
    // Custom Argon2id hasher replaces the default PBKDF2 hasher
});
services.AddSingleton<IPasswordHasher<ApplicationUser>, Argon2IdPasswordHasher>();
```

### Password Reset Flow

1. User requests reset via email; a time-limited, single-use token (HMAC-SHA256, 1-hour expiry) is emailed.
2. Token is stored as a hash (not plaintext) in the database.
3. On submission, token is validated, then immediately marked as used.
4. All existing refresh tokens for the user are revoked after a successful reset.
5. User is notified by email that the password was changed (even if they did not initiate the reset — security alert).

---

## 5. RBAC — Role Definitions and Permission Matrix

### Role Definitions

The system defines exactly three roles. No additional roles exist.

**Admin**
- Full access to all features including system configuration, user management, and rule engine configuration.
- Intended for system administrators and trade compliance managers responsible for the platform.
- Can create, modify, and delete any resource in any tenant they administer.

**Operator**
- Day-to-day operational access: managing trade agreements, product rules, materials, and running origin calculations.
- Cannot access system-level settings, rule engine configuration, or user management.
- Intended for trade compliance analysts and operators who work within an established rule framework.

**Viewer**
- Read-only access to trade agreements, countries, HS codes, origin calculation results, and reports.
- Cannot create, modify, or delete any resource.
- Intended for auditors, management stakeholders, and external reviewers who need visibility without write access.

### Permission Matrix

| Feature | Admin | Operator | Viewer |
|---------|-------|----------|--------|
| Trade Agreements - View | ✅ | ✅ | ✅ |
| Trade Agreements - Create/Edit | ✅ | ✅ | ❌ |
| Trade Agreements - Delete | ✅ | ❌ | ❌ |
| Countries - View | ✅ | ✅ | ✅ |
| HS Codes - View | ✅ | ✅ | ✅ |
| Product Rules - Configure | ✅ | ✅ | ❌ |
| Materials - Manage | ✅ | ✅ | ❌ |
| Origin Calculation - Run | ✅ | ✅ | ✅ |
| Rule Engine - Configure | ✅ | ❌ | ❌ |
| User Management | ✅ | ❌ | ❌ |
| Reports - View | ✅ | ✅ | ✅ |
| System Settings | ✅ | ❌ | ❌ |

### Role Assignment Rules

- A user belongs to exactly one role per tenant. Cross-tenant access requires separate role assignments per tenant.
- Role assignments are recorded in the audit log with the assigning admin's identity.
- Privilege escalation (assigning a role equal to or higher than one's own) is prohibited even for Admins; a superadmin flag exists at the platform level for this purpose.

---

## 6. Claims-Based Authorization

### Claim Types

All claims are emitted by the token service at login and embedded in the JWT payload:

| Claim Type | Description | Example Value |
|------------|-------------|---------------|
| `sub` | User unique identifier (UUID) | `"a1b2c3d4-..."` |
| `role` | Single assigned role | `"Operator"` |
| `tenant_id` | Tenant UUID | `"t9e8f7..."` |
| `permissions` | Fine-grained permission list | `["trade_agreements:read", "rules:write"]` |
| `email` | User email (for display; never used for identity lookups) | `"user@corp.com"` |
| `jti` | Token unique ID (for revocation) | `"tok_xyz123"` |

### Policy Definitions

Policies are registered in `Program.cs`:

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("RequireOperator", policy =>
        policy.RequireRole("Admin", "Operator"));

    options.AddPolicy("RequireViewer", policy =>
        policy.RequireRole("Admin", "Operator", "Viewer"));

    options.AddPolicy("CanConfigureRuleEngine", policy =>
        policy.RequireClaim("permissions", "rules:configure"));

    options.AddPolicy("CanManageUsers", policy =>
        policy.RequireRole("Admin")
              .RequireClaim("permissions", "users:manage"));
});
```

### IAuthorizationRequirement for Resource-Based Checks

When authorization depends on the resource being accessed (e.g., a user can only edit a trade agreement belonging to their tenant), implement `IAuthorizationRequirement`:

```csharp
public class TradeAgreementOwnerRequirement : IAuthorizationRequirement { }

public class TradeAgreementAuthorizationHandler
    : AuthorizationHandler<TradeAgreementOwnerRequirement, TradeAgreement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TradeAgreementOwnerRequirement requirement,
        TradeAgreement resource)
    {
        var tenantId = context.User.FindFirstValue("tenant_id");
        if (resource.TenantId.ToString() == tenantId)
            context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
```

Resource-based authorization handlers are registered as scoped services and invoked explicitly in controllers after loading the resource.

---

## 7. Rate Limiting

ASP.NET Core 9's built-in `RateLimiter` middleware is used. No third-party rate-limiting library is needed.

### Global Configuration

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.Headers["Retry-After"] = "60";
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            success = false,
            message = "Too many requests. Please try again later.",
            errors = Array.Empty<string>(),
            traceId = context.HttpContext.TraceIdentifier
        }, token);
    };
});
```

### Per-Endpoint Limits

| Endpoint | Window | Limit | Burst | Notes |
|----------|--------|-------|-------|-------|
| `POST /api/auth/login` | 1 minute | 5 requests | 2 | Per IP address |
| `POST /api/auth/refresh` | 1 minute | 10 requests | 5 | Per user identity |
| `POST /api/auth/forgot-password` | 1 hour | 3 requests | 1 | Per email address |
| `POST /api/origin-calculation/run` | 1 minute | 20 requests | 5 | Per tenant |
| All other API endpoints | 1 minute | 100 requests | 20 | Per authenticated user |

### Applying Rate Limiter Policies

```csharp
app.MapPost("/api/auth/login", LoginHandler)
   .RequireRateLimiting("login-policy");

app.MapPost("/api/origin-calculation/run", CalculateHandler)
   .RequireRateLimiting("calculation-policy")
   .RequireAuthorization("RequireViewer");
```

### Rate Limit Key Selection

- **Unauthenticated endpoints** (login, forgot-password): keyed by remote IP address (with consideration for X-Forwarded-For in proxy deployments — the proxy must be in a trusted network for this header to be trusted).
- **Authenticated endpoints**: keyed by `sub` claim (user UUID) to prevent circumvention by changing IP.
- **Tenant-scoped operations**: keyed by `tenant_id` claim.

---

## 8. HTTPS and HSTS

### Enforcement in Production

```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
app.UseHttpsRedirection();
```

`UseHttpsRedirection` issues HTTP 301 (permanent) redirects from HTTP to HTTPS. In containerized deployments behind a load balancer, HTTPS termination occurs at the load balancer; the container receives HTTP internally. In this topology:
- `UseHttpsRedirection` is disabled on the container (the load balancer handles it).
- The `X-Forwarded-Proto` header is trusted from the load balancer's IP range only.
- HSTS is set on the load balancer's HTTPS response.

### HSTS Configuration

```csharp
builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
    options.Preload = true;
});
```

The domain is submitted to the [HSTS Preload List](https://hstspreload.org) before production launch to ensure browsers never make plaintext requests even on first visit.

### Certificate Management

- **Production (cloud):** Azure-managed certificates via App Service or Azure Front Door. Auto-renewal is automatic.
- **Production (on-premise):** Certificates issued by the organization's internal CA or Let's Encrypt (automated renewal via Certbot/ACME protocol). Minimum RSA 2048-bit or ECDSA 256-bit.
- **Development:** ASP.NET Core development certificate (`dotnet dev-certs https --trust`). Never use a development certificate in staging or production.
- TLS versions: TLS 1.2 and TLS 1.3 are accepted. TLS 1.0 and TLS 1.1 are disabled at the server/load balancer level.

---

## 9. Security Headers

All security headers are set by a custom middleware registered early in the pipeline:

```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "script-src 'self'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data:; " +
        "font-src 'self'; " +
        "connect-src 'self'; " +
        "frame-ancestors 'none'; " +
        "form-action 'self'; " +
        "base-uri 'self'; " +
        "upgrade-insecure-requests;";

    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] =
        "camera=(), microphone=(), geolocation=(), payment=()";
    context.Response.Headers["X-XSS-Protection"] = "0";  // Disabled; rely on CSP instead
    context.Response.Headers.Remove("Server");
    context.Response.Headers.Remove("X-Powered-By");

    await next();
});
```

### Header Reference

| Header | Value | Purpose |
|--------|-------|---------|
| `Content-Security-Policy` | See above | Restricts resource loading to prevent XSS and data injection |
| `X-Frame-Options` | `DENY` | Prevents clickjacking by disallowing any framing |
| `X-Content-Type-Options` | `nosniff` | Prevents MIME-type sniffing |
| `Referrer-Policy` | `strict-origin-when-cross-origin` | Limits referrer leakage to same-origin requests |
| `Permissions-Policy` | `camera=(), microphone=(),...` | Disables unused browser features |
| `Server` | (removed) | Eliminates server fingerprinting |

### CSP Notes

- `unsafe-inline` for styles is a temporary allowance for component libraries; a nonce-based approach is the target once all inline styles are audited.
- `script-src 'self'` prohibits inline scripts and `eval()`. React's production build is fully compatible with this restriction.
- `connect-src 'self'` restricts AJAX/fetch calls to the same origin; external API calls must be proxied through the backend.

---

## 10. Input Validation

### Principle: Never Trust Client Input

All input validation occurs server-side. Client-side validation (React form validators, HTML5 attributes) is a UX convenience only and is never the security gate. An attacker can bypass the browser entirely.

### FluentValidation as the Gate

Every command and query object that arrives at an API endpoint passes through a FluentValidation validator registered in the MediatR pipeline:

```csharp
public class CreateTradeAgreementCommandValidator
    : AbstractValidator<CreateTradeAgreementCommand>
{
    public CreateTradeAgreementCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200)
            .Matches(@"^[\w\s\-\(\)]+$")
            .WithMessage("Agreement name contains invalid characters.");

        RuleFor(x => x.CountryCode)
            .NotEmpty()
            .Length(2)
            .Matches(@"^[A-Z]{2}$");

        RuleFor(x => x.EffectiveDate)
            .GreaterThan(new DateTime(1990, 1, 1))
            .LessThan(new DateTime(2100, 1, 1));
    }
}
```

A MediatR `ValidationBehavior<TRequest, TResponse>` pipeline behavior runs validators before the handler; if validation fails, it throws a `ValidationException` that the global exception handler converts to a 422 Unprocessable Entity response.

### Validation Rules for Domain Fields

| Field | Rule |
|-------|------|
| HS Code | Exactly 6 or 10 digits, numeric only |
| Country Code | Exactly 2 uppercase letters (ISO 3166-1 alpha-2) |
| Agreement Name | 1–200 characters, alphanumeric + spaces and hyphens |
| Percentage Values (RVC) | 0.00–100.00, two decimal places |
| Dates | Must be parseable as ISO 8601; no dates before 1990 or after 2099 |
| Free text / descriptions | Max 4000 characters; HTML tags stripped before storage |

---

## 11. Output Encoding

### JSON Serialization Escaping

`System.Text.Json` (the default serializer in ASP.NET Core 9) encodes potentially dangerous characters in JSON strings by default. The serializer options in this project explicitly configure `UnsafeRelaxedJsonEscaping = false` to ensure `<`, `>`, `&`, `'`, and `"` are always escaped in JSON output, preventing XSS when the JSON is reflected into HTML contexts.

```csharp
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Encoder =
        System.Text.Encodings.Web.JavaScriptEncoder.Default;  // NOT UnsafeRelaxed
    options.SerializerOptions.PropertyNamingPolicy =
        JsonNamingPolicy.CamelCase;
});
```

### React JSX Auto-Escaping

React auto-escapes all values interpolated inside JSX. For example:

```jsx
// Safe — React escapes userInput before inserting into the DOM
<p>{userInput}</p>
```

This escaping is automatic and covers the vast majority of rendering cases. No additional encoding library is required for normal JSX rendering.

### API Error Envelope

The API error response envelope **never** includes stack traces, file paths, or internal exception messages. The exact envelope format is:

```json
{
  "success": false,
  "message": "A descriptive, user-safe error message.",
  "errors": ["Field-level validation error 1", "Field-level validation error 2"],
  "traceId": "00-4af7e9c1234abcde-01"
}
```

Stack traces are written to the internal log only, identified by `traceId`. A support engineer can retrieve the full trace from the log aggregator using the `traceId`. The `traceId` is the only correlation token returned to the client.

---

## 12. SQL Injection Prevention

### Parameterized Queries Are Mandatory

Every Dapper query uses parameterized form:

```csharp
// CORRECT — parameterized
var sql = "SELECT * FROM trade_agreements WHERE tenant_id = @TenantId AND hs_code = @HsCode";
var result = await connection.QueryAsync<TradeAgreementDto>(sql, new { TenantId = tenantId, HsCode = hsCode });

// FORBIDDEN — string concatenation (instant code review failure)
var sql = $"SELECT * FROM trade_agreements WHERE hs_code = '{hsCode}'";
```

### Why String Concatenation Is Forbidden

String-concatenated SQL is forbidden without exception because:
1. It passes user-controlled data directly into the SQL parser, allowing an attacker to alter the query's structure.
2. Parameterized queries cause the database to treat the parameter value as data, never as executable SQL syntax.
3. There is no "safe" escaping function that reliably substitutes for parameterization across all edge cases and database drivers.

### EF Core

EF Core LINQ queries are automatically parameterized. The only risk is `FromSqlRaw` with string interpolation:

```csharp
// FORBIDDEN — vulnerable to injection even through EF Core
dbContext.TradeAgreements.FromSqlRaw($"SELECT * FROM trade_agreements WHERE name = '{name}'");

// CORRECT — use FromSqlInterpolated which parameterizes automatically
dbContext.TradeAgreements.FromSqlInterpolated($"SELECT * FROM trade_agreements WHERE name = {name}");
```

`FromSqlRaw` is prohibited unless the SQL string is a compile-time constant with no user-supplied values. Any exception requires a security review.

### Tenant Isolation Enforcement

Every repository method must inject the tenant ID from `ITenantContext`, never accept it as a client-supplied parameter:

```csharp
public async Task<IEnumerable<TradeAgreementDto>> GetAllAsync()
{
    var tenantId = _tenantContext.TenantId;  // from validated JWT claim
    var sql = "SELECT * FROM trade_agreements WHERE tenant_id = @TenantId AND deleted_at IS NULL";
    return await _connection.QueryAsync<TradeAgreementDto>(sql, new { TenantId = tenantId });
}
```

---

## 13. XSS Prevention

### React JSX Auto-Escaping

React's JSX syntax escapes all dynamic values before inserting them into the DOM. Strings, numbers, and expressions within `{}` in JSX are always treated as text nodes, not HTML markup. This prevents reflected and stored XSS in the vast majority of rendering scenarios.

### `dangerouslySetInnerHTML` Is Forbidden

The React prop `dangerouslySetInnerHTML` bypasses JSX escaping and injects raw HTML into the DOM. Its use is **absolutely prohibited** in this codebase:

- ESLint rule `react/no-danger` is configured to error on any use of `dangerouslySetInnerHTML`.
- Code reviews must reject any PR containing `dangerouslySetInnerHTML`.
- If rich-text rendering is required for a legitimate business reason (e.g., displaying a legal agreement text), use a vetted sanitization library (DOMPurify) with an explicit allowlist of tags, and document the security review exception.

### Stored XSS via Database Content

Although React escapes output, stored XSS can still occur if content is rendered via `dangerouslySetInnerHTML` or injected into non-React DOM manipulation. Defense layers:
- User-supplied free text is stripped of HTML tags server-side using a whitelist-based sanitizer before storage.
- Content Security Policy (`script-src 'self'`) prevents inline script execution even if an XSS vector is discovered.

### DOM-Based XSS

- Never use `document.write`, `innerHTML`, or `eval` with user-controlled data in JavaScript.
- `location.href`, `location.search`, and `location.hash` values must be validated before use.
- React Router handles all navigation; direct `window.location` manipulation with untrusted input is prohibited.

---

## 14. CSRF Protection

### SameSite Cookie Policy

The refresh token cookie is set with `SameSite=Strict`, which means the browser will not include the cookie in requests originated from a different site. This provides strong CSRF protection for the token refresh endpoint.

### CSRF Token for Form Endpoints

Despite JWT-based stateless authentication (which is not inherently CSRF-vulnerable because the token is in `Authorization: Bearer` header, not a cookie), endpoints that use cookie-based flows (refresh token endpoint) use double-submit cookie pattern or Antiforgery token:

```csharp
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});
```

The React frontend retrieves the CSRF token from the `XSRF-TOKEN` cookie (JavaScript-readable) and sends it as the `X-CSRF-TOKEN` request header. Cross-site attackers cannot read cookies from a different origin, so they cannot replicate the token.

### State-Changing Operations via JSON API

API endpoints that accept JSON bodies with the `Authorization: Bearer` header are not CSRF-vulnerable in the traditional sense — the `Authorization` header is a custom header that SameSite and CORS policies prevent from being added by cross-origin form submissions. However, CSRF defense-in-depth is maintained by:
- Requiring `Content-Type: application/json` (browsers cannot set this on cross-origin form submissions without CORS preflight).
- CORS policy limiting `Access-Control-Allow-Origin` to the known frontend origin.

---

## 15. SSRF Prevention

### Outbound HTTP Restrictions

The application makes outbound HTTP calls only to a defined allowlist of external services (EU Customs tariff databases, partner trade portals). All `HttpClient` instances are configured via named clients with pre-set base addresses:

```csharp
builder.Services.AddHttpClient("EuTariffClient", client =>
{
    client.BaseAddress = new Uri("https://ec.europa.eu/taxation_customs/dds2/taric/");
    client.Timeout = TimeSpan.FromSeconds(30);
});
```

URL construction from user-supplied strings is prohibited. If a user provides a URL (e.g., for a webhook or integration), it must be:
1. Validated against the allowlist using `Uri.TryCreate` and scheme/host checks.
2. Rejected if the host resolves to a private IP range (RFC 1918: 10.x.x.x, 172.16.x.x, 192.168.x.x) or loopback (127.0.0.1).
3. Rejected if the scheme is not `https`.

### Network-Level Controls

- Container egress is restricted at the Kubernetes NetworkPolicy or firewall level to the allowlisted external IPs.
- Cloud metadata endpoints (`169.254.169.254` for Azure IMDS, `100.100.100.200` for Alibaba Cloud) are blocked at the egress firewall.
- DNS resolution for internal hostnames (`.internal`, `.local`, `.cluster.local`) from user-supplied URLs is rejected.

---

## 16. Secrets Management

### Prohibited Practices

The following are strictly prohibited and will cause a CI build failure if detected by secret-scanning tools:

- Hardcoded connection strings, API keys, or JWT private keys in source code.
- Secrets in `appsettings.json` or `appsettings.Production.json` committed to version control.
- Secrets in Docker `ENV` instructions in `Dockerfile` (they appear in image layer history).
- Secrets in GitHub Actions `run:` steps without masking.

### Environment Tiers

| Environment | Secrets Storage |
|-------------|----------------|
| Development (local) | .NET User Secrets (`dotnet user-secrets`) — stored outside the project directory in the OS user profile |
| CI/CD pipeline | GitHub Actions Encrypted Secrets (never logged, masked in output) |
| Staging / Production (cloud) | Azure Key Vault, accessed via Managed Identity (no credentials needed) |
| Production (on-premise) | Environment variables injected by the orchestrator (Kubernetes Secrets or HashiCorp Vault) |

### .NET Configuration Integration

```csharp
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}
else
{
    builder.Configuration.AddAzureKeyVault(
        new Uri($"https://{keyVaultName}.vault.azure.net/"),
        new DefaultAzureCredential());
}
```

### Secret Rotation Policy

- JWT signing keys: rotated every 90 days. Old keys remain valid for 24 hours after rotation to allow in-flight tokens to expire gracefully (JWKS endpoint serves multiple keys with different `kid` values).
- Database credentials: rotated every 180 days. Zero-downtime rotation uses a two-credential pattern.
- External API keys: rotated annually or immediately upon suspected compromise.

---

## 17. Logging Security

### What to Log

Every API exception log entry must include all of the following fields:

| Field | Description |
|-------|-------------|
| `Timestamp` | UTC timestamp with millisecond precision |
| `User` | Authenticated user UUID (`sub` claim); "anonymous" for unauthenticated requests |
| `API` | HTTP method + route template (e.g., `POST /api/trade-agreements/{id}`) |
| `Stack Trace` | Full exception stack trace — **written to internal log only, never returned to client** |
| `Correlation Id` | Trace ID from `HttpContext.TraceIdentifier`, also present in the response as `traceId` |
| `Request Body` | Sanitized copy of the request body (PII fields replaced with `[REDACTED]`) |
| `Response Code` | HTTP status code |

### What NOT to Log

The following must never appear in logs under any circumstances:

- Plaintext passwords (even "attempted" passwords from login failures).
- Full JWT tokens or refresh tokens.
- Credit card numbers, bank account details.
- Personal identification numbers (national ID, passport numbers in their entirety).
- Full email addresses unless necessary — prefer truncated form (`u***@corp.com`).
- Secret keys, API keys, or connection strings.
- Full request bodies for endpoints that handle credentials (login, password reset).

### PII Masking in Structured Logs

Serilog destructuring policy masks known sensitive fields before log events are written:

```csharp
Log.Logger = new LoggerConfiguration()
    .Destructure.ByTransforming<CreateUserCommand>(cmd => new
    {
        cmd.Username,
        Password = "[REDACTED]",
        cmd.Role,
        cmd.TenantId
    })
    .WriteTo.Seq(serverUrl)
    .CreateLogger();
```

### Log Tamper Protection

- Logs are shipped to a write-once SIEM/log aggregator in real-time. The application never has write access to delete or modify shipped logs.
- Log entries include a hash chain or sequence number in production to detect log tampering or deletion.
- Application service accounts have no direct access to the log aggregator's storage backend.

---

## 18. Audit Logging

### Audit Fields on Every Entity

Every database entity that represents a business object carries the following audit fields:

| Field | Type | Description |
|-------|------|-------------|
| `CreatedBy` | `uuid` | UUID of the user who created the record |
| `UpdatedBy` | `uuid` | UUID of the user who last modified the record |
| `DeletedBy` | `uuid` | UUID of the user who soft-deleted the record (null if not deleted) |
| `CreatedDate` | `timestamptz` | UTC timestamp of creation |
| `ModifiedDate` | `timestamptz` | UTC timestamp of last modification |
| `IPAddress` | `inet` | IP address from which the mutating request originated |
| `Machine` | `varchar(255)` | Hostname or container ID of the API server that processed the request |

### Audit Log Table

All write operations (Create, Update, Delete) produce an entry in the `audit_log` table in addition to updating the entity's audit fields:

```sql
CREATE TABLE audit_log (
    id              uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id       uuid NOT NULL,
    entity_type     varchar(100) NOT NULL,
    entity_id       uuid NOT NULL,
    action          varchar(20) NOT NULL CHECK (action IN ('CREATE', 'UPDATE', 'DELETE')),
    performed_by    uuid NOT NULL,
    performed_at    timestamptz NOT NULL DEFAULT now(),
    ip_address      inet,
    machine         varchar(255),
    correlation_id  varchar(36),
    before_value    jsonb,
    after_value     jsonb
);
```

`before_value` and `after_value` are JSON snapshots of the entity before and after the change. PII fields are masked in these snapshots.

### EF Core Interceptor for Automatic Audit

```csharp
public class AuditInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        var context = eventData.Context;
        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
        {
            var now = DateTime.UtcNow;
            var userId = _currentUser.UserId;
            var ip = _currentUser.IPAddress;
            var machine = Environment.MachineName;

            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedBy = userId;
                entry.Entity.CreatedDate = now;
                entry.Entity.IPAddress = ip;
                entry.Entity.Machine = machine;
            }
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Entity.UpdatedBy = userId;
                entry.Entity.ModifiedDate = now;
            }
        }
        return base.SavingChanges(eventData, result);
    }
}
```

---

## 19. File Upload Security

### Allowed MIME Types

Only the following MIME types are accepted for file uploads:

| Use Case | Allowed MIME Types | Max Size |
|----------|--------------------|----------|
| Trade agreement documents | `application/pdf` | 10 MB |
| HS code import spreadsheets | `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet` (xlsx) | 5 MB |
| Product rule batch import | `text/csv` | 2 MB |

All other MIME types are rejected with HTTP 415 Unsupported Media Type.

### Upload Validation Steps

1. **Content-Type header check** — immediate rejection if not in the allowlist.
2. **File extension check** — must match the declared MIME type; `.pdf` must be `application/pdf`.
3. **Magic byte validation** — the first bytes of the file are checked against known signatures (PDF: `%PDF-`, XLSX: `PK\x03\x04`). The declared Content-Type is not trusted alone.
4. **Size check** — files exceeding the per-type limit are rejected before streaming to storage.
5. **Filename sanitization** — the original filename is discarded. A new UUID-based filename is assigned on the server (`{uuid}.{safe-extension}`). This prevents directory traversal and filename injection.
6. **Virus scan hook** — files are queued to a background service that calls an antivirus scanner API (ClamAV or cloud-based). The file is quarantined and unavailable for download until the scan completes. The scan result is recorded in the `uploaded_files` table.

### Storage

- Uploaded files are stored outside the web root (not in `wwwroot`). Files are served via a dedicated download endpoint that checks authorization before streaming.
- In cloud deployments, files are stored in Azure Blob Storage with a private access level. Download URLs are short-lived SAS tokens generated by the API.

---

## 20. Encryption

### Data in Transit

- All external traffic uses TLS 1.2 or TLS 1.3 (see Section 8).
- Internal service-to-service traffic within the cluster also uses mTLS where feasible.
- Database connections from the API to PostgreSQL use SSL mode `require` (TLS encrypted connection, server certificate verified).

### Data at Rest — Column-Level Encryption for PII

PII columns are encrypted at the database level using PostgreSQL's `pgcrypto` extension with AES-256-CBC:

```sql
-- Storing encrypted email
UPDATE users SET email_encrypted = pgp_sym_encrypt(email_plaintext, current_setting('app.encryption_key'));

-- Reading encrypted email
SELECT pgp_sym_decrypt(email_encrypted, current_setting('app.encryption_key')) AS email FROM users;
```

The encryption key (`app.encryption_key`) is set as a PostgreSQL session parameter from the connection string, which is itself stored in Azure Key Vault. The key never appears in source code or configuration files.

### Backup Encryption

Database backups are encrypted with AES-256 using `gpg --symmetric` before being written to backup storage. The backup encryption passphrase is stored in Azure Key Vault and is separate from the application encryption key. See Section 23 for the full backup strategy.

---

## 21. PII Protection

### PII Fields in the Domain

| Entity | PII Fields |
|--------|-----------|
| `Users` | `email`, `full_name`, `phone_number` |
| `Companies` | `vat_number`, `registered_address`, `contact_email` |
| `AuditLog` | IP addresses are considered quasi-PII |
| `OriginCalculations` | None (calculations reference HS codes and rules, not personal data) |

### Masking in Logs

PII fields are masked in all log output:
- `email` → `u***@corp.com` (first character of local part + stars + domain)
- `full_name` → `J*** S***` (first character of each name component)
- `vat_number` → `DE****12345` (first two characters + stars + last five digits)
- `ip_address` → `192.168.*.*` (last two octets masked)

### GDPR Right-to-Erasure Strategy

When a data erasure request is received:
1. The user account is deactivated immediately; all active sessions are revoked.
2. PII fields in the `users` and `companies` tables are overwritten with `[REDACTED]` markers.
3. Audit log entries referencing the user are anonymized — the `performed_by` UUID is replaced with a `deleted-user` sentinel UUID, preserving audit integrity without retaining personal data.
4. The erasure operation itself is recorded in a separate `gdpr_requests` table (retained for legal compliance, containing only the request date and a non-personal reference ID).
5. Backups: PII is irrecoverable from backups after the backup retention period (90 days) has passed. If a restore is required within the retention window, the data must be re-anonymized before the restored database is used.

---

## 22. GDPR Considerations

### Consent

- User accounts require explicit consent to Terms of Service and Privacy Policy at registration.
- Consent records (timestamp, version of documents consented to, user UUID) are stored in `user_consents`.
- Marketing communications require a separate opt-in consent; default is opt-out.

### Data Minimization

- Only the minimum data required for origin calculation and trade compliance is collected.
- Optional profile fields are clearly marked optional in the UI and schema.
- Calculated result data is retained per the business retention policy; raw inputs are retained only as long as needed for audit.

### Retention Policy

| Data Category | Retention Period | Basis |
|---------------|-----------------|-------|
| Origin calculation results | 7 years | EU Customs Code audit requirement |
| Audit log entries | 7 years | Legal compliance |
| User account data | Duration of relationship + 1 year | Contractual necessity |
| Application logs | 90 days | Operational need |
| Uploaded documents | 7 years | Trade compliance |
| Session/auth logs | 90 days | Security monitoring |

### Erasure Procedure

Documented erasure procedure with an SLA of 30 days from verified request receipt. Erasure requests are tracked in the `gdpr_requests` table. The Data Protection Officer (DPO) is notified by automated email for each request.

---

## 23. Backup Strategy

### PostgreSQL Backup Schedule

| Backup Type | Frequency | Retention | Method |
|-------------|-----------|-----------|--------|
| Full backup | Daily (02:00 UTC) | 90 days | `pg_dump --format=custom` |
| WAL archiving (point-in-time recovery) | Continuous | 14 days | `pg_basebackup` + WAL streaming to S3/Blob |
| Pre-migration snapshot | Before each deployment | 72 hours | `pg_dump` triggered by CI/CD pipeline |

### Backup Encryption

All backup files are encrypted before being written to backup storage:

```bash
pg_dump -Fc praeferenz_prod | gpg --symmetric --cipher-algo AES256 \
    --passphrase-file /run/secrets/backup_passphrase \
    --output "backup_$(date +%Y%m%d_%H%M%S).dump.gpg"
```

The encrypted backup files are transferred to an isolated storage account that the application service account cannot access (write-only from the backup job, read by the DBA role only).

### Restore Testing

- Automated restore verification runs weekly in an isolated environment.
- The restore test validates: backup decryption succeeds, `pg_restore` completes without errors, row counts match expected ranges, and a sample of critical queries returns expected results.
- Manual restore drills are conducted quarterly by the infrastructure team.

---

## 24. Container Security

### Non-Root User

The application container runs as a non-root user. The `Dockerfile` creates a dedicated system user:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
RUN addgroup --system --gid 1001 appgroup && \
    adduser --system --uid 1001 --ingroup appgroup --no-create-home appuser
WORKDIR /app
COPY --from=build /app/publish .
RUN chown -R appuser:appgroup /app
USER appuser
EXPOSE 8080
ENTRYPOINT ["dotnet", "PraeferenzDummy.Api.dll"]
```

### Read-Only Filesystem

Where possible, the container filesystem is mounted read-only (`--read-only` in Docker, `readOnlyRootFilesystem: true` in Kubernetes). Directories that require write access (temporary files, ASP.NET Core data protection keys) are explicitly mounted as writable volumes:

```yaml
securityContext:
  readOnlyRootFilesystem: true
  runAsNonRoot: true
  runAsUser: 1001
volumeMounts:
  - name: tmp
    mountPath: /tmp
  - name: dataprotection-keys
    mountPath: /app/keys
```

### No Secrets in Image Layers

- `ARG` and `ENV` instructions in `Dockerfile` must never contain secret values. Each `RUN` instruction that uses a secret must use Docker BuildKit secret mounts (`--mount=type=secret`) so the secret is not written to any image layer.
- `docker history <image>` must not reveal any credentials. This is verified in CI.

### Image Scanning

- Docker images are scanned with Trivy (or equivalent) in CI before being pushed to the container registry. Builds fail on `HIGH` or `CRITICAL` CVEs in OS packages or language runtime.
- Base images use the specific digest hash (`FROM mcr.microsoft.com/dotnet/aspnet:9.0@sha256:...`) to prevent supply chain attacks via mutable tags.

---

## 25. API Security

### Versioning

- API versions use URL path prefixes: `/api/v1/...`, `/api/v2/...`.
- The version is part of the route template, not a query parameter or header, to ensure cache and proxy behavior is predictable.
- Breaking changes always increment the major version; minor additions may be made to existing versions.

### Deprecation Policy

- A version is deprecated with at minimum 6 months of notice before removal.
- Deprecated endpoints return a `Deprecation` response header (`Deprecation: true; rel="successor-version"`) with the URL of the replacement.
- A `Sunset` header specifies the date after which the endpoint will no longer be available.

### No Sensitive Data in URLs

URLs are logged by web servers, load balancers, CDNs, and browser history. The following are prohibited in URL paths or query strings:
- JWT tokens or any credential.
- User passwords or secrets.
- Personal data (names, email addresses, national IDs).
- Internal system identifiers that could reveal architecture.

Resource identifiers in URLs use UUIDs (not sequential integers) to prevent enumeration attacks.

### Request/Response Logging

API request logging records the method, route template, response code, and duration. It never logs:
- Raw query strings that may contain sensitive filters.
- Request or response bodies by default (opt-in only for designated debug endpoints in development).
- Authorization headers.

---

## 26. Pre-Deployment Security Checklist

Complete this checklist before every production deployment. Items marked `[BLOCK]` are release blockers; failing them must halt the deployment.

### Authentication and Authorization
- [ ] `[BLOCK]` JWT signing algorithm is RS256; HS256 is absent from the codebase.
- [ ] `[BLOCK]` All API endpoints have `[Authorize]` attributes or are explicitly marked `[AllowAnonymous]` with justification in a comment.
- [ ] `[BLOCK]` Refresh token cookies are `HttpOnly`, `Secure`, `SameSite=Strict`.
- [ ] RBAC permission matrix has been reviewed and matches the deployed code's policy definitions.
- [ ] Token revocation list (Redis) is deployed and reachable.

### Transport and Headers
- [ ] `[BLOCK]` HTTPS is enforced; HTTP returns 301 redirect or is blocked at the load balancer.
- [ ] `[BLOCK]` HSTS header is present with `max-age` ≥ 365 days and `includeSubDomains`.
- [ ] `[BLOCK]` All security headers (CSP, X-Frame-Options, X-Content-Type-Options, Referrer-Policy) are present in production responses.
- [ ] `Server` and `X-Powered-By` headers are absent from responses.
- [ ] TLS 1.0 and 1.1 are disabled at the load balancer level; confirmed with `testssl.sh` or equivalent.

### Data and Code
- [ ] `[BLOCK]` Secret scanner (truffleHog / gitleaks) reports zero findings in the repository.
- [ ] `[BLOCK]` `dotnet list package --vulnerable` reports zero `High` or `Critical` vulnerabilities.
- [ ] `[BLOCK]` `npm audit` reports zero `High` or `Critical` vulnerabilities.
- [ ] `[BLOCK]` Container image Trivy scan reports zero `HIGH` or `CRITICAL` CVEs.
- [ ] `[BLOCK]` `dangerouslySetInnerHTML` is absent from the React codebase (ESLint enforced).
- [ ] `[BLOCK]` No raw SQL string concatenation exists in Dapper query methods.
- [ ] `[BLOCK]` All FluentValidation validators are registered and wired into the MediatR pipeline behavior.
- [ ] Database migrations have been reviewed for destructive operations; a rollback migration exists.

### Infrastructure
- [ ] `[BLOCK]` Production `appsettings.json` contains no secret values; all secrets come from Azure Key Vault or environment variables.
- [ ] `[BLOCK]` Docker containers run as non-root user (UID 1001).
- [ ] Container images use pinned digest hashes, not mutable tags.
- [ ] Read-only filesystem is enabled for the application container.
- [ ] WAL archiving and daily backups have been verified to be running in the target environment.
- [ ] Backup encryption and restore procedure have been tested in the past 30 days.

### Logging and Monitoring
- [ ] `[BLOCK]` Structured logs are shipping to the centralized log aggregator.
- [ ] `[BLOCK]` API error responses do not contain stack traces or internal exception messages.
- [ ] Alerting rules for repeated 401/403 responses are active.
- [ ] Rate limiting is active on `/api/auth/login` and `/api/auth/refresh`.

### Compliance
- [ ] GDPR data retention policies are configured; automated deletion jobs are scheduled.
- [ ] Privacy Policy and Terms of Service versions referenced in `user_consents` match the current published documents.
- [ ] PII masking in logs has been verified by inspecting a sample of log entries from staging.

---

*This document is reviewed and updated with every major release. Security questions or incident reports should be directed to the project security contact. Last reviewed: 2026-06-26.*
