module publishing_messages

open System
open System.Threading.Tasks
open Rebus.Activation
open Notifications
open Xunit
open FsUnit.Xunit
open RebusMessaging.Messaging

[<Fact>] 
let ``GIVEN a notification WHEN published THEN it can be consumed`` () =
    // Arrange
    let expectedNotification = { Id = int64 5; SomeText = Guid.NewGuid().ToString(); Amount = 95M }
    let tcs = TaskCompletionSource<WhateverHappened>()
    let handler message = async {
      printf "%A" message
      tcs.SetResult message
    }
    use activator = new BuiltinHandlerActivator() |> registerHandler handler
    use bus = configure "amqp://localhost"
                    "test-connection"
                    activator
    bus |> turnSubscriptionsOn markerNeighbourTypes<Marker> |> Async.RunSynchronously
    // Act
    publish bus expectedNotification |> Async.RunSynchronously
    // Assert
    let actualNotification = tcs.Task |> Async.AwaitTask |> Async.RunSynchronously
    actualNotification |> should equal expectedNotification
     
    