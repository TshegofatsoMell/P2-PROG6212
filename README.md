# RaceDay.API - P2-PROG6212

## Two Roles
1. **Athlete:** Can register for races, view races, view results. Cannot create races.
2. **Organizer (Admin):** Can create/update/delete races, manage registrations, publish results. Full access.

## Setup and Run Instructions
1. Clone repo: `git clone https://github.com/TshegofatsoMe11/P2-PROG6212.git`
2. Open `RaceDay.API.sln` in Visual Studio 2022
3. Update connection string in `appsettings.json`
4. Run `Update-Database` in Package Manager Console
5. Press F5 to run. Swagger opens automatically at /swagger
6..NET 8 SDK required

## CI/CD
GitHub Actions configured for CI/CD. Build runs on push to main.
![CI/CD Screenshot](actions-screenshot.png)

## YouTube Video Presentation (Unlisted)
Link: https://youtu.be/j-7NXlHWkOU
Video demonstrates: Running API, Code Structure, Endpoints, Authentication & Roles, Unit Tests.
