using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;
using MySqlConnector;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

//https://github.com/warxander/vSql

namespace OriginFrameworkServer
{
  public class VSql : BaseScript
  {
    private static string connectionString;
    private static bool wasInit = false;
    private static bool tablesEnsured = false;
    public static bool IsReadyToUse {get { return (tablesEnsured && wasInit); } }

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
      EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
    }

    // delegate method
    private async void OnResourceStart(string resourceName)
    {
      if (GetCurrentResourceName() != resourceName) return;

      if (!await InternalDependencyManager.CanStart(eScriptArea.VSql))
        return;

      VSql.Init();
      await EnsureDB_cfw_jsondata_table();
      EnsureDB_tables();

      tablesEnsured = true;

      InternalDependencyManager.Started(eScriptArea.VSql);
    }

    private Task<int> EnsureDB_cfw_jsondata_table()
    {
      return VSql.ExecuteAsync("CREATE TABLE IF NOT EXISTS `cfw_jsondata` (`key` varchar(60) NOT NULL, `data` TEXT NOT NULL, PRIMARY KEY (`key`)) DEFAULT CHARSET=latin1;", null);
    }
    private async void EnsureDB_tables()
    {
      await VSql.ExecuteAsync("CREATE TABLE IF NOT EXISTS `prop_map` (`id` int NOT NULL AUTO_INCREMENT, `name` varchar(200) NOT NULL, PRIMARY KEY (`id`));", null);
      await VSql.ExecuteAsync("CREATE TABLE IF NOT EXISTS `prop_map_item` (`id` int NOT NULL AUTO_INCREMENT, `prop_map_id` int NOT NULL, `data` TEXT NOT NULL, PRIMARY KEY (`id`), CONSTRAINT `fk_prop_map_item_id` FOREIGN KEY (`prop_map_id`) REFERENCES prop_map (`id`) ON DELETE RESTRICT ON UPDATE RESTRICT );", null);
      await VSql.ExecuteAsync("CREATE TABLE IF NOT EXISTS `user` (`identifier` varchar(200) NOT NULL, `name` varchar(200) NULL, `steam` varchar(200) NULL, `license` varchar(200) NULL, `discord` varchar(200) NULL, `ip` varchar(200) NULL, `admin_level` int NOT NULL DEFAULT(0), `active_character` int NULL, PRIMARY KEY (`identifier`));", null);
      await VSql.ExecuteAsync("CREATE TABLE IF NOT EXISTS `character` (`id` int NOT NULL AUTO_INCREMENT, `user_identifier` varchar(200) NOT NULL, `name` varchar(200) NOT NULL, `pos` varchar(500) NULL, `model` int NULL, `is_disabled` int NOT NULL DEFAULT(0), PRIMARY KEY (`id`), CONSTRAINT `fk_character_user_identifier` FOREIGN KEY (`user_identifier`) REFERENCES user (`identifier`) ON DELETE RESTRICT ON UPDATE RESTRICT);", null);
      await VSql.ExecuteAsync("CREATE TABLE IF NOT EXISTS `inventory_item` " +
                                " (`id` int NOT NULL AUTO_INCREMENT, " +
                                "  `place` varchar(200) NOT NULL, " +
                                "  `item_id` int NOT NULL, " +
                                "  `x` int NOT NULL, " +
                                "  `y` int NOT NULL, " +
                                "  `count` int NOT NULL, " +
                                "  `metadata` varchar(2000) NULL, " +
                                " PRIMARY KEY (`id`), " +
                                " INDEX (`place`) " +
                                " );", null);
      await VSql.ExecuteAsync("CREATE TABLE IF NOT EXISTS `item_definition` (`id` int NOT NULL, `data` LONGTEXT NOT NULL, PRIMARY KEY (`id`));", null);
      await VSql.ExecuteAsync("CREATE TABLE IF NOT EXISTS `organization` " +
                    " (`id` int NOT NULL AUTO_INCREMENT, " +
                    "  `name` varchar(50) NOT NULL, " +
                    "  `tag` varchar(8) NOT NULL, " +
                    "  `owner` int NOT NULL, " +
                    " PRIMARY KEY (`id`), " +
                    " CONSTRAINT `fk_organization_owner_id` FOREIGN KEY (`owner`) REFERENCES `character` (`id`) ON DELETE RESTRICT ON UPDATE RESTRICT " +
                    " );", null);
      await VSql.ExecuteAsync("CREATE TABLE IF NOT EXISTS `vehicle` " +
                          " (`id` int NOT NULL AUTO_INCREMENT, " +
                          "  `model` varchar(50) NOT NULL, " +
                          "  `plate` varchar(8) NOT NULL, " +
                          "  `place` varchar(20) NOT NULL, " +
                          "  `properties` varchar(2000) NOT NULL, " +
                          "  `owner_char` int NULL, " +
                          "  `owner_organization` int NULL, " +
                          " PRIMARY KEY (`id`), " +
                          " CONSTRAINT `fk_vehicle_character_id` FOREIGN KEY (`owner_char`) REFERENCES `character` (`id`) ON DELETE RESTRICT ON UPDATE RESTRICT," +
                          " CONSTRAINT `fk_vehicle_organization_id` FOREIGN KEY (`owner_organization`) REFERENCES `organization` (`id`) ON DELETE RESTRICT ON UPDATE RESTRICT, " +
                          " CONSTRAINT `unique_plate` UNIQUE (`plate`) " +
                          " );", null);
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
            using (var pm = new PerformanceMeter("Execute", command))
            {
              numberOfUpdatedRows = await command.ExecuteNonQueryAsync();
            }
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
            using (var pm = new PerformanceMeter("FetchScalar", command))
            {
              result = await command.ExecuteScalarAsync();
            }
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

            using (var pm = new PerformanceMeter("FetchAll", command))
            {
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
    }

    private class PerformanceMeter : IDisposable
    {
      string _method = null;
      Stopwatch _sw = new Stopwatch();
      MySqlCommand _command = null;

      public PerformanceMeter(string method, MySqlCommand command)
      {
        this._method = method;
        this._command = command;

        _sw.Start();
      }

      public void Dispose()
      {
        _sw.Stop();

        if (_sw.ElapsedMilliseconds < 200)
          return;

        CitizenFX.Core.Debug.WriteLine($"[VSql - {_method}, time: {_sw.ElapsedMilliseconds}ms]");
        CitizenFX.Core.Debug.WriteLine($"     Query: \"{_command.CommandText}\"");
        CitizenFX.Core.Debug.WriteLine($"     Params:{string.Join(", ", _command.Parameters?.ToArray().Select(a => $"[Name:\"{a.ParameterName}\" Value:\"{a.Value?.ToString()}\"]"))}");
      }
    }
  }
}

