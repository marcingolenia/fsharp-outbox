module DbConnection

open Outbox

let create =
  DapperFSharp.createSqlConnection
    "Host=localhost;User Id=postgres;Password=Secret!Passw0rd;Database=outbox;Port=5432"