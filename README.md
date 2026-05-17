# Event Service

Backend service for Simplify Yours event capabilities.

## Current API

- `GET /ping` returns a service-up message and the current GMT/UTC date-time.
- `POST /events` creates an event with name, time, type, optional description, audit timestamps, and soft-delete fields.

## Configuration

Set `ConnectionStrings__EventServiceDb` before using event persistence.

## Local Checks

```bash
dotnet restore EventService.sln
dotnet build EventService.sln
dotnet test EventService.sln
```
