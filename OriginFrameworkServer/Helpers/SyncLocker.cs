using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CitizenFX.Core;
using Debug = CitizenFX.Core.Debug;

namespace OriginFrameworkServer
{
	public class SyncLocker : IDisposable
	{
		private LockObj Lock = null;
		private Stopwatch sw = null;
		private SyncLocker(LockObj li)
		{
			if (li == null)
				throw new ApplicationException("Incorrect SyncLocker usage");
			
			Lock = li;
			Lock.Locked = true;
			sw = Stopwatch.StartNew();
    }

		private static async Task<bool> WaitForUnlock(LockObj li)
		{
			int nextWarning = 10000;
      Stopwatch swUnlock = Stopwatch.StartNew();
			while (li.Locked)
			{
				if (swUnlock.ElapsedMilliseconds > nextWarning)
				{
					Debug.WriteLine($"SYNCLOCK [{li.Name ?? "ANONYMOUS"}] !!! WARNING !!! already waiting for {swUnlock.ElapsedMilliseconds}ms");
					nextWarning = nextWarning * 2;
				}
        await BaseScript.Delay(0);
			}

			swUnlock.Stop();
			//Debug.WriteLine($"SYNCLOCK [{li.Name ?? "ANONYMOUS"}] waited {swUnlock.ElapsedMilliseconds}ms");
			return true;
		}

		public void Dispose()
		{
			if (Lock != null)
				Lock.Locked = false;
			sw.Stop();
			if (sw.ElapsedMilliseconds > 100)
				Debug.WriteLine($"SYNCLOCK [{Lock.Name ?? "ANONYMOUS"}] was locked for {sw.ElapsedMilliseconds}ms");
    }

		public static async Task<SyncLocker> GetLockerWhenAvailible(LockObj lockObj)
		{
			await WaitForUnlock(lockObj);

			return new SyncLocker(lockObj);
		}
	}

	public class LockObj
	{
		public string Name { get; set; }
		public bool Locked { get; set; } = false;

		public LockObj(string name)
		{
			Name = name;
		}
	}
}
