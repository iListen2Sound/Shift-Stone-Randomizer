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
	/// <summary>
	/// Represents an item associated with a shift stone, providing functionality to manage its state and behavior.
	/// </summary>
	/// <remarks>A <see cref="StoneItem"/> can either represent a valid shift stone or an empty slot.  It provides
	/// properties to manage the stone's state, such as enabling or disabling it,  and tracks the number of disabled stones
	/// globally through <see cref="BlackListCount"/>.</remarks>
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

				//MelonLogger.Msg($"{Name} Icon: {_icon.active}");

				if (value)
					_blacklistCount++;

				else
					_blacklistCount--;
				if (_icon == null)
					return;
				_icon.SetActive(!_isEnabled);
			}
		}

		private GameObject _icon;
		public GameObject Icon { set { _icon = value; _icon.SetActive(false); } }
		public StoneItem(ShiftStone shiftStone)
		{
			shiftStone.gameObject.SetActive(false);// Disable the stone so it doesn't show up in the game
			ShiftStone = shiftStone;
			_isEnabled = true;
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
		private const string USER_DATA = "Userdata/ShiftStoneRandomizer/";
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
		private GameObject IndicatorsBase;
		private GameObject leftHand;
		private GameObject rightHand;


		private enum HandLock
		{
			Neither = -1,
			Left = 0,
			Right = 1
		}
		private GameObject dropSign;

		private PlayerHaptics haptics;

		private GameObject glasses;

		public override void OnLateInitializeMelon()
		{
			//CreateCosmetics();
			Calls.onMapInitialized += SceneReady;
			Calls.onMatchEnded += CreateButtonsForAll;//CreateButtonsForAll;

		}
		public void logOnMatchEnded()
		{
			Log("Match Ended", true);
		}
		private void SceneReady()
		{
			InitializeShiftStones();
			if (CurrentScene == "Gym")
			{

				if (firstLoad)
				{
					IndicatorsBase = GameObject.Instantiate(Calls.LoadAssetFromStream<GameObject>(this, "ShiftStoneRandomizer.assets.randomizer", "ShiftstoneRandomizer"));
					GameObject.DontDestroyOnLoad(IndicatorsBase);
					IndicatorsBase.SetActive(false);
				}
				CreatePhysicalGUI();
				firstLoad = false;



				GameObject Cabinet = Calls.GameObjects.Gym.LOGIC.Heinhouserproducts.ShiftstoneCabinet.Cabinet.GetGameObject();
				for (int i = 0; i < stones.Length; i++)
				{
					stones[i].Icon = CreateBlackListIcons(Cabinet.transform.GetChild(i).gameObject);
				}
				LoadLoadOut(true);

			}
			CreateButtonsForAll();
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

		private void LoadLoadOut(bool dontEquip = false, bool skipLock = false)
		{

			if (File.Exists(Path.Combine(USER_DATA, LOADOUT_FILE)))
			{
				string[] config = File.ReadAllLines(Path.Combine(USER_DATA, LOADOUT_FILE));
				bool parsed = System.Enum.TryParse(config[0], out HandLock handLock);

				if ( !skipLock)
				{
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


			if (dontEquip)
				return;
			EquipStones(defaultStones);

		}
		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// Blacklist icons are added at sceneready and only in the gym
		/// </remarks>
		private void InitializeShiftStones()
		{
			stones = new StoneItem[]
			{
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

			ShowHandLock();
		}

		private void ShowHandLock()
		{
			HandLock hand = (HandLock)lockedHand;
			Color disabled = new Color(1f, 1f, 1f, 0.2f);
			Color enabled = new Color(1f, 1f, 1f, 1f);
			switch (hand)
			{
				case HandLock.Neither:
					leftHand.transform.GetChild(0).GetComponent<RawImage>().color = enabled;
					rightHand.transform.GetChild(0).GetComponent<RawImage>().color = enabled;
					break;
				case HandLock.Left:
					leftHand.transform.GetChild(0).GetComponent<RawImage>().color = disabled;
					rightHand.transform.GetChild(0).GetComponent<RawImage>().color = enabled;
					break;
				case HandLock.Right:
					leftHand.transform.GetChild(0).GetComponent<RawImage>().color = enabled;
					rightHand.transform.GetChild(0).GetComponent<RawImage>().color = disabled;
					break;
			}
		}

		private void SignFall()
		{
			string[] layers = new string[] { "Player", "Floor", "PedestalFloor", "CombatFloor", "Environment", "Clouds" };

			if (random.Next(50) == 1)
			{
				Rigidbody rb = dropSign.AddComponent<Rigidbody>();
				rb.AddForce(new Vector3(1.5f, 1f, 0), ForceMode.Impulse);

				// I don't think either of these work
				rb.angularVelocity = new Vector3(0, 0, 0);
				rb.AddRelativeTorque(new Vector3(0, -100000, -100000), ForceMode.Impulse);
				rb.includeLayers = new LayerMask().AddToMask(layers);
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
			string loadout = $"{(HandLock)lockedHand} \n{left.Name}\n{right.Name}";
			
			File.WriteAllText(Path.Combine(USER_DATA, LOADOUT_FILE), loadout);
			Log($"Saved loadout: {loadout}", true);

			defaultStones[0] = left;
			defaultStones[1] = right;
			ActivateEffect(true, true);


			SignFall();
		}

		private void ActivateEffect(bool left, bool right)
		{
			if (left)
				Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().ActivateUseShiftstoneEffects(Il2CppRUMBLE.Input.InputManager.Hand.Left);
			if (right)
				Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().ActivateUseShiftstoneEffects(Il2CppRUMBLE.Input.InputManager.Hand.Right);
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

		/*		private int i = 0;
				private int a = 0;
				public override void OnUpdate()
				{ // i hate this so much
					i++;
					if (i > 50)
					{
						i = 0;
						CreateButtonsForAll();
					}
				}*/

		private void CreateButtonsForAll()
		{
			var swappers = GameObject.FindObjectsOfType<GameObject>().Where(go => go.name == "ShiftstoneQuickswapper").ToArray();
			foreach (var swapper in swappers)
			{
				//Dont add more then 1 button
				if (swapper.transform.GetChild(0).GetChildCount() < 4)
					CreateQSSRandomButton(swapper);

			}
		}
		/// <summary>
		/// Create blacklist icons for each stone in the blacklist. Game object is disabled by default
		/// </summary>
		/// <param name="TargetParent">Box to parent the icons to</param>
		/// <returns>Reference to the blacklist icon for the shiftstone</returns>
		private GameObject CreateBlackListIcons(GameObject TargetParent)
		{

			//indicator.SetActive(false);

			//nameBendingObject = GameObject.Instantiate(Calls.LoadAssetFromStream<GameObject>(this, "NameBending.assets.namebending", "NameBending"));

			//GameObject box = Cabinet.transform.GetChild(0).gameObject;

			System.Random jitter = new System.Random(TargetParent.GetHashCode());

			GameObject blackListIcon = GameObject.Instantiate(IndicatorsBase.transform.GetChild(0).gameObject);
			//GameObject blackListIcon = indicator;//.transform.GetChild(0).gameObject;
			blackListIcon.SetActive(false);
			blackListIcon.transform.SetParent(TargetParent.transform, false);
			blackListIcon.transform.localScale = Vector3.one * 0.0002f;
			blackListIcon.transform.localRotation = Quaternion.Euler(-0f, 90f * jitter.Next(1, 2), (90f * jitter.Next(4)) + jitter.Next(-10, 10));
			blackListIcon.transform.localPosition = new Vector3(-0.053f, 0f + jitter.Next(-100, 100) * 0.0001f, 0f + jitter.Next(-100, 100) * 0.0001f);
			return blackListIcon;
		}

		private void CreatePhysicalGUI()
		{
			var Button = GameObject.Find("ShiftstoneQuickswapper").transform.GetChild(0).GetChild(2).gameObject;
			GameObject titleBar = GameObject.Instantiate(Calls.GameObjects.Gym.LOGIC.Heinhouserproducts.MatchConsole.MatchmakingSettings.TitleBar.GetGameObject());
			GameObject titleText = GameObject.Instantiate(Calls.GameObjects.Gym.LOGIC.Heinhouserproducts.MatchConsole.MatchmakingSettings.TitleText.GetGameObject());

			titleText.transform.localRotation = Quaternion.Euler(0f, 90f, 90f);
			titleText.transform.localPosition = new Vector3(-0.09f, 0.736f, 0.0002f);
			titleText.transform.localScale = new Vector3(6.8f, 4.9f, 1f);
			titleText.transform.SetParent(titleBar.transform, false);

			GameObject blackListButton = GameObject.Instantiate(Button);
			blackListButton.transform.parent = GameObject.Find("ShiftstoneCabinet").transform;
			blackListButton.transform.localPosition = new Vector3(-0.0509f, 1.6473f, 0.6836f);
			blackListButton.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
			blackListButton.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().isToggleButton = false;

			GameObject blackListLabel = GameObject.Instantiate(titleBar);
			blackListLabel.transform.SetParent(blackListButton.transform, false);
			blackListLabel.transform.localScale = new Vector3(0.09f, 0.3f, 0.3f);
			blackListLabel.transform.localPosition = new Vector3(0.118f, 0f, 0.212f);
			blackListLabel.transform.localRotation = Quaternion.Euler(0.3f, 83.491f, 275.6874f);
			blackListLabel.transform.GetChild(0).gameObject.GetComponent<TextMeshPro>().text = "Blacklist";


			GameObject keepHandButton = GameObject.Instantiate(Button);
			keepHandButton.transform.parent = GameObject.Find("ShiftstoneCabinet").transform;
			keepHandButton.transform.localPosition = new Vector3(-0.0509f, 1.3537f, 0.6836f);
			keepHandButton.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
			keepHandButton.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().isToggleButton = false;

			GameObject keepHandLabel = GameObject.Instantiate(titleBar);
			keepHandLabel.transform.SetParent(keepHandButton.transform, false);
			keepHandLabel.transform.localScale = new Vector3(0.09f, 0.3f, 0.3f);
			keepHandLabel.transform.localPosition = new Vector3(0.168f, 0.02f, 0.212f);
			keepHandLabel.transform.localRotation = Quaternion.Euler(1.9346f, 97f, 268.269f);
			keepHandLabel.transform.GetChild(0).gameObject.GetComponent<TextMeshPro>().text = "Lock Hand";

			leftHand = GameObject.Instantiate(IndicatorsBase.transform.GetChild(2).gameObject);

			rightHand = GameObject.Instantiate(IndicatorsBase.transform.GetChild(1).gameObject);

			leftHand.transform.SetParent(keepHandButton.transform.GetChild(0), false);
			leftHand.transform.localPosition = new Vector3(-0.14f, 0.01f, 0.07f);
			leftHand.transform.localRotation = Quaternion.Euler(90f, 90f, 0f);
			leftHand.transform.localScale = new Vector3(0.0003f, 0.0003f, 0.0003f);





			rightHand.transform.SetParent(keepHandButton.transform.GetChild(0), false);
			rightHand.transform.localPosition = new Vector3(-0.14f, 0.01f, -0.07f);
			rightHand.transform.localRotation = Quaternion.Euler(90f, 90f, 0f);
			rightHand.transform.localScale = new Vector3(0.0003f, 0.0003f, 0.0003f);
			//position -0.13 0.01 -0.06
			//Rotation 90 90 0
			//0.0003 0.0003 0.0003



			GameObject saveLoadOutButton = GameObject.Instantiate(Button);
			saveLoadOutButton.transform.parent = GameObject.Find("ShiftstoneCabinet").transform;
			saveLoadOutButton.transform.localPosition = new Vector3(-0.0509f, 1.6473f, -1.0164f);
			saveLoadOutButton.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
			saveLoadOutButton.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().isToggleButton = false;

			GameObject saveLoadOutLabel = GameObject.Instantiate(titleBar);
			saveLoadOutLabel.transform.SetParent(saveLoadOutButton.transform, false);
			saveLoadOutLabel.transform.localPosition = new Vector3(0.128f, 0.025f, 0.212f);
			saveLoadOutLabel.transform.localScale = new Vector3(0.09f, 0.3f, 0.3f);
			saveLoadOutLabel.transform.localRotation = Quaternion.Euler(5.0254f, 87.0001f, 271.7236f);
			saveLoadOutLabel.transform.GetChild(0).gameObject.GetComponent<TextMeshPro>().text = "Loadout";

			GameObject saveLabel = GameObject.Instantiate(titleBar);
			saveLabel.transform.SetParent(saveLoadOutButton.transform.GetChild(0), false);
			saveLabel.transform.localPosition = new Vector3(0.208f, -0.05f, 0.212f);
			saveLabel.transform.localScale = new Vector3(0.09f, 0.2f, 0.3f);
			saveLabel.transform.localRotation = Quaternion.Euler(5.0254f, 82.3274f, 274.8562f);
			saveLabel.transform.GetChild(0).gameObject.GetComponent<TextMeshPro>().text = "Save";
			saveLabel.transform.GetChild(0).gameObject.transform.localScale = new Vector3(9.7f, 4.9f, 1f);

			dropSign = saveLabel;
			//Local Position 0.208 -0.05 0.212
			//Rotation 5.0254 82.3274 274.8562
			//Scale 0.09 0.2 0.3

			//Text Scale 9.7 4.9 1


			blackListButton.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().onPressed.AddListener((System.Action)delegate
			{
				BlackListStones(Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().GetCurrentShiftStoneConfiguration());
			});

			keepHandButton.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().onPressed.AddListener((System.Action)delegate
			{
				CycleHandLock();
			});

			saveLoadOutButton.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().onPressed.AddListener((System.Action)delegate
		  	{
				  SaveLoadOut(Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().GetCurrentShiftStoneConfiguration());
			  });

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
				LoadLoadOut(false, true);
			});


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