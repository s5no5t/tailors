# CSharp

My C# implementation of my playground social media app.

## Building Blocks

* [Microsoft Razor Pages](https://learn.microsoft.com/en-us/aspnet/core/razor-pages/?view=aspnetcore-7.0&tabs=visual-studio) framework for backend HTML templating
* [Unpoly](https://unpoly.com/) for nice but unobtrusive frontend interactions
* [RavenDB](https://ravendb.net/) for storing and querying data
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
