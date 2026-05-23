# Event Service

Backend service for Simplify Yours event capabilities.

## Current API

### `GET /ping`

Returns a service-up message and the current GMT/UTC date-time.

### `POST /events`

Creates an event with name, time, type, optional description, audit timestamps, and soft-delete fields.

Request body:

```json
{
  "eventName": "Product launch",
  "eventTime": "2026-06-01T10:00:00Z",
  "eventType": "event",
  "eventDescription": "Launch plan"
}
```

Responses:

- `201 Created` with the created event details and `Location` value `/events/{id}`.
- `400 Bad Request` with validation details when the request is invalid.

Supported event types:

- `birthday`
- `wedding`
- `event`

### `GET /events/{eventId}`

Returns details for an active event by id.

Responses:

- `200 OK` with event details.
- `404 Not Found` when the event does not exist or is soft deleted.

Response body:

```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "eventName": "Product launch",
  "eventTime": "2026-06-01T10:00:00+00:00",
  "eventType": "event",
  "eventDescription": "Launch plan",
  "createdAt": "2026-05-17T10:00:00+00:00",
  "updatedAt": "2026-05-17T10:00:00+00:00"
}
```

## Developer Commands

Run these commands from `code/backend/event-service/`.

### Restore

```bash
dotnet restore EventService.sln
```

### Build

```bash
dotnet build EventService.sln --configuration Release --no-restore
```

### Test

```bash
dotnet test EventService.sln --configuration Release --no-build
```

### Test With Coverage

```bash
dotnet test EventService.sln --configuration Release --no-build /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:Threshold=80 /p:ThresholdType=line /p:ThresholdStat=total
```

### Run The API Locally

```bash
dotnet run --project src/EventService.Api/EventService.Api.csproj
```

### Install EF CLI

Install the EF CLI once if `dotnet ef` is not available.

```bash
dotnet tool install --global dotnet-ef --version 8.*
```

### Add A Migration

```bash
dotnet ef migrations add <MigrationName> \
  --project src/EventService.Infrastructure/EventService.Infrastructure.csproj \
  --startup-project src/EventService.Api/EventService.Api.csproj \
  --context EventServiceDbContext \
  --output-dir Persistence/Migrations
```

### Apply Migrations

```bash
dotnet ef database update \
  --project src/EventService.Infrastructure/EventService.Infrastructure.csproj \
  --startup-project src/EventService.Api/EventService.Api.csproj \
  --context EventServiceDbContext
```

### List Migrations

```bash
dotnet ef migrations list \
  --project src/EventService.Infrastructure/EventService.Infrastructure.csproj \
  --startup-project src/EventService.Api/EventService.Api.csproj \
  --context EventServiceDbContext
```

## README Maintenance

Keep this README up to date during development. When a feature introduces a new
endpoint, configuration value, migration workflow, local dependency, test
command, script, or operational command, add or update the relevant README
section in the same change.

## CI Checks

```bash
dotnet restore EventService.sln
dotnet build EventService.sln --configuration Release --no-restore
dotnet test EventService.sln --configuration Release --no-build /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:Threshold=80 /p:ThresholdType=line /p:ThresholdStat=total
```
