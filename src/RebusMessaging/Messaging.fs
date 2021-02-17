namespace RebusMessaging

open System
open System.Threading.Tasks
open Rebus.Activation
open Rebus.Bus
open Rebus.Logging
open Rebus.Config

module Messaging =
    let private queueName = "mcode.fun"
    
    let private toTask asyncA =
      asyncA |> Async.StartAsTask :> Task
    
    let publish (bus: IBus) message =
      bus.Publish message |> Async.AwaitTask
        
    let configure (endpoint: string)
                  (connectionName: string)
                  (activator: BuiltinHandlerActivator)
                  =
      Configure.With(activator)
               .Transport(fun transport -> transport.UseRabbitMq(endpoint, queueName)
                                                    .ClientConnectionName(connectionName) |> ignore)
               .Logging(fun logConfig -> logConfig.Console(LogLevel.Info))
               .Start()
    
    let registerHandler
      (handler: 'a -> Async<Unit>)
      (activator: BuiltinHandlerActivator) =
        activator.Handle<'a>(fun message -> handler message |> toTask)
    
    let markerNeighbourTypes<'marker> =
      (typeof<'marker>.DeclaringType).GetNestedTypes()
        |> Array.filter(fun type_ -> type_.IsAbstract = false)
    
    let turnSubscriptionsOn (types: Type[]) (bus: IBus) =
      async {
        types |> Array.iter(fun type_ -> bus.Subscribe type_ |> ignore)
      }