
module Tweed.Data.Queries

open Raven.Client.Documents.Session

[<CLIMutable>]
type Tweed = {
    Id: string
    Text: string
}

let storeTweed (session:IAsyncDocumentSession) (text: string) =
    task {
        printfn $"Creating tweed %s{text}"
        let tweed = {
            Id = null
            Text = text
        }
        do! session.StoreAsync tweed
        printfn $"Tweed %s{tweed.Id} stored"
    }
