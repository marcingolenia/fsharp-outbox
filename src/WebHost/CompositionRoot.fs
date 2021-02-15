namespace WebHost

open Outbox
open DapperFSharp
open RebusMessaging

module CompositionRoot =

  type CompositionRoot = {
    Read: Async<seq<OutboxMessage>>
    SetProcessed: OutboxMessage -> Async<unit>
    Publish: obj -> Async<unit>
    Commit: obj list -> Async<unit>
  }
  
  let compose bus =
    let connectionS = "Host=localhost;User Id=postgres;Password=Secret!Passw0rd;Database=outbox;Port=5432"
    {
      Read = PostgresPersistence.read (createSqlConnection connectionS)
      SetProcessed = PostgresPersistence.moveToProcessed (createSqlConnection connectionS)
      Publish = (Messaging.publish bus)
      Commit = PostgresPersistence.save (createSqlConnection connectionS)
    }