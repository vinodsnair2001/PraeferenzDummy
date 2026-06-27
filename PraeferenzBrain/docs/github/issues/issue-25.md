# Meeting Summary — Issue #25: [T25] Claude Skill File — .claude/skills/praeferenz-roo.md

**Date:** 2026-06-27
**Branch:** issue-25-claude-skill-developer-guide
**Participants:** Praveen (Architect), Sreejith (Senior Dev), Sojiya (UX), Vinod (Planner)

---

## Issue Context

- **Issue:** #25 — [T25] Claude skill file — .claude/skills/praeferenz-roo.md developer guide
- **URL:** https://github.com/vinodsnair2001/PraeferenzDummy/issues/25
- **Phase:** 7 — Tooling
- **Label:** `skill`
- **Depends on:** T13 (architecture established — satisfied by ESSENTIAL/ handbooks)

---

## Business Decisions

This is a pure tooling artifact. No business logic, no user-facing features. The goal is to enable any AI assistant (Claude Code) opening this repository to instantly know the architecture rules, coding patterns, and procedural guides without reading all ESSENTIAL/ files from scratch. This reduces the risk of architecture violations introduced by AI agents working on the project.

---

## Functional Decisions

Create a single Markdown file at `.claude/skills/praeferenz-roo.md` with:
- Valid YAML front matter (name, description)
- 8 specified sections covering project identity, architecture rules, how-to guides, reference tables, and a pre-completion checklist
- File must be discoverable as `/praeferenz-roo` in Claude Code

---

## Technical Decisions

### File Format and Location
- **Path:** `.claude/skills/praeferenz-roo.md` (flat file, not a directory)
- **Format:** Markdown with YAML front matter
- **Convention:** Flat `.md` file is correct for reference-type skills that require no support files. The existing `team-meeting` skill uses the directory pattern because it has a SKILL.md; `praeferenz-roo` is purely a reference document.
- **Skill name:** `praeferenz-roo` — must match the filename stem exactly and appear in the `name:` YAML field

### YAML Front Matter (exact)
```yaml
---
name: praeferenz-roo
description: >
  Development guide for PraeferenzRoO — EU Preferential Rules of Origin
  Calculation System. Use when adding features, fixing bugs, or extending
  the rule engine. Covers Clean Architecture rules, CQRS patterns, and
  the metadata-driven rule engine design.
---
```

### Section Content Sources (per section)

| Section | Source |
|---|---|
| 1 — Project Identity | `ESSENTIAL/architecture.md` Section 1 (Vision) — 3-4 paragraphs: purpose, users, "originating" definition, regulatory context |
| 2 — Architecture Rules | `CLAUDE.md` Non-Negotiable Rules + `ESSENTIAL/coding-standards.md` Section 1.3 — exactly 7 rules, imperative sentences, bold rule names |
| 3 — Add Backend Feature | `ESSENTIAL/architecture.md` Sections 4-5 + `ESSENTIAL/coding-standards.md` Section 3 — 8 steps, include key method signature per step |
| 4 — Add Rule Type | `ESSENTIAL/rule-engine.md` Section 24 — 6 steps, CRITICAL: use "DI key string" language, NOT "C# enum" |
| 5 — Data Access Cheatsheet | 3-column table: Operation / Correct Tool / Example — 8 rows |
| 6 — Frontend Patterns | `ESSENTIAL/coding-standards.md` Sections 10 and 14 — 3-layer pattern with Zod/RHF wiring + loading/error state |
| 7 — File Location Map | `ESSENTIAL/architecture.md` Section 4 — table of Layer → Canonical Path; preamble: design-intent paths |
| 8 — Pre-Completion Checklist | Mirror `ESSENTIAL/coding-standards.md` Section 23.1 — 17 items including rule-engine completeness item |

### Critical Correction — Section 4 (Rule Type)
The issue body says "Add to `RuleType` enum in Domain." This is INCORRECT for the rule engine context. The correct behavior:
- `RuleDefinition.RuleType` is a **string** that serves as the DI key
- New rule types are registered via `services.AddKeyedScoped<IRule, [Name]Rule>("[Name]Rule")`
- The DI key string must exactly match the `rule_type` value in the `rule_engine.rule_definitions` DB row
- There is NO C# enum to update for new rule engine rule types
- If there is a `RuleCategory` constant for UI grouping, that may be added as a string constant — but it is not a formal enum

### Data Access Cheatsheet (Section 5) — agreed table content

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

### File Location Map (Section 7) — agreed canonical paths

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

## Rejected Approaches

| Approach | Reason Rejected |
|---|---|
| Directory-wrapped format (`.claude/skills/praeferenz-roo/SKILL.md`) | Unnecessary — directory pattern is for skills needing support files. This is a flat reference document. |
| C# enum update for RuleType in Section 4 | Incorrect. `RuleDefinition.RuleType` is a string DI key. No enum exists for rule engine rule types. |
| Prose instead of table for Section 5 (Data Access) | Reduces scannability. Team agreed on Markdown table format. |
| Verbose tutorial style (explaining "why") | Handbooks cover "why." Skill file is a reference card — prescriptive and terse. |

---

## Risks

| Risk | Type | Likelihood | Impact | Mitigation |
|---|---|---|---|---|
| Content inaccuracy vs. ESSENTIAL/ handbooks | Quality | Low | High | Implementation agent reads all ESSENTIAL/ files before writing; content sources specified per section |
| Skill not discoverable (flat .md format) | Technical | Low | Medium | Flat .md files are a valid Claude Code skill pattern; fallback is to wrap in directory |
| RuleType enum confusion in Section 4 | Quality | Medium (if unmitigated) | Medium | Decision Summary explicitly requires "DI key string" language, not "enum" |
| Section 7 staleness as project grows | Timeline/Quality | Medium (long-term) | Low | Written as design intent with explicit preamble; not a listing of current files |
| PraeferenzBrain/decisions/ does not exist | Technical | High (directory missing) | Low | Implementation agent creates directory |

---

## Dependencies

- ESSENTIAL/ handbooks (all 8) — must be read by implementation agent before writing content
- Existing `.claude/skills/` directory — already exists in the repository
- No blocking dependencies on other open issues

---

## Open Questions

None. All requirements are fully specified. All ambiguities resolved in the team meeting.

---

## Final Implementation Approach

Create exactly one new file: `.claude/skills/praeferenz-roo.md`

The file is a flat Markdown document with YAML front matter and 8 sections. No backend code, no frontend code, no migration, no tests, no CQRS artifacts. The sole deliverable is the documentation file itself.

**Deliverable scope:** Single file only. Any implementation that creates additional files is out of scope.

**Verification:** After creation, manual invocation of `/praeferenz-roo` in Claude Code confirms skill discoverability.

---

## Action Items

**Backend:**
- None

**Frontend:**
- None

**Database:**
- None

**Testing:**
- None (no automated tests; manual verification via `/praeferenz-roo` in Claude Code)

**Documentation / Tooling:**
- Create `.claude/skills/praeferenz-roo.md` with valid YAML front matter and all 8 sections
- Read all ESSENTIAL/ files before writing content
- Use content sources as specified in the Technical Decisions table above
- CRITICAL: Section 4 must use "DI string key" language, not "C# enum"
- Section 5 must be a 3-column Markdown table (not prose)
- Section 6 must include loading and error state examples
- Section 7 preamble must state paths are design-intent targets
- Section 8 must include 17-item checklist mirroring coding-standards.md Section 23.1

**Deployment:**
- None (documentation file; no deployment required)
