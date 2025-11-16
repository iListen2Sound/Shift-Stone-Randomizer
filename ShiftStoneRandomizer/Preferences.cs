
using MelonLoader;

using System.IO;
using Action = System.Action;
using Type = Il2CppSystem.Type;

namespace ShiftStoneRandomizer
{

	public partial class Class1 : MelonMod
	{
		private const string USER_DATA = "UserData/ShiftStoneRandomizer/";
		private const string CONFIG_FILE = "config.cfg";

		private MelonPreferences_Category CatSettings;
		private MelonPreferences_Entry<string> PrefEnabledHand;
		private MelonPreferences_Entry<string> PrefAutomation;

		private MelonPreferences_Category CatEnabledStones;
		private MelonPreferences_Entry<bool> PrefAdamant;
		private MelonPreferences_Entry<bool> PrefCharge;
		private MelonPreferences_Entry<bool> PrefFlow;
		private MelonPreferences_Entry<bool> PrefGuard;
		private MelonPreferences_Entry<bool> PrefStubborn;
		private MelonPreferences_Entry<bool> PrefSurge;
		private MelonPreferences_Entry<bool> PrefVigor;
		private MelonPreferences_Entry<bool> PrefVolitile;

		private MelonPreferences_Category CatMap0;
		private MelonPreferences_Entry<string> PrefMap0HostLeft;
		private MelonPreferences_Entry<string> PrefMap0HostRight;
		private MelonPreferences_Entry<string> PrefMap0ClientLeft;
		private MelonPreferences_Entry<string> PrefMap0ClientRight;

		private MelonPreferences_Category CatMap1;
		private MelonPreferences_Entry<string> PrefMap1HostLeft;
		private MelonPreferences_Entry<string> PrefMap1HostRight;
		private MelonPreferences_Entry<string> PrefMap1ClientLeft;
		private MelonPreferences_Entry<string> PrefMap1ClientRight;

		private MelonPreferences_Category CatLoadOutButton;
		private MelonPreferences_Entry<string> PrefLobLeft;
		private MelonPreferences_Entry<string> PrefLobRight;

		AutomationPrefs AutomationMode;


		private void InitPreferences()
		{
			if (!Directory.Exists(USER_DATA))
			{
				Log("Userdata folder not found. Creating...");
				Directory.CreateDirectory(USER_DATA);
			}

			CatSettings = MelonPreferences.CreateCategory("Preferences");
			CatSettings.SetFilePath(Path.Combine(USER_DATA, CONFIG_FILE));
			PrefEnabledHand = CatSettings.CreateEntry("Enabled Hand", "Both", null, "Hand where randomization is Enabled");
			PrefAutomation = CatSettings.CreateEntry("Auto-Equip Mode", "Random", null, "Random: Randomize every match | Auto: Based on Map automation config | None: No action");

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
			PrefLobLeft = CatLoadOutButton.CreateEntry("Left Hand: ", "Empty", null, "[Preferred Stone] | Empty");
			PrefLobRight = CatLoadOutButton.CreateEntry("Right Hand: ", "Empty", null, "[Preferred Stone] | Empty");
		}

		private void ReadPrefs()
		{
			CatSettings.LoadFromFile();
			CatEnabledStones.LoadFromFile();
			CatMap0.LoadFromFile();
			CatMap1.LoadFromFile();

			if(!System.Enum.TryParse<AutomationPrefs>(PrefAutomation.Value, out AutomationMode)) 
			{
				Log($"Failed to parse automation mode preference: {PrefAutomation.Value}");
				AutomationMode = AutomationPrefs.Random;
			}
			if(!System.Enum.TryParse<RandomedHand>(PrefEnabledHand.Value, out EnabledHand)) 
			{
				Log($"Failed to parse enabled hand preference: {PrefEnabledHand.Value}");
				EnabledHand = RandomedHand.Both;
			}
		}

		private void SavePrefs()
		{
			CatSettings.SaveToFile();
			CatEnabledStones.SaveToFile();
			CatMap0.SaveToFile();
			CatMap1.SaveToFile();
		}

		private void ApplyPrefsToState()
		{
			stones[0].IsEnabled = PrefAdamant.Value;
			stones[1].IsEnabled = PrefCharge.Value;
			stones[2].IsEnabled = PrefFlow.Value;
			stones[3].IsEnabled = PrefGuard.Value;
			stones[4].IsEnabled = PrefStubborn.Value;
			stones[5].IsEnabled = PrefSurge.Value;
			stones[6].IsEnabled = PrefVigor.Value;
			stones[7].IsEnabled = PrefVolitile.Value;

			if (!System.Enum.TryParse<RandomedHand>(PrefEnabledHand.Value, out EnabledHand)) ;
			{
				Log($"Failed to parse enabled hand preference: {PrefEnabledHand.Value}");
			}
		}

		private void UpdatePrefsFromState()
		{
			PrefAdamant.Value = stones[0].IsEnabled;
			PrefCharge.Value = stones[1].IsEnabled;
			PrefFlow.Value = stones[2].IsEnabled;
			PrefGuard.Value = stones[3].IsEnabled;
			PrefStubborn.Value = stones[4].IsEnabled;
			PrefSurge.Value = stones[5].IsEnabled;
			PrefVigor.Value = stones[6].IsEnabled;
			PrefVolitile.Value = stones[7].IsEnabled;

			PrefEnabledHand.Value = EnabledHand.ToString();

		}


		private void ApplyLoadOut()
		{
			ShiftStonePrefs left;
			ShiftStonePrefs right;

			if(!System.Enum.TryParse<ShiftStonePrefs>(PrefLobLeft.Value, out left))
			{
				Log("Failed to parse Left hand loadout from config file");
			}
			if(!System.Enum.TryParse<ShiftStonePrefs>(PrefLobRight.Value, out right))
			{
				Log("Failed to parse Right hand loadout from config file");
			}
			EquipStones(left, right);
		}

		private void ToggleStones(int[] equipped RandomedHand hand = RandomedHand.Both)
		{
			StoneItem left; 
			StoneItem right;
			stoneItem single; 
			if (Equipped[0] == -1)
				left = new StoneItem();
			else
				left = stones[Equipped[0]];

			if (Equipped[1] == -1)
				right = new StoneItem();
			else
				right = stones[Equipped[1]];

			if(hand = RandomedHand.Both)
			{
				if(left.IsEnabled || right.IsEnabled)
				{
					left.IsEnabled = false;
					right.IsEnabled = false;
					EquipStones(new StoneItem(), new StoneItem());
				}else
				{
					left.IsEnabled = true;
					right.IsEnabled = true;
				}
			}
			else if(hand = RandomedHand.Left)
			{
				single = left;
			}
			else if(hand = RandomedHand.Right)
			{
				single = right;
			}

			single.IsEnabled = !Single.IsEnabled;

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
				left = stones[Equipped[0]];

			if (Equipped[1] == -1)
				right = new StoneItem();
			else
				right = stones[Equipped[1]];


			PrefLobLeft.Value = left.Name;
			PrefLobRight.Value = right.Name;

			CatLoadOutButton.SaveToFile();
			
			ActivateEffect(true, true);


			SignFall();
		}
	}
}