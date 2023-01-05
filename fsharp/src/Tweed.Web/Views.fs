module Tweed.Web.Views

open Giraffe.ViewEngine
open ViewModels

let layout (content: XmlNode list) =
    html
        []
        [ head
              []
              [ title [] [ encodedText "Tweed.Web" ]
                link [ _rel "stylesheet"; _type "text/css"; _href "/main.css" ] ]
          body [] content ]

let partial () = h1 [] [ encodedText "Tweed.Web" ]


module Index = 
    let indexGetView (model: IndexViewModel) =
        [ partial ()
          yield! model.Tweeds |> List.map (fun t -> div [] [ str t.Content ]) ]
        |> layout

