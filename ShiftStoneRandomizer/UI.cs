using AsmResolver.PE.DotNet.Cil;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppInterop.Runtime.Runtime.VersionSpecific.Class;
using Il2CppPhoton.Realtime;
using Il2CppRootMotion;
using Il2CppRUMBLE.CharacterCreation.Interactable;

using Il2CppRUMBLE.Managers;
using Il2CppRUMBLE.Players.Subsystems;
using Il2CppSystem;
using Il2CppSystem.Data;

using MelonLoader.TinyJSON;
using MelonLoader.Utils;

using System.Collections;

using System.IO;

using UnityEditor;

using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Action = System.Action;
using Type = Il2CppSystem.Type;
using Il2CppTMPro;
using MelonLoader;
using Il2CppRUMBLE.Combat.ShiftStones;
using Il2CppRUMBLE.Interactions.InteractionBase;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using RumbleModdingAPI;
using System.Collections.Generic;
using System.Linq;

namespace ShiftStoneRandomizer
{

	public partial class ShiftStoneRandomizer : MelonMod
	{
		#region UI


		GameObject LoadCluster;
		GameObject SaveCluster;
		GameObject QssSaveCluster;

		LoadoutInteractor saveInteractor;
		LoadoutInteractor loadInteractor;
		LoadoutInteractor qssSaveInteractor;

		GameObject ShiftStoneBoxSource;

		GameObject PortableStoneCase;
		bool isQssReplacementPressed = false;


		//Legacy code from Darkener. Need optimization. 
		private void CreateButtonsForAll()
		{
			var swappers = GameObject.FindObjectsOfType<GameObject>().Where(go => go.name == "ShiftstoneQuickswapper").ToArray();
			foreach (var swapper in swappers)
			{

				try
				{
					CreateQuickSwapButtons(swapper);
				}
				catch (System.Exception e)
				{
					Debug.Log($"Error creating quickswap buttons: {e}", true, 2);
				}
			}
		}
		/// <summary>
		/// Create blacklist icons for each stone in the blacklist. Game object is disabled by default
		/// </summary>
		/// <param name="TargetParent">Box to parent the icons to</param>
		/// <returns>Reference to the blacklist icon for the shiftstone</returns>
		private GameObject CreateBlackListIcons(GameObject TargetParent)
		{
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

			//-0.07 1.334 -1.016
			//-0 90 0
			saveInteractor = new LoadoutInteractor(true);
			SaveCluster = saveInteractor.Cluster;
			SaveCluster.transform.SetParent(GameObject.Find("ShiftstoneCabinet").transform, false);
			SaveCluster.transform.localPosition = new Vector3(-0.07f, 1.334f, -1.016f);
			SaveCluster.transform.localRotation = Quaternion.Euler(0, 90, 0);
			SaveCluster.SetActive(true);


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


			blackListButton.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().onPressed.AddListener((System.Action)delegate
			{
				ToggleStones(Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().GetCurrentShiftStoneConfiguration());
				ActivateEffect(true, true);
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

		private void CreateQuickSwapButtons(GameObject swapper)
		{
			isQssReplacementPressed = false;
			GameObject Button = swapper.transform.GetChild(0).GetChild(2).gameObject;

			loadInteractor = new LoadoutInteractor(false);
			LoadCluster = loadInteractor.Cluster;
			LoadCluster.transform.SetParent(swapper.transform, false);
			LoadCluster.transform.localPosition = new Vector3(0.144f, 0.42f, 0f);
			LoadCluster.transform.localRotation = Quaternion.Euler(0, 180, 0);
			LoadCluster.SetActive(true);

			qssSaveInteractor = new LoadoutInteractor(true);
			QssSaveCluster = qssSaveInteractor.Cluster;
			QssSaveCluster.transform.SetParent(swapper.transform, false);
			QssSaveCluster.transform.localPosition = new Vector3(0.144f, 0.72f, 0f);
			QssSaveCluster.transform.localRotation = Quaternion.Euler(0, 180, 0);
			QssSaveCluster.SetActive(true);

			GameObject RandomButton = GameObject.Instantiate(Button);
			RandomButton.transform.parent = swapper.transform.GetChild(0);
			RandomButton.transform.localPosition = new Vector3(-0.096f, 0.064f, -0.025f);
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

			
			BuildPortableCase(swapper);
			
			//Replace default qss button with custom button that pulls up the portable stone case instead of the qss tablets 
			
			Button.SetActive(false);
			GameObject qssReplacement = GameObject.Instantiate(Button);
			//0.0131 0.0179 0.0577
			qssReplacement.transform.localPosition = new Vector3(0.0131f, 0.0179f, 0.0577f);
			qssReplacement.transform.SetParent(swapper.transform.GetChild(0), false);
			qssReplacement.SetActive(true);
			InteractionButton qssButton = qssReplacement.transform.GetChild(0).gameObject.GetComponent<InteractionButton>();
			//qssButton.IsToggleButton = false;
			qssReplacement.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().onToggleStateChanged.AddListener((System.Action<bool>)delegate (bool isOn)
			{
				Debug.Log("Pressed", true);
				PortableStoneCase.SetActive(qssButton.IsPressed);
				QssSaveCluster.SetActive(qssButton.IsPressed);
			
				loadInteractor.Cluster.SetActive(!qssButton.IsPressed);
				
			});
		}

		
		private void GrabBoxSource()
		{
			
			ShiftStoneBoxSource = GameObject.Instantiate(Calls.GameObjects.Gym.LOGIC.Heinhouserproducts.ShiftstoneCabinet.Cabinet.ShiftstoneBox___________.GetGameObject());
			ShiftStoneBoxSource.SetActive(false);
			ShiftStoneBoxSource.name = "ShiftStoneBoxSource";
			GameObject.DontDestroyOnLoad(ShiftStoneBoxSource);
		}
		private void BuildPortableCase(GameObject swapper)
		{

			double mulCol = 0.1;
			double mulRow = 0.12;
			PortableStoneCase = new GameObject("PortableStoneCase");
			for (int i = 0; i < 12; i++)
			{
				//Standard shift stones correspond to 0 to 7 but control stones correspond to -4 to -1
				ShiftStonePrefs currentStoneItem = i < 8 ? (ShiftStonePrefs)i : (ShiftStonePrefs)(i - 12);

				//Create shift stone box
				GameObject box = GameObject.Instantiate(ShiftStoneBoxSource);
				box.transform.SetParent(PortableStoneCase.transform, false);
				box.name = $"{currentStoneItem.ToString()}_Case";
				box.transform.localPosition = new Vector3((float)(mulCol * (i % 4) * -1), (float)(mulRow * (Math.Floor(i / 4) * -1)), 0.07f);
				box.transform.rotation = Quaternion.Euler(0, 90, 0);
				box.transform.GetChild(0).localPosition = new Vector3(-0.03f, -0.04f, -0f);
				box.transform.SetParent(PortableStoneCase.transform);
				box.SetActive(true);

				//Create the blacklist icon for the standard shiftstones and bind them to their respective stoneitem by adding it to the stoneItem's icon list and parent it to box
				if(i < 8)
				{
					StoneItem.AllStones[i].AddIcon(CreateBlackListIcons(box));
				}


				GameObject boxDisplay = StoneItem.GetDisplayObject(currentStoneItem);
				//Standard shift stones should be rotated on their broad face to show off their outline. But charge's outline is clearer from the side				
				if (currentStoneItem >= 0 && currentStoneItem != ShiftStonePrefs.Charge)
				{
					boxDisplay.transform.rotation = Quaternion.Euler(0, 0, 90);
				}

				//The current placeholder icons for control stones are best displayed rotated this way
				else if (currentStoneItem < 0)
				{
					boxDisplay.transform.rotation = Quaternion.Euler(0, 90, 0);
				}
				boxDisplay.transform.SetParent(box.transform, false);


				boxDisplay.SetActive(true);

				
				box.transform.GetChild(1).gameObject.GetComponent<InteractionButton>().onPressed.AddListener((System.Action)delegate
				{
					SelectStoneItem(currentStoneItem);
				});
				box.transform.GetChild(1).gameObject.GetComponent<InteractionButton>().enabled = true;
			}

			PortableStoneCase.transform.SetParent(swapper.transform, false);
			PortableStoneCase.SetActive(true);
			PortableStoneCase.transform.localPosition = new Vector3(0.3f, 0.6f, 0.0f);
			
		}

		#endregion






		#region Easter Egg
		#endregion
	}
}