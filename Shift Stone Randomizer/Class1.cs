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

namespace Shift_Stone_Randomizer
{
	public class Class1 : MelonMod
	{
		private System.Random random = new System.Random();
		private ShiftStone[] shiftStones;
		private int[] blackList = { -2 };
		
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
		}

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

			shiftStones = new ShiftStone[] {
				AdamantStone, ChargeStone, FlowStone, GuardStone, StubbornStone, SurgeStone, VigorStone, VolatileStone,
			};
			foreach (ShiftStone stone in shiftStones)
			{
				stone.transform.position = new Vector3(0f, -1000f, 0f);
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

		private void BlackListStones(int[] Stones)
		{
			if (blackList.Length + Stones.Length > 7)
			{
				MelonLogger.Warning("Reached stone blacklist count");
				return;
			}
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
					// add equiped stones to the blacklist
					blackList = blackList.Append(Stones[i]).ToArray();
				}
			}
			EquipStones(-1, -1, false, true);
		}

		

		
		private void CycleHandLock()
		{
			lockedHand++;
			if (lockedHand > 1)
			{
				lockedHand = -1;
			}

			HandLock hand = (HandLock)lockedHand;
			MelonLogger.Msg($"Hand lock set to: {hand}");


			switch (hand)
			{
				case HandLock.None:
					haptics.PlayControllerHaptics(1, 1, 1, 1);
					break;
				case HandLock.Left:
					haptics.PlayControllerHaptics(1, 1, 0, 0);
					break;
				case HandLock.Right:
					haptics.PlayControllerHaptics(0, 0, 1, 1);
					break;
			}


			
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
				RandomizeStones();
			});
		}
		/// <summary>
		/// Randomizes shift stones
		/// Avoids currently equipped stones
		/// Should enable equipping currently equipped stones if available stones are less than 4
		/// </summary>
		private void RandomizeStones()
		{
			int[] randomStones = { 0, 1, 2, 3, 4, 5, 6, 7 };
			var equipedStones = Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().GetCurrentShiftStoneConfiguration();
			// shuffle stones list
			randomStones = randomStones.OrderBy(x => random.Next()).ToArray();
			// remove blacklisted stones
			randomStones = randomStones.Except(blackList).ToArray();

			
			if (randomStones.Length < 2)
			{
				MelonLogger.Warning("Not enough stones to randomize, using equipped stones instead.");
				return;
			}
			if ( lockedHand == -1)
			{
				if (randomStones.Length > 4)
					randomStones = randomStones.Except(equipedStones).ToArray();
				else
				{
					MelonLogger.Warning("Too few stones to disallow repeats.");
				}
			}
			else
			{
				if (randomStones.Length > 2)
					randomStones = randomStones.Except(equipedStones).ToArray();
				else
					MelonLogger.Warning("Too few stones to disallow repeats.");
			}

			switch(lockedHand)
			{
				case -1:
					EquipStones(randomStones[0], randomStones[1], equipEffect, true);
					break;
				case 0:
					EquipStones(-1, randomStones[0], equipEffect, false);
					break;
				case 1:
					
					EquipStones(randomStones[0], -1, equipEffect, false);
					break;
			}

			
		}
		/// <summary>
		/// Equips the stones to the player.
		/// </summary>
		/// <param name="leftstone">-1 to not equip shiftstone</param>
		/// <param name="rightstone">-1 to not equip shiftstone</param>
		/// <param name="isempty"></param>
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
		}


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