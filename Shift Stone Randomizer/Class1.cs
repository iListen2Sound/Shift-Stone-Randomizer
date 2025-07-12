using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppPhoton.Realtime;
using Il2CppRUMBLE.Combat.ShiftStones;
using Il2CppRUMBLE.Interactions.InteractionBase;
using Il2CppRUMBLE.Players;
using Il2CppRUMBLE.Players.Subsystems;
using Il2CppSystem;
using MelonLoader;
using RumbleModdingAPI;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using HarmonyLib;
using Type = Il2CppSystem.Type;
using Action = System.Action;
using Il2CppRUMBLE.Managers;
using MelonLoader.Utils;
using System.Collections.Generic;
using System.Collections;

class StoneItem
{
	private static int _blacklistCount = 0;
	public static int BlackListCount
	{
		get { return _blacklistCount; }
	}
	public ShiftStone ShiftStone { get; set; }
	public string Name
	{
		get
		{
			return ShiftStone != null ? ShiftStone.name.Replace("Stone", "") : "None";
		}
	}
	private bool _isEnabled;
	public bool IsEnabled
	{
		get { return _isEnabled; }
		set
		{
			_isEnabled = value;

			if (value)
				_blacklistCount++;

			else
				_blacklistCount--;
		}
	}

	public StoneItem(ShiftStone shiftStone)
	{

		ShiftStone = shiftStone;
		shiftStone.gameObject.SetActive(false);// Disable the stone so it doesn't show up in the game
		IsEnabled = true;
	}
	/// <summary>
	/// No arguments to indicate empty stone slot
	/// </summary>
	public StoneItem()
	{
		ShiftStone = null;
	}
}

namespace Shift_Stone_Randomizer
{
	public class Class1 : MelonMod
	{
		private string userData = "Userdata/Shift_Stone_Randomizer/";
		private string blackListFile = "blacklist.txt";
		private System.Random random = new System.Random();
		//private ShiftStone[] shiftStones;
		private StoneItem[] stones;
		//private int[] blackList = new int[0];



		private bool equipEffect = true;
		private bool physicalGUI = true;
		private int lockedHand = -1; // -1 no lock, 0 left hand, 1 right hand
		private enum HandLock
		{
			None = -1,
			Left = 0,
			Right = 1
		}
		private PlayerHaptics haptics;

		private GameObject glasses;

		public override void OnLateInitializeMelon()
		{
			CreateCosmetics();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="buildIndex"></param>
		/// <param name="sceneName"></param>
		/// TODO: Create default loadout system that loads only once.
		public override void OnSceneWasLoaded(int buildIndex, string sceneName)
		{
			InitializeShiftStones();
			CreateButtonsForAll();
			if (sceneName == "Gym" && physicalGUI)
			{
				CreatePhysicalGUI();
			}
			AddCosmetics();
			haptics = PlayerManager.instance.playerControllerPrefab.gameObject.GetComponent<PlayerHaptics>();

			if (!Directory.Exists(userData))
			{
				Directory.CreateDirectory(userData);
			}
			if (File.Exists(Path.Combine(userData, blackListFile)))
			{
				string[] lines = File.ReadAllLines(Path.Combine(userData, blackListFile));
				//blackList = new int[lines.Length];
				//Broken: no longer reading from file
				Log("BlackListing stones from file");
				for (int i = 0; i < lines.Length; i++)
				{
					foreach (StoneItem stone in stones)
					{
						if (stone.Name == lines[i])
						{
							//blackList[i] = System.Array.IndexOf(stones, stone);
							stone.IsEnabled = false;
							Log($"\t{stone.Name}");

						}
					}
				}
			}
			else
			{

				Log("No blacklist file found");
			}
			if (File.Exists(Path.Combine(userData, "config.txt")))
			{
				string[] config = File.ReadAllLines(Path.Combine(userData, "config.txt"));
				foreach (string line in config)
				{

				}
			}
			else
			{

			}
		}





		private void InitializeShiftStones()
		{
			ShiftStone StubbornStone = Calls.Managers.GetPoolManager().GetPooledObject("StubbornStone").gameObject.GetComponent<StubbornStone>();
			ShiftStone VolatileStone = Calls.Managers.GetPoolManager().GetPooledObject("VolatileStone").gameObject.GetComponent<VolatileStone>();
			ShiftStone AdamantStone = Calls.Managers.GetPoolManager().GetPooledObject("AdamantStone").gameObject.GetComponent<UnyieldingStone>();
			ShiftStone ChargeStone = Calls.Managers.GetPoolManager().GetPooledObject("ChargeStone").gameObject.GetComponent<ChargeStone>();
			ShiftStone GuardStone = Calls.Managers.GetPoolManager().GetPooledObject("GuardStone").gameObject.GetComponent<GuardStone>();
			ShiftStone SurgeStone = Calls.Managers.GetPoolManager().GetPooledObject("SurgeStone").gameObject.GetComponent<CounterStone>();
			ShiftStone VigorStone = Calls.Managers.GetPoolManager().GetPooledObject("VigorStone").gameObject.GetComponent<VigorStone>();
			ShiftStone FlowStone = Calls.Managers.GetPoolManager().GetPooledObject("FlowStone").gameObject.GetComponent<FlowStone>();

			/*shiftStones = new ShiftStone[] {
				AdamantStone, ChargeStone, FlowStone, GuardStone, StubbornStone, SurgeStone, VigorStone, VolatileStone,
			};*/
			stones = new StoneItem[] {
				new StoneItem(AdamantStone),
				new StoneItem(ChargeStone),
				new StoneItem(FlowStone),
				new StoneItem(GuardStone),
				new StoneItem(StubbornStone),
				new StoneItem(SurgeStone),
				new StoneItem(VigorStone),
				new StoneItem(VolatileStone)
			};
		}





		/// <summary>
		/// Blacklists stones and writes to file.
		/// </summary>
		/// <param name="Stones">Equipped stones</param>
		/// <remarks>Breaks when one hand is empty out of bounds exception</remarks>
		private void BlackListStones(int[] EquippedStones)
		{
			bool hasEnabledEquipedStones = false;
			string blackListOut = "";
			foreach (int i in EquippedStones)
			{
				if (stones[i].IsEnabled)
				{
					hasEnabledEquipedStones = true;
					stones[i].IsEnabled = false;
				}
			}

			if (hasEnabledEquipedStones)
				EquipStones(new StoneItem(), new StoneItem());
			
			else
			{
				foreach (int i in EquippedStones)
				{
					if (i > -1)
						stones[i].IsEnabled = true;
				}
			}
			Log("Blacklisted stones: ");
			foreach(StoneItem stone in stones)
			{
				if (!stone.IsEnabled)
				{
					Log($"\t{stone.Name}");
					blackListOut += stone.Name + "\n";
				}	
			}

			File.WriteAllText(Path.Combine(userData, blackListFile), blackListOut);
/*
			for (int i = 0; i < Stones.Length; i++)
			{
				if (blackList.Contains(Stones[i]))
				{
					int stoneIndex = System.Array.IndexOf(blackList, Stones[i]);
					// remove equiped stones from the blacklist 
					blackList = blackList.Where((val, idx) => idx != stoneIndex).ToArray();
				}
				else
				{
					if (blackList.Length + Stones.Length > 7)
					{
						Log("Reached stone blacklist count");
						return;
					}
					// add equiped stones to the blacklist
					blackList = blackList.Append(Stones[i]).ToArray();
					EquipStones(new StoneItem(), new StoneItem());
				}
			}

			Log("Blacklisted stones: ");
			foreach (int i in blackList)
			{

				blackListOut += shiftStones[i].name + "\n";
				Log($"\t\t {shiftStones[i].name.Replace("Stone", "")}");
			}
			File.WriteAllText(Path.Combine(userData, blackListFile), blackListOut);*/
		}




		private void CycleHandLock()
		{
			lockedHand++;
			if (lockedHand > 1)
			{
				lockedHand = -1;
			}

			HandLock hand = (HandLock)lockedHand;
			Log($"Hand lock set to: {hand}");


			switch (hand)
			{
				case HandLock.None:

					MelonCoroutines.Start(HapticImpulse(true, true));
					break;
				case HandLock.Left:
					MelonCoroutines.Start(HapticImpulse(true, false));
					break;
				case HandLock.Right:
					MelonCoroutines.Start(HapticImpulse(false, true));
					break;
			}
		}

		private void LoadOutButton_Pressed(int[] Equipped)
		{

		}


		/// <summary>
		/// Randomizes shift stones
		/// Avoids currently equipped stones
		/// Should enable equipping currently equipped stones if available stones are less than 4
		/// </summary>
		/// <remarks>Equips empty slot sometimes </remarks>>
		private void RandomizeStones(int[] Equipped)
		{
			List<StoneItem> randomStones = new List<StoneItem>();
			foreach (StoneItem stone in stones)
			{
				if (stone.IsEnabled && System.Array.IndexOf(Equipped, stone) == -1)
				{
					randomStones.Add(stone);
				}
			}

			if (randomStones.Count <= 4 && lockedHand == -1)
			{
				Log("Not enough stones to disallow repeats");
				// If there are less than 4 stones available, add the equipped stones to the list
				foreach (int stoneIndex in Equipped)
				{
					randomStones.Add(stones[stoneIndex]);
				}
			}

			// shuffle stones list
			randomStones = randomStones.OrderBy(x => random.Next()).ToList(); 
			if (randomStones.Count < 2)
			{
				Log("Not enough stones to randomize, using equipped stones instead.");
				return;
			}

			switch (lockedHand)
			{
				case -1:
					EquipStones(randomStones[0], randomStones[1]);
					break;
				case 0:
					EquipStones(null, randomStones[0]);
					break;
				case 1:

					EquipStones(randomStones[0], null);
					break;
			}
		}
		/// <summary>
		/// Pass null to ignore hand. Pass new StoneItem() to equip empty stone slot.
		/// </summary>
		/// <param name="leftStone"></param>
		/// <param name="rightStone"></param>
		private void EquipStones(StoneItem leftStone, StoneItem rightStone)
		{
			if (leftStone != null)
			{

				if (leftStone.ShiftStone == null)
					Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().RemoveShiftStone(0, true, true);
				else
				{

					Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().AttachShiftStone(leftStone.ShiftStone, 0, true, true);
					// This ensures that the stones exist (According to Darkener. Don't know why this is -iListen2Sound)
					Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().RemoveAndReattachShiftstones(true, true);
					Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().ActivateUseShiftstoneEffects(Il2CppRUMBLE.Input.InputManager.Hand.Left);
				}
			}
			if (rightStone != null)
			{
				if (rightStone.ShiftStone == null)
					Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().RemoveShiftStone(1, true, true);
				else
				{
					Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().AttachShiftStone(rightStone.ShiftStone, 1, true, true);
					Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().RemoveAndReattachShiftstones(true, true);
					Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().ActivateUseShiftstoneEffects(Il2CppRUMBLE.Input.InputManager.Hand.Right);
				}
			}
		}

		/*/// <summary>
			/// Equips the stones to the player.
			/// </summary>
			/// <param name="leftstone">-1 to not equip shiftstone</param>
			/// <param name="rightstone">-1 to not equip shiftstone</param>
			/// <param name="isempty"></param>
			/// TODO: Add overload for equipping stones using shiftstone objects directly. 
		private void EquipStones(int leftstone, int rightstone, bool effect, bool isempty)
		{

			if (isempty)// If is empty is true then i want -1 to be an epty spot so remove all equiped stones
			{

				Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().RemoveShiftStone(0, true, true);
				Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().RemoveShiftStone(1, true, true);
			}
			else
			{// If is empty is false then i want -1 to leave the stone unchanged so only remove stones if there is actualy a stone to replace it
				if (leftstone > -1)
				{
					Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().RemoveShiftStone(0, true, true);
				}
				if (rightstone > -1)
				{
					Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().RemoveShiftStone(1, true, true);
				}
			}

			// Dont equip stones if the index is -1
			if (leftstone > -1)
			{
				Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().AttachShiftStone(shiftStones[leftstone], 0, true, true);
			}
			if (rightstone > -1)
			{
				Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().AttachShiftStone(shiftStones[rightstone], 1, true, true);
			}

			// This ensures that the stones exist
			Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().RemoveAndReattachShiftstones(true, true);

			if (effect)
			{// Only do the effect if the stone got changed or exists at all
				if (leftstone > -1)
				{
					Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().ActivateUseShiftstoneEffects(Il2CppRUMBLE.Input.InputManager.Hand.Left);
				}
				if (rightstone > -1)
				{
					Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().ActivateUseShiftstoneEffects(Il2CppRUMBLE.Input.InputManager.Hand.Right);
				}
			}
		}*/

		IEnumerator HapticImpulse(bool left, bool right)
		{
			if (left)
			{
				haptics.PlayControllerHaptics(1f, 1f, 0, 0);
				Log("Haptic Left");
			}
			if (right)
			{
				haptics.PlayControllerHaptics(0, 0, 1f, 1f);
				Log("Haptic Right");
			}
			yield return new WaitForSeconds(0.2f);
			if (left)
			{
				haptics.PlayControllerHaptics(1f, 1f, 0, 0);
				Log("Haptic Left");
			}
			if (right)
			{
				haptics.PlayControllerHaptics(0, 0, 1f, 1f);
				Log("Haptic Right");
			}
		}
		private void Log(string message)
		{
			LoggerInstance.Msg(message);
		}

		#region UI
		private int i = 0;
		private int a = 0;
		public override void OnUpdate()
		{ // i hate this so much
			i++;
			if (i > 400)
			{
				i = 0;
				CreateButtonsForAll();
			}
		}

		private void CreateButtonsForAll()
		{
			var swappers = GameObject.FindObjectsOfType<GameObject>().Where(go => go.name == "ShiftstoneQuickswapper").ToArray();
			foreach (var swapper in swappers)
			{
				//Dont add more then 1 button
				if (swapper.transform.GetChild(0).GetChildCount() < 4)
				{
					CreateQSSRandomButton(swapper);
				}
			}
		}


		private void CreatePhysicalGUI()
		{
			var Button = GameObject.Find("ShiftstoneQuickswapper").transform.GetChild(0).GetChild(2).gameObject;
			//GameObject blackListLabel = Calls.Create.NewText();
			//GameObject keepHandLabel = Calls.Create.NewText();

			GameObject blackListButton = GameObject.Instantiate(Button);
			blackListButton.transform.parent = GameObject.Find("ShiftstoneCabinet").transform;
			blackListButton.transform.localPosition = new Vector3(-0.0509f, 1.6473f, 0.6836f);
			blackListButton.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
			blackListButton.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().isToggleButton = false;

			GameObject keepHandButton = GameObject.Instantiate(Button);
			keepHandButton.transform.parent = GameObject.Find("ShiftstoneCabinet").transform;
			keepHandButton.transform.localPosition = new Vector3(-0.0509f, 1.4037f, 0.6836f);
			keepHandButton.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
			keepHandButton.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().isToggleButton = false;

			blackListButton.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().onPressed.AddListener((System.Action)delegate
			{
				BlackListStones(Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().GetCurrentShiftStoneConfiguration());
			});

			keepHandButton.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().onPressed.AddListener((System.Action)delegate
			{
				CycleHandLock();

			});
		}

		private void CreateQSSRandomButton(GameObject swapper)
		{
			GameObject Button = swapper.transform.GetChild(0).GetChild(2).gameObject;

			GameObject RandomButton = GameObject.Instantiate(Button);
			RandomButton.transform.parent = swapper.transform.GetChild(0);
			RandomButton.transform.localPosition = new Vector3(-0.096f, 0.069f, -0.02f);
			RandomButton.transform.localRotation = Quaternion.Euler(296.57f, 84.038f, 359.9f);
			RandomButton.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().isToggleButton = false;

			RandomButton.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().onPressed.AddListener((System.Action)delegate
			{
				RandomizeStones(Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().GetCurrentShiftStoneConfiguration());
			});


			//Local Position 0.1835 -0.001 -0.02
			//Rotation 43.108 348.0498 275.3816
			GameObject Button2 = swapper.transform.GetChild(0).GetChild(2).gameObject;
			GameObject loadOutButton = GameObject.Instantiate(Button2);
			loadOutButton.transform.parent = swapper.transform.GetChild(0);
			loadOutButton.transform.localPosition = new Vector3(0.1835f, -0.001f, -0.02f);
			loadOutButton.transform.localRotation = Quaternion.Euler(43.108f, 348.0498f, 275.3816f);
			loadOutButton.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().isToggleButton = false;

			loadOutButton.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().onPressed.AddListener((System.Action)delegate
			{
				LoadOutButton_Pressed(Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().GetCurrentShiftStoneConfiguration());
			});


		}
		#endregion

		#region Easter Egg
		private void CreateCosmetics()
		{
			glasses = this.LoadAssetBundle("Shift_Stone_Randomizer.assets.glasses", "Glasses");
			UnityEngine.Object.DontDestroyOnLoad(glasses);
			glasses.SetActive(false);
		}
		private void AddCosmetics()
		{
			bool isme = Calls.Players.GetLocalPlayer().Data.GeneralData.PlayFabMasterId == "91D650B737D47020";
			var players = Calls.Players.GetAllPlayers();
			foreach (var player in players)
			{
				if (player.Data.GeneralData.PlayFabMasterId == "91D650B737D47020")
				{
					GameObject myglasses = UnityEngine.Object.Instantiate<GameObject>(glasses);
					myglasses.transform.parent = player.Controller.gameObject.transform.FindChild("Visuals/Skelington/Bone_Pelvis/Bone_Spine_A/Bone_Chest/Bone_Neck/Bone_Head");
					myglasses.transform.localPosition = new Vector3(0.0035f, 0.0431f, 0.0846f);
					myglasses.transform.localRotation = Quaternion.Euler(5.2364f, 0f, 0f);
					myglasses.transform.localScale = new Vector3(1.1576f, 1.1958f, 1.3094f);
					myglasses.SetActive(!isme);
				}
			}
			if (isme)
			{
				GameObject ppc = GameObject.Find("Preview Player Controller");
				GameObject myglasses = UnityEngine.Object.Instantiate<GameObject>(glasses);
				myglasses.transform.parent = ppc.transform.FindChild("Visuals/Skelington/Bone_Pelvis/Bone_Spine_A/Bone_Chest/Bone_Neck/Bone_Head");
				myglasses.transform.localPosition = new Vector3(0.0035f, 0.0431f, 0.0846f);
				myglasses.transform.localRotation = Quaternion.Euler(5.2364f, 0f, 0f);
				myglasses.transform.localScale = new Vector3(1.1576f, 1.1958f, 1.3094f);
				myglasses.SetActive(true);
			}
		}
		private GameObject LoadAssetBundle(string bundleName, string objectName)
		{
			using Stream stream = ((MelonBase)this).MelonAssembly.Assembly.GetManifestResourceStream(bundleName);
			byte[] array = new byte[stream.Length];
			stream.Read(array, 0, array.Length);

			// Manually create Il2CppStructArray<byte> from byte[]
			Il2CppStructArray<byte> il2CppArray = new Il2CppStructArray<byte>(array.Length);
			for (int i = 0; i < array.Length; i++)
			{
				il2CppArray[i] = array[i];
			}

			Il2CppAssetBundle val = Il2CppAssetBundleManager.LoadFromMemory(il2CppArray);
			return UnityEngine.Object.Instantiate<GameObject>(val.LoadAsset<GameObject>(objectName));
		}
		#endregion
	}
}