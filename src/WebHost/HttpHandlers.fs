namespace WebHost

open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open Notifications

module HttpHandlers =
    let whateverHappened (commit: obj list -> Async<unit>)
                         generateId
                         : HttpHandler =
      fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
          let whateverHappened: WhateverHappened = {
            Id = generateId()
            SomeText = "Hi there!"
            Amount = 100.20M
          }
          do! commit [whateverHappened]
          return! text $"Thank you. %A{whateverHappened} was scheduled" next ctx
        }

    let handlers (root: CompositionRoot.Dependencies) =
      choose [
        route "/whatever" >=> (whateverHappened root.OutboxCommit root.GenerateId)
        route "/"         >=> text "Hello :)" ]