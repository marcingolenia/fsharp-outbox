namespace WebHost

open System.Threading.Tasks
open Quartz

module PollingPublisher = 
    let trigger = TriggerBuilder
                    .Create()
                    .WithSimpleSchedule(fun scheduler ->
                        scheduler.WithIntervalInSeconds(5)
                                 .RepeatForever() |> ignore)
                    .Build()
                          
    [<DisallowConcurrentExecution>]
    type Job(outboxExecute: Async<unit>) =
      interface IJob with
        member _.Execute _ =
          outboxExecute |> Async.StartAsTask :> Task
          
    let job = JobBuilder
                .Create<Job>()
                .WithIdentity("PollingPublisher")
                .Build();