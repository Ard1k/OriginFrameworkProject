using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;

namespace OriginFrameworkServer
{
	public class SyncLocker : IDisposable
	{
		private LockObj Lock = null;

		private SyncLocker(LockObj li)
		{
			if (li == null)
				throw new ApplicationException("Incorrect SyncLocker usage");
			
			Lock = li;
			Lock.Locked = true;
		}

		private static async Task<bool> WaitForUnlock(LockObj li)
		{
			while (li.Locked)
				await BaseScript.Delay(0);

			return true;
		}

		public void Dispose()
		{
			if (Lock != null)
				Lock.Locked = false;
		}

		public static async Task<SyncLocker> GetLockerWhenAvailible(LockObj lockObj)
		{
			await WaitForUnlock(lockObj);

			return new SyncLocker(lockObj);
		}
	}

	public class LockObj
	{
		public bool Locked { get; set; } = false;
	}
}
