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
              [ _method "POST" ]
              [ label [ _for "text" ] []
                textarea [ _rows "5"; _name "Text"; _value value ] []
                button [ _type "submit" ] [ encodedText "Submit" ] ] ]
        |> layout
