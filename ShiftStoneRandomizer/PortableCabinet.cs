using Il2CppTMPro;
using MelonLoader;
using Il2CppRUMBLE.Combat.ShiftStones;
using Il2CppRUMBLE.Interactions.InteractionBase;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using RumbleModdingAPI;
using System.Collections.Generic;
using System.Linq;
using System;
using Il2CppRUMBLE.Managers;
using UnityEngine.Bindings;

namespace ShiftStoneRandomizer
{
	public class PortableCabinet
	{
		private static GameObject qssReplacementBase;
		private static GameObject ShiftStoneBoxSource;
		/// <summary>
		/// Static constructor: Only runs once. Make sure no reference to portable cabinet is made before first load
		/// </summary>
		static PortableCabinet()
		{
			qssReplacementBase = GameObject.Instantiate(Calls.GameObjects.Gym.LOGIC.Heinhouserproducts.ShiftstoneQuickswapper.FloatingButton.InteractionButtonToggleVariant.GetGameObject());
			GameObject.DontDestroyOnLoad(qssReplacementBase);
			ShiftStoneBoxSource = GameObject.Instantiate(Calls.GameObjects.Gym.LOGIC.Heinhouserproducts.ShiftstoneCabinet.Cabinet.ShiftstoneBox___________.GetGameObject());
			GameObject.DontDestroyOnLoad(ShiftStoneBoxSource);
			qssReplacementBase.SetActive(false);
			ShiftStoneBoxSource.SetActive(false);
		}


		private GameObject Cubbies { get; set; }
		public GameObject StoneCase { get; private set; }
		private GameObject NewQssButton { get; set; }

		private LoadoutInteractor loadInteractor;
		private GameObject LoadCluster;

		private LoadoutInteractor qssSaveInteractor;
		private GameObject QssSaveCluster;



		public PortableCabinet(GameObject swapper)
		{
			GameObject Button = swapper.transform.GetChild(0).GetChild(2).gameObject;
			Button.SetActive(false);
			//Randomizer Button
			// GameObject RandomButton = GameObject.Instantiate(Button);
			// RandomButton.transform.SetParent(swapper.transform.GetChild(0));
			// RandomButton.transform.localPosition = new Vector3(-0.096f, 0.064f, -0.025f);
			// RandomButton.transform.localRotation = Quaternion.Euler(298.0022f, 83.3369f, 359.8999f);
			// RandomButton.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().isToggleButton = false;

			// RandomButton.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().onPressed.AddListener((System.Action)delegate
			// {
			// 	ShiftStoneRandomizer.RandomizeStones(Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().GetCurrentShiftStoneConfiguration());
			// });






			//Create loadout interactors
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
			QssSaveCluster.SetActive(false);


			//Create Portable Stone Case
			double mulCol = 0.1; double mulRow = 0.12;
			StoneCase = new GameObject("Portable Stone Case");

			for (int i = 0; i < 12; i++)
			{
				ShiftStonePrefs currentStonePref = i < 8 ? (ShiftStonePrefs)i : (ShiftStonePrefs)(i - 12);

				GameObject box = GameObject.Instantiate(ShiftStoneBoxSource);
				box.name = $"{currentStonePref.ToString()}_Case";
				box.transform.localPosition = new Vector3((float)(mulCol * (i % 4) * -1), (float)(mulRow * (i / 4) * -1), 0.07f);
				box.transform.rotation = Quaternion.Euler(0, 90, 0);
				box.transform.GetChild(0).localPosition = new Vector3(-0.03f, -0.04f, -0f);
				box.transform.SetParent(StoneCase.transform, false);
				box.SetActive(true);

				if (i < 8)
				{
					StoneItem.AllStones[i].AddIcon(ShiftStoneRandomizer.CreateBlackListIcons(box));
				}

				GameObject boxDisplay = StoneItem.GetDisplayObject(currentStonePref);
				//Standard shift stones should be rotated on their broad face to show off their outline. But charge's outline is clearer from the side				
				if (currentStonePref >= 0 && currentStonePref != ShiftStonePrefs.Charge)
				{
					boxDisplay.transform.rotation = Quaternion.Euler(0, 0, 90);
				}
				//The current placeholder icons for control stones are best displayed rotated this way
				else if (currentStonePref < 0)
				{
					boxDisplay.transform.rotation = Quaternion.Euler(0, 90, 0);
				}

				boxDisplay.transform.SetParent(box.transform, false);

				boxDisplay.SetActive(true);

				box.transform.GetChild(1).gameObject.GetComponent<InteractionButton>().onPressed.AddListener((System.Action)delegate
				{
					Hands usedHand = FindCulprit(box);

					LoadoutInteractor.HighlightItem(currentStonePref, usedHand);
				});
				box.transform.GetChild(1).gameObject.GetComponent<InteractionButton>().enabled = true;
				StoneCase.transform.SetParent(swapper.transform, false);
				StoneCase.SetActive(false);
				StoneCase.transform.localPosition = new Vector3(0.3f, 0.6f, 0.0f);
			}


			//Create replacement swapper button
			NewQssButton = GameObject.Instantiate(qssReplacementBase);
			//TODO: Disable original swapper button
			NewQssButton.transform.localPosition = new Vector3(0.0131f, 0.0179f, 0.0577f);
			NewQssButton.transform.SetParent(swapper.transform.GetChild(0), false);
			NewQssButton.SetActive(true);
			InteractionButton interaction = NewQssButton.transform.GetChild(0).gameObject.GetComponent<InteractionButton>();
			NewQssButton.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().onToggleStateChanged.AddListener((System.Action<bool>)delegate (bool isOn)
			{
				Debug.Log("Pressed", true);
				StoneCase.SetActive(interaction.IsPressed);
				QssSaveCluster.SetActive(interaction.IsPressed);

				loadInteractor.Cluster.SetActive(!interaction.IsPressed);

			});



		}
		Hands FindCulprit(GameObject selectedButton)
		{
			float leftDist = Vector3.Distance(ShiftStoneRandomizer.leftPoint.transform.position, selectedButton.transform.position);
			float rightDist = Vector3.Distance(ShiftStoneRandomizer.rightPoint.transform.position, selectedButton.transform.position);

			Debug.Log($"LeftDist: {leftDist} RightDist: {rightDist}", true);

			return leftDist < rightDist ? Hands.Left : Hands.Right;
		}


	}
}