# ExpenseTracker Web

Standalone Blazor WebAssembly frontend for the `ExpenseTracker` API. This project intentionally communicates with the backend over HTTP instead of merging frontend and backend concerns.

## Run

```powershell
dotnet run --urls http://localhost:5290
```

The backend API must be running at:

```text
http://localhost:5090
```

## Pages

- `/`: dashboard with yearly or monthly summary data.
- `/expenses`: expense filtering plus create, update, and delete actions.

The client talks to the API through `Services/ExpenseApiClient.cs`.

The root `.env.example` documents the development URLs. The current Blazor client uses `http://localhost:5090` in `Program.cs` for the API base address.
