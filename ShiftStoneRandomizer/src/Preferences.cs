
using MelonLoader;
using ShiftStoneManager.src.Helpers;
using System.IO;

namespace ShiftStoneManager
{

	internal class Preferences : MelonMod
	{
		const string USER_DATA = "UserData/ShiftStoneManager/";
		internal const string CONFIG_FILE = "Manager.cfg";

		internal static MelonPreferences_Category CatSettings;
		internal static MelonPreferences_Entry<bool> PrefDebugMode;
		internal static MelonPreferences_Entry<Hands> PrefEnabledHand;
		internal static MelonPreferences_Entry<AutomationPrefs> PrefAutomation;


		internal static MelonPreferences_Category CatEnabledStones;
		internal static MelonPreferences_Entry<bool> PrefAdamant;
		internal static MelonPreferences_Entry<bool> PrefCharge;
		internal static MelonPreferences_Entry<bool> PrefFlow;
		internal static MelonPreferences_Entry<bool> PrefGuard;
		internal static MelonPreferences_Entry<bool> PrefStubborn;
		internal static MelonPreferences_Entry<bool> PrefSurge;
		internal static MelonPreferences_Entry<bool> PrefVigor;
		internal static MelonPreferences_Entry<bool> PrefVolitile;

		internal static MelonPreferences_Category CatMap0;
		internal static MelonPreferences_Entry<ShiftStonePrefs> PrefMap0HostLeft { get; set; }
		internal static MelonPreferences_Entry<ShiftStonePrefs> PrefMap0HostRight { get; set; }
		internal static MelonPreferences_Entry<ShiftStonePrefs> PrefMap0ClientLeft { get; set; }
		internal static MelonPreferences_Entry<ShiftStonePrefs> PrefMap0ClientRight { get; set; }

		internal static MelonPreferences_Category CatMap1;
		internal static MelonPreferences_Entry<ShiftStonePrefs> PrefMap1HostLeft { get; set; }
		internal static MelonPreferences_Entry<ShiftStonePrefs> PrefMap1HostRight { get; set; }
		internal static MelonPreferences_Entry<ShiftStonePrefs> PrefMap1ClientLeft { get; set; }
		internal static MelonPreferences_Entry<ShiftStonePrefs> PrefMap1ClientRight { get; set; }


		internal static void InitPreferences()
		{
			if (!Directory.Exists(USER_DATA))
			{
				Debug.Log("Userdata folder not found. Creating...");
				Directory.CreateDirectory(USER_DATA);
			}

			CatSettings = MelonPreferences.CreateCategory("ShiftStoneManager_General", "Preferences");
			CatSettings.SetFilePath(Path.Combine(USER_DATA, CONFIG_FILE));
			PrefDebugMode = CatSettings.CreateEntry("Debug Mode", false, "Enable Debug Mode", "Enable for more verbose logging");
			PrefEnabledHand = CatSettings.CreateEntry("RandomHand", Hands.Both, "Randomized Hand", "Hand where randomization is Enabled");
			PrefAutomation = CatSettings.CreateEntry("AutoMode", AutomationPrefs.None, "Auto-Equip Mode", "Random: Randomize every match | Auto: Based on Map automation config | Mirror: Copy opponent's shift stones | None: No action");

			CatEnabledStones = MelonPreferences.CreateCategory("ShiftStoneManager_BlackList", "Stone Toggles");
			CatEnabledStones.SetFilePath(Path.Combine(USER_DATA, CONFIG_FILE));
			PrefAdamant = CatEnabledStones.CreateEntry("Adamant", true);
			PrefCharge = CatEnabledStones.CreateEntry("Charge", true);
			PrefFlow = CatEnabledStones.CreateEntry("Flow", true);
			PrefGuard = CatEnabledStones.CreateEntry("Guard", true);
			PrefStubborn = CatEnabledStones.CreateEntry("Stubborn", true);
			PrefSurge = CatEnabledStones.CreateEntry("Surge", true);
			PrefVigor = CatEnabledStones.CreateEntry("Vigor", true);
			PrefVolitile = CatEnabledStones.CreateEntry("Volatile", true);


			CatMap0 = MelonPreferences.CreateCategory("ShiftStoneManager_RingAutomation", "Ring Automation Settings");
			CatMap0.SetFilePath(Path.Combine(USER_DATA, CONFIG_FILE));
			PrefMap0HostLeft = CatMap0.CreateEntry("RingLeftHost: ", ShiftStonePrefs.Random, "Left Hand for Host: ", "[Preferred Stone] | Random | Mirror | Empty");
			PrefMap0HostRight = CatMap0.CreateEntry("RingRightHost: ", ShiftStonePrefs.Random, "Right Hand for Host: ", "[Preferred Stone] | Random | Mirror | Empty");
			PrefMap0ClientLeft = CatMap0.CreateEntry("RingLeftClient: ", ShiftStonePrefs.Random, "Left Hand for Client: ", "[Preferred Stone] | Random | Mirror | Empty");
			PrefMap0ClientRight = CatMap0.CreateEntry("RingRightClient: ", ShiftStonePrefs.Random, "Right Hand for Client: ", "[Preferred Stone] | Random | Mirror | Empty");

			CatMap1 = MelonPreferences.CreateCategory("ShiftStoneManager_PitAutomation", "Pit Automation Settings");
			CatMap1.SetFilePath(Path.Combine(USER_DATA, CONFIG_FILE));
			PrefMap1HostLeft = CatMap1.CreateEntry("PitLeftHost: ", ShiftStonePrefs.Random, "Pit Left Hand for Host: ", "[Preferred Stone] | Random | Mirror | Empty");
			PrefMap1HostRight = CatMap1.CreateEntry("PitRightHost: ", ShiftStonePrefs.Random, "Pit Right Hand for Host", "[Preferred Stone] | Random | Mirror| Empty");
			PrefMap1ClientLeft = CatMap1.CreateEntry("PitLeftClient: ", ShiftStonePrefs.Random, "Pit Left Hand for Client: ", "[Preferred Stone] | Random | Mirror | Empty");
			PrefMap1ClientRight = CatMap1.CreateEntry("PitRightClient: ", ShiftStonePrefs.Random, "Pit Right Hand for Client: ", "[Preferred Stone] | Random | Mirror | Empty");

		}
	}
}