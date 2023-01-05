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
                let documentStore = Queries.documentStore ["http://localhost:8080"] "TweedFsharp"
                use session = Queries.createSession documentStore
                let! tweed = ctx.BindFormAsync<CreateTweed>()
                do! Queries.storeTweed session tweed.Text
                do! session.SaveChangesAsync()
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
