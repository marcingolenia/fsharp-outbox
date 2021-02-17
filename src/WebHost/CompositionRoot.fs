namespace WebHost

open Outbox
open DapperFSharp
open Rebus.Activation
open Rebus.Bus
open RebusMessaging

module CompositionRoot =

  type Dependencies = {
    OutboxCommit: obj list -> Async<unit>
    OutboxExecute: Async<unit>
    SubBus: IBus
    GenerateId: unit -> int64
  }
  
  let compose =
    let pubBus = Messaging.configure
                  "amqp://localhost"
                  "pub-connection"
                  (new BuiltinHandlerActivator())
    let connectionS = "Host=localhost;User Id=postgres;Password=Secret!Passw0rd;Database=outbox;Port=5432"
    {
      OutboxCommit = Outbox.commit
                       IdGenerator.generateId
                       (PostgresPersistence.save (createSqlConnection connectionS))
      OutboxExecute = Outbox.execute
                        (PostgresPersistence.read (createSqlConnection connectionS))
                        (PostgresPersistence.moveToProcessed (createSqlConnection connectionS))
                        (Messaging.publish pubBus)
      SubBus = Messaging.configure
                          "amqp://localhost"
                          "subs-connection"
                          (new BuiltinHandlerActivator()
                           |> Messaging.registerHandler Handlers.printWhateverHappenedWithSmiley)
      GenerateId = IdGenerator.generateId
    }