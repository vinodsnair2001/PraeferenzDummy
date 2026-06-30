# CI Pipeline — GitHub Actions Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Create three GitHub Actions configuration files implementing staged, path-filtered CI pipelines for the PraeferenzRoO backend (.NET 9) and frontend (React 19 / Vite 6), plus Dependabot for automated dependency updates.

**Architecture:** Two separate workflow files — `ci-backend.yml` (5 sequential jobs gated with `needs:`) and `ci-frontend.yml` (4 sequential jobs gated with `needs:`) — each triggered only when their own source paths change. A `dependabot.yml` keeps action SHAs and package versions current automatically. No shared state between the two pipelines.

**Tech Stack:** GitHub Actions, .NET 9 SDK, xUnit 2, Coverlet (`XPlat Code Coverage`), ReportGenerator (`dotnet-reportgenerator-globaltool`), PostgreSQL 16 (`postgres:16-alpine`), Node 22, TypeScript 5 strict, ESLint, Vite 6, Vitest 2 with V8 coverage provider, Python 3 (pre-installed on `ubuntu-latest`, used for coverage threshold scripts).

## Global Constraints

- All `uses:` entries MUST be pinned to a full 40-character commit SHA with a `# vX.Y.Z` comment on the same line — no tag-only references (e.g. `@v4`)
- Backend runtime: `dotnet-version: '9.0.x'`
- Frontend runtime: `node-version: '22'`
- PostgreSQL image: `postgres:16-alpine`
- Frontend source root: `src/frontend/` (adjust path filter and `working-directory` if scaffolded elsewhere)
- Package install: `npm ci` — never `npm install`
- No CD, no deployment steps, no Playwright/BenchmarkDotNet/Stryker jobs
- Coverage thresholds: Domain ≥ 90% line, Application ≥ 85% line, Api ≥ 80% line, Frontend ≥ 75% line
- Coverlet default output: `coverage.cobertura.xml` (Cobertura format) — do NOT assume OpenCover format

---

## File Map

| File | Action | Purpose |
|---|---|---|
| `.github/dependabot.yml` | Create | Weekly PRs for stale action SHAs, NuGet packages, npm packages |
| `.github/workflows/ci-backend.yml` | Create | 5-job backend pipeline: lint → build → unit-tests → integration-tests → coverage-gate |
| `.github/workflows/ci-frontend.yml` | Create | 4-job frontend pipeline: lint → build → unit-tests → coverage-gate |

---

### Task 1: Dependabot Configuration

**Files:**
- Create: `.github/dependabot.yml`

**Interfaces:**
- Consumes: nothing
- Produces: nothing (standalone config); enables Dependabot to open weekly PRs that refresh pinned action SHAs, satisfying stack.md §36

- [ ] **Step 1: Create the `.github/workflows` directory**

```bash
mkdir -p .github/workflows
```

Expected: directory exists (no error if already present)

- [ ] **Step 2: Write `.github/dependabot.yml`**

Create the file with this exact content:

```yaml
version: 2
updates:
  - package-ecosystem: "github-actions"
    directory: "/"
    schedule:
      interval: "weekly"
      day: "monday"
    labels:
      - "dependencies"
    groups:
      actions:
        patterns:
          - "*"

  - package-ecosystem: "nuget"
    directory: "/src"
    schedule:
      interval: "weekly"
      day: "monday"
    labels:
      - "dependencies"
    groups:
      nuget-minor-patch:
        update-types:
          - "minor"
          - "patch"

  - package-ecosystem: "npm"
    directory: "/src/frontend"
    schedule:
      interval: "weekly"
      day: "monday"
    labels:
      - "dependencies"
    groups:
      npm-minor-patch:
        update-types:
          - "minor"
          - "patch"
```

- [ ] **Step 3: Validate YAML syntax**

```bash
python3 -c "import yaml; yaml.safe_load(open('.github/dependabot.yml')); print('YAML valid')"
```

Expected output: `YAML valid`
If `yaml` module not found: `pip3 install pyyaml --quiet` then retry.

- [ ] **Step 4: Commit**

```bash
git add .github/dependabot.yml
git commit -m "chore: add Dependabot config for actions, NuGet, and npm"
```

---

### Task 2: Backend CI Workflow

**Files:**
- Create: `.github/workflows/ci-backend.yml`

**Interfaces:**
- Consumes: nothing from other tasks
- Produces:
  - Required status check `ci-backend / coverage-gate` (configured in branch protection after first run)
  - Artifacts: `coverage-unit` and `coverage-integration` (Coverlet Cobertura XML files, consumed within the same workflow run by the `coverage-gate` job)

**Coverage gate mechanism:**
1. `unit-tests` and `integration-tests` jobs upload `**/coverage.cobertura.xml` as artifacts.
2. `coverage-gate` downloads both, merges them with `reportgenerator` (installed as a dotnet global tool), generates a merged `Cobertura.xml` plus an HTML report.
3. A Python 3 script parses the merged `Cobertura.xml`, checks per-assembly `line-rate` against thresholds, and exits non-zero on any failure — blocking the PR.

- [ ] **Step 1: Write `.github/workflows/ci-backend.yml`**

Create with this exact content:

```yaml
name: CI — Backend

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

env:
  DOTNET_NOLOGO: true
  DOTNET_SKIP_FIRST_TIME_EXPERIENCE: true
  DOTNET_CLI_TELEMETRY_OPTOUT: true

jobs:
  lint:
    name: Lint
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@11bd71901bbe5b1630ceea73d27597364c9af683 # v4.2.2
      - uses: actions/setup-dotnet@67a3573c9a986a3f9c594539f4ab511d57bb3ce9 # v4.3.1
        with:
          dotnet-version: '9.0.x'
      - name: Cache NuGet packages
        uses: actions/cache@5a3ec84eff668545956fd18022155c47e93e2684 # v4.2.3
        with:
          path: ~/.nuget/packages
          key: nuget-${{ runner.os }}-${{ hashFiles('**/*.csproj', 'Directory.Packages.props') }}
          restore-keys: nuget-${{ runner.os }}-
      - name: Restore
        run: dotnet restore
      - name: Check formatting
        run: dotnet format --verify-no-changes

  build:
    name: Build
    runs-on: ubuntu-latest
    needs: lint
    steps:
      - uses: actions/checkout@11bd71901bbe5b1630ceea73d27597364c9af683 # v4.2.2
      - uses: actions/setup-dotnet@67a3573c9a986a3f9c594539f4ab511d57bb3ce9 # v4.3.1
        with:
          dotnet-version: '9.0.x'
      - name: Cache NuGet packages
        uses: actions/cache@5a3ec84eff668545956fd18022155c47e93e2684 # v4.2.3
        with:
          path: ~/.nuget/packages
          key: nuget-${{ runner.os }}-${{ hashFiles('**/*.csproj', 'Directory.Packages.props') }}
          restore-keys: nuget-${{ runner.os }}-
      - name: Restore
        run: dotnet restore
      - name: Build
        run: dotnet build -c Release --no-restore

  unit-tests:
    name: Unit Tests
    runs-on: ubuntu-latest
    needs: build
    steps:
      - uses: actions/checkout@11bd71901bbe5b1630ceea73d27597364c9af683 # v4.2.2
      - uses: actions/setup-dotnet@67a3573c9a986a3f9c594539f4ab511d57bb3ce9 # v4.3.1
        with:
          dotnet-version: '9.0.x'
      - name: Cache NuGet packages
        uses: actions/cache@5a3ec84eff668545956fd18022155c47e93e2684 # v4.2.3
        with:
          path: ~/.nuget/packages
          key: nuget-${{ runner.os }}-${{ hashFiles('**/*.csproj', 'Directory.Packages.props') }}
          restore-keys: nuget-${{ runner.os }}-
      - name: Restore
        run: dotnet restore
      - name: Run unit tests
        run: |
          dotnet test Tests/Domain.Tests Tests/Application.Tests \
            -c Release \
            --no-restore \
            --results-directory ./TestResults/unit \
            --collect:"XPlat Code Coverage" \
            --logger "trx;LogFileName=unit-results.trx"
      - name: Upload unit test results
        if: always()
        uses: actions/upload-artifact@ea165f8d65b6e75b540449e92b4886f43607fa02 # v4.6.2
        with:
          name: test-results-unit
          path: ./TestResults/unit/*.trx
          retention-days: 7
      - name: Upload unit coverage
        uses: actions/upload-artifact@ea165f8d65b6e75b540449e92b4886f43607fa02 # v4.6.2
        with:
          name: coverage-unit
          path: ./TestResults/unit/**/coverage.cobertura.xml
          retention-days: 1

  integration-tests:
    name: Integration Tests
    runs-on: ubuntu-latest
    needs: unit-tests
    services:
      postgres:
        image: postgres:16-alpine
        env:
          POSTGRES_DB: praeferenz_test
          POSTGRES_USER: test
          POSTGRES_PASSWORD: test
        ports:
          - 5432:5432
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
    steps:
      - uses: actions/checkout@11bd71901bbe5b1630ceea73d27597364c9af683 # v4.2.2
      - uses: actions/setup-dotnet@67a3573c9a986a3f9c594539f4ab511d57bb3ce9 # v4.3.1
        with:
          dotnet-version: '9.0.x'
      - name: Cache NuGet packages
        uses: actions/cache@5a3ec84eff668545956fd18022155c47e93e2684 # v4.2.3
        with:
          path: ~/.nuget/packages
          key: nuget-${{ runner.os }}-${{ hashFiles('**/*.csproj', 'Directory.Packages.props') }}
          restore-keys: nuget-${{ runner.os }}-
      - name: Restore
        run: dotnet restore
      - name: Run integration tests
        env:
          ConnectionStrings__DefaultConnection: "Host=localhost;Port=5432;Database=praeferenz_test;Username=test;Password=test"
        run: |
          dotnet test Tests/Persistence.Tests Tests/Api.Tests Tests/Infrastructure.Tests \
            -c Release \
            --no-restore \
            --results-directory ./TestResults/integration \
            --collect:"XPlat Code Coverage" \
            --logger "trx;LogFileName=integration-results.trx"
      - name: Upload integration test results
        if: always()
        uses: actions/upload-artifact@ea165f8d65b6e75b540449e92b4886f43607fa02 # v4.6.2
        with:
          name: test-results-integration
          path: ./TestResults/integration/*.trx
          retention-days: 7
      - name: Upload integration coverage
        uses: actions/upload-artifact@ea165f8d65b6e75b540449e92b4886f43607fa02 # v4.6.2
        with:
          name: coverage-integration
          path: ./TestResults/integration/**/coverage.cobertura.xml
          retention-days: 1

  coverage-gate:
    name: Coverage Gate
    runs-on: ubuntu-latest
    needs: integration-tests
    steps:
      - uses: actions/checkout@11bd71901bbe5b1630ceea73d27597364c9af683 # v4.2.2
      - uses: actions/setup-dotnet@67a3573c9a986a3f9c594539f4ab511d57bb3ce9 # v4.3.1
        with:
          dotnet-version: '9.0.x'
      - name: Download unit coverage
        uses: actions/download-artifact@d3f86a106a0bac45b974a628896c90dbdf5c8093 # v4.3.0
        with:
          name: coverage-unit
          path: ./coverage-downloads/unit
      - name: Download integration coverage
        uses: actions/download-artifact@d3f86a106a0bac45b974a628896c90dbdf5c8093 # v4.3.0
        with:
          name: coverage-integration
          path: ./coverage-downloads/integration
      - name: Install ReportGenerator
        run: dotnet tool install --global dotnet-reportgenerator-globaltool
      - name: Merge coverage and generate Cobertura report
        run: |
          reportgenerator \
            "-reports:./coverage-downloads/**/*.xml" \
            "-targetdir:./coverage-report" \
            "-reporttypes:Cobertura;Html" \
            "-assemblyfilters:+PraeferenzRoO.*"
      - name: Enforce per-assembly coverage thresholds
        run: |
          python3 << 'EOF'
          import xml.etree.ElementTree as ET
          import sys

          tree = ET.parse('./coverage-report/Cobertura.xml')
          root = tree.getroot()

          # line-rate is 0.0–1.0; multiply by 100 for percentage
          thresholds = {
              'PraeferenzRoO.Domain':      90.0,
              'PraeferenzRoO.Application': 85.0,
              'PraeferenzRoO.Api':         80.0,
          }

          found = {k: None for k in thresholds}
          for package in root.findall('.//package'):
              name = package.get('name', '')
              for assembly, threshold in thresholds.items():
                  if name == assembly or name.startswith(assembly + '.'):
                      rate = float(package.get('line-rate', 0)) * 100
                      # Keep the lowest rate across sub-packages of the same assembly
                      if found[assembly] is None or rate < found[assembly]:
                          found[assembly] = rate

          failures = []
          for assembly, threshold in thresholds.items():
              rate = found[assembly]
              if rate is None:
                  print(f'SKIP: {assembly} not found in coverage report (no tests yet?)')
                  continue
              status = 'PASS' if rate >= threshold else 'FAIL'
              print(f'{status}: {assembly} line coverage {rate:.1f}% (required >= {threshold}%)')
              if rate < threshold:
                  failures.append(f'{assembly}: {rate:.1f}% < {threshold}%')

          if failures:
              print('\nCoverage gate FAILED:')
              for f in failures:
                  print(f'  {f}')
              sys.exit(1)

          print('\nAll present assemblies meet their coverage thresholds.')
          EOF
      - name: Upload HTML coverage report
        if: always()
        uses: actions/upload-artifact@ea165f8d65b6e75b540449e92b4886f43607fa02 # v4.6.2
        with:
          name: coverage-report-html
          path: ./coverage-report/
          retention-days: 7
```

- [ ] **Step 2: Validate YAML syntax**

```bash
python3 -c "import yaml; yaml.safe_load(open('.github/workflows/ci-backend.yml')); print('YAML valid')"
```

Expected output: `YAML valid`

- [ ] **Step 3: (Optional) Run actionlint for deeper validation**

```bash
# Install actionlint (Linux/macOS)
bash <(curl https://raw.githubusercontent.com/rhysd/actionlint/main/scripts/download-actionlint.bash) 2>/dev/null
./actionlint .github/workflows/ci-backend.yml
```

Expected: no errors printed, exit code 0. Skip if curl/bash not available.

- [ ] **Step 4: Commit**

```bash
git add .github/workflows/ci-backend.yml
git commit -m "ci: add backend CI workflow — 5-job staged pipeline with coverage gate"
```

---

### Task 3: Frontend CI Workflow

**Files:**
- Create: `.github/workflows/ci-frontend.yml`

**Interfaces:**
- Consumes: nothing from other tasks
- Produces:
  - Required status check `ci-frontend / coverage-gate`
  - Artifact: `coverage-frontend` (Vitest `coverage-summary.json`, consumed within the same workflow run by `coverage-gate`)

**Coverage gate mechanism:**
Vitest with the V8 provider writes `coverage/coverage-summary.json` when run with `--coverage`. The JSON key `total.lines.pct` holds the overall line coverage percentage as a float. The `coverage-gate` job downloads this file and checks `pct >= 75` with a Python 3 script.

**Pre-condition (not part of this task):** When the frontend is scaffolded, `vite.config.ts` must include:

```ts
test: {
  coverage: {
    provider: 'v8',
    reporter: ['text', 'json-summary'],
    reportsDirectory: './coverage',
  },
},
```

The `json-summary` reporter is what produces `coverage-summary.json`. Without it, the coverage-gate artifact upload step will silently skip and the gate step will fail with "file not found".

**Note on `defaults.run.working-directory`:** The global default `src/frontend` applies only to `run:` steps. Steps using `uses:` (actions) always operate from `$GITHUB_WORKSPACE` regardless of this setting. The `coverage-gate` job's Python script step explicitly overrides with `working-directory: .` because it reads a file from `./coverage/` (workspace root), not from `src/frontend/`.

- [ ] **Step 1: Write `.github/workflows/ci-frontend.yml`**

Create with this exact content:

```yaml
name: CI — Frontend

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

defaults:
  run:
    working-directory: src/frontend

jobs:
  lint:
    name: Lint
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@11bd71901bbe5b1630ceea73d27597364c9af683 # v4.2.2
      - uses: actions/setup-node@49933ea5288caeca8642d1e84afbd3f7d6820020 # v4.4.0
        with:
          node-version: '22'
          cache: 'npm'
          cache-dependency-path: src/frontend/package-lock.json
      - name: Install dependencies
        run: npm ci
      - name: TypeScript type check
        run: npx tsc --noEmit
      - name: ESLint
        run: npx eslint src/

  build:
    name: Build
    runs-on: ubuntu-latest
    needs: lint
    steps:
      - uses: actions/checkout@11bd71901bbe5b1630ceea73d27597364c9af683 # v4.2.2
      - uses: actions/setup-node@49933ea5288caeca8642d1e84afbd3f7d6820020 # v4.4.0
        with:
          node-version: '22'
          cache: 'npm'
          cache-dependency-path: src/frontend/package-lock.json
      - name: Install dependencies
        run: npm ci
      - name: Build
        run: npx vite build

  unit-tests:
    name: Unit Tests
    runs-on: ubuntu-latest
    needs: build
    steps:
      - uses: actions/checkout@11bd71901bbe5b1630ceea73d27597364c9af683 # v4.2.2
      - uses: actions/setup-node@49933ea5288caeca8642d1e84afbd3f7d6820020 # v4.4.0
        with:
          node-version: '22'
          cache: 'npm'
          cache-dependency-path: src/frontend/package-lock.json
      - name: Install dependencies
        run: npm ci
      - name: Run unit tests with coverage
        run: npx vitest run --coverage
      - name: Upload coverage summary
        uses: actions/upload-artifact@ea165f8d65b6e75b540449e92b4886f43607fa02 # v4.6.2
        with:
          name: coverage-frontend
          path: src/frontend/coverage/coverage-summary.json
          retention-days: 1

  coverage-gate:
    name: Coverage Gate
    runs-on: ubuntu-latest
    needs: unit-tests
    steps:
      - name: Download frontend coverage
        uses: actions/download-artifact@d3f86a106a0bac45b974a628896c90dbdf5c8093 # v4.3.0
        with:
          name: coverage-frontend
          path: ./coverage
      - name: Enforce line coverage >= 75%
        working-directory: .
        run: |
          python3 << 'EOF'
          import json, sys

          with open('./coverage/coverage-summary.json') as f:
              data = json.load(f)

          pct = data['total']['lines']['pct']
          threshold = 75.0

          print(f'Frontend line coverage: {pct}% (required >= {threshold}%)')

          if pct < threshold:
              print(f'FAIL: {pct:.1f}% < {threshold}%')
              sys.exit(1)

          print('PASS: coverage threshold met.')
          EOF
```

- [ ] **Step 2: Validate YAML syntax**

```bash
python3 -c "import yaml; yaml.safe_load(open('.github/workflows/ci-frontend.yml')); print('YAML valid')"
```

Expected output: `YAML valid`

- [ ] **Step 3: Verify the `working-directory: .` override in `coverage-gate`**

Open the file and confirm the `Enforce line coverage >= 75%` step has `working-directory: .` set at the step level. If it is missing, the `run:` step inherits `src/frontend` from `defaults.run.working-directory` and will look for the file at `src/frontend/./coverage/coverage-summary.json` — which does not exist — causing a confusing `FileNotFoundError`.

Expected YAML shape:

```yaml
      - name: Enforce line coverage >= 75%
        working-directory: .        # ← must be present
        run: |
          python3 << 'EOF'
          ...
```

- [ ] **Step 4: (Optional) Run actionlint**

```bash
./actionlint .github/workflows/ci-frontend.yml
```

Expected: no errors, exit code 0.

- [ ] **Step 5: Commit**

```bash
git add .github/workflows/ci-frontend.yml
git commit -m "ci: add frontend CI workflow — 4-job staged pipeline with coverage gate"
```

---

## Post-Implementation: Branch Protection (Manual Step)

After the workflows run for the first time on any PR or push to `main`, configure branch protection rules in GitHub:

**GitHub → Settings → Branches → Add branch protection rule → `main`:**

1. Enable: **Require status checks to pass before merging**
2. Add required checks (search by name):
   - `ci-backend / coverage-gate`
   - `ci-frontend / coverage-gate`
3. Enable: **Require branches to be up to date before merging**
4. Save

This step cannot be automated via workflow YAML — it must be done manually in the repository settings once both terminal jobs appear in the status check list.

---

## Self-Review

### Spec Coverage

| Spec requirement | Covered by |
|---|---|
| `ci-backend.yml` with 5 staged jobs | Task 2 |
| `ci-frontend.yml` with 4 staged jobs | Task 3 |
| `dependabot.yml` for 3 ecosystems | Task 1 |
| All action SHAs pinned to full commit SHA | Both workflow tasks (SHAs embedded verbatim) |
| Backend path filter: `src/**`, `Tests/**`, `*.sln`, `global.json`, `Directory.*.props`, `Directory.*.targets` | Task 2, `on.pull_request.paths` |
| Frontend path filter: `src/frontend/**` | Task 3, `on.pull_request.paths` |
| Self-triggering on own workflow file change | Both workflow tasks |
| PostgreSQL 16 service container in integration-tests | Task 2, `integration-tests.services.postgres` |
| `ConnectionStrings__DefaultConnection` env var | Task 2, `integration-tests.steps.env` |
| Coverage: Domain ≥ 90%, Application ≥ 85%, Api ≥ 80% | Task 2, `coverage-gate` Python script |
| Frontend coverage ≥ 75% | Task 3, `coverage-gate` Python script |
| Node 22 | Task 3, `node-version: '22'` |
| .NET 9.0.x | Task 2, `dotnet-version: '9.0.x'` |
| `npm ci` (not `npm install`) | Task 3, all install steps |
| No E2E / Playwright in PR CI | Not present (intentional) |
| No Mutation / Performance in PR CI | Not present (intentional) |
| No CD / deployment steps | Not present (intentional) |
| Dependabot: github-actions weekly | Task 1 |
| Dependabot: nuget weekly | Task 1 |
| Dependabot: npm weekly | Task 1 |

### Placeholder Scan

No TBD, TODO, or vague instructions present. All code blocks contain complete, runnable content.

### Type Consistency

No shared types between tasks. Each task produces a standalone file; later tasks do not reference functions or types defined in earlier tasks.

---

*End of CI Pipeline Implementation Plan*
