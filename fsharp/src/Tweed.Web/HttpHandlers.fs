module Tweed.Web.HttpHandlers

open Microsoft.AspNetCore.Hosting
open Giraffe    
open ViewModels

let indexHandler =
    let tweed = { Content = "tweed4" }
    let model = { Tweeds = [ tweed ] }
    let view = Views.index model
    htmlView view

let handlers : HttpHandler list =
        [ GET
          >=> choose [ route "/" >=> indexHandler
          ]
          setStatusCode 404 >=> text "Not Found" ]
