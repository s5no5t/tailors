module Tweed.Web.Views

open Giraffe.ViewEngine
open ViewModels

let _upSubmit = flag "up-submit"
let _upLayer = attr "up-layer"

let layout (content: XmlNode list) =
    html
        []
        [ head
              []
              [ title [] [ encodedText "Tweed.Web" ]
                link [ _rel "stylesheet"; _type "text/css"; _href "/main.css" ] ]
          body [] [ yield! content; a [ _href "/tweed/create" ] [ encodedText "Create Tweed" ] ] ]

let titlePartial () = h1 [] [ encodedText "Tweed.Web" ]


module Index =
    let indexGetView (model: IndexViewModel) =
        [ titlePartial ()
          yield! model.Tweeds |> List.map (fun t -> div [] [ str t.Content ]) ]
        |> layout

module Tweed =
    let createTweedView text =
        let value =
            match text with
            | Some(t) -> t
            | None -> ""

        [ titlePartial ()
          form
              [ _method "POST"; _upSubmit; _upLayer "parent" ] 
              [ label [ _for "text" ] []
                textarea [ _rows "5"; _name "Text"; _value value ] []
                button [ _type "submit" ] [ encodedText "Submit" ] ] ]
        |> layout
