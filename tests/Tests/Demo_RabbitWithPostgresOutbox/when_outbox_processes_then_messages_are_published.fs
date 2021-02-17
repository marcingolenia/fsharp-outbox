module when_outbox_processes_then_messages_are_published

open System.Threading.Tasks
open Rebus.Activation
open RebusMessaging
open Xunit
open FsUnit.Xunit
open Notifications
open Toolbox
open Outbox.PostgresPersistence
open Outbox

[<Fact>]
let ``GIVEN pending outbox messages WHEN execute THEN messages are published to the broker and can be consumed from the broker`` () =
    // Arrange
    let expectedNotification1 = { Id = (generateId()); SomeText = "Whatever1"; Amount = 11.11M }
    let expectedNotification2 = { Id = (generateId()); SomeText = "Whatever2"; Amount = 22.22M }
    let (tcs1, tcs2) = (TaskCompletionSource<WhateverHappened>(), TaskCompletionSource<WhateverHappened>())
    let handler (message: WhateverHappened) = async {
      message.Id |> function
        | _ when expectedNotification1.Id = message.Id -> tcs1.SetResult message
        | _ when expectedNotification2.Id = message.Id -> tcs2.SetResult message
        | _ -> failwith $"This shouldn't happened, %s{nameof WhateverHappened} with unexpected Id: %d{message.Id} was received."
    }
    use activator = new BuiltinHandlerActivator() |> Messaging.registerHandler handler
    use bus = Messaging.configure "amqp://localhost" "two-way-connection-tests" activator
    bus |> Messaging.turnSubscriptionsOn Messaging.markerNeighbourTypes<Marker> |> Async.RunSynchronously
    Outbox.commit generateId (save DbConnection.create) [expectedNotification1; expectedNotification2] |> Async.RunSynchronously
    // Act
    Outbox.execute (read DbConnection.create)
                   (moveToProcessed DbConnection.create)
                   (Messaging.publish bus)
                   |> Async.RunSynchronously
    // Assert
    let actualNotification1 = tcs1.Task |> Async.AwaitTask |> Async.RunSynchronously
    let actualNotification2 = tcs2.Task |> Async.AwaitTask |> Async.RunSynchronously
    actualNotification1 |> should equal expectedNotification1
    actualNotification2 |> should equal expectedNotification2
    