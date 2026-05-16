# Event Service

Backend service for Simplify Yours event capabilities.

## Current API

- `GET /ping` returns a service-up message and the current GMT/UTC date-time.

## Local Checks

```bash
dotnet restore EventService.sln
dotnet build EventService.sln
dotnet test EventService.sln
```
