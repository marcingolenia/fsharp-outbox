module publishing_messages

open System.Threading.Tasks
open Rebus.Activation
open Xunit
open FsUnit.Xunit
open RebusMessaging.Publisher

[<Fact>] 
let ``GIVEN an event WHEN published THEN it can be consumed`` () =
    // Arrange
    let expectedEvent = { Id = int64 5; Number = "FV1"; Amount = 95M }
    let tcs = TaskCompletionSource<WhateverHappened>()
    let handleInvoiceIssued invoice = async {
      printf "%A" invoice
      tcs.SetResult invoice
    }
    use activator = new BuiltinHandlerActivator()
    use bus = start handleInvoiceIssued
                    activator
                    "amqp://localhost" |> Async.RunSynchronously
    // Act
    publish expectedEvent bus |> Async.RunSynchronously
    // Assert
    let actualEvent = tcs.Task.Result
    actualEvent |> should equal expectedEvent
     
    