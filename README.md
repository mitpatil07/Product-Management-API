# Product Management RESTful API & Management Dashboard

A production-ready RESTful backend API and integrated frontend management dashboard built with **.NET 8** and **Clean Architecture**.

---

## 🚀 Quick Start & Links

| Component | URL | Description |
| :--- | :--- | :--- |
| **Web Dashboard** | `http://localhost:5000/` | Single-Page Application (SPA) for Product & Inventory management |
| **Swagger API Docs** | `http://localhost:5000/swagger` | OpenAPI interactive API documentation |
| **Health Check** | `http://localhost:5000/health` | Container & API health status endpoint |

---

## 🔑 Pre-Configured Test Credentials

The application automatically initializes database tables and seeds default user accounts on startup:

| Account Type | Username | Password | Access Level |
| :--- | :--- | :--- | :--- |
| **Admin** | `admin` | `AdminPassword123!` | Full Access (Create, Read, Update, Delete) |
| **User** | `user` | `UserPassword123!` | Standard Access (Create, Read, Update) |

*You can also register a custom user via the UI or the `/api/v1/auth/register` API endpoint.*

---

## 🛠️ Tech Stack & Architecture

- **Framework**: .NET 8 (ASP.NET Core Web API)
- **Architecture**: Clean Architecture (Domain, Application, Infrastructure, API)
- **Database**: SQL Server 2022 + EF Core 8
- **Authentication**: JWT (JSON Web Tokens) with Refresh Tokens
- **Validation**: FluentValidation
- **Logging**: Serilog (Console & File logging)
- **Testing**: xUnit, Moq, EF Core InMemory, WebApplicationFactory
- **Containerization**: Docker & Docker Compose

---

## 📁 Project Structure

```
Solution/
├── src/
│   ├── API/            # Controllers, Middleware, Filters, & Frontend SPA (wwwroot)
│   ├── Application/    # CQRS Handlers, DTOs, Mapping profiles, & FluentValidators
│   ├── Domain/         # Entities (Product, Item, User, RefreshToken) & Custom Exceptions
│   └── Infrastructure/ # EF Core DbContext, Repositories, Unit of Work, & JWT Auth
├── tests/
│   ├── API.Tests/            # WebApplicationFactory Integration Tests
│   ├── Application.Tests/    # Handlers & Service Unit Tests
│   └── Infrastructure.Tests/ # EF Core Repository Unit Tests
└── docker-compose.yml        # Docker Multi-Container Configuration
```

---

## ⚡ How to Run Locally

### Option 1: Using Docker Compose (Recommended)

Run the containerized application (Web API + SQL Server 2022):

```bash
docker compose up --build -d
```

Access the Web Dashboard at **`http://localhost:5000/`**.

To stop the containers:
```bash
docker compose down
```

---

### Option 2: Running via .NET CLI

1. Update the SQL Server connection string in `src/API/appsettings.json` if necessary.
2. Run the application:
   ```bash
   dotnet run --project src/API/API.csproj
   ```

---

## 🧪 Running Automated Tests

Run the full test suite (18 unit & integration tests):

```bash
dotnet test
```

---

## 📌 Main API Endpoints Summary

| Method | Endpoint | Description | Auth Required |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/auth/register` | Register new user | No |
| `POST` | `/api/v1/auth/login` | Authenticate user & get JWT token | No |
| `POST` | `/api/v1/auth/refresh-token` | Renew access token | No |
| `GET` | `/api/v1/products` | Get paginated list of products | Yes |
| `GET` | `/api/v1/products/{id}` | Get product details by ID | Yes |
| `POST` | `/api/v1/products` | Create a new product | Yes |
| `PUT` | `/api/v1/products/{id}` | Update product name | Yes |
| `DELETE` | `/api/v1/products/{id}` | Delete product | Yes (Admin) |
| `GET` | `/api/v1/products/{productId}/items` | Get inventory items for a product | Yes |
| `POST` | `/api/v1/products/{productId}/items` | Add inventory item quantity | Yes |
| `DELETE` | `/api/v1/products/{productId}/items/{id}` | Remove inventory item | Yes (Admin) |
