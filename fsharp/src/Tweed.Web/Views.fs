module Tweed.Web.Views

open Giraffe.ViewEngine
open Model

let layout (content: XmlNode list) =
    html [] [
        head [] [
            title []  [ encodedText "Tweed.Web" ]
            link [ _rel  "stylesheet"
                   _type "text/css"
                   _href "/main.css" ]
        ]
        body [] content
    ]

let partial () =
    h1 [] [ encodedText "Tweed.Web" ]

let index (model : Message) =
    [
        partial()
        p [] [ encodedText model.Text ]
    ] |> layout
