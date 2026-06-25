# ExpenseTracker API

Backend service for the `ExpenseTracker` full-stack project. It uses ASP.NET Core Web API, SQLite through Entity Framework Core, and Swagger for direct API inspection.

## Features

- CRUD endpoints for expenses.
- Category lookup through a separate database table.
- Filtering by category and date range.
- Monthly or yearly spending summary grouped by category, with monthly totals.
- Structured logging for key write operations.
- Global exception middleware that returns consistent `ProblemDetails` responses.

## Run Locally

```powershell
dotnet restore
dotnet run
```

Open the Swagger UI at:

```text
http://localhost:5090/swagger
```

The app runs EF Core migrations automatically on startup and creates a local SQLite file named `expense-tracker.db`.

The root `.env.example` documents the development URLs used by the API and Blazor client. The API still reads its connection string from `appsettings.json`.

## Endpoints

| Method | Endpoint | Description |
| --- | --- | --- |
| `GET` | `/api/categories` | List seeded categories. |
| `GET` | `/api/expenses` | List expenses. Supports `category`, `from`, and `to` query filters. |
| `GET` | `/api/expenses/{id}` | Get one expense by id. |
| `POST` | `/api/expenses` | Create an expense. |
| `PUT` | `/api/expenses/{id}` | Update an expense. |
| `DELETE` | `/api/expenses/{id}` | Delete an expense. |
| `GET` | `/api/expenses/summary?year=2026` | Full-year summary for all 12 months, grouped by category. |
| `GET` | `/api/expenses/summary?year=2026&month=6` | Single-month summary grouped by category. |

## Example Create Request

```json
{
  "amount": 24.5,
  "categoryId": 1,
  "description": "Lunch",
  "date": "2026-06-25"
}
```

## Example Filter Request

```text
GET /api/expenses?category=Food&from=2026-06-01&to=2026-06-30
```

## Example Summary Requests

```text
GET /api/expenses/summary?year=2026
GET /api/expenses/summary?year=2026&month=6
```
