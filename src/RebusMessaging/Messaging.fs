namespace RebusMessaging

open System.Threading.Tasks
open Rebus.Activation
open Rebus.Bus
open Rebus.Logging
open Rebus.Config
open Notifications

module Messaging =
    let queueName = "mcode.fun"
    
    let private toTask asyncA =
      asyncA |> Async.StartAsTask :> Task
      
    let subscribe<'a> (bus: IBus) =
      bus.Subscribe<'a>() |> Async.AwaitTask
    
    let publish (bus: IBus) message =
      bus.Publish message |> Async.AwaitTask
    
    let start (handler: WhateverHappened -> Async<Unit>)
              (activator: BuiltinHandlerActivator)
              (endpoint: string)
              =
      activator.Handle<WhateverHappened>(fun message -> handler message |> toTask) |> ignore
      let bus = Configure.With(activator)
                         .Transport(fun transport -> transport.UseRabbitMq(endpoint, queueName) |> ignore)
                         .Logging(fun logConfig -> logConfig.Console(LogLevel.Info))
                         .Start()
      async {
        do! subscribe<WhateverHappened> bus
        return bus
      }
      