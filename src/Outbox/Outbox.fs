namespace Outbox

open System
open System.Reflection
open Notifications
open Newtonsoft.Json

module Outbox =
  [<Literal>]
  let ParallelizationThreshold = 10
  
  let commit generateId save notifications =
    let outboxMessages =
      notifications |> List.map (fun notification ->
        { Id = generateId()
          OccuredOn = DateTime.UtcNow
          Payload = JsonConvert.SerializeObject notification
          Type = notification.GetType().FullName })
    async { do! save outboxMessages }
  
  let execute read setProcessed publish =
    async {
      let! (messages: OutboxMessage seq) = read 
      let notificationAssembly = Assembly.GetAssembly(typeof<Marker>)   
      let processes = messages |> Seq.map(fun message ->
        async {
                do! publish (JsonConvert.DeserializeObject(message.Payload, notificationAssembly.GetType(message.Type)))
                do! setProcessed message })
      do! processes |> (if messages |> Seq.length > ParallelizationThreshold
                        then Async.Sequential else Async.Parallel)
                    |> Async.Ignore
    }
  