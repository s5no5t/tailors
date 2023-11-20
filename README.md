# Tailors

A social media web app built with [HTMX](https://htmx.org/).

## Design Goals

- Easy to maintain and extend
- Easy to test
- Simple frontend architecture
- Fast to build and run
- Code organized by responsibility, not by file types

## What works

- Signup & Login
- Post a Tweed
- Follow other users
- See Tweeds from users you follow
- Show Tweed threads
- Like Tweeds
- Reply to Tweeds in threads
- Search for Tweeds and users
- Show notification for new Tweeds

## Building Blocks

- [HTMX](https://htmx.org/) A hypermedia approach to building Single Page Apps
- [ASP.NET Core MVC](https://github.com/dotnet/aspnetcore) framework for backend HTML templating (Blazor not used)
- [RavenDB](https://ravendb.net/) for application data
- [Bootstrap](https://getbootstrap.com/) for easy-to-use frontend components

Find more info about how this app is structured under [STRUCTURE.md](./STRUCTURE.md).

## Build & Run

1. Install [dotnet 8](https://dotnet.microsoft.com/en-us/download)
2. Install [Docker](https://www.docker.com/)
3. Start RavenDB

        docker compose up

4. Optional: Create some fake data

        dotnet run --project ./src/Tailors.GenerateFakes

5. Run web app

        dotnet run --project ./src/Tailors.Web

## Run Tests

    dotnet test
