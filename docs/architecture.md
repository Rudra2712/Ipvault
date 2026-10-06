# IPVault — System Architecture Documentation

## Overview

IPVault is designed with a **clean, beginner-friendly layered architecture** adhering to SOLID principles and separation of concerns.

```text
┌─────────────────────────────────────────────────────────┐
│                    Angular Frontend                     │
│   (Standalone Components, Signals, Reactive Forms,      │
│    Auth Interceptor, Route Guards, Typed Services)      │
└────────────────────────────┬────────────────────────────┘
                             │ HTTP / JSON
                             ▼
┌─────────────────────────────────────────────────────────┐
│               ASP.NET Core Web API 8.0                  │
│                                                         │
│  ├── Middleware (Global Exception Handler)              │
│  ├── Authentication (JWT Bearer Token Validation)       │
│  ├── Controllers (Thin REST Endpoints)                  │
│  ├── Services (Business Rules & Domain Validations)     │
│  ├── Repositories (Data Access Abstraction)             │
│  └── Entity Framework Core (LINQ & Fluent API Mapping)  │
└────────────────────────────┬────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────┐
│              Relational Database Engine                 │
│      (Microsoft SQL Server / SQLite Local Database)     │
└─────────────────────────────────────────────────────────┘
```

---

## 1. Backend Layer Breakdown

### Controllers (`IPVault.API/Controllers`)
- Keep controllers thin and declarative.
- Extract authentication claims (`UserId`, `Role`) from JWT via `BaseApiController`.
- Return standardized `ApiResponse<T>` wrappers with appropriate HTTP status codes (200, 201, 400, 403, 404).

### Service Layer (`IPVault.API/Services`)
- Encapsulate all business logic and rule validation:
  - **Rule 1:** Expiry date cannot be earlier than filing date.
  - **Rule 2:** A patent must have at least one inventor.
  - **Rule 3:** Only Admin can delete a patent record.
  - **Rule 4:** Patent numbers must remain unique across the system.
  - **Rule 5:** Clients with active patents cannot be deleted.

### Repository Layer (`IPVault.API/Repositories`)
- Generic base repository `IRepository<T>` provides core CRUD operations.
- Specialized repositories handle domain queries, eager-loading (`Include`), and server-side filtering and pagination.

### Data Layer (`IPVault.API/Data`)
- `ApplicationDbContext`: Configured via Fluent API with composite keys, cascading rules, and unique indexes.
- `DbInitializer`: Automatically populates realistic corporate clients, inventors, patent records, and mock documents on initial launch.

---

## 2. Frontend Layer Breakdown

### Core Module (`src/app/core`)
- `AuthService`: Manages user authentication state and role queries using Angular Signals (`currentUser`, `isAuthenticated`, `isAdmin`, `canEdit`).
- `authInterceptor`: Automatically appends `Authorization: Bearer <token>` to outgoing HTTP requests and handles 401 Unauthorized responses.
- `authGuard` & `roleGuard`: Restrict navigation to protected pages and enforce permissions.

### Shared Module (`src/app/shared`)
- Reusable UI widgets: `PaginationComponent`, `ToastContainerComponent`, status badges, and TypeScript interfaces.

### Feature Modules (`src/app/features`)
- `auth/login`: Interactive login with quick-fill buttons for testing roles.
- `dashboard`: Real-time KPI summaries, status distribution progress bars, filing trends, and expiry watch tables.
- `patents`: Search, filters, pagination, creation/editing reactive forms, and detail views.
- `clients`: Corporate client management with patent counts and delete protection.
- `inventors`: Research scientist directory.
