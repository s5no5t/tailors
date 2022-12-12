# Tweed

A playground web app. 

## Design Goals

* Easy to maintain and extend
* Easy to test
* As little client state as possible
* Simple backend architecture

## What works

* Post a Tweed
* See Tweeds by users you follow
* Like Tweeds
* Search for users

## Building Blocks

* [Microsoft Razor Pages](https://learn.microsoft.com/en-us/aspnet/core/razor-pages/?view=aspnetcore-7.0&tabs=visual-studio) framework for backend HTML templating
* [Unpoly](https://unpoly.com/) for nice but unobtrusive frontend interactions
* [RavenDB](https://ravendb.com) for storing and querying data
* [Bootstrap](https://getbootstrap.com/) for easy-to-use frontend components

## Build & Run

1. Install [dotnet 6](https://dotnet.microsoft.com/en-us/download)
2. Install [Docker Desktop](https://www.docker.com/)
3. Start RavenDB

       cd Tweed.Web
       docker compose up

4. Run Tweed

       cd Tweed.Web
       dotnet run
