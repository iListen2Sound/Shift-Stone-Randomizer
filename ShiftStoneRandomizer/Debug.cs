using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;

namespace ShiftStoneRandomizer
{
	public static class Debug
	{
		public static bool debugMode = true;
		public static void Log(string message, bool debugOnly = false, int logLevel = 0)
		{
			
			if (debugOnly && !debugMode)
				return;

			switch (logLevel)
			{
				case 1:
					Melon<ShiftStoneRandomizer>.Logger.Warning("Warn: " + message);
					break;
				case 2:
					Melon<ShiftStoneRandomizer>.Logger.Error("Error: " + message);
					break;
				default:
					Melon<ShiftStoneRandomizer>.Logger.Msg(message);
					break;
			}

		}
	}
}
