# Agent Guidelines

## Overview

Utgifter is a personal expense tracking application with:
- **Backend**: .NET 10 API using vertical slice architecture with FastEndpoints
- **Frontend**: Vue 3.5 SPA with Tailwind CSS
- **Database**: PostgreSQL with Dapper micro-ORM
- **Deployment**: Multi-arch Docker container (amd64/arm64)

## Build & Test Commands

### Backend (API)
```bash
dotnet build src/Utgifter.slnx              # Build solution
dotnet run --project src/Utgifter.Api       # Run API (http://localhost:5160)
dotnet test src/Utgifter.Tests              # Run all tests (TUnit framework)
dotnet test src/Utgifter.Tests --filter "FullyQualifiedName~TestMethodName"  # Single test
```

### Frontend (Site)
```bash
bun install                  # Install dependencies
bun run dev                  # Dev server (http://localhost:3000, proxies /api to :5160)
bun run build                # Production build to dist/
bun run lint                 # ESLint check
```

### Docker (Local Development)
```bash
docker compose -f docker/docker-compose.yml up -d   # Start PostgreSQL
```

## Code Style

### C# (Backend)

**File Structure:**
- File-scoped namespaces (no braces)
- Primary constructors for dependency injection
- `internal sealed class` for endpoints
- `public record` or `internal sealed record` for DTOs

**Naming:**
| Element | Convention | Example |
|---------|------------|---------|
| Private fields | `_camelCase` | `_connectionString` |
| Classes/Records | `PascalCase` | `DataBaseOptions` |
| SQL columns | `lowercase` | `originalCurrency` |

**SQL Conventions:**
- Raw string literals for multi-line SQL (`"""SELECT ... FROM ..."""`)
- Parameterized queries always (never string interpolation)
- Lowercase column names in SQL, PascalCase in C# (Dapper maps automatically)

**Example Endpoint:**
```csharp
namespace Utgifter.Api.Features.Expenses.List;

internal sealed class Endpoint(IOptions<DataBaseOptions> options) : Endpoint<Request, Response>
{
    private readonly string _connectionString = options.Value.ConnectionString;

    public override void Configure()
    {
        Get("/expenses");
        AllowAnonymous();
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        var expenses = await connection.QueryAsync<Expense>(
            """
            SELECT id, date, person, store, city, originalCurrency, amount, hash, category, shared, trip
            FROM Expenses
            ORDER BY Date DESC
            LIMIT @PageSize OFFSET @PageSize * @PageNumber
            """,
            new { req.PageSize, req.PageNumber }
        );
        await SendOkAsync(new Response([.. expenses]), ct);
    }
}

internal sealed record Request(int PageNumber, int PageSize);
internal sealed record Response(Expense[] Expenses);
```

### TypeScript/Vue (Frontend)

**Formatting (Prettier):**
- No semicolons
- Single quotes
- 2-space indent
- 100 char line width
- No trailing commas

**Component Pattern:**
```vue
<script setup lang="ts">
import { ref } from 'vue'
import type { Expense } from '@/api/server'

type Props = { expenses: Expense[] }
defineProps<Props>()

const emit = defineEmits<{
  update: [expense: Expense]
}>()

const model = defineModel<string>()
</script>

<template>
  <!-- Tailwind utility classes directly in template -->
</template>
```

**Key Conventions:**
- `<script setup lang="ts">` always
- Type imports: `import type { Foo } from '@/api/server'`
- Path alias: `@/` maps to `src/`
- Barrel exports via `index.ts` for component folders
- `defineModel()` for v-model bindings
- Top-level await with `<Suspense>` wrapper for async data

## Architecture

### Backend: Vertical Slice Architecture

```
src/Utgifter.Api/
├── Configuration/       # Options classes
├── DataBase/
│   └── Migrations/      # Sequential SQL migration files
├── Extensions/          # Helper extension methods
├── Features/            # Vertical slices
│   ├── Categories/
│   │   └── Get/
│   │       ├── Endpoint.cs
│   │       └── Response.cs
│   ├── Expenses/
│   │   ├── Delete/
│   │   ├── Insert/
│   │   ├── List/
│   │   ├── Update/
│   │   └── Upload/
│   │       └── ExcelParsers/
│   ├── Rules/
│   │   ├── Create/
│   │   ├── Delete/
│   │   ├── List/
│   │   ├── RenameStores/
│   │   │   ├── Apply/
│   │   │   └── Preview/
│   │   └── Update/
│   └── Trips/
│       └── GetAll/
└── Models/              # Domain records
```

**Endpoint Base Classes:**
| Scenario | Base Class |
|----------|------------|
| Request + Response | `Endpoint<Request, Response>` |
| Request only | `Endpoint<Request>` |
| Response only | `EndpointWithoutRequest<Response>` |

### Frontend: Component Structure

```
src/Utgifter.Site/src/
├── api/                 # Typed fetch wrappers
│   ├── server.ts        # Expenses API + types
│   ├── categories.ts
│   ├── rules.ts
│   ├── trips.ts
│   └── error.ts
├── components/
│   ├── ExpensesTable/   # Feature component with barrel export
│   └── ui/              # Generic UI primitives
│       ├── autocomplete/
│       ├── button.vue
│       ├── modal.vue
│       ├── select.vue
│       └── switch.vue
├── router/
│   └── index.ts
└── views/
    ├── Rules/           # Co-located view components
    │   ├── Table/
    │   ├── NewRule.vue
    │   └── RenameStoresModal.vue
    ├── Expenses.vue
    └── ExpenseProcessing.vue
```

**State Management:** No Pinia/Vuex - local refs with prop/emit patterns only.

## API Endpoints

| Path | Method | Feature |
|------|--------|---------|
| `/api/categories` | GET | List distinct categories |
| `/api/expenses` | GET | List expenses (paginated) |
| `/api/expenses` | POST | Insert new expenses |
| `/api/expenses` | PUT | Batch update expenses |
| `/api/expenses/{id}` | DELETE | Delete single expense |
| `/api/expenses/upload` | POST | Upload Excel, parse, apply rules |
| `/api/rules` | GET | List rules (paginated) |
| `/api/rules` | POST | Create rule |
| `/api/rules/{id}` | PUT | Update rule |
| `/api/rules/{id}` | DELETE | Delete rule |
| `/api/rules/{id}/rename-preview` | GET | Preview rule application |
| `/api/rules/{id}/rename-apply` | POST | Apply store rename |
| `/api/trips` | GET | List distinct trip names |

## Domain Models

### Expense
```csharp
public record Expense(
    Guid Id,
    DateOnly Date,
    string Person,
    string Store,
    string City,
    string OriginalCurrency,
    decimal Amount,
    string Hash,              // SHA256 for duplicate detection
    string? Category = null,
    bool Shared = true,
    string? Trip = null       // null = not a trip
);
```

### Rule
```csharp
public record Rule(
    Guid Id,
    string ExpectedStore,     // Supports * wildcards
    string? NewStore,
    string? NewCategory,
    bool? Shared,
    bool? Trip
);
```

**Wildcard Patterns:** `*SUFFIX` (ends with), `PREFIX*` (starts with), `*CONTAINS*`, or exact match.

## Database

### Migrations
Custom file-based runner in `Migration.cs`:
- Files named `XX-Description.sql` in `DataBase/Migrations/`
- Tracks applied migrations in `migrations` table
- Runs automatically on startup

### Type Mappings
- `DateOnly` → custom `SqlDateOnlyMapper` handler
- All store/category/person values normalized to `UPPERCASE` on insert

## Testing (TUnit)

```csharp
public class ExcelParserTest
{
    [Test]
    public async Task ParseExcelFile()
    {
        var fileName = "data/transactions.xlsx";
        await using var file = File.OpenRead(fileName);
        
        var expenses = parser.Parse();
        
        await Assert.That(expenses.Length).IsEqualTo(expected.Length);
    }
}
```

Global setup in `GlobalSetup.cs` sets EPPlus license.

## Tech Stack

### Backend
| Package | Version | Purpose |
|---------|---------|---------|
| .NET | 10.0 | Runtime |
| FastEndpoints | 7.1.1 | Minimal API framework |
| Dapper | 2.1.66 | Micro-ORM |
| Npgsql | 10.0.1 | PostgreSQL driver |
| EPPlus | 8.4.0 | Excel parsing |
| TUnit | 1.6.28 | Testing framework |

### Frontend
| Package | Version | Purpose |
|---------|---------|---------|
| Vue | 3.5.18 | UI framework |
| Vue Router | 4.5.1 | Routing |
| Tailwind CSS | 4.1.11 | Styling |
| Vite | 7.0.6 | Build tool |
| Bun | latest | Package manager & runtime |
| Fuse.js | 7.1.0 | Fuzzy search |
| VueUse | 13.6.0 | Composition utilities |

## Deployment

### Docker
- Multi-stage build: SDK → Bun → Alpine runtime
- Single container serves API + static frontend
- Port 80, `ASPNETCORE_ENVIRONMENT=Production`

### CI/CD
- GitHub Actions workflow on push to `main` or version tags
- Multi-arch builds (linux/amd64, linux/arm64)
- Published to `ghcr.io/ldellisola/utgifter`
