using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginFrameworkServer
{
	public static class OfwServerFunctions
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

			BaseScript.TriggerEvent(eventName, expandedArgs);

			while (!completed)
			{
				await BaseScript.Delay(0);
			}

			return ret;
		}
	}
}
