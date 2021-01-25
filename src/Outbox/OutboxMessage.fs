namespace Outbox

  open System

  type OutboxMessage =
    { Id: int64
      OccuredOn: DateTime
      Payload: string
      Type: string }