using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;
using MySqlConnector;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

//https://github.com/warxander/vSql

namespace OriginFrameworkServer
{
  public class VSql : BaseScript
  {
    private static string connectionString;
    private static bool wasInit;

    private class DbConnection : IDisposable
    {
      public readonly MySqlConnection Connection;

      public DbConnection(string connectionString)
      { Connection = new MySqlConnection(connectionString); }

      public void Dispose()
      { Connection.Close(); }
    }

    public VSql()
    {
      //var numberOfUpdatedRows = await ExecuteAsync(query, parameters);
      //var isSucceed = await TransactionAsync(queries.Select(query => query.ToString()).ToList(), parameters);
      //var result = await FetchScalarAsync(query, parameters);
      //var result = await FetchAllAsync(query, parameters);

      EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
    }

    // delegate method
    private void OnResourceStart(string resourceName)
    {
      if (GetCurrentResourceName() != resourceName) return;

      VSql.Init();
    }

    private static void PrintException(Exception ex)
    { CitizenFX.Core.Debug.Write("^4[" + DateTime.Now + "] ^2[vSql] ^1[Error] " + ex.Message + "\n"); }

    public static async Task<int> ExecuteAsync(string query, IDictionary<string, object> parameters)
    {
      int numberOfUpdatedRows = 0;

      try
      {
        using (var db = new DbConnection(connectionString))
        {
          await db.Connection.OpenAsync();

          using (var command = db.Connection.CreateCommand())
          {
            BuildCommand(command, query, parameters);
            numberOfUpdatedRows = await command.ExecuteNonQueryAsync();
          }
        }
      }
      catch (Exception ex)
      { PrintException(ex); }

      return numberOfUpdatedRows;
    }

    public static async Task<bool> TransactionAsync(IList<string> queries, IDictionary<string, object> parameters)
    {
      bool isSucceed = false;

      try
      {
        using (var db = new DbConnection(connectionString))
        {
          await db.Connection.OpenAsync();

          using (var command = db.Connection.CreateCommand())
          {
            foreach (var parameter in parameters ?? Enumerable.Empty<KeyValuePair<string, object>>())
              command.Parameters.AddWithValue(parameter.Key, parameter.Value);

            using (var transaction = await db.Connection.BeginTransactionAsync())
            {
              command.Transaction = transaction;

              try
              {
                foreach (var query in queries)
                {
                  command.CommandText = query;
                  await command.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
                isSucceed = true;
              }
              catch (Exception ex)
              {
                PrintException(ex);

                try
                { await transaction.RollbackAsync(); }
                catch (Exception rollbackEx)
                { PrintException(rollbackEx); }
              }
            }
          }
        }
      }
      catch (Exception ex)
      { PrintException(ex); }

      return isSucceed;
    }

    public static async Task<object> FetchScalarAsync(string query, IDictionary<string, object> parameters)
    {
      object result = null;

      try
      {
        using (var db = new DbConnection(connectionString))
        {
          await db.Connection.OpenAsync();

          using (var command = db.Connection.CreateCommand())
          {
            BuildCommand(command, query, parameters);
            result = await command.ExecuteScalarAsync();
          }
        }
      }
      catch (Exception ex)
      { PrintException(ex); }

      return result;
    }

    public static async Task<List<Dictionary<string, object>>> FetchAllAsync(string query, IDictionary<string, object> parameters)
    {
      var result = new List<Dictionary<string, Object>>();

      try
      {
        using (var db = new DbConnection(connectionString))
        {
          await db.Connection.OpenAsync();

          using (var command = db.Connection.CreateCommand())
          {
            BuildCommand(command, query, parameters);

            using (var reader = await command.ExecuteReaderAsync())
            {
              while (await reader.ReadAsync())
              {
                result.Add(Enumerable.Range(0, reader.FieldCount).ToDictionary(
                    i => reader.GetName(i),
                    i => reader.IsDBNull(i) ? null : reader.GetValue(i)
                ));
              }
            }
          }
        }
      }
      catch (Exception ex)
      { PrintException(ex); }

      return result;
    }

    private static void BuildCommand(MySqlCommand command, string query, IDictionary<string, object> parameters)
    {
      command.CommandText = query;

      foreach (var parameter in parameters ?? Enumerable.Empty<KeyValuePair<string, object>>())
        command.Parameters.AddWithValue(parameter.Key, parameter.Value);
    }

    private static void Init()
    {
      if (wasInit)
        return;

      var temp = Function.Call<string>(Hash.GET_CONVAR, "mysql_connection_string");

      if (temp != null && temp.StartsWith("mysql://"))
      {
        temp = temp.Substring(8);

        var usernameEnd = temp.IndexOf(':');
        var passwordEnd = temp.LastIndexOf('@');
        var serverEnd = temp.LastIndexOf('/');
        var dbEnd = temp.LastIndexOf('?');

        var user = temp.Substring(0, usernameEnd);
        var pass = temp.Substring(usernameEnd + 1, passwordEnd - usernameEnd - 1);
        var server = temp.Substring(passwordEnd + 1, serverEnd - passwordEnd - 1);
        var db = temp.Substring(serverEnd + 1, dbEnd - serverEnd - 1);

        connectionString = $"server={server};database={db};userid={user};password={pass}";
      }
      else
      {
        connectionString = temp;
      }

      
      wasInit = true;

      Debug.WriteLine($"OFW: VSql initialized");
    }
  }
}

