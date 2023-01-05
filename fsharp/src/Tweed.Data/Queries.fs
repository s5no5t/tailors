
module Tweed.Data.Queries

module RavenDbTypes =
    [<CLIMutable>]
    type Tweed = {
        Id: string
        Text: string
    }

open Raven.Client.Documents.Session
open RavenDbTypes

let storeTweed (text: string) (session: IAsyncDocumentSession) =
    task {
        printfn $"Creating tweed %s{text}"
        let tweed = {
            Id = null
            Text = text
        }
        do! session.StoreAsync tweed
        printfn $"Tweed %s{tweed.Id} stored"
    }
