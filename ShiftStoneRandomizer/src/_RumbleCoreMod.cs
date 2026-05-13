
using MelonLoader;


using UIFramework;
using RumbleModdingAPI;
using RumbleModdingAPI.RMAPI;
[assembly: MelonInfo(typeof(ShiftStoneManager.Core), "ShiftStoneManager", "0.5.0", "iListen2Sound, Darkener")]
[assembly: MelonGame("Buckethead Entertainment", "RUMBLE")]
[assembly: MelonAuthorColor(255, 87, 166, 80)]
[assembly: MelonColor(255, 87, 166, 80)]
namespace ShiftStoneManager
{
	
	public class Core : MelonMod
	{
		public override void OnInitializeMelon()
		{
			Preferences.InitPreferences();

			UI.RegisterMelon(this, Preferences.CatEnabledStones, Preferences.CatSettings, Preferences.CatMap0, Preferences.CatMap1);
		}

	}
}