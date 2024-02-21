# Structure

The repo contains these main projects:

* `Tailors.Domain` contains domain models for the different domains used in the app
* `Tailors.Infrastructure` contains data access code
* `Tailors.Web` contains the web app including views and controllers

```mermaid
    C4Component
    title Component diagram for Tailors
    Container_Boundary(Tailors, "Tailors App") {
        Component(Tailors.Web, "Web", "HTMX + ASP.NET Core MVC", "app config, views, event handling")
        Component(Tailors.Domain, "Domain", "C# Library", "auth, tweeds, followers, likes")
        Component(Tailors.Infrastructure, "Infrastructure", "C# Library", "database access")

        Rel(Tailors.Web, Tailors.Domain, "Uses")
        Rel(Tailors.Web, Tailors.Infrastructure, "Uses")
        Rel(Tailors.Infrastructure, Tailors.Domain, "Uses")
    }

    ContainerDb(db, "Database", "RavenDB", "Stores Users, Tweeds, Likes, Follows")
    Rel(Tailors, db, "Uses")
    
    UpdateLayoutConfig($c4ShapeInRow="2", $c4BoundaryInRow="1")
```
