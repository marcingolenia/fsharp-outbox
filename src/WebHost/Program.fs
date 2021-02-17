namespace WebHost

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Notifications
open RebusMessaging

module App =
    let configureApp (compositionRoot: CompositionRoot.Dependencies)
                     (app : IApplicationBuilder) =
      app.UseGiraffe (HttpHandlers.handlers compositionRoot)

    let configureServices (compositionRoot: CompositionRoot.Dependencies)
                          (services: IServiceCollection)
                          =
      services
        .AddGiraffe()
        .AddHostedService(fun _ -> QuartzHosting.Service compositionRoot.OutboxExecute)
      |> ignore

    [<EntryPoint>]
    let main _ =
      let root = CompositionRoot.compose
      Messaging.turnSubscriptionsOn
        Messaging.markerNeighbourTypes<Marker>
        root.SubBus |> Async.RunSynchronously
      Host.CreateDefaultBuilder()
        .ConfigureWebHostDefaults(fun webHostBuilder ->
          webHostBuilder
            .Configure(configureApp root)
            .ConfigureServices(configureServices root)
            |> ignore)
        .Build()
        .Run()
      0