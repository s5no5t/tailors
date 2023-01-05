module Tweed.Data.RavenDb

open Raven.Client.Documents

let documentStore urls databaseName =
    let documentStore = new DocumentStore()
    documentStore.Urls <- urls |> List.toArray
    documentStore.Database <- databaseName
    documentStore.Initialize()

let createSession (documentStore: IDocumentStore) =
    documentStore.OpenAsyncSession()
