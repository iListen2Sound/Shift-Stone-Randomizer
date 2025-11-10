using AsmResolver.PE.DotNet.Cil;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppInterop.Runtime.Runtime.VersionSpecific.Class;
using Il2CppPhoton.Realtime;
using Il2CppRootMotion;
using Il2CppRUMBLE.CharacterCreation.Interactable;
using Il2CppRUMBLE.Combat.ShiftStones;
using Il2CppRUMBLE.Interactions.InteractionBase;
using Il2CppRUMBLE.Managers;
using Il2CppRUMBLE.Players.Subsystems;
using Il2CppSystem;
using Il2CppSystem.Data;
using Il2CppTMPro;
using MelonLoader;
using MelonLoader.TinyJSON;
using MelonLoader.Utils;
using RumbleModdingAPI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Action = System.Action;
using Type = Il2CppSystem.Type;

namespace ShiftStoneRandomizer
{

	public partial class Class1 : MelonMod
    {
        private const string USER_DATA = $"UserData/ShiftStoneRandomizer/";
		private const string CONFIG_FILE = "config.cfg";

        private MelonPreferences_Category CatSettings;
        private MelonPreferences_Entry<string> PrefEnabledHand; 
        private MelonPreferences_Entry<string> PrefAutoMation;

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

        private MelonPreferences_Category CatMap1
        private MelonPreferences_Entry<string> PrefMap1HostLeft;
        private MelonPreferences_Entry<string> PrefMap1HostRight;
        private MelonPreferences_Entry<string> PrefMap1ClientLeft;
        private MelonPreferences_Entry<string> PrefMap1ClientRight;


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
            PrefAutoMation = CatSettings.CreateEntry("Auto-Equip Mode", "Random", null, "Random: Randomize every match | Auto: Based on Map automation config | None: No action");

            CatEnabledStones = MelonPreferences.CreateCategory(BuildInfo.Name, BuildInfo.Name);	
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
            PrefMap0HostLeft = CatMap0.CreateEntry("Ring Left Hand for Host: ", "Random", null, "Preferred Stone, Random, Empty");
            PrefMap0HostRight = CatMap0.CreateEntry("Ring Right Hand for Host: ", "Random", null, "Preferred Stone, Random, Empty");
            PrefMap0ClientLeft = CatMap0.CreateEntry("Ring Left Hand for Client: ", "Random", null, "Preferred Stone, Random, Empty");
            PrefMap0ClientRight = CatMap0.CreateEntry("Ring Right Hand for Client: ", "Random", null, "Preferred Stone, Random, Empty");

            CatMap1 = MelonPreferences.CreateCategory("Pit Automation Settings");
            CatMap1.SetFilePath(Path.Combine(USER_DATA, CONFIG_FILE));
            PrefMap1HostLeft = CatMap1.CreateEntry("Pit Left Hand for Host: ", "Random", null, "Preferred Stone, Random, Empty");
            PrefMap1HostRight = CatMap1.CreateEntry("Pit Right Hand for Host: ", "Random", null, "Preferred Stone, Random, Empty");
            PrefMap1ClientLeft = CatMap1.CreateEntry("Pit Left Hand for Client: ", "Random", null, "Preferred Stone, Random, Empty");
            PrefMap1ClientRight = CatMap1.CreateEntry("Pit Right Hand for Client: ", "Random", null, "Preferred Stone, Random, Empty");
            

        }
    }
}