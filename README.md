# Booking API

Resource scheduling and time-slot reservation REST API built with **ASP.NET Core (.NET 10)**, **Entity Framework Core**, and **PostgreSQL**, featuring deterministic time-range overlap detection and clean status handling.

## Key Features
- **Resource Management:** Create and list bookable resources (rooms, equipment, pods) with unique name validation and capacity tracking.
- **Overlap-Safe Booking Creation:** Validates time intervals (`EndAt > StartAt`), ensures the target resource exists and is active, and checks for overlapping `Confirmed` bookings (`existing.StartAt < newEndAt && existing.EndAt > newStartAt`) so back-to-back bookings are allowed without false collisions.
- **Booking Cancellation:** Cancel bookings cleanly (`Confirmed` -> `Cancelled`), freeing the time slot for subsequent reservations.
- **Database Migrations:** Managed with Entity Framework Core migrations (`Npgsql.EntityFrameworkCore.PostgreSQL`) against PostgreSQL 17 via Docker Compose.
- **Consistent Error Responses:** Maps domain and validation errors to `400 Bad Request`, `404 Not Found`, and `409 Conflict`.

## Tech Stack
- .NET 10 / ASP.NET Core Web API
- Entity Framework Core 10 & Npgsql (PostgreSQL 17)
- xUnit & EF Core InMemory provider
- Docker Compose & GitHub Actions CI

## Running Locally

1. Start PostgreSQL with `docker compose up -d`.
2. Apply EF Core migrations with `dotnet ef database update`.
3. Run the API with `dotnet run`.
4. Run unit tests with `dotnet test`.

## Example Endpoints
- `POST /api/v1/resources` — Create a bookable resource
- `GET /api/v1/resources` — List resources
- `POST /api/v1/bookings` — Create a booking with overlap validation
- `GET /api/v1/bookings?resourceId=1` — List bookings (optionally filtered by resource)
- `POST /api/v1/bookings/{id}/cancel` — Cancel a booking
