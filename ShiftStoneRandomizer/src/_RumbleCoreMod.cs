
using MelonLoader;


using UIFramework;
using RumbleModdingAPI;
using RumbleModdingAPI.RMAPI;
using System.Diagnostics;
using UnityEngine.Rendering;
[assembly: MelonInfo(typeof(ShiftStoneManager.Core), "ShiftStoneManager", "0.5.0", "iListen2Sound, Darkener")]
[assembly: MelonGame("Buckethead Entertainment", "RUMBLE")]
[assembly: MelonAuthorColor(255, 87, 166, 80)]
[assembly: MelonColor(255, 87, 166, 80)]
namespace ShiftStoneManager
{
	
	public class Core : MelonMod
	{
		public static Core Instance;

		internal static Scenes Scene;
		internal static bool IsFirstLoad = true;
		public override void OnInitializeMelon()
		{
			Instance = this;
			Preferences.InitPreferences();

			UI.RegisterMelon(this, Preferences.CatEnabledStones, Preferences.CatSettings, Preferences.CatMap0, Preferences.CatMap1);

			Actions.onMapInitialized += OnMapInitialized;
		}

		public override void OnSceneWasLoaded(int buildIndex, string sceneName)
		{
			//use enum for cleaner scene resolution
			Scene = (Scenes)buildIndex;



		}
		public void OnMapInitialized(string scene)
		{
			if (IsFirstLoad)
				FirstLoad();
		}

		public void FirstLoad()
		{

			if(Scene == Scenes.Gym)
			{
				Prefabs.InitializePrefabs();
			}

			IsFirstLoad = false;
		}
	}
}