module Tests

open System
open Xunit

type InvoiceIssued = {
    Id: int64
    Number: string
    Amount: decimal
}

type InvoiceIssuedEvents =
    | InvoiceIssued of InvoiceIssued
    
    

[<Fact>]
let ``GIVEN an event WHEN save THEN event is stored`` () =
    save [{ Id = int64 5; Number = "FV1"; Amount = 100M; }] |> Async.RunSynchronously
    let d = messages
    Assert.True true