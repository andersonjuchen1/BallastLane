# Task Management API

A REST API for managing personal tasks, built with **.NET 10 / ASP.NET Core** following
**Clean Architecture**. Users register, authenticate with JWT bearer tokens, and manage
their own tasks (with statuses and due dates). Ownership is enforced server-side from JWT
claims — a user can only ever see or touch their own tasks.

> **User story:** *As a busy professional, I want to create, organize, and track my
> personal tasks with statuses and due dates, so that I can prioritize my work and never
> miss a deadline.*

---

## Architecture

Five projects, with a strict inward-pointing dependency rule:

```
Api  ─►  Infrastructure  ─►  Application  ─►  Domain
                 └──────────────►────────────────┘
```

| Project | Responsibility | Notable dependencies |
|---|---|---|
| **Domain** | Entities (`User`, `TaskItem`), `TaskItemStatus` enum, invariants | *none* |
| **Application** | Use-case services, DTOs, FluentValidation validators, repository & service interfaces, unit-of-work abstraction | FluentValidation (no EF Core) |
| **Infrastructure** | EF Core `DbContext` + configs, repositories, `UnitOfWork`, `PasswordHasher`, JWT generation, seeding, migrations | EF Core, SQL Server, Identity |
| **Api** | Thin controllers, DI composition, JWT auth, validation filter, exception handling, Swagger | ASP.NET Core |
| **UnitTests** | xUnit + Moq + FluentAssertions covering the Application layer (TDD) | — |

The **Domain** has zero external dependencies; the **Application** layer never references
EF Core. Both were verified as part of the build.

---

## Tech stack

- .NET 10, ASP.NET Core Web API (controllers)
- Entity Framework Core 10 + SQL Server (code-first)
- JWT bearer authentication (HMAC-SHA256)
- Password hashing via ASP.NET Core `PasswordHasher<T>` (PBKDF2)
- FluentValidation
- Swagger / Swashbuckle
- xUnit, Moq, FluentAssertions (Apache-licensed 7.x)

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server reachable at `localhost` with Windows authentication
  (LocalDB / Express / Developer all work — adjust the connection string otherwise)
- `dotnet-ef` tool (only needed to run migrations manually):
  ```bash
  dotnet tool install --global dotnet-ef
  ```

---

## Setup

### 1. Connection string

`src/TaskManagement.Api/appsettings.json` points at a local database:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=TaskManagementDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Change `Server=` if your instance differs (e.g. `Server=.\\SQLEXPRESS`).

### 2. JWT signing secret (required)

The signing key is **not** stored in the repo. Provide it through user-secrets:

```bash
dotnet user-secrets set "Jwt:Key" "<a long random secret, 32+ chars>" -p src/TaskManagement.Api
```

Generate one, for example:

```bash
# bash
openssl rand -base64 64
```

The app fails fast at startup with a clear message if `Jwt:Key` is missing.

### 3. Database

In **Development** the API automatically applies migrations and seeds demo data on startup —
no manual step needed. To do it by hand:

```bash
dotnet ef database update -p src/TaskManagement.Infrastructure -s src/TaskManagement.Infrastructure
```

---

## Running

```bash
dotnet run --project src/TaskManagement.Api
```

Then open Swagger at **http://localhost:5276/swagger**.

Use **Authorize** in Swagger (or an `Authorization: Bearer <token>` header) after logging in.

### Demo credentials (seeded in Development)

| Username | Password |
|---|---|
| `demo` | `Passw0rd!` |

The demo account comes with four sample tasks.

---

## API surface

All task endpoints require a bearer token. `register` and `login` are anonymous.

| Method | Route | Auth | Description | Success |
|---|---|---|---|---|
| POST | `/api/auth/register` | anon | Create account, returns JWT (auto-login) | 200 |
| POST | `/api/auth/login` | anon | Authenticate, returns JWT | 200 |
| GET | `/api/auth/me` | ✔ | Current user profile | 200 |
| GET | `/api/tasks` | ✔ | List own tasks (optional `?status=Pending\|InProgress\|Completed`) | 200 |
| GET | `/api/tasks/{id}` | ✔ | Get one own task | 200 |
| POST | `/api/tasks` | ✔ | Create a task | 201 + `Location` |
| PUT | `/api/tasks/{id}` | ✔ | Update a task | 200 |
| DELETE | `/api/tasks/{id}` | ✔ | Delete a task | 204 |

### Status codes

- **400** — validation failure, with field-level `ProblemDetails` (`errors` keyed by field)
- **401** — missing/invalid token, or bad login credentials (generic message; never reveals
  which field was wrong)
- **404** — task missing **or owned by another user** (existence is never leaked)
- **409** — username or email already registered
- **500** — unexpected error, generic `ProblemDetails` (stack traces are logged, never returned)

### Business rules

- A task requires a **title** and a **due date that is not in the past** (enforced on create).
- `status` is one of `Pending`, `InProgress`, `Completed` (serialized by name).
- Ownership is always taken from the JWT `sub` claim — never from the request body.

See [`src/TaskManagement.Api/TaskManagement.Api.http`](src/TaskManagement.Api/TaskManagement.Api.http)
for ready-to-run request examples.

---

## Tests

```bash
dotnet test
```

The Application layer was built test-first (TDD): each use case has a failing test committed
before its implementation. Tests cover task CRUD with ownership enforcement, the status
filter, registration (uniqueness + password complexity), login (generic failure), and the
validators.

---

## Notable design decisions

- **Guid primary keys** — non-sequential, so task ids can't be enumerated across users
  (defense in depth alongside the ownership 404).
- **Ownership via 404, not 403** — a task owned by someone else is reported as *not found*,
  so the API never confirms another user's data exists.
- **Rich domain entities** — private setters and guard clauses; `TaskItem.Update()`
  encapsulates edits, and `Id`/`UserId` are immutable after construction.
- **Unit of Work** — repositories stage writes; services own the transaction boundary via
  `IUnitOfWork.SaveChangesAsync`, so multi-entity operations commit atomically as the app grows.
- **Validation at the API boundary** — a filter runs FluentValidation and returns field-level
  400s, keeping controllers and services free of validation plumbing.
- **Status stored as a string** in SQL Server — readable and resilient to enum reordering.
- **Password policy** — length 8–128 plus upper, lower, and a special character.
