using Il2CppPhoton.Pun;

using Il2CppRootMotion;

using Il2CppRUMBLE.Combat.ShiftStones;

using Il2CppRUMBLE.Managers;
using Il2CppRUMBLE.Players.Subsystems;
using Il2CppSystem;
using Il2CppRUMBLE.Players;

using MelonLoader;

using RumbleModdingAPI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;
using Action = System.Action;
using Type = Il2CppSystem.Type;

namespace ShiftStoneRandomizer
{
	
	public partial class ShiftStoneRandomizer : MelonMod
	{
		public static ShiftStoneRandomizer Instance { get; private set; }
		//private const string USER_DATA = "Userdata/ShiftStoneRandomizer/";


		private const string BLACKLIST_FILE = "blacklist.txt";
		private const string LOADOUT_FILE = "loadout.txt";
		private const string DEBUG_FILE = ".debug";
		private static System.Random random = new System.Random();
		//private ShiftStone[] shiftStones;
		//private StoneItem[] stones;
		private ShiftStonePrefs[] defaultStones = new ShiftStonePrefs[] {
			ShiftStonePrefs.Random,
			ShiftStonePrefs.Random,
		};

		private static string CurrentScene;
		public static string CurrentLoadedScene { get { return CurrentScene.ToLower().Trim(); } }

		private static string LastScene;
		public static string LastLoadedScene { get { return LastScene.ToLower().Trim(); } }

		public static bool IsFirstMatchLoad { get; private set; }

		public static Il2CppSystem.Collections.Generic.List<Player> Players { get; set; }
		public static int PlayerCount { get { return Players.Count; } }
		public static bool IsInMatch { get { return CurrentLoadedScene.Contains("map") && Players.Count > 0; } }


		public static bool IsHost { get { return PhotonNetwork.IsMasterClient; } }

		//private int[] blackList = new int[0];
		private bool firstLoad = true;

		private static bool IsSceneLoaded = false;

		//private int lockedHand = -1; // -1 no lock, 0 left hand, 1 right hand 
		public static GameObject RandomizerAssets { get; private set; }
		public static GameObject IndicatorsBase { get; private set; }
		private GameObject leftHand;
		private GameObject rightHand;


		public static Hands EnabledHand;

		private GameObject dropSign;

		private PlayerHaptics haptics;


		public static PlayerController Player0 { get; set; }
		public static PlayerController Player1 { get { return Players.Count > 1 ? Players[1].Controller : null; } }

		public static GameObject leftPoint;
		public static GameObject rightPoint;




		public override void OnLateInitializeMelon()
		{
			Instance = this;
			//CreateCosmetics();
			Calls.onMapInitialized += SceneReady;
			Calls.onMatchEnded += CreateButtonsForAll;//CreateButtonsForAll;


			InitPreferences();


		}
		public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
		{
			IsSceneLoaded = false;
			Debug.Log($"Unloaded: \"{sceneName}\"", true);

			if (sceneName == "Gym" || sceneName == "Park" || sceneName.Contains("Map"))
			{
				StoneItem.ResetAllIcons();
				LoadoutInteractor.UnsubAll();
			}


		}
		public override void OnUpdate()
		{
			Vector3 leftPointPos;
			Vector3 rightPointPos;
			string bothPos;

			leftPointPos = leftPoint != null ? leftPoint.transform.position : new Vector3(0, 0, 0);
			rightPointPos = rightPoint != null ? rightPoint.transform.position : new Vector3(0, 0, 0);
			bothPos = $"Left Point: {leftPointPos.ToString()} \nRight Point: {rightPointPos.ToString()}";
			if (IsSceneLoaded)
			{
				//Debug.PrintInGame($"{LoadoutInteractor.Selection[0].ToString()} \n{LoadoutInteractor.Selection[1].ToString()}");
				Debug.PrintInGame($"Automation Mode: {LoadoutInteractor.AutomationMode} \n IsFirstMatchLoad: {IsFirstMatchLoad} \n IsInMatch: {IsInMatch} \n LoadoutIsPrimed: {LoadoutInteractor.IsNextSelectionPrimed}");
				if (LoadoutInteractor.LeftSocket != null && LoadoutInteractor.RightSocket != null)
				{
					try
					{
						foreach(GameObject displayedItem in LoadoutInteractor.DisplayedItem)
						{
							if (displayedItem != null)
							{
								displayedItem.transform.rotation = Quaternion.LookRotation(displayedItem.transform.position - Camera.main.transform.position, Vector3.up);
							}
						}
					}
					catch (System.Exception e)
					{
						// Ignore
					}
				}
			}

			
			
		}


		private void SceneReady()
		{


			//InitializeShiftStones();
			Players = PlayerManager.instance.AllPlayers;

			if (CurrentLoadedScene == "gym")
			{

				if (firstLoad)
				{
					IndicatorsBase = GameObject.Instantiate(Calls.LoadAssetFromStream<GameObject>(this, "ShiftStoneRandomizer.assets.randomizer", "ShiftstoneRandomizer"));
					GameObject.DontDestroyOnLoad(IndicatorsBase);
					IndicatorsBase.SetActive(false);
					GrabBoxSource();
				}
				//CreatePhysicalGUI();

				//Adds a blacklist icon game object to the the shift stone case. 
				//Each stone item has a list of icons for every instance of that stone on the scene that this gets added to. 
				GameObject Cabinet = Calls.GameObjects.Gym.LOGIC.Heinhouserproducts.ShiftstoneCabinet.Cabinet.GetGameObject();
				for (int i = 0; i < StoneItem.AllStones.Length; i++)
				{
					StoneItem.AllStones[i].AddIcon(CreateBlackListIcons(Cabinet.transform.GetChild(i).gameObject));
				}

				firstLoad = false;
			}


			ApplyPrefsToState();
			if (CurrentLoadedScene != "loader")
			{
				Player0 = Players[0].Controller;
				Debug.CreateDebugUi(Player0.gameObject.transform.GetChild(6).GetChild(0).gameObject);
				IsSceneLoaded = true;


				//LoadoutInteractor.MainInteractor = null;


				leftPoint = Player0.gameObject.transform.Find("Visuals/Skelington/Bone_Pelvis/Bone_Spine_A/Bone_Chest/Bone_Shoulderblade_L/Bone_Shoulder_L/Bone_Lowerarm_L/Bone_HandAlpha_L/Bone_Pointer_A_L/Bone_Pointer_B_L/Bone_Pointer_C_L").gameObject;
				rightPoint = Player0.gameObject.transform.Find("Visuals/Skelington/Bone_Pelvis/Bone_Spine_A/Bone_Chest/Bone_Shoulderblade_R/Bone_Shoulder_R/Bone_Lowerarm_R/Bone_HandAlpha_R/Bone_Pointer_A_R/Bone_Pointer_B_R/Bone_Pointer_C_R").gameObject;

				GameObject leftArm = Player0.gameObject.transform.Find("Visuals/Skelington/Bone_Pelvis/Bone_Spine_A/Bone_Chest/Bone_Shoulderblade_L/Bone_Shoulder_L/Bone_Lowerarm_L").gameObject;
				GameObject rightArm = Player0.gameObject.transform.Find("Visuals/Skelington/Bone_Pelvis/Bone_Spine_A/Bone_Chest/Bone_Shoulderblade_R/Bone_Shoulder_R/Bone_Lowerarm_R").gameObject;

				LoadoutInteractor.LeftSocket = leftArm.transform.DeepFind("SnapTransform").gameObject;
				LoadoutInteractor.RightSocket = rightArm.transform.DeepFind("SnapTransform").gameObject;
				LoadoutInteractor.HighlightCurrentEquippedStones();
				LoadoutInteractor.OnMatchLoad();
				CreateButtonsForAll();
			}
			ShowRandomedHand();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="buildIndex"></param>
		/// <param name="sceneName"></param>

		public override void OnSceneWasLoaded(int buildIndex, string sceneName)
		{
			LastScene = CurrentScene;
			CurrentScene = sceneName;

			IsFirstMatchLoad = CurrentLoadedScene.Contains("map") && LastLoadedScene == "gym";
		}
		/*private void BlackListStones(int[] Hand)
		{
			bool hasEnabledEquipedStones = false;
			foreach (int i in Hand)
			{
				if (i > -1 && StoneItem.AllStones[i].IsEnabled)
				{
					hasEnabledEquipedStones = true;
					StoneItem.AllStones[i].IsEnabled = false;
				}
			}

			if (hasEnabledEquipedStones)
				EquipStones(new StoneItem(), new StoneItem());
			else
			{
				foreach (int i in Hand)
				{
					if (i > -1)
						StoneItem.AllStones[i].IsEnabled = true;
				}
			}

			string blackListOut = "";
			Debug.Log("Blacklisted stones: ");
			foreach (StoneItem stone in StoneItem.AllStones)
			{
				if (!stone.IsEnabled)
				{
					Debug.Log($"\t{stone.Name}");
					blackListOut += stone.Name + "\n";
				}
			}
			File.WriteAllText(Path.Combine(USER_DATA, BLACKLIST_FILE), blackListOut);
			ActivateEffect(true, true);
		}*/

		public void CycleHandLock()
		{
			EnabledHand++;
			if ((int)EnabledHand > 1)
			{
				EnabledHand = (Hands)(-1);
			}
			Hands hand = (Hands)EnabledHand;
			Debug.Log($"Hand lock set to: {hand}", true);

			switch (hand)
			{
				case Hands.Both:
					ActivateEffect(true, true);
					break;
				case Hands.Left:
					ActivateEffect(true, false);
					break;
				case Hands.Right:
					ActivateEffect(false, true);
					break;
			}

			PrefEnabledHand.Value = EnabledHand.ToString();
			CatSettings.SaveToFile();

			ShowRandomedHand();
		}

		private void ShowRandomedHand()
		{
			//Exit if hands arent initialized
			if (leftHand == null || rightHand == null)
				return;
			
			Hands hand = EnabledHand;
			Color disabled = new Color(1f, 1f, 1f, 0.25f);
			Color enabled = new Color(1f, 1f, 1f, 1f);
			switch (hand)
			{
				case Hands.Both:
					leftHand.transform.GetChild(0).GetComponent<RawImage>().color = enabled;
					rightHand.transform.GetChild(0).GetComponent<RawImage>().color = enabled;
					break;
				case Hands.Left:
					leftHand.transform.GetChild(0).GetComponent<RawImage>().color = enabled;
					rightHand.transform.GetChild(0).GetComponent<RawImage>().color = disabled;
					break;
				case Hands.Right:
					leftHand.transform.GetChild(0).GetComponent<RawImage>().color = disabled;
					rightHand.transform.GetChild(0).GetComponent<RawImage>().color = enabled;
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


		public static void ActivateEffect(bool left, bool right)
		{

			if (left)
				Player0.GetComponent<PlayerShiftstoneSystem>().ActivateUseShiftstoneEffects(Il2CppRUMBLE.Input.InputManager.Hand.Left);
			if (right)
				Player0.GetComponent<PlayerShiftstoneSystem>().ActivateUseShiftstoneEffects(Il2CppRUMBLE.Input.InputManager.Hand.Right);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="hand"></param>
		/// <param name="includeDisabled"></param>
		/// <returns></returns>
		private static List<ShiftStonePrefs> GetRandomStones(ShiftStonePrefs[] equipped, Hands hand = Hands.Both, bool includeDisabled = false)
		{

			List<StoneItem> options = StoneItem.AllStones.Where(s => s.IsEnabled || includeDisabled).ToList();


			int stonesNeeded = (hand == Hands.Both) ? 2 : 1;

			var filteredOptions = options.Where(x => !equipped.Contains(x.GetEnum())).ToList();


			if (filteredOptions.Count >= stonesNeeded)
			{
				options = filteredOptions; // Safe to remove equipped stones
			}

			options.OrderBy(x => random.Next()).Take(stonesNeeded).ToList();

			List<ShiftStonePrefs> result = new List<ShiftStonePrefs>();


			return result;


		}

		/// <summary>
		/// Randomizes shift stones
		/// Avoids currently equipped stones
		/// Should enable equipping currently equipped stones if available stones are less than 4
		/// </summary>
		public static void RandomizeStones(int[] Equipped, Hands hand = Hands.Both)
		{
			int handIndex = (int)hand;
			int otherHandIndex = hand == Hands.Left ? (int)Hands.Right : (int)Hands.Left;


			List<StoneItem> randomStones = new List<StoneItem>();
			Debug.Log("Stone check:", true);
			for (int i = 0; i < StoneItem.AllStones.Length; i++)
			{
				StoneItem stone = StoneItem.AllStones[i];
				Debug.Log($"\t{stone.Name} - Enabled: {stone.IsEnabled}", true);
				if (stone.IsEnabled && System.Array.IndexOf(Equipped, i) == -1)
				{
					Debug.Log("\t Added", true);
					randomStones.Add(stone);
				}
				else
				{
					Debug.Log("\t Not added", true);
				}
			}


			if (randomStones.Count <= 2 && hand == Hands.Both)
			{
				Debug.Log("Not enough stones to disallow repeats", true);
				// If there are less than 4 stones available, add the equipped stones to the list
				foreach (int stoneIndex in Equipped)
				{
					if (stoneIndex > -1)
						randomStones.Add(StoneItem.AllStones[stoneIndex]);
				}
			}

			// shuffle stones list
			randomStones = randomStones.OrderBy(x => random.Next()).ToList();
			if (randomStones.Count < 2)
			{
				Debug.Log("Not enough stones to randomize, using equipped stones instead.", false, 1);
				if (hand == Hands.Both)
				{
					randomStones.AddRange(new StoneItem[] {
						StoneItem.AllStones[Equipped[0]],
						StoneItem.AllStones[Equipped[1]]
					});

				}
				else
				{
					randomStones.Add(StoneItem.AllStones[Equipped[handIndex]]);
				}
			}

			switch (hand)
			{
				case Hands.Both:
					EquipStones(randomStones[0], randomStones[1]);
					break;
				case Hands.Right:
					EquipStones(null, randomStones[0]);
					break;
				case Hands.Left:
					EquipStones(randomStones[0], null);
					break;
			}
		}

		private static void EquipStones(StoneItem[] StonesToEquip)
		{
			EquipStones(StonesToEquip[0], StonesToEquip[1]);
		}
		/// <summary>
		/// Pass null to ignore hand. Pass new StoneItem() to equip empty stone slot.
		/// Will break if you pass null to the right hand and you equip a stone to the left hand that is already equipped in the right hand.
		/// </summary>
		/// <param name="leftStone"></param>
		/// <param name="rightStone"></param>
		public static void EquipStones(StoneItem leftStone, StoneItem rightStone, bool applyEffect = true)
		{
			Debug.Log("Equipping stones: " + (leftStone != null ? leftStone.Name : "Null") + " | " + (rightStone != null ? rightStone.Name : "Null"), true);

			if (rightStone != null) //Makes sure you don't equip a stone on the left hand if it's already equipped in the right hand
				Player0.GetComponent<PlayerShiftstoneSystem>().RemoveShiftStone(1, true, true);
			if (leftStone != null)
			{
				Player0.GetComponent<PlayerShiftstoneSystem>().RemoveShiftStone(0, true, true);
				if (leftStone.ShiftStone != null)
				{
					Player0.GetComponent<PlayerShiftstoneSystem>().AttachShiftStone(leftStone.ShiftStone, 0, true, true);
					// This ensures that the stones exist (According to Darkener. Don't know why this is -iListen2Sound)
					Player0.GetComponent<PlayerShiftstoneSystem>().RemoveAndReattachShiftstones(true, true);

				}
			}
			if (rightStone != null)
			{
				Player0.GetComponent<PlayerShiftstoneSystem>().RemoveShiftStone(1, true, true);
				if (rightStone.ShiftStone != null)
				{
					Player0.GetComponent<PlayerShiftstoneSystem>().AttachShiftStone(rightStone.ShiftStone, 1, true, true);
					Player0.GetComponent<PlayerShiftstoneSystem>().RemoveAndReattachShiftstones(true, true);
				}
			}
			if (applyEffect)
				ActivateEffect(leftStone != null, rightStone != null);
		}

		private static void EquipStones(ShiftStonePrefs single, Hands hand)
		{
			if (hand == Hands.Left)
			{
				EquipStones(single, ShiftStonePrefs.Stay);
			}
			else if (hand == Hands.Right)
			{
				EquipStones(ShiftStonePrefs.Stay, single);
			}
			else
			{
				Debug.Log("EquipStones: Single Equip method given invalid hand either neither or both");
			}
		}
		
		private static void EquipStones(ShiftStonePrefs left, ShiftStonePrefs right)
		{
			StoneItem leftStone = null;
			StoneItem rightStone = null;

			if (left == ShiftStonePrefs.Empty)
			{
				leftStone = new StoneItem();
			}
			else if (left == ShiftStonePrefs.Random)
			{
				leftStone = PickRandomStoneExcept(rightStone);
			}
			else if (left == ShiftStonePrefs.Stay)
			{
				leftStone = null;
			}
			else
			{
				leftStone = StoneItem.AllStones[(int)left];
			}
			if (right == ShiftStonePrefs.Empty)
			{
				rightStone = new StoneItem();
			}
			else if (right == ShiftStonePrefs.Random)
			{
				rightStone = PickRandomStoneExcept(leftStone);
			}
			else if (right == ShiftStonePrefs.Stay)
			{
				rightStone = null;
			}
			else
			{
				rightStone = StoneItem.AllStones[(int)right];
			}

			EquipStones(leftStone, rightStone);
		}
		private static StoneItem PickRandomStoneExcept(StoneItem excludeStone = null)
		{
			//TODO: Create select random enabled stone function
			StoneItem selectedStone = null;
			List<StoneItem> possibleStones = StoneItem.AllStones.Where(s => s.IsEnabled == true).ToList();
			if (possibleStones.Count() >= 2)
			{
				do
				{
					selectedStone = possibleStones[random.Next(0, possibleStones.Count())];

				} while (selectedStone.Name == excludeStone.Name);
				return selectedStone;
			}
			else
			{
				return null;
			}
		}


	}
}