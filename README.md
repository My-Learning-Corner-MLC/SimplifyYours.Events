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
  "updatedAt": "2026-05-17T10:00:00+00:00",
  "concurrencyToken": "AAAAAAAAAAAAAAAAAAAAAA=="
}
```

### `POST /events/query`

Returns a paginated list of active events. Supports search by event name or description, filtering by event type and event time, and sorting by created or updated date.

Request body:

```json
{
  "pageNumber": 1,
  "pageSize": 20,
  "search": "launch",
  "eventType": "event",
  "timeFilter": "upcoming",
  "sortBy": "createdAt",
  "sortDirection": "desc"
}
```

Request options:

- `pageNumber`: optional, defaults to `1`.
- `pageSize`: optional, defaults to `20`, maximum `100`.
- `search`: optional, matches event name or description.
- `eventType`: optional, one of `birthday`, `wedding`, or `event`.
- `timeFilter`: optional, one of `all`, `upcoming`, or `past`.
- `sortBy`: optional, one of `createdAt` or `updatedAt`.
- `sortDirection`: optional, one of `asc` or `desc`.

Responses:

- `200 OK` with matching events and pagination metadata.
- `400 Bad Request` with validation details when the request is invalid.

Response body:

```json
{
  "items": [
    {
      "id": "00000000-0000-0000-0000-000000000000",
      "eventName": "Product launch",
      "eventTime": "2026-06-01T10:00:00+00:00",
      "eventType": "event",
      "eventDescription": "Launch plan",
      "createdAt": "2026-05-17T10:00:00+00:00",
      "updatedAt": "2026-05-17T10:00:00+00:00"
    }
  ],
  "pageNumber": 1,
  "pageSize": 20,
  "totalCount": 1,
  "totalPages": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

### `PUT /events/{id}`

Updates an active event's editable details. Only event name, event time, and event description can be changed.

Clients must send the latest `concurrencyToken` returned by create, get details, or the previous update response. Stale tokens are rejected to prevent overwriting another update.

Request body:

```json
{
  "eventName": "Updated product launch",
  "eventTime": "2026-06-01T11:00:00Z",
  "eventDescription": "Updated launch plan",
  "concurrencyToken": "AAAAAAAAAAAAAAAAAAAAAA=="
}
```

Responses:

- `200 OK` with updated event details and a new `concurrencyToken`.
- `400 Bad Request` with validation details when the request is invalid.
- `404 Not Found` when the event does not exist or is soft deleted.
- `409 Conflict` when the supplied `concurrencyToken` is stale.

Response body:

```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "eventName": "Updated product launch",
  "eventTime": "2026-06-01T11:00:00+00:00",
  "eventType": "event",
  "eventDescription": "Updated launch plan",
  "createdAt": "2026-05-17T10:00:00+00:00",
  "updatedAt": "2026-05-17T10:15:00+00:00",
  "concurrencyToken": "BBBBBBBBBBBBBBBBBBBBBB=="
}
```

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

## Local Observability

Start shared infrastructure before running the API:

```bash
docker compose --env-file ../../infra/shared-infrastructure/infrastructure/.env -f ../../infra/shared-infrastructure/infrastructure/docker-compose.yml up -d --remove-orphans
```

The API launch profiles export logs, traces, and metrics to the local Aspire
Dashboard:

```text
OTEL_SERVICE_NAME=event-service
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317
OTEL_EXPORTER_OTLP_PROTOCOL=grpc
OTEL_EXPORTER_OTLP_HEADERS=x-otlp-api-key=<SIMPLIFYYOURS_ASPIRE_OTLP_API_KEY>
OTEL_RESOURCE_ATTRIBUTES=service.namespace=SimplifyYours,deployment.environment=local
```

Set `OTEL_EXPORTER_OTLP_HEADERS` in your shell before running the service. The
value must match `SIMPLIFYYOURS_ASPIRE_OTLP_API_KEY` from the shared
infrastructure `infrastructure/.env` file.

Open `http://localhost:18888` and use the token from
`docker container logs simplify-yours-aspire-dashboard`.

Do not log request bodies, response bodies, passwords, tokens, authorization
codes, refresh tokens, payment data, customer data, or unnecessary personal
data. Prefer safe context such as operation name, event ID, correlation ID,
causation ID, status, elapsed time, and attempt count.

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
