# RaceDay.API - P2-PROG6212

## Two Roles
1. **Athlete:** Can register for races, view races, view results. Cannot create races.
2. **Organizer (Admin):** Can create/update/delete races, manage registrations, publish results. Full access.

## Tech Stack
- .NET 8 Web API
- Entity Framework Core (InMemory)
- Swagger / OpenAPI
- GitHub Actions for CI/CD

## Setup and Run Instructions
1. Clone repo: `git clone https://github.com/TshegofatsoMe11/P2-PROG6212.git`
2. Open `RaceDay.API.sln` in Visual Studio 2022
3. Restore NuGet packages
4. Update connection string in `appsettings.json` if using SQL Server
5. Press F5 to run. Swagger opens automatically at /swagger
6. .NET 8 SDK required

## API Endpoints
- POST /api/Auth/register - Register
- POST /api/Auth/login - Login
- GET /api/RaceEvents - List races
- POST /api/RaceEvents - Create race (Admin)
- GET /api/Bookings - List bookings
- POST /api/Bookings - Create booking
- GET /api/Categories - List categories
- POST /api/Categories - Create category
- GET /api/EventEnrolments - List enrolments
- POST /api/EventEnrolments - Enrol
- GET /api/Results - List results
- POST /api/Results - Create result
- GET /api/Results/forecast/{eventId} - Weather forecast

## CI/CD
GitHub Actions used for CI/CD. Build runs on push to main branch.
Workflow file: .github/workflows/build.yml

## YouTube Video Presentation Link
https://youtu.be/YOUR-LINK-HERE

## Logging
ILogger implemented in all controllers.

## Author
Tshegofatso Mell - PROG6212 POE Part 2
