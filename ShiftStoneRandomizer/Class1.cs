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

	public partial class ShiftStoneRandomizer : MelonMod
	{
		//private const string USER_DATA = "Userdata/ShiftStoneRandomizer/";
		private const string BLACKLIST_FILE = "blacklist.txt";
		private const string LOADOUT_FILE = "loadout.txt";
		private const string DEBUG_FILE = ".debug";
		private System.Random random = new System.Random();
		//private ShiftStone[] shiftStones;
		private StoneItem[] stones;
		private ShiftStonePrefs[] defaultStones = new ShiftStonePrefs[] {
			ShiftStonePrefs.Random,
			ShiftStonePrefs.Random,
		};
		//private int[] blackList = new int[0];
		private bool debugMode = false;
		private bool firstLoad = true;
		private string CurrentScene;
		//private int lockedHand = -1; // -1 no lock, 0 left hand, 1 right hand 
		private GameObject RandomizerAssets;
		private GameObject IndicatorsBase;
		private GameObject leftHand;
		private GameObject rightHand;


		private Hands EnabledHand;

		private GameObject dropSign;

		private PlayerHaptics haptics;

		private GameObject glasses;

		public override void OnLateInitializeMelon()
		{
			//CreateCosmetics();
			Calls.onMapInitialized += SceneReady;
			Calls.onMatchEnded += CreateButtonsForAll;//CreateButtonsForAll;
			InitPreferences();
			

		}
		public void logOnMatchEnded()
		{
			Debug.Log("Match Ended", true);
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
					GrabSourceObjects();
				}
				CreatePhysicalGUI();




				GameObject Cabinet = Calls.GameObjects.Gym.LOGIC.Heinhouserproducts.ShiftstoneCabinet.Cabinet.GetGameObject();
				for (int i = 0; i < stones.Length; i++)
				{
					stones[i].Icon = CreateBlackListIcons(Cabinet.transform.GetChild(i).gameObject);
				}
				
				ShowRandomedHand();
				firstLoad = false;
			}
			CreateButtonsForAll();
			ApplyPrefsToState();


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
			Debug.Log("Blacklisted stones: ");
			foreach (StoneItem stone in stones)
			{
				if (!stone.IsEnabled)
				{
					Debug.Log($"\t{stone.Name}");
					blackListOut += stone.Name + "\n";
				}
			}
			File.WriteAllText(Path.Combine(USER_DATA, BLACKLIST_FILE), blackListOut);
			ActivateEffect(true, true);
		}

		private void CycleHandLock()
		{
			EnabledHand++;
			if ((int)EnabledHand > 1)
			{
				EnabledHand = (Hands)(-1);
			}
			Hands hand = (Hands)EnabledHand;
			Debug.Log($"Hand lock set to: {hand}");

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
			Hands hand = EnabledHand;
			Color disabled = new Color(1f, 1f, 0f, 0.5f);
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
			Debug.Log("Stone check:", true);
			for (int i = 0; i < stones.Length; i++)
			{
				StoneItem stone = stones[i];
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


			if (randomStones.Count <= 2 && EnabledHand == Hands.Both)
			{
				Debug.Log("Not enough stones to disallow repeats", true);
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
				Debug.Log("Not enough stones to randomize, using equipped stones instead.");
				return;
			}

			switch (EnabledHand)
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
			Debug.Log("Equipping stones: " + (leftStone != null ? leftStone.Name : "Null") + " | " + (rightStone != null ? rightStone.Name : "Null"), true);

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

		private void EquipStones(ShiftStonePrefs single, Hands hand)
		{
			if(hand == Hands.Left)
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

		private void EquipStones(ShiftStonePrefs left, ShiftStonePrefs right)
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
				leftStone = stones[(int)left];
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
				rightStone = stones[(int)right];
			}

			EquipStones(leftStone, rightStone);


		}

		private StoneItem PickRandomStoneExcept(StoneItem excludeStone = null)
		{
			//TODO: Create select random enabled stone function
			StoneItem selectedStone = null;
			List<StoneItem> possibleStones = stones.Where(s => s.IsEnabled = true).ToList();
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

/*		private void Log(string message, bool debugOnly = false)
		{
			if (!debugOnly)
			{
				LoggerInstance.Msg(message);
				return;
			}
			if (debugMode)
				LoggerInstance.Msg(message);


		}*/

	
	}
}