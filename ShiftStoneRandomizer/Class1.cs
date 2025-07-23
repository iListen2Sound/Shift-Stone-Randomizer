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
using UnityEditor;

using UnityEngine.SceneManagement;
using HarmonyLib;
using Type = Il2CppSystem.Type;
using Action = System.Action;
using Il2CppRUMBLE.Managers;
using MelonLoader.Utils;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.InputSystem.Utilities;

namespace ShiftStoneRandomizer
{
	class StoneItem
	{
		private static int _blacklistCount = 0;
		public static int BlackListCount
		{
			get { return _blacklistCount; }
		}
		public ShiftStone ShiftStone { get; set; }
		/// <summary>
		/// Returns "None" if the stone is null.
		/// </summary>
		public string Name
		{
			get
			{
				return ShiftStone != null ? ShiftStone.name.Replace("Stone", "") : "Empty";
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

	public class Class1 : MelonMod
	{
		private const string USER_DATA = "Userdata/Shift_Stone_Randomizer/";
		private const string BLACKLIST_FILE = "blacklist.txt";
		private const string LOADOUT_FILE = "loadout.txt";
		private const string DEBUG_FILE = ".debug";
		private System.Random random = new System.Random();
		//private ShiftStone[] shiftStones;
		private StoneItem[] stones;
		private StoneItem[] defaultStones = new StoneItem[] {
			new StoneItem(), // Empty slot
			new StoneItem(), // Empty slot
		};
		//private int[] blackList = new int[0];
		private bool debugMode = false;
		private bool firstLoad = true;
		private string CurrentScene;
		private int lockedHand = -1; // -1 no lock, 0 left hand, 1 right hand 
		private GameObject RandomizerAssets;
		private enum HandLock
		{
			Neither = -1,
			Left = 0,
			Right = 1
		}


		private PlayerHaptics haptics;

		private GameObject glasses;

		public override void OnLateInitializeMelon()
		{
			//CreateCosmetics();
			Calls.onMatchEnded += CreateButtonsForAll;
			Calls.onMapInitialized += SceneReady;
		}
		private void SceneReady()
		{
			InitializeShiftStones();
			CreateButtonsForAll();

			if (CurrentScene == "Gym")
			{
				if (firstLoad)
				{
					/*RandomizerAssets = LoadAsset();
					GameObject.DontDestroyOnLoad(RandomizerAssets);
					RandomizerAssets.SetActive(false);*/

					LoadLoadOut(true);

					LoadBlackListFile();
				}
				firstLoad = false;
			}

			//AddCosmetics();
            CreatePhysicalGUI();


            haptics = PlayerManager.instance.playerControllerPrefab.gameObject.GetComponent<PlayerHaptics>();
			LoadBlackListFile();


		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="buildIndex"></param>
		/// <param name="sceneName"></param>
		/// TODO: Create default loadout system that loads only once.

		public override void OnSceneWasLoaded(int buildIndex, string sceneName)
		{
			CurrentScene = sceneName;
		}

		private void LoadBlackListFile()
		{

			if (!Directory.Exists(USER_DATA))
				Directory.CreateDirectory(USER_DATA);

			if (File.Exists(Path.Combine(USER_DATA, DEBUG_FILE)))
			{
				debugMode = true;
				Log("Debug mode enabled");
			}
			else
			{
				debugMode = false;
				Log("Debug mode disabled");
			}


			if (File.Exists(Path.Combine(USER_DATA, BLACKLIST_FILE)))
			{
				string[] lines = File.ReadAllLines(Path.Combine(USER_DATA, BLACKLIST_FILE));
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
				Log("No blacklist file found");
		}

		private void LoadLoadOut(bool dontEquip = false)
		{
			if (firstLoad) //Read from file on first load then store in defaultStones[]. Subsequent loads will just use the defaultStones array.
			{
				if (File.Exists(Path.Combine(USER_DATA, LOADOUT_FILE)))
				{
					string[] config = File.ReadAllLines(Path.Combine(USER_DATA, LOADOUT_FILE));
					bool parsed = System.Enum.TryParse(config[0], out HandLock handLock);

					Log($"Parsed string:\n {string.Join("\n\t ", config)}", true);
					if (!parsed)
					{
						Log($"Failed to parse hand lock from file: {config[0]}. Defaulting to None.", true);
						handLock = HandLock.Neither;
					}
					else
					{
						Log($"Parsed hand lock from file: {handLock}", true);
						lockedHand = (int)handLock;
					}
					//Run through the lines in the conf file. Skip the first line which is the hand lock
					for (int i = 1; i < config.Length; i++)
					{
						if (config[i] == "Empty")
							defaultStones[i - 1] = new StoneItem();
						else
						{
							foreach (StoneItem stone in stones)
							{
								if (stone.Name == config[i])
								{
									defaultStones[i - 1] = stone;
									Log($"Loaded {stone.Name} from file", true);
								}
							}
						}
					}
				}
			}

			if (!dontEquip)
			{
				EquipStones(defaultStones); 
			}
		}

		private void InitializeShiftStones()
		{
			/*shiftStones = new ShiftStone[] {
				AdamantStone, ChargeStone, FlowStone, GuardStone, StubbornStone, SurgeStone, VigorStone, VolatileStone,
			};*/
			stones = new StoneItem[] {
				new StoneItem(Calls.Managers.GetPoolManager().GetPooledObject("AdamantStone").gameObject.GetComponent<UnyieldingStone>()),
				new StoneItem(Calls.Managers.GetPoolManager().GetPooledObject("ChargeStone").gameObject.GetComponent<ChargeStone>()),
				new StoneItem(Calls.Managers.GetPoolManager().GetPooledObject("FlowStone").gameObject.GetComponent<FlowStone>()),
				new StoneItem(Calls.Managers.GetPoolManager().GetPooledObject("GuardStone").gameObject.GetComponent<GuardStone>()),
				new StoneItem(Calls.Managers.GetPoolManager().GetPooledObject("StubbornStone").gameObject.GetComponent<StubbornStone>()),
				new StoneItem(Calls.Managers.GetPoolManager().GetPooledObject("SurgeStone").gameObject.GetComponent<CounterStone>()),
				new StoneItem(Calls.Managers.GetPoolManager().GetPooledObject("VigorStone").gameObject.GetComponent<VigorStone>()),
				new StoneItem(Calls.Managers.GetPoolManager().GetPooledObject("VolatileStone").gameObject.GetComponent<VolatileStone>())
			};
		}

		/// <summary>
		/// Blacklists stones and writes to file.
		/// </summary>
		/// <param name="Stones">Equipped stones</param>
		private void BlackListStones(int[] Hand)
		{
			bool hasEnabledEquipedStones = false;
			foreach (int i in Hand)
			{
				if (i > -1 && stones[i].IsEnabled)
				{
					hasEnabledEquipedStones = true;
					stones[i].IsEnabled = false;
				}
			}

			if (hasEnabledEquipedStones)
				EquipStones(new StoneItem(), new StoneItem());
			else
			{
				foreach (int i in Hand)
				{
					if (i > -1)
						stones[i].IsEnabled = true;
				}
			}

			string blackListOut = "";
			Log("Blacklisted stones: ");
			foreach (StoneItem stone in stones)
			{
				if (!stone.IsEnabled)
				{
					Log($"\t{stone.Name}");
					blackListOut += stone.Name + "\n";
				}
			}
			File.WriteAllText(Path.Combine(USER_DATA, BLACKLIST_FILE), blackListOut);
			ActivateEffect(true, true);
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
				case HandLock.Neither:
					
					ActivateEffect(true, true);
					break;
				case HandLock.Left:
					
					ActivateEffect(true, false);
					break;
				case HandLock.Right:
					
					ActivateEffect(false, true);
					break;
			}
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
			File.WriteAllText(Path.Combine(USER_DATA, LOADOUT_FILE), $"{(HandLock)lockedHand} \n{left.Name}\n{right.Name}");
			defaultStones[0] = left;
			defaultStones[1] = right;


			ActivateEffect(true, true);
		}

		private void ActivateEffect(bool left, bool right)
		{

			if (left)
			{
				Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().ActivateUseShiftstoneEffects(Il2CppRUMBLE.Input.InputManager.Hand.Left); 
			}
			if (right)
			{
				Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().ActivateUseShiftstoneEffects(Il2CppRUMBLE.Input.InputManager.Hand.Right); 
			}
		}

		private void LoadOutButton_Pressed(int[] Equipped)
		{
			LoadLoadOut();
		}

		/// <summary>
		/// Randomizes shift stones
		/// Avoids currently equipped stones
		/// Should enable equipping currently equipped stones if available stones are less than 4
		/// </summary>
		private void RandomizeStones(int[] Equipped)
		{
			List<StoneItem> randomStones = new List<StoneItem>();
			Log("Stone check:", true);
			for (int i = 0; i < stones.Length; i++)
			{
				StoneItem stone = stones[i];
				Log($"\t{stone.Name} - Enabled: {stone.IsEnabled}", true);
				if (stone.IsEnabled && System.Array.IndexOf(Equipped, i) == -1)
				{
					Log("\t Added", true);
					randomStones.Add(stone);
				}
				else
				{
					Log("\t Not added", true);
				}
			}


			if (randomStones.Count <= 2 && lockedHand == -1)
			{
				Log("Not enough stones to disallow repeats", true);
				// If there are less than 4 stones available, add the equipped stones to the list
				foreach (int stoneIndex in Equipped)
				{
					if (stoneIndex > -1)
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

		private void EquipStones(StoneItem[] StonesToEquip)
		{
			EquipStones(StonesToEquip[0], StonesToEquip[1]);
		}
		/// <summary>
		/// Pass null to ignore hand. Pass new StoneItem() to equip empty stone slot.
		/// Will break if you pass null to the right hand and you equip a stone to the left hand that is already equipped in the right hand.
		/// </summary>
		/// <param name="leftStone"></param>
		/// <param name="rightStone"></param>
		private void EquipStones(StoneItem leftStone, StoneItem rightStone)
		{
			Log("Equipping stones: " + (leftStone != null ? leftStone.Name : "Null") + " | " + (rightStone != null ? rightStone.Name : "Null"), true);

			if (rightStone != null) //Makes sure you don't equip a stone on the left hand if it's already equipped in the right hand
				Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().RemoveShiftStone(1, true, true);



			if (leftStone != null)
			{

				Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().RemoveShiftStone(0, true, true);
				if (leftStone.ShiftStone != null)
				{
					Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().AttachShiftStone(leftStone.ShiftStone, 0, true, true);
					// This ensures that the stones exist (According to Darkener. Don't know why this is -iListen2Sound)
					Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().RemoveAndReattachShiftstones(true, true);

				}
			}
			if (rightStone != null)
			{
				Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().RemoveShiftStone(1, true, true);
				if (rightStone.ShiftStone != null)
				{
					Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().AttachShiftStone(rightStone.ShiftStone, 1, true, true);
					Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().RemoveAndReattachShiftstones(true, true);

				}
			}
			ActivateEffect(leftStone != null, rightStone != null);
		}

		private void Log(string message, bool debugOnly = false)
		{
			if (!debugOnly)
			{
				LoggerInstance.Msg(message);
				return;
			}
			if (debugMode)
				LoggerInstance.Msg(message);


		}

		#region UI

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


			//-0.0509 1.6473 -1.0164
			GameObject saveLoadOutButton = GameObject.Instantiate(Button);
			saveLoadOutButton.transform.parent = GameObject.Find("ShiftstoneCabinet").transform;
			saveLoadOutButton.transform.localPosition = new Vector3(-0.0509f, 1.6473f, -1.0164f);
			saveLoadOutButton.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
			saveLoadOutButton.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().isToggleButton = false;
			saveLoadOutButton.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().onPressed.AddListener((System.Action)delegate
			{
				SaveLoadOut(Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().GetCurrentShiftStoneConfiguration());
			});

			/*
			GameObject Cabinet = Calls.GameObjects.Gym.Logic.HeinhouserProducts.ShiftstoneCabinet.Cabinet.GetGameObject();
			for (int i = 0; i < 8; i++)
			{
				GameObject child = Cabinet.transform.GetChild(i).gameObject;
				GameObject iconCopy = GameObject.Instantiate(RandomizerAssets.transform.GetChild(0).gameObject);
				iconCopy.SetActive(true);
				iconCopy.transform.SetParent(child.transform);
			}*/
		}

		private void CreateQSSRandomButton(GameObject swapper)
		{
			GameObject Button = swapper.transform.GetChild(0).GetChild(2).gameObject;

			GameObject RandomButton = GameObject.Instantiate(Button);
			RandomButton.transform.parent = swapper.transform.GetChild(0);
			//-0.096 0.064 - 0.025
			RandomButton.transform.localPosition = new Vector3(-0.096f, 0.064f, -0.025f);
			//298.0022 83.3369 359.8999
			RandomButton.transform.localRotation = Quaternion.Euler(298.0022f, 83.3369f, 359.8999f);
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
				EquipStones(defaultStones);
			});

			//-0.0634 -0.0571 -0.0366
			//332.1686 247.6016 193.0708

			//0.0531 0.054 -0.1023
			//292.25 6.781 350.6181
			GameObject Button3 = swapper.transform.GetChild(0).GetChild(2).gameObject;
			GameObject ClearStones = GameObject.Instantiate(Button3);
			ClearStones.transform.parent = swapper.transform.GetChild(0);
			ClearStones.transform.localPosition = new Vector3(0.0531f, 0.054f, -0.1023f);
			ClearStones.transform.localRotation = Quaternion.Euler(292.25f, 6.781f, 350.6181f);
			ClearStones.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().isToggleButton = false;
			ClearStones.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().onPressed.AddListener((System.Action)delegate
			{
				EquipStones(new StoneItem(), new StoneItem());
			});


		}

		public GameObject LoadAsset()
		{
			using (System.IO.Stream bundleStream = MelonAssembly.Assembly.GetManifestResourceStream("ShiftStoneRandomizer.assets.randomizer"))
			{
				byte[] bundleBytes = new byte[bundleStream.Length];
				bundleStream.Read(bundleBytes, 0, bundleBytes.Length);
				Il2CppAssetBundle bundle = Il2CppAssetBundleManager.LoadFromMemory(bundleBytes);
				var asset = GameObject.Instantiate(bundle.LoadAsset<GameObject>("ShiftstoneRandomizer"));
				return asset;
			}
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