using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace OriginFramework
{
  public class Callbacks
  {
    public static async Task<T> ServerAsyncCallbackToSync<T>(string eventName, params object[] args)
    {
      if (args == null)
        args = new object[0];

      var expandedArgs = new object[args.Length + 1];
      args.CopyTo(expandedArgs, 0);

      T ret = default;
      bool completed = false;
      Func<T, bool> CallbackFunction = (data) =>
      {
        ret = data;
        completed = true;
        return true;
      };

      expandedArgs[args.Length] = CallbackFunction;

      BaseScript.TriggerServerEvent(eventName, expandedArgs);

      while (!completed)
      {
        await BaseScript.Delay(0);
      }

      return ret;
    }

    /// <summary>
    /// Pouziva dvouparametrovy callback kde prvni parametr je bool jestli se operace povedla a druha error message. Pokud se callback vrati s false, automaticky se vyhodi error notifikace
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="eventName"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static async Task<bool> ServerAsyncCallbackToSyncWithErrorMessage(string eventName, params object[] args)
    {
      if (args == null)
        args = new object[0];

      var expandedArgs = new object[args.Length + 1];
      args.CopyTo(expandedArgs, 0);

      bool ret = default;
      string errorMessage = default;
      bool completed = false;
      Func<bool, string, bool> CallbackFunction = (data, data2) =>
      {
        ret = data;
        errorMessage = data2;
        completed = true;
        return true;
      };

      expandedArgs[args.Length] = CallbackFunction;

      BaseScript.TriggerServerEvent(eventName, expandedArgs);

      while (!completed)
      {
        await BaseScript.Delay(0);
      }

      if (ret == false)
      {
        Notify.Error(errorMessage ?? String.Empty);
      }

      return ret;
    }

    public static async Task<T> ServerAsyncCallbackToSyncWithText<T>(string eventName, string waitText, params object[] args)
    {
      if (args == null)
        args = new object[0];

      var expandedArgs = new object[args.Length + 1];
      args.CopyTo(expandedArgs, 0);

      T ret = default;
      bool completed = false;
      Func<T, bool> CallbackFunction = (data) =>
      {
        ret = data;
        completed = true;
        return true;
      };

      expandedArgs[args.Length] = CallbackFunction;

      BaseScript.TriggerServerEvent(eventName, expandedArgs);

      while (!completed)
      {
        await BaseScript.Delay(0);

        TextUtils.DrawScreenText(waitText, 255, 255, 255, 150);
      }

      return ret;
    }
  }
}
