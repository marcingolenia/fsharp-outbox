namespace RebusMessaging

open System.Threading.Tasks
open Rebus.Activation
open Rebus.Bus
open Rebus.Logging
open Rebus.Config

module Publisher =
    
    type WhateverHappened = {
      Id: int64
      Number: string
      Amount: decimal
    }
    
    let queueName = "mcode.fun"
    
    let private toTask asyncA =
      asyncA |> Async.StartAsTask :> Task
      
    let subscribe<'a> (bus: IBus) =
      bus.Subscribe<'a>() |> Async.AwaitTask
    
    let publish message (bus: IBus) =
      bus.Publish message |> Async.AwaitTask
    
    let start (handleInvoiceIssued: WhateverHappened -> Async<Unit>)
              (activator: BuiltinHandlerActivator)
              (endpoint: string)
              =
      activator.Handle<WhateverHappened>(fun inv -> handleInvoiceIssued inv |> toTask) |> ignore
      let bus = Configure.With(activator)
                         .Transport(fun transport -> transport.UseRabbitMq(endpoint, queueName) |> ignore)
                         .Logging(fun logConfig -> logConfig.Console(LogLevel.Info))
                         .Start()
      async {
        do! subscribe<WhateverHappened> bus
        return bus
      }
      