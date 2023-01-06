module Tweed.Web.HttpHandlers

open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Giraffe
open Tweed.Data
open ViewModels

module Index =
    let indexGetHandler =
        let tweedViewModel = { Content = "Some tweed" }
        let indexViewModel = { Tweeds = [ tweedViewModel ] }
        let view = Views.Index.indexGetView indexViewModel
        htmlView view

    let handlers = GET >=> indexGetHandler

module Tweed =
    [<CLIMutable>]
    type CreateTweedDto = { Text: string }

    let storeTweedHandler documentStore =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let! tweedDto = ctx.BindFormAsync<CreateTweedDto>()
                use session = RavenDb.createSession documentStore
                do! session |> Queries.storeTweed tweedDto.Text
                do! session |> RavenDb.saveChangesAsync
                return! next ctx
            }

    let tweedPostHandler documentStore = storeTweedHandler documentStore >=> redirectTo false "/"

    let createTweedGetHandler =
        let view = Views.Tweed.createTweedView None
        htmlView view

    let handlers documentStore =
        route "/create"
        >=> choose [ POST >=> tweedPostHandler documentStore; GET >=> createTweedGetHandler ]

module Fallback =
    let notFoundHandler = setStatusCode 404 >=> text "Not Found"

let handler documentStore: HttpHandler =
    choose
        [ subRoute "/tweed" (Tweed.handlers documentStore)
          route "/" >=> Index.handlers
          Fallback.notFoundHandler ]
