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

module Tweed =
    [<CLIMutable>]
    type CreateTweed = {
        Text: string    
    }

    let storeTweedHandler = 
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                let! tweed = ctx.BindFormAsync<CreateTweed>()
                Queries.storeTweed tweed.Text |> ignore
                return! next ctx
            }

    let tweedPostHandler =
        storeTweedHandler
        >=> redirectTo false "/"

module Fallback =
    let notFoundHandler =
        setStatusCode 404 >=> text "Not Found"

let handler : HttpHandler =
    choose [
        route "/" >=> GET >=> Index.indexGetHandler
        route "/tweed/create" >=> POST >=> Tweed.tweedPostHandler 
        Fallback.notFoundHandler
    ]
