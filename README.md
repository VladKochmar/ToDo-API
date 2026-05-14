# Todo API

ASP.NET Core Web API for managing tasks

---

## Tech Stack

- ASP.NET Core
- Entity Framework Core
- PostgreSQL
- Docker
- Auth0 Authentication
- xUnit

---

## Clone Repository

```bash
git clone https://github.com/VladKochmar/ToDo-API.git
cd ToDo-API
```

---

## Environment Configuration

Widnows (PowerShell):

```PowerShall
$Env:ASPNETCORE_ENVIRONMENT="Developmnet"
```

Windows (CMD)

```cmd
set ASPNETCORE_ENVIRONMENT=Developmnet
```

Linux / macOS (bash)

```bash
export ASPNETCORE_ENVIRONMENT=Developmnet
```

---

## Configure User Secrets

Initialize secrets:

```bash
dotnet user-secrets init --project Todo.Api
```

Set database password:

```bash
dotnet user-secrets set "DbConnection:Password" "your_password" --project Todo.Api
```

---

## Configure Docker Environment

Create `.env` file in the repository root:

```env
POSTGRES_USER=your_user
POSTGRES_PASSWORD=your_password
POSTGRES_DB=your_db
```

---

## Start PostgreSQL

Start database container:

```bash
docker compose up -d
```

Stop database container:

```bash
docker compose down
```

Verify container status:

```bash
docker ps
```

---

## EF Core Migrations

Add migration:

```bash
dotnet ef migrations add MigrationName --project Todo.Api
```

Apply migration:

```bash
dotnet ef database update --project Todo.Api
```

---

## Run Application

```bash
dotnet run --project Todo.Api
```

---

## Run Tests

Run all tests:

```bash
dotnet tests
```
