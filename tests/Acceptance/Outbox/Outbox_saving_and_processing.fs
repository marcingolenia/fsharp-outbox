module Outbox_saving_and_processing

open Thoth.Json.Net
open Xunit
open Outbox
open PostgresPersistence
open FsUnit.Xunit
open Toolbox

type InvoiceIssued = {
    Id: int64
    Number: string
    Amount: decimal
}

type InvoiceNotifications =
    | InvoiceIssued of InvoiceIssued
    
let private decodePayload message =
    Decode.Auto.fromString<InvoiceNotifications>(message.Payload,
                                                 CaseStrategy.PascalCase,
                                                 Extra.empty |> Extra.withInt64 |> Extra.withDecimal)
    |> function | Ok value -> value | Error error -> failwith $"Failed to convert: {error}"

[<Fact>]
let ``GIVEN some notifications WHEN commit THEN messages are stored`` () =
    // Arrange
    let expectedEvent = { Id = 123L;  Number = "FV1"; Amount = 100M } |> InvoiceNotifications.InvoiceIssued
    let id = generateId()
    // Act
    Outbox.commit (fun _ -> id) (save DbConnection.create) [expectedEvent] |> Async.RunSynchronously
    let messages = read DbConnection.create |> Async.RunSynchronously
    // Assert
    let message = messages |> Seq.find(fun message -> message.Id = id)
    let actualEvent = message |> decodePayload
    actualEvent |> should equal expectedEvent

[<Fact>] 
let ``GIVEN some notifications WHEN process THEN messages are published`` () =
    // Arrange
    let mutable publishedMessage : OutboxMessage option = None
    let expectedNotification = { Id = 123L;  Number = "FV1"; Amount = 100M } |> InvoiceNotifications.InvoiceIssued
    let publish = fun message -> async { publishedMessage <- Some message } 
    let id = generateId()
    Outbox.commit (fun _ -> id) (save DbConnection.create) [expectedNotification] |> Async.RunSynchronously
    // Act
    Outbox.execute (read DbConnection.create) (moveToProcessed DbConnection.create) publish |> Async.RunSynchronously
    // Assert
    (publishedMessage.Value |> decodePayload) |> should equal expectedNotification
    
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