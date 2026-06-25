# ExpenseTracker

Full-stack expense tracking portfolio project with a clear frontend/backend boundary.

- `expense-tracker-api`: ASP.NET Core Web API, Entity Framework Core, SQLite, Swagger.
- `expense-tracker-web`: standalone Blazor WebAssembly client that calls the API over HTTP.
- `ExpenseTracker.sln`: solution file that groups both projects.

## Run Locally

Copy the local environment template if you want a quick reference for ports and URLs:

```powershell
Copy-Item .env.example .env
```

Open two terminals.

Terminal 1:

```powershell
cd expense-tracker-api
dotnet run --urls http://localhost:5090
```

Terminal 2:

```powershell
cd expense-tracker-web
dotnet run --urls http://localhost:5290
```

Then open:

```text
http://localhost:5290
```

Swagger remains available at:

```text
http://localhost:5090/swagger
```

## Architecture

The Blazor app does not reference the API project. It uses typed HTTP models and an `ExpenseApiClient` service to call:

- `GET /api/categories`
- `GET /api/expenses`
- `POST /api/expenses`
- `PUT /api/expenses/{id}`
- `DELETE /api/expenses/{id}`
- `GET /api/expenses/summary`

The API enables CORS for the Blazor development origin, `http://localhost:5290`.

## Git Notes

Runtime files are ignored by `.gitignore`, including build outputs, SQLite database files, logs, and local `.env` files. Commit `.env.example`, not `.env`.
