
using UnityEngine;
using RumbleModdingAPI;
using RumbleModdingAPI.RMAPI;

using System.Collections;

using Il2CppRUMBLE.Interactions.InteractionBase;
using Il2CppRUMBLE.Combat.ShiftStones;
using Il2CppSmartLocalization.Editor;
using MelonLoader;
using Il2CppTMPro;
using UnityEngine.UI;
namespace ShiftStoneManager
{
	public class PortableCabinet : MelonMod
	{
		private static GameObject qssReplacementBase;
		private static GameObject ShiftStoneBoxSource;
		/// <summary>
		/// Static constructor: Only runs once. Make sure no reference to portable cabinet is made before first load
		/// </summary>
		static PortableCabinet()
		{
			qssReplacementBase = GameObject.Instantiate(GameObjects.Gym.INTERACTABLES.Shiftstones.ShiftstoneQuickswapper.FloatingButton.InteractionButtonToggleVariant.GetGameObject());
			qssReplacementBase.transform.SetParent(ShiftStoneManager.DDOLParent.transform, false);
			ShiftStoneBoxSource = GameObject.Instantiate(GameObjects.Gym.INTERACTABLES.Shiftstones.ShiftstoneCabinet.Cabinet.ShiftstoneBox___________.GetGameObject());
			ShiftStoneBoxSource.transform.SetParent(ShiftStoneManager.DDOLParent.transform, false);
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

		private GameObject leftHand;
		private GameObject rightHand;

		public void ShowRandomedHand()
		{
			//Exit if hands arent initialized
			if (leftHand == null || rightHand == null)
				return;
			
			Hands hand = ShiftStoneManager.EnabledHand;
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

		public void CycleHandLock()
		{
			ShiftStoneManager.EnabledHand++;
			if ((int)ShiftStoneManager.EnabledHand > 1)
			{
				ShiftStoneManager.EnabledHand = (Hands)(-1);
			}
			Hands hand = (Hands)ShiftStoneManager.EnabledHand;
			Debug.Log($"Hand lock set to: {hand}", true);

			switch (hand)
			{
				case Hands.Both:
					ShiftStoneManager.ActivateEffect(true, true);
					break;
				case Hands.Left:
					ShiftStoneManager.ActivateEffect(true, false);
					break;
				case Hands.Right:
					ShiftStoneManager.ActivateEffect(false, true);
					break;
			}

			ShiftStoneManager.PrefEnabledHand.Value = ShiftStoneManager.EnabledHand.ToString();
			ShiftStoneManager.CatSettings.SaveToFile();

			ShowRandomedHand();
		}


		public PortableCabinet(GameObject swapper)
		{
			GameObject Button = swapper.transform.GetChild(0).GetChild(2).gameObject;
			Button.SetActive(false);

			//Create loadout interactors
			loadInteractor = new LoadoutInteractor(false);
			LoadCluster = loadInteractor.Cluster;
			LoadCluster.transform.SetParent(swapper.transform, false);
			LoadCluster.transform.localPosition = new Vector3(0.144f, 0.53f, 0f);
			LoadCluster.transform.localRotation = Quaternion.Euler(0, 180, 0);
			LoadCluster.SetActive(true);

			qssSaveInteractor = new LoadoutInteractor(true);
			QssSaveCluster = qssSaveInteractor.Cluster;
			QssSaveCluster.transform.SetParent(swapper.transform, false);
			QssSaveCluster.transform.localPosition = new Vector3(0.144f, 0.77f, 0.055f);
			QssSaveCluster.transform.localRotation = Quaternion.Euler(0, 180, 0);
			QssSaveCluster.SetActive(false);

			

			//TODO: test clear and copy buttons
			GameObject auxPanel = new GameObject("Aux Buttons");
			auxPanel.transform.SetParent(swapper.transform, false);
			auxPanel.transform.localPosition = new Vector3(0.385f, 0.36f, -0.06f);


			GameObject automationButton = GameObject.Instantiate(LoadoutInteractor.ButtonSource);
			GameObject automationButtonLabel = Create.NewText($"Auto Mode\n{ShiftStoneManager.AutomationMode.ToString()}", 0.2f, Color.white, new Vector3(0.0f, 0.0f, 0f), Quaternion.Euler(0, 0, 0));
			automationButtonLabel.name = "AutomationLabel";
			automationButtonLabel.GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.Center;
			automationButtonLabel.transform.SetParent(automationButton.transform, false);
			automationButtonLabel.transform.localRotation = Quaternion.Euler(90, 270, 0);
			automationButtonLabel.transform.localPosition = new Vector3(0.05f, 0.09f, 0f);

			automationButton.name = "Auto Enable";
			automationButton.transform.localPosition = new Vector3(0.04f, 0.11f, -0.01f);
			automationButton.transform.localRotation = Quaternion.Euler(0, 270, 270);
			automationButton.transform.SetParent(auxPanel.transform, false);
			automationButton.SetActive(true);
			automationButton.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().enabled = true;
			automationButton.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().onPressed.AddListener((System.Action)delegate
			{
				int nextAutomationMode = (int)ShiftStoneManager.AutomationMode + 1;
				if (nextAutomationMode > (int)AutomationPrefs.Mirror)
				{
					ShiftStoneManager.AutomationMode = AutomationPrefs.None;
				}
				else
				{
					ShiftStoneManager.AutomationMode = (AutomationPrefs)nextAutomationMode;
				}
				ShiftStoneManager.PrefAutomation.Value = ShiftStoneManager.AutomationMode.ToString();
				ShiftStoneManager.CatSettings.SaveToFile();
				Debug.PrintInGame($"Automation Mode: {LoadoutInteractor.AutomationMode}");

				automationButtonLabel.GetComponent<TextMeshPro>().text = $"Auto Mode\n{ShiftStoneManager.AutomationMode.ToString()}";


			});

			GameObject clearButton = GameObject.Instantiate(LoadoutInteractor.ButtonSource);
			GameObject clearLabel = Create.NewText($"Clear Stones", 0.2f, Color.white, new Vector3(0.0f, 0.0f, 0f), Quaternion.Euler(0, 0, 0));

			clearLabel.name = "Clear Label";
			clearLabel.GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.Center;
			clearLabel.transform.SetParent(clearButton.transform, false);
			clearLabel.transform.localRotation = Quaternion.Euler(90, 270, 0);
			clearLabel.transform.localPosition = new Vector3(0.05f, 0.09f, 0f);


			clearButton.name = "Clear Shift Stones";
			clearButton.transform.localPosition = new Vector3(-0.04f, 0.11f, -0.01f);
			clearButton.SetActive(true);
			clearButton.transform.localRotation = Quaternion.Euler(0, 270, 270);
			clearButton.transform.SetParent(auxPanel.transform, false);
			clearButton.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().enabled = true;
			clearButton.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().onPressed.AddListener((System.Action)delegate
			{
				ShiftStoneManager.EquipStones(new StoneItem(), new StoneItem());

			});

			GameObject copyButton = GameObject.Instantiate(LoadoutInteractor.ButtonSource);
			GameObject copyLabel = Create.NewText($"Copy Stones", 0.2f, Color.white, new Vector3(0.0f, 0.0f, 0f), Quaternion.Euler(0, 0, 0));

			copyLabel.name = "Copy Label";
			copyLabel.GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.Center;
			copyLabel.transform.SetParent(copyButton.transform, false);
			copyLabel.transform.localRotation = Quaternion.Euler(90, 270, 0);
			copyLabel.transform.localPosition = new Vector3(0.05f, 0.09f, 0f);

			copyButton.name = "Mirror Shift Stones";
			copyButton.transform.localPosition = new Vector3(0.04f, 0.22f, -0.01f);
			copyButton.transform.localRotation = Quaternion.Euler(0, 270, 270);
			copyButton.transform.SetParent(auxPanel.transform, false);
			copyButton.SetActive(true);
			copyButton.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().enabled = true;
			copyButton.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().onPressed.AddListener((System.Action)delegate
			{

				if (ShiftStoneManager.Player1 != null)
				{
					int[] opponentEquipped = ShiftStoneManager.Player1.GetComponent<PlayerShiftstoneSystem>().GetCurrentShiftStoneConfiguration();

					ShiftStoneManager.EquipStones(opponentEquipped[0] == -1 ? new StoneItem() : StoneItem.AllStones[opponentEquipped[0]],
													 opponentEquipped[1] == -1 ? new StoneItem() : StoneItem.AllStones[opponentEquipped[1]]);

				}
			});

			GameObject blackListButton = GameObject.Instantiate(LoadoutInteractor.ButtonSource);
			GameObject blackListButtonLabel = Create.NewText("Blacklist", 0.2f, Color.white, new Vector3(0.0f, 0.0f, 0f), Quaternion.Euler(0, 0, 0));
			blackListButtonLabel.name = "BlackListLabel";
			blackListButtonLabel.GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.Center;
			blackListButtonLabel.transform.SetParent(blackListButton.transform, false);
			blackListButtonLabel.transform.localRotation = Quaternion.Euler(90, 270, 0);
			blackListButtonLabel.transform.localPosition = new Vector3(0.05f, 0.09f, 0f);

			blackListButton.name = "BlackList";
			blackListButton.transform.localPosition = new Vector3(-0.04f, 0.22f, -0.01f);
			blackListButton.transform.localRotation = Quaternion.Euler(0, 270, 270);
			blackListButton.transform.SetParent(auxPanel.transform, false);
			blackListButton.SetActive(true);
			blackListButton.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().enabled = true;
			blackListButton.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().onPressed.AddListener((System.Action)delegate
			{
				ShiftStoneManager.ToggleStones(ShiftStoneManager.Player0.GetComponent<PlayerShiftstoneSystem>().GetCurrentShiftStoneConfiguration());
				ShiftStoneManager.ActivateEffect(true, true);
			});


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
				box.transform.GetChild(0).gameObject.GetComponent<LocalizedTextTMPro>().enabled = false;
				box.transform.GetChild(0).gameObject.GetComponent<TextMeshPro>().enabled = false;
				box.transform.SetParent(StoneCase.transform, false);

				box.SetActive(true);


				if (i < 8)
				{
					StoneItem.AllStones[i].AddIcon(ShiftStoneManager.CreateBlackListIcons(box));
				}

				GameObject replacementText = Create.NewText(currentStonePref.ToString(), 0.2f, Color.white, new Vector3(0.0f, -0.0f, 0f), Quaternion.Euler(0, 0, 0));
				replacementText.name = "ReplacementLabel";
				replacementText.transform.SetParent(box.transform, false);
				//-0.02 -0.04 0.002
				replacementText.transform.localPosition = new Vector3(-0.02f, -0.03f, 0.002f);
				replacementText.transform.localRotation = Quaternion.Euler(0, 90, 0);

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
					InteractionButton ib = box.transform.GetChild(1).gameObject.GetComponent<InteractionButton>();
					ib.enabled = false;
					MelonCoroutines.Start(Debouncer(ib));
				});
				box.transform.GetChild(1).gameObject.GetComponent<InteractionButton>().enabled = true;
				StoneCase.transform.SetParent(swapper.transform, false);
				StoneCase.SetActive(false);
				StoneCase.transform.localPosition = new Vector3(0.3f, 0.6f, 0.0f);
			}


			
			GameObject keepHandButton = GameObject.Instantiate(Button);
			keepHandButton.transform.SetParent(swapper.transform.GetChild(0), false);

			keepHandButton.transform.localPosition = new Vector3(0.1835f, -0.001f, -0.02f);
			keepHandButton.transform.localRotation = Quaternion.Euler(43.108f, 348.0498f, 275.3816f);
			keepHandButton.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().isToggleButton = false;

			keepHandButton.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().onPressed.AddListener((System.Action)delegate
			{
				CycleHandLock();
			});
			keepHandButton.SetActive(true);

			GameObject handHolder = new GameObject("Hand Holder");
			handHolder.transform.SetParent(keepHandButton.transform, false);
			//-0.04 0.25 - 0.16
			handHolder.transform.localPosition = new Vector3(-0.04f, 0.25f, -0.16f);
			//284.9999 315.0001 150
			handHolder.transform.localRotation = Quaternion.Euler(285f, 315f, 150f);

			leftHand = GameObject.Instantiate(ShiftStoneManager.IndicatorsBase.transform.GetChild(2).gameObject);

			rightHand = GameObject.Instantiate(ShiftStoneManager.IndicatorsBase.transform.GetChild(1).gameObject);

			leftHand.transform.SetParent(handHolder.transform, false);
			//-0.18 0.01 0.05
			leftHand.transform.localPosition = new Vector3(-0.18f, 0.01f, 0.05f);
			//90 45 0
			leftHand.transform.localRotation = Quaternion.Euler(90f, 45f, 0f);
			leftHand.transform.localScale = new Vector3(0.0003f, 0.0003f, 0.0003f);

			rightHand.transform.SetParent(handHolder.transform, false);
			//-0.14 0.01 - 0.07
			rightHand.transform.localPosition = new Vector3(-0.14f, 0.01f, -0.07f);
			//90 105 0
			rightHand.transform.localRotation = Quaternion.Euler(90f, 105f, 0f);
			rightHand.transform.localScale = new Vector3(0.0003f, 0.0003f, 0.0003f);

			ShowRandomedHand();


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
				blackListButton.SetActive(interaction.IsPressed);

				loadInteractor.Cluster.SetActive(!interaction.IsPressed);

				auxPanel.transform.localPosition = interaction.IsPressed ? new Vector3(0.385f, 0.6f, -0.005f) : new Vector3(0.385f, 0.36f, -0.06f);

				LoadoutInteractor.ClearAllSlots();
				LoadoutInteractor.HighlightCurrentEquippedStones();
			});



		}
		Hands FindCulprit(GameObject selectedButton)
		{
			float leftDist = Vector3.Distance(ShiftStoneManager.leftPoint.transform.position, selectedButton.transform.position);
			float rightDist = Vector3.Distance(ShiftStoneManager.rightPoint.transform.position, selectedButton.transform.position);

			Debug.Log($"LeftDist: {leftDist} RightDist: {rightDist}", true);

			return leftDist < rightDist ? Hands.Left : Hands.Right;
		}

		private IEnumerator Debouncer(InteractionButton ib)
		{
			yield return new WaitForSeconds(0.5f);
			ib.enabled = true;
		}
	}
}