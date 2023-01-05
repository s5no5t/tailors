module Tweed.Web.HttpHandlers

open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Giraffe    
open Tweed.Data
open ViewModels

let indexGetHandler =
    let tweed = { Content = "Some tweed" }
    let model = { Tweeds = [ tweed ] }
    let view = Views.index model
    htmlView view

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

let notFoundHandler =
    setStatusCode 404 >=> text "Not Found"

let handler : HttpHandler =
    choose [
        route "/" >=> GET >=> indexGetHandler
        route "/tweed/create" >=> POST >=> tweedPostHandler 
        notFoundHandler
    ]
