
using MelonLoader;

using System.IO;

namespace ShiftStoneRandomizer
{

	public partial class ShiftStoneRandomizer : MelonMod
	{
		private const string USER_DATA = "UserData/ShiftStoneRandomizer/";
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

		private static MelonPreferences_Category CatLoadOutButton;
		public static MelonPreferences_Entry<string> PrefLobLeft;
		public static MelonPreferences_Entry<string> PrefLobRight;

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
			PrefMap0HostLeft = CatMap0.CreateEntry("Ring Left Hand for Host: ", "Random", null, "[Preferred Stone] | Random | Empty");
			PrefMap0HostRight = CatMap0.CreateEntry("Ring Right Hand for Host: ", "Random", null, "[Preferred Stone] | Random | Empty");
			PrefMap0ClientLeft = CatMap0.CreateEntry("Ring Left Hand for Client: ", "Random", null, "[Preferred Stone] | Random | Empty");
			PrefMap0ClientRight = CatMap0.CreateEntry("Ring Right Hand for Client: ", "Random", null, "[Preferred Stone] | Random | Empty");

			CatMap1 = MelonPreferences.CreateCategory("Pit Automation Settings");
			CatMap1.SetFilePath(Path.Combine(USER_DATA, CONFIG_FILE));
			PrefMap1HostLeft = CatMap1.CreateEntry("Pit Left Hand for Host: ", "Random", null, "[Preferred Stone] | Random | Empty");
			PrefMap1HostRight = CatMap1.CreateEntry("Pit Right Hand for Host: ", "Random", null, "[Preferred Stone] | Random | Empty");
			PrefMap1ClientLeft = CatMap1.CreateEntry("Pit Left Hand for Client: ", "Random", null, "[Preferred Stone] | Random | Empty");
			PrefMap1ClientRight = CatMap1.CreateEntry("Pit Right Hand for Client: ", "Random", null, "[Preferred Stone] | Random | Empty");

			CatLoadOutButton = MelonPreferences.CreateCategory("Load Out Button Selection");
			CatLoadOutButton.SetFilePath(Path.Combine(USER_DATA, CONFIG_FILE));
			PrefLobLeft = CatLoadOutButton.CreateEntry("Left Hand: ", "Empty", null, "[Preferred Stone] | Empty");
			PrefLobRight = CatLoadOutButton.CreateEntry("Right Hand: ", "Empty", null, "[Preferred Stone] | Empty");

			Debug.debugMode = CatDebugMode.Value;
		}

		private void ReadPrefs()
		{
			CatSettings.LoadFromFile();
			CatEnabledStones.LoadFromFile();
			CatMap0.LoadFromFile();
			CatMap1.LoadFromFile();


		}

		public void SavePrefs()
		{
			CatSettings.SaveToFile();
			CatEnabledStones.SaveToFile();
			CatMap0.SaveToFile();
			CatMap1.SaveToFile();
		}

		private void ApplyPrefsToState()
		{
			StoneItem.AllStones[0].IsEnabled = PrefAdamant.Value;
			StoneItem.AllStones[1].IsEnabled = PrefCharge.Value;
			StoneItem.AllStones[2].IsEnabled = PrefFlow.Value;
			StoneItem.AllStones[3].IsEnabled = PrefGuard.Value;
			StoneItem.AllStones[4].IsEnabled = PrefStubborn.Value;
			StoneItem.AllStones[5].IsEnabled = PrefSurge.Value;
			StoneItem.AllStones[6].IsEnabled = PrefVigor.Value;
			StoneItem.AllStones[7].IsEnabled = PrefVolitile.Value;

			if (!System.Enum.TryParse<Hands>(PrefEnabledHand.Value, out EnabledHand))
			{
				Debug.Log($"Failed to parse enabled hand preference: {PrefEnabledHand.Value}");
			}
			if (!System.Enum.TryParse<AutomationPrefs>(PrefAutomation.Value, out AutomationMode))
			{
				Debug.Log($"Failed to parse automation mode preference: {PrefAutomation.Value}");
			}
			if (!System.Enum.TryParse<Hands>(PrefEnabledHand.Value, out EnabledHand))
			{
				Debug.Log($"Failed to parse enabled hand preference: {PrefEnabledHand.Value}");
				EnabledHand = Hands.Both;
			}
			if (CurrentScene == "Gym")
			{
				ShowRandomedHand();
			}
		}

		private static void UpdatePrefsFromState()
		{
			PrefAdamant.Value = StoneItem.AllStones[0].IsEnabled;
			PrefCharge.Value = StoneItem.AllStones[1].IsEnabled;
			PrefFlow.Value = StoneItem.AllStones[2].IsEnabled;
			PrefGuard.Value = StoneItem.AllStones[3].IsEnabled;
			PrefStubborn.Value = StoneItem.AllStones[4].IsEnabled;
			PrefSurge.Value = StoneItem.AllStones[5].IsEnabled;
			PrefVigor.Value = StoneItem.AllStones[6].IsEnabled;
			PrefVolitile.Value = StoneItem.AllStones[7].IsEnabled;

			PrefEnabledHand.Value = EnabledHand.ToString();

		}


		private void ApplyLoadOut()
		{
			ShiftStonePrefs left;
			ShiftStonePrefs right;

			if (!System.Enum.TryParse<ShiftStonePrefs>(PrefLobLeft.Value, out left))
			{
				Debug.Log("Failed to parse Left hand loadout from config file");
			}
			if (!System.Enum.TryParse<ShiftStonePrefs>(PrefLobRight.Value, out right))
			{
				Debug.Log("Failed to parse Right hand loadout from config file");
			}
			EquipStones(left, right);
		}

		public static void ToggleStones(int[] equipped, Hands hand = Hands.Both)
		{
			StoneItem left;
			StoneItem right;
			StoneItem single;

			if (equipped[0] == -1)
				left = new StoneItem();
			else
				left = StoneItem.AllStones[equipped[0]];

			if (equipped[1] == -1)
				right = new StoneItem();
			else
				right = StoneItem.AllStones[equipped[1]];

			if (hand == Hands.Both)
			{
				if (left.IsEnabled || right.IsEnabled)
				{
					left.IsEnabled = false;
					right.IsEnabled = false;
					EquipStones(new StoneItem(), new StoneItem());
				}
				else
				{
					left.IsEnabled = true;
					right.IsEnabled = true;
				}
			}

			else
			{
				if (hand == Hands.Left)
				{
					single = left;
				}
				else if (hand == Hands.Right)
				{
					single = right;
				}
				else return;

				single.IsEnabled = !single.IsEnabled;

				if (single.IsEnabled)
					EquipStones(single.GetEnum(), hand);
				else
					EquipStones(ShiftStonePrefs.Empty, hand);
			}

			UpdatePrefsFromState();
			CatEnabledStones.SaveToFile();

		}

		private void SaveLoadOut(int[] Equipped)
		{
			StoneItem left;
			StoneItem right;
			if (Equipped[0] == -1)
				left = new StoneItem();
			else
				left = StoneItem.AllStones[Equipped[0]];

			if (Equipped[1] == -1)
				right = new StoneItem();
			else
				right = StoneItem.AllStones[Equipped[1]];


			PrefLobLeft.Value = left.Name;
			PrefLobRight.Value = right.Name;

			CatLoadOutButton.SaveToFile();

			ActivateEffect(true, true);


			SignFall();
		}
	}
}