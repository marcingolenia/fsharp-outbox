namespace WebHost

open System.Threading.Tasks
open Microsoft.Extensions.Hosting
open FSharp.Control.Tasks.V2.ContextInsensitive
open Quartz
open Quartz.Impl
open Quartz.Spi

module QuartzHosting =
    
    type JobFactory(outboxExecute: Async<unit>) = 
      interface IJobFactory with
        member _.NewJob(bundle, _) =
          match bundle.JobDetail.JobType with
          | _type when _type = typeof<PollingPublisher.Job> -> PollingPublisher.Job(outboxExecute) :> IJob
          | _ -> failwith "Not supported Job"
        member _.ReturnJob _ = ()
    
    type Service(outboxExecute: Async<unit>) =
      let mutable scheduler: IScheduler = null 
      interface IHostedService with
      
        member _.StartAsync(cancellation) =
          printfn $"Starting Quartz Hosting Service"
          task {
            let! schedulerConfig = StdSchedulerFactory().GetScheduler()
            schedulerConfig.JobFactory <- JobFactory(outboxExecute)
            let! _ = schedulerConfig.ScheduleJob(
                      PollingPublisher.job,
                      PollingPublisher.trigger,
                      cancellation)
            do! schedulerConfig.Start(cancellation)
            scheduler <- schedulerConfig
          } :> Task
        
        member _.StopAsync(cancellation) =
          printfn $"Stopping Quartz Hosting Service"
          scheduler.Shutdown(cancellation)