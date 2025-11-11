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
		public static void Log(string message, bool debugOnly = false)
		{
			if (!debugOnly)
			{
				Melon<ShiftStoneRandomizer>.Logger.Msg(message);
				return;
			}
			if (debugMode)
				Melon<ShiftStoneRandomizer>.Logger.Msg(message);


		}
	}
}
