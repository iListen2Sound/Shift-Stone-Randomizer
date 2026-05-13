
using MelonLoader;
using ShiftStoneManager.src.Helpers;
using System.IO;

namespace ShiftStoneManager
{

	public class Preferences : MelonMod
	{
		private const string USER_DATA = "UserData/Core/";
		private const string CONFIG_FILE = "config.cfg";

		public static MelonPreferences_Category CatSettings;
		public static MelonPreferences_Entry<bool> CatDebugMode;
		public static MelonPreferences_Entry<string> PrefEnabledHand;
		public static MelonPreferences_Entry<string> PrefAutomation;


		private static MelonPreferences_Category CatEnabledStones;
		public static MelonPreferences_Entry<bool> PrefAdamant;
		public static MelonPreferences_Entry<bool> PrefCharge;
		public static MelonPreferences_Entry<bool> PrefFlow;
		public static MelonPreferences_Entry<bool> PrefGuard;
		public static MelonPreferences_Entry<bool> PrefStubborn;
		public static MelonPreferences_Entry<bool> PrefSurge;
		public static MelonPreferences_Entry<bool> PrefVigor;
		public static MelonPreferences_Entry<bool> PrefVolitile;

		private static MelonPreferences_Category CatMap0;
		public static MelonPreferences_Entry<string> PrefMap0HostLeft { get; private set; }
		public static MelonPreferences_Entry<string> PrefMap0HostRight { get; private set; }
		public static MelonPreferences_Entry<string> PrefMap0ClientLeft { get; private set; }
		public static MelonPreferences_Entry<string> PrefMap0ClientRight { get; private set; }

		private static MelonPreferences_Category CatMap1;
		public static MelonPreferences_Entry<string> PrefMap1HostLeft { get; private set; }
		public static MelonPreferences_Entry<string> PrefMap1HostRight { get; private set; }
		public static MelonPreferences_Entry<string> PrefMap1ClientLeft { get; private set; }
		public static MelonPreferences_Entry<string> PrefMap1ClientRight { get; private set; }

		public static AutomationPrefs AutomationMode;


		private static void InitPreferences()
		{
			if (!Directory.Exists(USER_DATA))
			{
				Debug.Log("Userdata folder not found. Creating...");
				Directory.CreateDirectory(USER_DATA);
			}

			CatSettings = MelonPreferences.CreateCategory("Preferences");
			CatSettings.SetFilePath(Path.Combine(USER_DATA, CONFIG_FILE));
			CatDebugMode = CatSettings.CreateEntry("Enable Debug Mode", false, null, "Enable for more verbose logging");
			PrefEnabledHand = CatSettings.CreateEntry("Randomized Hand", "Both", null, "Hand where randomization is Enabled");
			PrefAutomation = CatSettings.CreateEntry("Auto-Equip Mode", "Random", null, "Random: Randomize every match | Auto: Based on Map automation config | Mirror: Copy opponent's shift stones | None: No action");

			CatEnabledStones = MelonPreferences.CreateCategory("Enabled Stones", "Black List");
			CatEnabledStones.SetFilePath(Path.Combine(USER_DATA, CONFIG_FILE));
			PrefAdamant = CatEnabledStones.CreateEntry("Adamant Stone", true);
			PrefCharge = CatEnabledStones.CreateEntry("Charge Stone", true);
			PrefFlow = CatEnabledStones.CreateEntry("Flow Stone", true);
			PrefGuard = CatEnabledStones.CreateEntry("Guard Stone", true);
			PrefStubborn = CatEnabledStones.CreateEntry("Stubborn Stone", true);
			PrefSurge = CatEnabledStones.CreateEntry("Surge Stone", true);
			PrefVigor = CatEnabledStones.CreateEntry("Vigor Stone", true);
			PrefVolitile = CatEnabledStones.CreateEntry("Volatile Stone", true);


			CatMap0 = MelonPreferences.CreateCategory("Ring Automation Settings");
			CatMap0.SetFilePath(Path.Combine(USER_DATA, CONFIG_FILE));
			PrefMap0HostLeft = CatMap0.CreateEntry("Ring Left Hand for Host: ", "Random", null, "[Preferred Stone] | Random | Mirror | Empty");
			PrefMap0HostRight = CatMap0.CreateEntry("Ring Right Hand for Host: ", "Random", null, "[Preferred Stone] | Random | Mirror | Empty");
			PrefMap0ClientLeft = CatMap0.CreateEntry("Ring Left Hand for Client: ", "Random", null, "[Preferred Stone] | Random | Mirror | Empty");
			PrefMap0ClientRight = CatMap0.CreateEntry("Ring Right Hand for Client: ", "Random", null, "[Preferred Stone] | Random | Mirror | Empty");

			CatMap1 = MelonPreferences.CreateCategory("Pit Automation Settings");
			CatMap1.SetFilePath(Path.Combine(USER_DATA, CONFIG_FILE));
			PrefMap1HostLeft = CatMap1.CreateEntry("Pit Left Hand for Host: ", "Random", null, "[Preferred Stone] | Random | Mirror | Empty");
			PrefMap1HostRight = CatMap1.CreateEntry("Pit Right Hand for Host: ", "Random", null, "[Preferred Stone] | Random | Mirror| Empty");
			PrefMap1ClientLeft = CatMap1.CreateEntry("Pit Left Hand for Client: ", "Random", null, "[Preferred Stone] | Random | Mirror | Empty");
			PrefMap1ClientRight = CatMap1.CreateEntry("Pit Right Hand for Client: ", "Random", null, "[Preferred Stone] | Random | Mirror | Empty");

			Debug.debugMode = CatDebugMode.Value;
		}
	}
}