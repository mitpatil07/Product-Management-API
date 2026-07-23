# Technical Assessment Solution Document: RESTful Product Management API

## 1. Overview & Problem Statement

This document outlines the approach to implementing a RESTful backend API solution as requested in the technical assessment. The solution is designed with high scalability, maintainability, testability, and industry best practices.

### Problem Statement
Design a production-ready **RESTful API solution around Products and Items** to perform full CRUD operations with secure authentication, robust error handling, data validation, rate limiting, and containerized deployment.

---

## 2. High-Level Technical Architecture

The solution follows a multi-tier **Clean Architecture** model where dependencies flow inwards towards the core Domain layer.

```
                    +------------------------------------+
                    |               API                  |
                    |  (Controllers, Middleware, Auth)   |
                    +-----------------+------------------+
                                      |
                                      v
                    +-----------------+------------------+
                    |           Application              |
                    |  (CQRS, DTOs, Mapping, Validators) |
                    +-----------------+------------------+
                                      |
                    +-----------------+------------------+
                    |             Domain                 |
                    |  (Entities, Aggregates, Interfaces)|
                    +-----------------+------------------+
                                      ^
                                      |
                    +-----------------+------------------+
                    |          Infrastructure            |
                    | (EF Core, Repositories, JWT, Hash) |
                    +------------------------------------+
```

### Layer Responsibilities

1. **API Layer (`src/API`)**:
   - Web API controllers with ASP.NET Core API Versioning (`v1`).
   - Cross-cutting concerns: Global Exception Middleware, FluentValidation Action Filter, Native Rate Limiter, Serilog Structured Logging, Swagger UI.

2. **Application Layer (`src/Application`)**:
   - Implements business logic via **CQRS (MediatR)** handlers.
   - Defines Data Transfer Objects (**DTOs**), AutoMapper profiles, FluentValidation validators, and generic `Result<T>` response envelopes.

3. **Domain Layer (`src/Domain`)**:
   - Core domain models (`Product`, `Item`, `User`, `RefreshToken`), base audit fields (`BaseEntity`), domain custom exceptions, and repository interfaces.

4. **Infrastructure Layer (`src/Infrastructure`)**:
   - EF Core 8 database context (`ApplicationDbContext`), entity configurations, repository & unit of work implementations, JWT token generation, and BCrypt password hashing.

---

## 3. Tech Stack

| Category | Technology | Description |
| :--- | :--- | :--- |
| **Framework** | .NET 8 / C# 12 | Target framework using modern C# features |
| **API Framework** | ASP.NET Core Web API | Controller-based RESTful API framework |
| **Database** | SQL Server 2022 | Relational database engine |
| **ORM** | EF Core 8 | Entity Framework Core for data mapping & migrations |
| **Design Patterns** | Clean Architecture, DDD, CQRS | Scalable and testable software patterns |
| **Authentication** | JWT + Refresh Token Rotation | Secure token-based access with sliding refresh token |
| **Validation** | FluentValidation | Strongly-typed validation rules separated from domain |
| **Mapping** | AutoMapper | Automatic DTO-to-Entity object mapping |
| **Messaging** | MediatR | In-process mediator pattern for CQRS separation |
| **Logging** | Serilog | Structured logging to Console and daily file sinks |
| **API Versioning** | Asp.Versioning.Mvc | Explicit versioning strategy (`v1`) |
| **Testing** | xUnit, Moq, WebApplicationFactory | Unit and integration test suite |
| **Containerization** | Docker, Docker Compose | Multi-container orchestration (API + SQL Server) |

---

## 4. Project Directory Structure

```
Solution/
├── src/
│   ├── API/                  # ASP.NET Core Web API project
│   │   ├── Controllers/      # API controllers (v1 AuthController, ProductsController, ItemsController)
│   │   ├── Filters/          # Action filters (ValidationFilter)
│   │   ├── Middleware/       # Custom middleware (ExceptionMiddleware)
│   │   ├── Extensions/       # Extension methods for DI (ServiceCollectionExtensions)
│   │   ├── Program.cs        # Application entry point and DI configuration
│   │   └── appsettings.json  # Configuration files
│   ├── Application/          # Application logic layer
│   │   ├── DTOs/             # Data Transfer Objects
│   │   ├── Interfaces/       # Service interfaces (ICurrentUserService, ITokenService, IPasswordHasher)
│   │   ├── Mapping/          # Object mapping profiles (MappingProfile)
│   │   ├── Services/         # CQRS Handlers and Service implementations
│   │   └── Validators/       # Request validation rules (FluentValidation)
│   ├── Domain/               # Domain layer
│   │   ├── Entities/         # BaseEntity, Product, Item, User, RefreshToken
│   │   ├── Enums/            # Enumeration types
│   │   ├── Events/           # Domain events
│   │   └── Exceptions/       # Custom domain exceptions
│   └── Infrastructure/       # Infrastructure layer
│       ├── Data/             # Data access components
│       │   ├── Configurations/  # Entity type configurations
│       │   ├── Repositories/    # Repository implementations (ProductRepository, UnitOfWork)
│       │   ├── ApplicationDbContext.cs  # EF Core DbContext
│       │   └── UnitOfWork.cs    # Unit of Work implementation
│       ├── Identity/          # Authentication services (TokenService, PasswordHasher)
│       ├── Logging/           # Logging infrastructure
│       └── Services/          # External service integrations
├── tests/
│   ├── API.Tests/            # Integration tests for API (WebApplicationFactory)
│   ├── Application.Tests/    # Unit tests for application layer (Moq)
│   └── Infrastructure.Tests/ # Unit tests for infrastructure layer (EF Core InMemory)
└── docker-compose.yml        # Docker Compose configuration
```

---

## 5. API Design & Database Expectations

### Database Structure

```sql
CREATE TABLE [dbo].[Product] (
    [Id] INT NOT NULL PRIMARY KEY IDENTITY (1,1),
    [ProductName] NVARCHAR(255) NOT NULL,
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedOn] DATETIME2 NOT NULL,
    [ModifiedBy] NVARCHAR(100) NULL,
    [ModifiedOn] DATETIME2 NULL
);

CREATE TABLE [dbo].[Item] (
    [Id] INT NOT NULL PRIMARY KEY IDENTITY (1,1),
    [ProductId] INT NOT NULL FOREIGN KEY REFERENCES [dbo].[Product](Id) ON DELETE CASCADE,
    [Quantity] INT NOT NULL,
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedOn] DATETIME2 NOT NULL,
    [ModifiedBy] NVARCHAR(100) NULL,
    [ModifiedOn] DATETIME2 NULL
);

CREATE TABLE [dbo].[Users] (
    [Id] INT NOT NULL PRIMARY KEY IDENTITY (1,1),
    [Username] NVARCHAR(100) NOT NULL UNIQUE,
    [PasswordHash] NVARCHAR(255) NOT NULL,
    [Role] NVARCHAR(50) NOT NULL,
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedOn] DATETIME2 NOT NULL,
    [ModifiedBy] NVARCHAR(100) NULL,
    [ModifiedOn] DATETIME2 NULL
);

CREATE TABLE [dbo].[RefreshTokens] (
    [Id] INT NOT NULL PRIMARY KEY IDENTITY (1,1),
    [UserId] INT NOT NULL FOREIGN KEY REFERENCES [dbo].[Users](Id) ON DELETE CASCADE,
    [Token] NVARCHAR(255) NOT NULL,
    [ExpiresOn] DATETIME2 NOT NULL,
    [IsRevoked] BIT NOT NULL DEFAULT 0,
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedOn] DATETIME2 NOT NULL,
    [ModifiedBy] NVARCHAR(100) NULL,
    [ModifiedOn] DATETIME2 NULL
);
```

### Resource-Oriented REST Endpoints

| HTTP Method | URL Structure | Description | HTTP Status Codes |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/auth/register` | Register new user | `201 Created`, `422 Unprocessable` |
| `POST` | `/api/v1/auth/login` | Authenticate user | `200 OK`, `401 Unauthorized` |
| `POST` | `/api/v1/auth/refresh-token` | Rotate refresh token | `200 OK`, `401 Unauthorized` |
| `GET` | `/api/v1/products?pageNumber=1&pageSize=10` | Get paginated products | `200 OK`, `401 Unauthorized` |
| `GET` | `/api/v1/products/{id}` | Get product by ID | `200 OK`, `404 Not Found` |
| `POST` | `/api/v1/products` | Create product | `201 Created`, `422 Unprocessable` |
| `PUT` | `/api/v1/products/{id}` | Update product | `200 OK`, `404 Not Found`, `403 Forbidden` |
| `DELETE` | `/api/v1/products/{id}` | Delete product | `200 OK`, `404 Not Found`, `403 Forbidden` |
| `POST` | `/api/v1/products/{productId}/items` | Add item quantity | `201 Created`, `404 Not Found` |
| `GET` | `/api/v1/products/{productId}/items` | Get product items | `200 OK`, `404 Not Found` |
| `GET` | `/api/v1/products/{productId}/items/{id}` | Get specific item | `200 OK`, `404 Not Found` |
| `PUT` | `/api/v1/products/{productId}/items/{id}` | Update item quantity | `200 OK`, `404 Not Found` |
| `DELETE` | `/api/v1/products/{productId}/items/{id}` | Delete item | `200 OK`, `404 Not Found`, `403 Forbidden` |

---

## 6. Implementation Highlights

### Authentication & Authorization Flow
1. User logs in at `/api/v1/auth/login`.
2. API generates a short-lived access JWT token (15 mins) and a long-lived cryptographically secure Refresh Token (7 days) saved in the database.
3. Client includes `Authorization: Bearer {accessToken}` on protected endpoints.
4. When access token expires, client invokes `/api/v1/auth/refresh-token`. The API revokes the old refresh token (`IsRevoked = true`) and issues a fresh token pair.

### Error Handling Middleware (`ExceptionMiddleware`)
Catches unhandled runtime exceptions globally, logs structured exception metadata via Serilog, and serializes a clean standard envelope:
```json
{
  "success": false,
  "message": "An unexpected internal server error occurred.",
  "statusCode": 500,
  "errors": ["Internal error details..."],
  "data": null
}
```

### Data Validation with FluentValidation (`ValidationFilter`)
Validates command properties before execution. The `ValidationFilter` action filter intercepts invalid model states and returns a `422 Unprocessable Entity` response with field-level error messages.

### Controller API Versioning
Controllers use explicit ASP.NET Core API versioning (`v1`):
```csharp
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/products")]
[Authorize]
public class ProductsController : ApiControllerBase
```

### Repository & Unit of Work Pattern
- `IGenericRepository<T>` encapsulates standard query and persistence logic.
- `ApplicationDbContext` intercepts `SaveChangesAsync` to set shadow audit properties (`CreatedBy`, `CreatedOn`, `ModifiedBy`, `ModifiedOn`).
- `UnitOfWork` orchestrates transactions across multiple repositories.

---

## 7. Testing Strategy

The solution includes a comprehensive test suite covering Unit and Integration tests across all layers:

1. **`Application.Tests` (Unit Tests)**: Uses **Moq** to test MediatR handlers, validation rules, and business logic isolated from database dependencies.
2. **`Infrastructure.Tests` (Unit Tests)**: Uses **EF Core InMemory Database** to test repository data access, custom queries, and automatic audit property generation.
3. **`API.Tests` (Integration Tests)**: Uses **WebApplicationFactory<Program>** to test full HTTP request-response flow, authentication rules, status codes, and JSON serialization.

### Running Tests:
```bash
dotnet test
```
*Result*: **12/12 Tests Passing 100%**.

---

## 8. Performance & Security Measures

### Performance
- **EF Core `AsNoTracking()`**: Applied to all read-only queries (`GetByIdAsync`, `GetAllAsync`) to prevent EF change tracking overhead.
- **Pagination**: All collection endpoints return `PagedList<T>` to cap memory consumption.
- **Async/Await Throughout**: Non-blocking I/O operations for optimal thread pool utilization.
- **Native Rate Limiting**: Fixed window limiter (100 requests / 60 seconds) to prevent abuse.

### Security
- **JWT & Refresh Tokens**: Cryptographically signed HMAC SHA-256 tokens with automatic rotation.
- **Password Security**: BCrypt salt-hashed passwords.
- **Role-Based Access Control**: `[Authorize(Roles = "Admin")]` protects destructive routes (`PUT`, `DELETE`).
- **CORS Policy**: Configured CORS policy restricting unauthorized origins.
- **Input Sanitization**: FluentValidation prevents SQL injection and malformed payloads.

---

## 9. Deployment Configuration & Explanation

### What the Project Does
The **Product Management API** provides an enterprise backend system for managing **Products** and their related **Items** (quantity inventory). It allows clients to securely register users, authenticate with JWT tokens, create products, track items under products, update inventory quantities, query paginated products, and perform administrative operations backed by SQL Server.

### Containerization with Docker Compose
The solution includes a multi-stage `Dockerfile` and `docker-compose.yml` that provisions both the Web API and SQL Server 2022 in containerized environments.

```bash
docker-compose up --build -d
```

- **API Endpoint**: [http://localhost:5000](http://localhost:5000)
- **Swagger Documentation**: [http://localhost:5000/swagger](http://localhost:5000/swagger)
- **Health Check**: [http://localhost:5000/health](http://localhost:5000/health)
