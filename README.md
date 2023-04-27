# Tweed

A social media web app to play with.

## Design Goals

- Easy to maintain and extend
- Easy to test
- As little client state as possible
- Simple backend architecture

## What works

- Signup & Login
- Post a Tweed
- See Tweeds from users you follow
- Follow other users
- Like Tweeds
- Search for Tweeds and users

## Building Blocks

- [ASP.NET Core MVC](https://github.com/dotnet/aspnetcore) framework for backend HTML templating
- [HTMX](https://htmx.org/) A hypermedia approach to building Single Page Apps
- [RavenDB](https://ravendb.net/) for storing and querying data
- [Bootstrap](https://getbootstrap.com/) for easy-to-use frontend components

## Build & Run

1. Install [dotnet 6](https://dotnet.microsoft.com/en-us/download)
2. Install [Docker Desktop](https://www.docker.com/)
3. Start RavenDB

        cd ./src/Tweed.Web
        docker compose up

4. Run Tweed

        cd ./src/Tweed.Web
        dotnet run

## Run Tests

    dotnet test
