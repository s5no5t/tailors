# Tweed

A playground web app. 

## Design Goals

* Easy to maintain and extend
* Easy to test
* As little client state as possible
* Simple backend architecture

## Building Blocks

* [Microsoft Razor Pages](https://learn.microsoft.com/en-us/aspnet/core/razor-pages/?view=aspnetcore-7.0&tabs=visual-studio) framework
* [Unpoly](https://unpoly.com/) for unobtrusive JavaScript code

## Build & Run

1. Install [dotnet 6](https://dotnet.microsoft.com/en-us/download)
2. Install [Docker](https://www.docker.com/)
3. Start RavenDB

       cd Tweed.Web
       docker compose up

4. Run Tweed

       cd Tweed.Web
       dotnet run
