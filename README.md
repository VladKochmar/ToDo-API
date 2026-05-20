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

## Multy-tenant model

Each client has:

- a separate PostgreSQL database;
- a dedicated database user;
- an isolated connection string;
- row in a global clients table containing its own metadata.

Application requires resolve the tenant context dynamically
and connect only to that tenant's database.

Because each tenant uses a physically separated database and DB user,
cross-tenant data leakage is prevented even if application level filtering
is accidentally omitted.

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

Connect to a specific database:

```bash
docker exec -it todo-postgres psql -U your_user -d your_database
```

Print list of relations:

```bash
docker exec -it todo-postgres psql -U your_user -d your_database

\dt
```

---

## EF Core Migrations

Add migration:

```bash
dotnet ef migrations add MigrationName --context YourContext --project Todo.Api --out-dir Migrations/YourContext
```

Apply migration:

```bash
dotnet ef database update --context YourContext --project Todo.Api
```

Print migrations:

```bash
dotnet ef migrations list --context YourContext --project Todo.Api
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
