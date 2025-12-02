using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using Il2CppTMPro;
using RumbleModdingAPI;
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

		public static GameObject DebugUi {get; private set;}
		public static TextMeshPro DebugUiText {get; private set;}
		public static GameObject CreateDebugUi(GameObject PlayerUi)
		{
			DebugUi = Calls.Create.NewText("Placeholder text. You shouldn't be seeing this without some UE Shenanigans\n or decompiled code. Doesn't count if it's you, Ava. I (probably) told you about this.", 1f, Color.white, new Vector3(0f, 0.1f, 1f), Quaternion.Euler(0, 0, 0));
			DebugUi.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
			DebugUi.transform.localPosition = new Vector3(0f, 0.1f, 0.96f);
			DebugUi.transform.SetParent(PlayerUi.transform, false);
			DebugUiText = DebugUi.GetComponent<TextMeshPro>();
			DebugUi.SetActive(isDebugMode.Value);
			return DebugUi;
		}

		public static void PrintInGame(string message)
		{
			if(!(DebugUi is null))
			{
				DebugUiText = message;
			}
			else 
			{
				Log($"Can't print message: \"{message}\" to debug ui. Not created and assigned", false, 2);
			}
		}
	}
}
