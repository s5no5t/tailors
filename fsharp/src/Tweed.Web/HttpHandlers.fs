module Tweed.Web.HttpHandlers

open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Giraffe    
open Tweed.Data
open ViewModels

module Index =
    let indexGetHandler =
        let tweed = { Content = "Some tweed" }
        let model = { Tweeds = [ tweed ] }
        let view = Views.index model
        htmlView view

    let handlers =
        GET >=> indexGetHandler

module Tweed =
    [<CLIMutable>]
    type CreateTweed = {
        Text: string
    }

    let storeTweedHandler = 
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                let documentStore = RavenDb.documentStore ["http://localhost:8080"] "TweedFsharp"
                let! tweed = ctx.BindFormAsync<CreateTweed>()
                use session = RavenDb.createSession documentStore
                do! session |> Queries.storeTweed tweed.Text
                do! session |> RavenDb.saveChangesAsync
                return! next ctx
            }

    let tweedPostHandler =
        storeTweedHandler
        >=> redirectTo false "/"

    let handlers =
        route "/create" >=> POST >=> tweedPostHandler

module Fallback =
    let notFoundHandler =
        setStatusCode 404 >=> text "Not Found"

let handler : HttpHandler =
    choose [
        subRoute "/tweed" Tweed.handlers 
        route "/" >=> Index.handlers
        Fallback.notFoundHandler
    ]
