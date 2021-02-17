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
    MessageBus: IBus
    GenerateId: unit -> int64
  }
  
  let compose =
    let pubBus = Messaging.configureOneWay
                   "amqp://localhost"
                   "pubConnection"
                   (new BuiltinHandlerActivator())
    let subBus = Messaging.configure
                   "amqp://localhost"
                   "subConnection"
                   (new BuiltinHandlerActivator() |> Messaging.registerHandler Handlers.printWhateverHappenedWithSmiley)
    let dbConnection = "Host=localhost;User Id=postgres;Password=Secret!Passw0rd;Database=outbox;Port=5432"
    {
      OutboxCommit = Outbox.commit
                       IdGenerator.generateId
                       (PostgresPersistence.save (createSqlConnection dbConnection))
      OutboxExecute = Outbox.execute
                        (PostgresPersistence.read (createSqlConnection dbConnection))
                        (PostgresPersistence.moveToProcessed (createSqlConnection dbConnection))
                        (Messaging.publish pubBus)
      MessageBus = subBus
      GenerateId = IdGenerator.generateId
    }