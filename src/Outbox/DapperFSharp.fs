namespace Outbox

open System.Data
open Dapper
open Npgsql

module DapperFSharp =
  
  let sqlQuery<'Result> (query: string) (connection: IDbConnection): Async<'Result seq> =
    connection.QueryAsync<'Result>(query)
    |> Async.AwaitTask

  let sqlExecute (sql: string) (param: obj) (connection: IDbConnection) =
    connection.ExecuteAsync(sql, param)
    |> Async.AwaitTask
    |> Async.Ignore

  let createSqlConnection (connectionString: string): unit -> Async<IDbConnection> =
    fun () -> async {
      let connection = new NpgsqlConnection(connectionString)
      if connection.State <> ConnectionState.Open
      then do! connection.OpenAsync() |> Async.AwaitTask
      return connection :> IDbConnection
    }
