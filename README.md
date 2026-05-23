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

## Configuration

Set `ConnectionStrings__EventServiceDb` before using event persistence.

## Local Checks

```bash
dotnet restore EventService.sln
dotnet build EventService.sln
dotnet test EventService.sln
```
