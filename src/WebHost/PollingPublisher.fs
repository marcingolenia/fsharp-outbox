namespace WebHost

open System
open System.Threading.Tasks
open Quartz

module PollingPublisher = 
    let trigger = TriggerBuilder
                    .Create()
                    .WithSimpleSchedule(fun scheduler ->
                        scheduler.WithIntervalInSeconds(3).RepeatForever() |> ignore)
                    .Build()
                          
    [<DisallowConcurrentExecution>]
    type Job() =
      interface IJob with
        member _.Execute ctx =
          printf "%s\n" (DateTime.Now.ToLongTimeString())
          Task.CompletedTask
          
    let job = JobBuilder
                .Create<Job>()
                .WithIdentity("PollingPublisher")
                .Build();