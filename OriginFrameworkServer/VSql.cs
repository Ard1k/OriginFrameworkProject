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
using OriginFrameworkData.DataBags;

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
      CleanupDBOnStart();

      tablesEnsured = true;

      InternalDependencyManager.Started(eScriptArea.VSql);
    }

    private Task<int> EnsureDB_cfw_jsondata_table()
    {
      return VSql.ExecuteAsync("CREATE TABLE IF NOT EXISTS `cfw_jsondata` (`key` varchar(60) NOT NULL, `data` TEXT NOT NULL, PRIMARY KEY (`key`)) DEFAULT CHARSET=latin1;", null);
    }
    private async void EnsureDB_tables()
    {
      await VSql.ExecuteAsync("CREATE TABLE IF NOT EXISTS `organization` " +
                    " (`id` int NOT NULL AUTO_INCREMENT, " +
                    "  `name` varchar(50) NOT NULL, " +
                    "  `tag` char(3) NOT NULL, " +
                    "  `owner` int NOT NULL, " +
                    "  `color` int NOT NULL, " +
                    "  `data` longtext NULL," +
                    "  `bank_money` INT NOT NULL DEFAULT(0), " +
                    " PRIMARY KEY (`id`), " +
                    //" CONSTRAINT `fk_organization_owner_id` FOREIGN KEY (`owner`) REFERENCES `character` (`id`) ON DELETE RESTRICT ON UPDATE RESTRICT, " + //Tohle se musi resit pozdeji kvuli vzajemne referenci
                    " CONSTRAINT `unique_orgtag` UNIQUE (`tag`)" +
                    " );", null);
      await VSql.ExecuteAsync("CREATE TABLE IF NOT EXISTS `prop_map` (`id` int NOT NULL AUTO_INCREMENT, `name` varchar(200) NOT NULL, PRIMARY KEY (`id`));", null);
      await VSql.ExecuteAsync("CREATE TABLE IF NOT EXISTS `prop_map_item` (`id` int NOT NULL AUTO_INCREMENT, `prop_map_id` int NOT NULL, `data` TEXT NOT NULL, PRIMARY KEY (`id`), CONSTRAINT `fk_prop_map_item_id` FOREIGN KEY (`prop_map_id`) REFERENCES prop_map (`id`) ON DELETE RESTRICT ON UPDATE RESTRICT );", null);
      await VSql.ExecuteAsync("CREATE TABLE IF NOT EXISTS `user` (`identifier` varchar(200) NOT NULL, `name` varchar(200) NULL, `steam` varchar(200) NULL, `license` varchar(200) NULL, `discord` varchar(200) NULL, `ip` varchar(200) NULL, `admin_level` int NOT NULL DEFAULT(0), `active_character` int NULL, PRIMARY KEY (`identifier`));", null);
      await VSql.ExecuteAsync("CREATE TABLE IF NOT EXISTS `character` (`id` int NOT NULL AUTO_INCREMENT, `user_identifier` varchar(200) NOT NULL, `name` varchar(200) NOT NULL, `pos` varchar(500) NULL, `model` int NULL, `is_disabled` int NOT NULL DEFAULT(0), `organization_id` int NULL, `bank_money` INT NOT NULL DEFAULT(0), `skin` LONGTEXT NULL, `is_new` BIT NOT NULL DEFAULT(1), `lore_id` BIT NOT NULL DEFAULT(0), `born` date NOT NULL DEFAULT('2000-01-01'), `job_id` int NOT NULL DEFAULT(0), `job_grade` int NOT NULL DEFAULT(0), PRIMARY KEY (`id`), CONSTRAINT `fk_character_user_identifier` FOREIGN KEY (`user_identifier`) REFERENCES user (`identifier`) ON DELETE RESTRICT ON UPDATE RESTRICT, CONSTRAINT `fk_organization_id_organization_id` FOREIGN KEY (`organization_id`) REFERENCES `organization` (`id`) ON DELETE RESTRICT ON UPDATE RESTRICT);", null);
      await VSql.ExecuteAsync("CREATE TABLE IF NOT EXISTS `inventory_item` " +
                                " (`id` int NOT NULL AUTO_INCREMENT, " +
                                "  `place` varchar(200) NOT NULL, " +
                                "  `item_id` int NOT NULL, " +
                                "  `x` int NOT NULL, " +
                                "  `y` int NOT NULL, " +
                                "  `count` int NOT NULL, " +
                                "  `metadata` varchar(2000) NULL, " +
                                "  `related_to` varchar(20) NULL, " +
                                " PRIMARY KEY (`id`), " +
                                " INDEX (`place`) " +
                                " );", null);
      await VSql.ExecuteAsync("CREATE TABLE IF NOT EXISTS `item_definition` (`id` int NOT NULL, `data` LONGTEXT NOT NULL, PRIMARY KEY (`id`));", null);
      await VSql.ExecuteAsync("CREATE TABLE IF NOT EXISTS `vehicle` " +
                          " (`id` int NOT NULL AUTO_INCREMENT, " +
                          "  `model` varchar(50) NOT NULL, " +
                          "  `plate` varchar(8) NOT NULL, " +
                          "  `place` varchar(20) NOT NULL, " +
                          "  `properties` varchar(2000) NOT NULL, " +
                          "  `damage` varchar(2000) NULL, " +
                          "  `odometer` int NOT NULL DEFAULT(0), " +
                          "  `owner_char` int NULL, " +
                          "  `owner_organization` int NULL, " +
                          " PRIMARY KEY (`id`), " +
                          " CONSTRAINT `fk_vehicle_character_id` FOREIGN KEY (`owner_char`) REFERENCES `character` (`id`) ON DELETE RESTRICT ON UPDATE RESTRICT," +
                          " CONSTRAINT `fk_vehicle_organization_id` FOREIGN KEY (`owner_organization`) REFERENCES `organization` (`id`) ON DELETE RESTRICT ON UPDATE RESTRICT, " +
                          " CONSTRAINT `unique_plate` UNIQUE (`plate`) " +
                          " );", null);

      var r1 = await VSql.FetchAllAsync("SELECT `CONSTRAINT_NAME` FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE CONSTRAINT_NAME = 'fk_organization_owner_id'", null);
      if (r1 == null || r1.Count <= 0)
        await VSql.ExecuteAsync("ALTER TABLE `organization` ADD CONSTRAINT `fk_organization_owner_id` FOREIGN KEY (`owner`) REFERENCES `character` (`id`) ON DELETE RESTRICT ON UPDATE RESTRICT;", null);

      await VSql.ExecuteAsync("CREATE TABLE IF NOT EXISTS `organization_manager` " +
                          " (`organization_id` int NOT NULL, " +
                          "  `character_id` int NOT NULL, " +
                          " PRIMARY KEY (`organization_id`, `character_id`), " +
                          " CONSTRAINT `fk_organization_manager_organization_id` FOREIGN KEY (`organization_id`) REFERENCES `organization` (`id`) ON DELETE RESTRICT ON UPDATE RESTRICT," +
                          " CONSTRAINT `fk_organization_manager_character_id` FOREIGN KEY (`character_id`) REFERENCES `character` (`id`) ON DELETE RESTRICT ON UPDATE RESTRICT " +
                          " );", null);

      await VSql.ExecuteAsync("CREATE TABLE IF NOT EXISTS `organization_vehiclerights` " +
                          " (`vehicle_id` int NOT NULL, " +
                          "  `character_id` int NOT NULL, " +
                          " PRIMARY KEY (`vehicle_id`, `character_id`), " +
                          " CONSTRAINT `fk_organization_vehiclerights_vehicle_id` FOREIGN KEY (`vehicle_id`) REFERENCES `vehicle` (`id`) ON DELETE RESTRICT ON UPDATE RESTRICT," +
                          " CONSTRAINT `fk_organization_vehiclerights_character_id` FOREIGN KEY (`character_id`) REFERENCES `character` (`id`) ON DELETE RESTRICT ON UPDATE RESTRICT " +
                          " );", null);
    }

    private async void CleanupDBOnStart()
    {
      var carKeys = ItemsDefinitions.Items.Where(it => it != null && it.UsableType == eUsableType.CarKey)?.ToList();

      if (carKeys != null && carKeys.Count > 0)
      {
        bool isFirst = true;
        string keysParam = String.Empty;
        foreach (var keyType in carKeys)
        {
          if (isFirst)
            keysParam += $"{keyType.ItemId}";
          else
            keysParam += $", {keyType.ItemId}";

          isFirst = false;
        }

        //var param = new Dictionary<string, object>();
        //param.Add("@keys", keysParam);
        await VSql.ExecuteAsync($"DELETE FROM `inventory_item` WHERE `item_id` in ({keysParam})", null);
      }
    }

    private static void PrintException(Exception ex, string query, IDictionary<string, object> parameters)
    { 
      CitizenFX.Core.Debug.Write("^4[" + DateTime.Now + "] ^2[vSql] ^1[Error] " + ex.Message + "\n"); 
      if (query != null)
        CitizenFX.Core.Debug.Write("^4[" + DateTime.Now + "] ^2[vSql] ^1[Query] " + query + "\n");
      if (parameters != null)
        CitizenFX.Core.Debug.Write("^4[" + DateTime.Now + "] ^2[vSql] ^1[Query] " + parameters.ToString() + "\n");
    }

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
      { PrintException(ex, query, parameters); }

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
                PrintException(ex, null, null);

                try
                { await transaction.RollbackAsync(); }
                catch (Exception rollbackEx)
                { PrintException(rollbackEx, null, null); }
              }
            }
          }
        }
      }
      catch (Exception ex)
      { PrintException(ex, null, null); }

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
      { PrintException(ex, query, parameters); }

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
      { PrintException(ex, query, parameters); }

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

