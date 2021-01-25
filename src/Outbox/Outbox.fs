namespace Outbox

open System
open Thoth.Json.Net

module Outbox =
  [<Literal>]
  let ParallelizationThreshold = 10
  
  let commit (generateId: unit -> int64) save notifications =
    let outboxMessages =
      notifications |> List.map (fun notification ->
        { Id = generateId()
          OccuredOn = DateTime.UtcNow
          Payload = Encode.Auto.toString(4, notification, CaseStrategy.PascalCase, Extra.empty
                                                                                   |> Extra.withDecimal
                                                                                   |> Extra.withInt64)
          Type = notification.GetType().FullName })
    async { do! save outboxMessages }
  
  let execute read setProcessed publish =
    async {
      let! (outboxMessages: OutboxMessage seq) = read
      let processes =
        outboxMessages |> Seq.map(fun message -> async { do! publish message
                                                         do! setProcessed message })
      do! processes |> (if outboxMessages |> Seq.length > ParallelizationThreshold
                        then Async.Sequential else Async.Parallel)
                    |> Async.Ignore
    }
  