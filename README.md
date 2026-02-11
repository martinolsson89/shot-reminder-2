# Shot Reminder 2

Shot Reminder 2 is a .NET 10 project for tracking recurring injection shots. It combines a Blazor WebAssembly client with an ASP.NET Core Web API and MongoDB persistence.

The app lets a user log in, register taken shots, track the latest shot, calculate the next due shot (default: every 14 days), and manage shot inventory. It also supports low-stock email alerts and Google Calendar event updates for the next due shot.

## Solution Structure

- `shot-reminder-2.Client` - Blazor WebAssembly UI (MudBlazor).
- `shot-reminder-2.Api` - ASP.NET Core API with JWT auth and controllers.
- `shot-reminder-2.Application` - Use cases, business logic, interfaces, and options.
- `shot-reminder-2.Domain` - Domain entities and enums.
- `shot-reminder-2.Infrastructure` - Mongo repositories, auth/token services, SMTP sender, Google Calendar integration.
- `shot-reminder-2.Contracts` - Shared request/response DTOs used by client and API.

## Current Features

- JWT-based login (`/api/auth/login`).
- Register a shot (`/api/shots`) with leg and optional comment.
- Retrieve latest shot (`/api/shots/latest`) and full shot history (`/api/shots`).
- Update and delete specific shots.
- Inventory operations:
  - Add stock
  - Restock
  - Consume one
  - Update stock
  - Delete inventory
- Automatic inventory consumption when a shot is registered.
- Low-stock email notification (Gmail SMTP) when remaining stock reaches threshold.
- Google Calendar upsert for next due shot reminder.
- MongoDB index initialization at startup.

## Tech Stack

- .NET 10
- ASP.NET Core Web API
- Blazor WebAssembly
- MongoDB (`MongoDB.Driver`)
- JWT bearer authentication
- MudBlazor UI components
- MailKit (SMTP)
- Google Calendar API

## Configuration

API configuration lives in `shot-reminder-2.Api/appsettings.json`.

Required sections:

- `Mongo`
  - `ConnectionString`
  - `DatabaseName`
- `Jwt`
  - `Key`
  - `Issuer`
  - `Audience`
  - `AccessTokenMinutes`
- `Smtp`
  - `Host`
  - `Port`
  - `Username`
  - `Password`
  - `SenderEmail`
  - `SenderName`
- `ShotSettings`
  - `IntervalDays` (default `14`)
  - `LowStockThreshold` (default `2`)
- `Google` (for calendar integration)
  - `ClientId`
  - `ClientSecret`
  - `RefreshToken`
  - `CalendarId` (default `primary`)
  - `TimeZoneId` (default `Europe/Stockholm`)

Client API base URL is configured in `shot-reminder-2.Client/wwwroot/appsettings.json`:

- `ApiBaseUrl` (default `https://localhost:8000/`)

Tip: Keep secrets out of source control by using `dotnet user-secrets` for the API project.

## Running Locally

1. Restore dependencies:

```powershell
dotnet restore .\shot-reminder-2.slnx
```

2. Configure `appsettings.json` (or user-secrets) for Mongo/JWT/SMTP/Google.

3. Start API:

```powershell
dotnet run --project .\shot-reminder-2.Api
```

4. Start client in another terminal:

```powershell
dotnet run --project .\shot-reminder-2.Client
```

Default local URLs:

- API: `https://localhost:8000`
- Client: `https://localhost:7046` (also `http://localhost:5208`)

## API Endpoints (Implemented)

- Auth
  - `POST /api/auth/login`
- Shots (authorized)
  - `POST /api/shots`
  - `GET /api/shots`
  - `GET /api/shots/latest`
  - `GET /api/shots/{id}`
  - `PUT /api/shots/{id}`
  - `DELETE /api/shots/{id}`
- Inventory (authorized)
  - `POST /api/inventory/add`
  - `POST /api/inventory/restock`
  - `POST /api/inventory/consume`
  - `PUT /api/inventory/update`
  - `DELETE /api/inventory`

## Notes

- User registration endpoint is currently commented out in `AuthController`, so login requires an existing user in the database.
- Several client pages are still placeholders (`/shots`, `/inventory`) while dashboard and shot registration flow are active.
