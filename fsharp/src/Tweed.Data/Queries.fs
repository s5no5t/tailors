
module Tweed.Data.Queries

open Raven.Client.Documents
open Raven.Client.Documents.Session

let documentStore urls databaseName =
    let documentStore = new DocumentStore()
    documentStore.Urls <- urls |> List.toArray
    documentStore.Database <- databaseName
    documentStore.Initialize() |> ignore
    documentStore

let createSession (documentStore: IDocumentStore) =
    documentStore.OpenAsyncSession()

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
