module when_outbox_crashes_then_processed_messages_are_published

open System.Threading.Tasks
open Newtonsoft.Json
open Rebus.Activation
open RebusMessaging
open Xunit
open FsUnit.Xunit
open Notifications
open Toolbox
open Outbox.PostgresPersistence
open Outbox
open System

let private decode (message: string) =
    JsonConvert.DeserializeObject<WhateverHappened>(message)

[<Fact(Skip="Run this test in isolation, so other tests won't steal pending messages.")>]
let ``GIVEN pending outbox messages WHEN execute AND publish crashes Tt relies on messages count.HEN published messages are marked as processed AND the rest of the messages may be processed again`` () =
    // Arrange
    let text4Filtering = Guid.NewGuid().ToString()
    let expectedNotifications = [ for i in 1 .. 20 -> { Id = int64 i; SomeText = text4Filtering; Amount = 22.22M } ]
    let receivedNotifications = ResizeArray<WhateverHappened>()
    let mutable publishTrial = 1
    let tcs = TaskCompletionSource<ResizeArray<WhateverHappened>>()
    let handler (notification: WhateverHappened) = async {
      receivedNotifications.Add notification
      if receivedNotifications.Count = expectedNotifications.Length then tcs.SetResult receivedNotifications
    }
    use activator = new BuiltinHandlerActivator()
    use bus = Messaging.start handler
                              activator
                              "amqp://localhost" |> Async.RunSynchronously
    Outbox.commit generateId (save DbConnection.create) expectedNotifications |> Async.RunSynchronously
    let whateverType = typeof<WhateverHappened>.FullName
    let filteredRead =
      async {
        let! messages = (read DbConnection.create)
        return messages
               |> Seq.filter(fun message -> message.Type = whateverType
                                            && (message.Payload |> decode).SomeText = text4Filtering) 
      } 
    let interruptedPublish =
      (fun (msg: obj) ->
        if (msg :?> WhateverHappened).Id = int64 10 && publishTrial = 1
        then publishTrial <- 2; failwith "Error"
        else msg)
      >> (Messaging.publish bus)
    let outboxExecute = Outbox.execute filteredRead
                                       (moveToProcessed DbConnection.create)
                                       interruptedPublish
    // Act
    try 
      outboxExecute |> Async.RunSynchronously
    with | _ -> () // Silent swallow, let's simulate next outbox execution, it should catch up.
    receivedNotifications.Count |> should be (lessThan expectedNotifications.Length)
    outboxExecute |> Async.RunSynchronously
    // Assert
    let actualNotifications = tcs.Task |> Async.AwaitTask |> Async.RunSynchronously
    actualNotifications.Count |> should equal expectedNotifications.Length
    