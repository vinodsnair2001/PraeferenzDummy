---
name: team-meeting
description: Use when a GitHub issue or feature request for PraeferenzDummy needs team-level design review before implementation — functional requirements, architectural changes, database/API/UI changes, security-sensitive features, or significant technical complexity. Produces a structured Decision Summary consumed by the Praeferenz Orchestrator. Trigger phrases include "discuss with the team", "run a team meeting", "review this issue", "analyze this requirement", "design this feature", "what would the team decide", "evaluate the architecture", "should we implement this".
---

# PraeferenzDummy Team Meeting Simulator

This skill simulates a realistic internal project meeting for the **PraeferenzDummy** project.

The objective is to review a requirement exactly as an experienced product development team would — before any implementation begins.

The meeting identifies:

- Functional requirements
- Hidden requirements
- Technical implications
- Database impacts
- API impacts
- UI/UX implications
- Security concerns
- Performance considerations
- Testing requirements
- Risks
- Implementation approach

The output becomes the project's design decision and may later be stored as project documentation for downstream agents (especially the **Praeferenz Orchestrator**).

---

## Participants

### 🏛️ Praveen — Software Architect

**Owns:** Overall architecture of PraeferenzDummy.

Focuses on: Clean Architecture, DDD boundaries, modular design, service responsibilities, API contracts, database design, maintainability, future scalability, integration with existing modules.

Typical questions:
- Does this fit our architecture?
- Are we introducing unwanted coupling?
- Should this become a separate module?
- Is this reusable across tenants?
- Will this affect future extensibility?

---

### 💻 Sreejith — Senior Programmer

**Owns:** Turning architecture into working code.

Focuses on: Implementation complexity, coding standards, existing code reuse, edge cases, error handling, validation, performance, refactoring opportunities, developer experience.

Typical questions:
- How difficult is this to implement correctly?
- Can we simplify the approach?
- What edge cases exist?
- What validations are missing from the spec?
- Are there hidden implementation costs?

---

### 🎨 Sojiya — UI / UX Designer

**Owns:** Ensuring the feature is intuitive and user-friendly.

Focuses on: User workflow, accessibility, responsive layouts, visual consistency, shadcn/ui design patterns, form usability, user feedback, error messaging, empty states, loading states.

Typical questions:
- Will users understand this without training?
- Can we reduce the number of clicks?
- Is the workflow obvious from the interface?
- Are validation messages clear and actionable?
- Is the interface consistent with the rest of PraeferenzDummy?

---

### 📋 Vinod — Project Planner

**Owns:** Ensuring the project remains deliverable.

Focuses on: Scope, timeline, dependencies, risks, prioritization, GitHub issue breakdown, sprint planning, documentation, acceptance criteria.

Typical questions:
- Is this MVP, or is this scope creep?
- Can this be split into smaller issues?
- What blocks implementation?
- What are the delivery risks?
- What should be deferred to a future iteration?

---

## Meeting Structure

### 1. Requirement Summary

Summarize the GitHub issue or requirement.

Identify:
- Functional requirements (what the system must do)
- Technical requirements (how it must do it)
- Non-functional requirements (performance, security, accessibility)
- Assumptions being made
- Missing information that must be resolved before implementation

If requirements are ambiguous, **Vinod identifies the gaps first** and the meeting pauses until they are resolved.

---

### 2. Initial Opinions

Each participant gives their first independent assessment.

**Order:**
1. Praveen (architecture fit, module placement, extensibility)
2. Sreejith (implementation complexity, edge cases, code reuse)
3. Sojiya (user workflow, UI components, accessibility)
4. Vinod (scope, dependencies, MVP boundary, risks)

Each participant contributes 2–5 concise paragraphs focused on their area of expertise. Initial opinions should be honest — disagreement at this stage is productive.

---

### 3. Discussion Round 1 — Challenge Assumptions

Participants challenge each other's initial positions.

Examples of healthy tension:
- Architecture ideals vs. implementation pragmatism
- UX aspirations vs. backend complexity
- Delivery timeline vs. code quality
- Performance optimisation vs. simplicity
- Security requirements vs. user convenience

Disagreement is encouraged when technically justified. Every concern must be backed by a reason.

---

### 4. Discussion Round 2 — Refine the Design

Resolve the tensions from Round 1 and produce concrete design decisions for each dimension:

- **Database changes** — new tables, new columns, migration strategy
- **API design** — endpoint paths, request/response shapes, HTTP methods
- **Frontend architecture** — page structure, component hierarchy, state management
- **Component reuse** — which existing shadcn/ui or internal components apply
- **Validation** — FluentValidation rules (backend), Zod schemas (frontend)
- **Authorization** — which roles can perform which actions, where enforcement lives
- **Error handling** — domain exceptions, error envelope, user-facing messages
- **Logging** — what events to log, at which level, with which structured fields
- **Performance** — pagination, caching, query optimisation
- **Security** — tenant isolation, parameterised queries, secret handling
- **Testing** — unit/integration/E2E test strategy, coverage targets

---

### 5. Final Technical Decision

Produce a unified recommendation. Structure it as:

#### Architecture
- Module placement (which layer, which feature folder)
- New services or interfaces required
- Repository changes
- Domain events

#### Backend
- API endpoints (verb + path)
- FluentValidation rules
- Business rules and domain logic location
- Database changes (tables, columns, indexes, migrations)
- Dapper queries needed
- EF Core entities and configurations

#### Frontend
- New pages and routes
- Dialogs and drawers
- Forms (React Hook Form + Zod schemas)
- shadcn/ui components to use
- TanStack Query hooks
- Zustand store changes (if any)
- RBAC gates in the UI

#### Security
- Role-based access control requirements
- Tenant isolation checks
- Input validation layers
- File handling (if applicable)

#### Testing
- Unit test targets (domain logic, handlers)
- Integration test targets (repositories, API endpoints)
- Frontend component test targets (happy path, error, loading, empty)
- Coverage expectations

---

### 6. Risks

List all identified risks:

| Risk | Type | Likelihood | Impact | Mitigation |
|---|---|---|---|---|
| ... | Technical / Business / Timeline | High / Med / Low | High / Med / Low | ... |

---

### 7. Deferred Items

List items that were discussed but intentionally postponed. For each:
- What was deferred
- Why it was deferred (complexity, scope, dependency)
- Suggested future issue or label

---

### 8. Action Items

Assign concrete next steps to each role:

**Praveen**
- Finalise API contracts
- Review architecture diagram updates
- Approve any new design patterns

**Sreejith**
- Implement backend (entity, CQRS, Dapper, repository, controller)
- Write unit and integration tests
- Add validation

**Sojiya**
- Design shadcn/ui component layout
- Specify loading, error, and empty states
- Review accessibility requirements

**Vinod**
- Update GitHub issues with acceptance criteria
- Create or update sprint plan
- Document final decisions in PraeferenzBrain

---

## Decision Summary (Required Output)

The meeting must conclude with a **Decision Summary** in this format:

```
## Decision Summary — [Feature Name]

### Agreed Architecture
[Description of module placement, layer boundaries, new patterns]

### Agreed Implementation Approach
[Backend: CQRS artifacts, DB changes, API design]
[Frontend: pages, components, state, forms]

### Accepted Trade-offs
[What was simplified and why]

### Risks
[List]

### Deferred Items
[List]

### Action Plan
[Praveen / Sreejith / Sojiya / Vinod — specific tasks]
```

This summary is suitable for saving to `PraeferenzBrain/decisions/` as a pre-implementation record and may be consumed by the **Praeferenz Orchestrator** when delegating to `backend-developer`, `frontend-developer`, `qa-validator`, and `technical-writer` agents.

---

## Guidelines

- Debate professionally — challenge ideas, not people.
- Encourage constructive disagreement; avoid superficial consensus.
- Every concern must be technically justified.
- Prefer simplicity unless complexity provides measurable long-term value.
- Consider existing PraeferenzDummy architecture before proposing new patterns.
- Recommend reuse of existing components wherever possible.
- Follow project coding standards, architecture guidelines, security guidelines, testing guidelines, and stack documentation at all times.
- If the requirement is ambiguous, Vinod identifies the missing information **before the discussion proceeds**.
- The meeting ends only when the Decision Summary is complete and all four participants have confirmed it.
