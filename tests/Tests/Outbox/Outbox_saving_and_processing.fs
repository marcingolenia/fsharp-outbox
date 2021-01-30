module Outbox_saving_and_processing

open Xunit
open Outbox
open PostgresPersistence
open FsUnit.Xunit
open Toolbox
open Notifications
open Newtonsoft.Json
    
let private decode (message: string) =
    JsonConvert.DeserializeObject<InvoiceNotifications>(message)

[<Fact>]
let ``GIVEN some notifications WHEN commit THEN messages are stored`` () =
    // Arrange
    let expectedNotification = { Id = 123L;  Number = "FV1"; Amount = 100M } |> InvoiceNotifications.InvoiceIssued
    let id = generateId()
    // Act
    Outbox.commit (fun _ -> id) (save DbConnection.create) [expectedNotification] |> Async.RunSynchronously
    let messages = read DbConnection.create |> Async.RunSynchronously 
    // Assert
    let message = messages |> Seq.find(fun message -> message.Id = id)
    let actualNotification = message.Payload |> decode
    actualNotification |> should equal expectedNotification

[<Fact>] 
let ``GIVEN some notifications WHEN process THEN messages are published`` () =
    // Arrange
    let mutable publishedNotification : InvoiceNotifications option = None
    let expectedNotification = { Id = 123L;  Number = "FV1"; Amount = 100M } |> InvoiceNotifications.InvoiceIssued
    let publish = fun (message: obj) -> async {
        publishedNotification <- Some (message :?> InvoiceNotifications)
    } 
    let id = generateId()
    Outbox.commit (fun _ -> id) (save DbConnection.create) [expectedNotification] |> Async.RunSynchronously
    // Act
    Outbox.execute (read DbConnection.create) (moveToProcessed DbConnection.create) publish |> Async.RunSynchronously
    // Assert
    publishedNotification.Value |> should equal expectedNotification
    
[<Fact>] 
let ``GIVEN some notifications WHEN process THEN message aren't read again.`` () =
    // Arrange
    let expectedNotification = { Id = 123L;  Number = "FV1"; Amount = 100M } |> InvoiceNotifications.InvoiceIssued
    let publish = fun _ -> async { () } 
    let id = generateId()
    Outbox.commit (fun _ -> id) (save DbConnection.create) [expectedNotification] |> Async.RunSynchronously
    // Act
    Outbox.execute (read DbConnection.create) (moveToProcessed DbConnection.create) publish |> Async.RunSynchronously
    let messagesToProcess =  (read DbConnection.create) |> Async.RunSynchronously
    messagesToProcess |> Seq.filter(fun message -> message.Id = id) |> should be Empty