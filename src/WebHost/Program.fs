namespace WebHost

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Giraffe


module App =   
    let webApp =
        choose [
            route "/ping"   >=> text "pong"
            route "/"       >=> text "Hello :)" ]

    let configureApp (app : IApplicationBuilder) =
        app.UseGiraffe webApp

    let configureServices (services: IServiceCollection) =
        services.AddGiraffe()
                .AddHostedService(fun _ -> QuartzHostedService.Service())
                |> ignore

    [<EntryPoint>]
    let main _ =
        Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(fun webHostBuilder ->
              webHostBuilder
                .Configure(configureApp)
                .ConfigureServices(configureServices)
                |> ignore)
            .Build()
            .Run()
        0