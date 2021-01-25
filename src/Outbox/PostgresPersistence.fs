namespace Outbox

open DapperFSharp

module PostgresPersistence =
  let read createConnection =
    let cmd = "SELECT id, occured_on as OccuredOn, payload, type FROM outbox_messages" 
    async {
      use! connection = createConnection ()
      return! connection |> sqlQuery<OutboxMessage> cmd
    }
    
  let readProcessed createConnection =
    let cmd = "SELECT id, occured_on as OccuredOn, payload, type FROM outbox_messages_processed" 
    async {
      use! connection = createConnection ()
      return! connection |> sqlQuery<OutboxMessage> cmd
    }
    
  let save createConnection outboxMessages =
    let cmd = "INSERT INTO outbox_messages(id, occured_on, payload, type) VALUES (@Id, @OccuredOn, @Payload::jsonb, @Type)"
    async {
      use! connection = createConnection ()
      do! connection |> sqlExecute cmd outboxMessages
    }
  
  let moveToProcessed createConnection outboxMessage =
    let cmd = "
    WITH moved_rows AS (
    DELETE FROM outbox_messages deleted
    WHERE Id = @id
    RETURNING deleted.* 
)
INSERT INTO outbox_messages_processed(id, occured_on, payload, type, processed_on)
SELECT id, occured_on, payload, type, now() at time zone 'utc' FROM moved_rows;
    "
    async {
      use! connection = createConnection ()
      do! connection |> sqlExecute cmd {| id = outboxMessage.Id |}
    }