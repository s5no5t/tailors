module Tweed.Web.HttpHandlers

open Microsoft.AspNetCore.Hosting
open Giraffe    
open ViewModels

let indexGetHandler =
    let tweed = { Content = "Some tweed" }
    let model = { Tweeds = [ tweed ] }
    let view = Views.index model
    htmlView view

let handler : HttpHandler =
    choose [
        route "/" >=> GET >=> indexGetHandler
        setStatusCode 404 >=> text "Not Found"
    ]
