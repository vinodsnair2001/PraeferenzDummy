# CI Pipeline Design — PraeferenzRoO

**Date:** 2026-06-27  
**Status:** Approved  
**Scope:** GitHub Actions CI for backend (ASP.NET Core 9) and frontend (React 19 / Vite 6)  
**Out of scope:** CD / deployment automation (deferred to a future spec)

---

## Context

The project is in the planning phase — no source code exists yet. The CI workflows are written forward-looking: structured for the expected `src/` and `Tests/` layout defined in CLAUDE.md and testing.md. Workflows will produce expected failures until source is added; all job scaffolding, path filters, service containers, and coverage thresholds are production-ready.

The approved CI platform is GitHub Actions (stack.md §36). All action references must be pinned to a full commit SHA with a version comment (supply-chain security requirement).

---

## Decisions

| Decision | Choice | Rationale |
|---|---|---|
| Scope | CI only — no CD | Deployment target not yet determined |
| Workflow structure | Two separate workflows | Path-filtered; a frontend change skips the backend pipeline entirely |
| Job structure | Staged gating (`needs:`) | Matches testing.md §24 philosophy: unit tests gate integration tests |
| E2E / Mutation / Performance | Excluded from PR CI | testing.md §31, §34: run nightly or on release branches only |
| PostgreSQL in CI | GitHub Actions service container | Matches the snippet in testing.md §24; TestContainers also starts its own container inside tests |
| Coverage provider (frontend) | V8 (built into Vite 6 / Vitest) | No extra package needed |
| Node version | 22 | Matches `node:22-alpine` pinned in stack.md §35 |

---

## File Layout

```
.github/
├── workflows/
│   ├── ci-backend.yml
│   └── ci-frontend.yml
└── dependabot.yml
```

---

## ci-backend.yml

### Triggers

```yaml
on:
  pull_request:
    branches: [main]
    paths:
      - 'src/**'
      - 'Tests/**'
      - '*.sln'
      - 'global.json'
      - 'Directory.*.props'
      - 'Directory.*.targets'
      - '.github/workflows/ci-backend.yml'
  push:
    branches: [main]
    paths:
      - 'src/**'
      - 'Tests/**'
      - '*.sln'
      - 'global.json'
      - 'Directory.*.props'
      - 'Directory.*.targets'
      - '.github/workflows/ci-backend.yml'
```

The workflow also triggers on changes to its own file so that workflow edits are validated on their own PR.

### Job Pipeline

```
lint ──► build ──► unit-tests ──► integration-tests ──► coverage-gate
```

#### Job: `lint`

- Runner: `ubuntu-latest`
- Steps: checkout → setup .NET 9 → restore → `dotnet format --verify-no-changes`
- Purpose: Fails the PR immediately if code is not formatted, before spending time on compilation or tests.

#### Job: `build`

- Runner: `ubuntu-latest`
- Needs: `lint`
- Steps: checkout → setup .NET 9 → restore (cached) → `dotnet build -c Release --no-restore`
- Uploads the build output as an artifact for downstream jobs to reuse (avoids recompiling per job).

#### Job: `unit-tests`

- Runner: `ubuntu-latest`
- Needs: `build`
- Test projects: `Tests/Domain.Tests` and `Tests/Application.Tests`
- No external dependencies (no database, no network)
- Target run time: under 30 seconds (testing.md §2)
- Coverage: `--collect:"XPlat Code Coverage"` (Coverlet), XML output uploaded as artifact

#### Job: `integration-tests`

- Runner: `ubuntu-latest`
- Needs: `unit-tests`
- Service container: `postgres:16-alpine` with health check (`pg_isready`)
- Test projects: `Tests/Persistence.Tests`, `Tests/Api.Tests`, `Tests/Infrastructure.Tests`
- Environment variable passed to test runner:
  ```
  ConnectionStrings__DefaultConnection: Host=localhost;Database=praeferenz_test;Username=test;Password=test
  ```
- Note: TestContainers inside `Persistence.Tests` will start its own Docker-in-Docker PostgreSQL container. The service container satisfies `Api.Tests` (WebApplicationFactory) which overrides the connection string.
- Coverage XML artifacts uploaded alongside unit-test coverage.

#### Job: `coverage-gate`

- Runner: `ubuntu-latest`
- Needs: `integration-tests`
- Downloads all Coverlet XML artifacts from prior jobs
- Merges reports with `reportgenerator` dotnet tool
- Enforces thresholds (fails workflow if not met):

| Layer | Line Coverage | Branch Coverage |
|---|---|---|
| Domain | ≥ 90% | ≥ 85% |
| Application | ≥ 85% | ≥ 80% |
| Api | ≥ 80% | ≥ 75% |

- Uploads the merged HTML report as a workflow artifact (viewable in GitHub Actions UI)

### Excluded from PR CI

| Test suite | Why excluded | When to run |
|---|---|---|
| `Tests/E2E.Tests` (Playwright) | Requires full stack + staging env; stack.md §31 | Nightly / release branches |
| `Tests/Performance.Tests` (BenchmarkDotNet) | Requires Release build + dedicated runner | Nightly |
| `Tests/Mutation.Tests` (Stryker.NET) | Slow by design; stack.md §34 | Nightly / pre-release |

---

## ci-frontend.yml

### Triggers

```yaml
on:
  pull_request:
    branches: [main]
    paths:
      - 'src/frontend/**'
      - '.github/workflows/ci-frontend.yml'
  push:
    branches: [main]
    paths:
      - 'src/frontend/**'
      - '.github/workflows/ci-frontend.yml'
```

### Job Pipeline

```
lint ──► build ──► unit-tests ──► coverage-gate
```

#### Job: `lint`

- Runner: `ubuntu-latest`
- Working directory: `src/frontend`
- Node version: `22`
- Package install: `npm ci` (reproducible from lockfile)
- Steps:
  1. `tsc --noEmit` — TypeScript strict check (fails on any type error)
  2. `eslint src/` — lint all `.ts` / `.tsx` files
- Purpose: Catches type errors and lint violations before running the build or tests.

#### Job: `build`

- Runner: `ubuntu-latest`
- Needs: `lint`
- Working directory: `src/frontend`
- Steps: checkout → setup Node 22 → `npm ci` (cached) → `vite build`
- Confirms the production bundle compiles without errors.

#### Job: `unit-tests`

- Runner: `ubuntu-latest`
- Needs: `build`
- Working directory: `src/frontend`
- Command: `vitest run --coverage`
- Coverage provider: V8 (configured in `vite.config.ts`)
- Runs all `*.test.ts` and `*.test.tsx` files
- No backend or service container required (API calls are mocked via Vitest)
- Coverage JSON output uploaded as artifact

#### Job: `coverage-gate`

- Runner: `ubuntu-latest`
- Needs: `unit-tests`
- Reads the Vitest coverage JSON summary
- Enforces: Frontend line coverage ≥ 75% (testing.md §23)
- Fails the workflow if the threshold is not met

---

## dependabot.yml

Three ecosystems, weekly schedule, PRs labelled `dependencies`:

| Ecosystem | Directory | Updates |
|---|---|---|
| `github-actions` | `/` | Action SHA pins — satisfies stack.md §36 pinning policy |
| `nuget` | `/src` | NuGet packages across all `.csproj` files |
| `npm` | `/src/frontend` | npm packages |

Major version bumps open separate PRs from patch/minor so they can receive additional review scrutiny.

---

## Branch Protection Requirements

Configured manually in GitHub repo settings (not in YAML). Required status checks before merging to `main`:

| Status check | Condition |
|---|---|
| `ci-backend / coverage-gate` | Required when backend paths change |
| `ci-frontend / coverage-gate` | Required when frontend paths change |

A PR touching only frontend files is not blocked on the backend pipeline (path filtering prevents it from running). A PR touching both must pass both terminal jobs.

---

## Action Version Policy

All `uses:` entries are pinned to a full commit SHA per stack.md §36:

```yaml
- uses: actions/checkout@11bd71901bbe5b1630ceea73d27597364c9af683 # v4.2.2
- uses: actions/setup-dotnet@67a3573c9a986a3f9c594539f4ab511d57bb3ce9 # v4.3.1
- uses: actions/setup-node@49933ea5288caeca8642d1e84afbd3f7d6820020 # v4.4.0
- uses: actions/upload-artifact@ea165f8d65b6e75b540449e92b4886f43607fa02 # v4.6.2
- uses: actions/download-artifact@d3f86a106a0bac45b974a628896c90dbdf5c8093 # v4.3.0
- uses: actions/cache@5a3ec84eff668545956fd18022155c47e93e2684 # v4.2.3
```

Dependabot keeps these SHAs current automatically.

---

## Source Path Assumptions

The workflows assume the following layout (per CLAUDE.md solution structure):

```
/src
  /Api
  /Application
  /Domain
  /Infrastructure
  /Persistence
  /Shared
  /frontend          ← React/Vite frontend root
/Tests
  /Domain.Tests
  /Application.Tests
  /Infrastructure.Tests
  /Persistence.Tests
  /Api.Tests
  /E2E.Tests
  /Performance.Tests
  /Mutation.Tests
*.sln
global.json
Directory.Build.props
Directory.Packages.props
```

If the frontend root is placed at a different path (e.g. `/frontend` instead of `/src/frontend`), the path filter and `working-directory` values in `ci-frontend.yml` must be updated accordingly.

---

*End of CI Pipeline Design Spec*
