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
				//SaveLoadOut(Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().GetCurrentShiftStoneConfiguration());
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
				ApplyLoadOut();
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