
using Il2CppTMPro;
using MelonLoader;
using Il2CppRUMBLE.Combat.ShiftStones;
using Il2CppRUMBLE.Interactions.InteractionBase;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using RumbleModdingAPI;
using System.Linq;

namespace ShiftStoneRandomizer
{

	public partial class ShiftStoneRandomizer : MelonMod
	{
		#region UI

		public static GameObject SmallButtonSource;
		GameObject LoadCluster;
		GameObject SaveCluster;
		public GameObject QssSaveCluster;

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
					Debug.Log("-------", false, 1);
					Debug.Log($"Error creating quickswap buttons: {e}", false, 2);
					Debug.Log("-------", false, 1);
				}
			}
		}
		/// <summary>
		/// Create blacklist icons for each stone in the blacklist. Game object is disabled by default
		/// </summary>
		/// <param name="TargetParent">Box to parent the icons to</param>
		/// <returns>Reference to the blacklist icon for the shiftstone</returns>
		public static GameObject CreateBlackListIcons(GameObject TargetParent)
		{
			System.Random jitter = new System.Random(TargetParent.GetHashCode());

			GameObject blackListIcon = GameObject.Instantiate(IndicatorsBase.transform.GetChild(0).gameObject);
			//GameObject blackListIcon = indicator;//.transform.GetChild(0).gameObject;
			blackListIcon.SetActive(false);
			blackListIcon.transform.SetParent(TargetParent.transform, false);
			blackListIcon.transform.localScale = Vector3.one * 0.000075f;
			blackListIcon.transform.localRotation = Quaternion.Euler(-0f, 90f * jitter.Next(1, 2), (90f * jitter.Next(4)) + jitter.Next(-10, 10));
			blackListIcon.transform.localPosition = new Vector3(-0.053f, -0.02f + jitter.Next(-100, 100) * 0.0001f, 0.02f + jitter.Next(-100, 100) * 0.0001f);// + jitter.Next(-100, 100) * 0.0001f, 0.01f + jitter.Next(-100, 100) * 0.0001f);
			return blackListIcon;
		}

		private void CreatePhysicalGUI()
		{



			// var Button = GameObject.Find("ShiftstoneQuickswapper").transform.GetChild(0).GetChild(2).gameObject;
			// GameObject titleBar = GameObject.Instantiate(Calls.GameObjects.Gym.LOGIC.Heinhouserproducts.MatchConsole.MatchmakingSettings.TitleBar.GetGameObject());
			// GameObject titleText = GameObject.Instantiate(Calls.GameObjects.Gym.LOGIC.Heinhouserproducts.MatchConsole.MatchmakingSettings.TitleText.GetGameObject());

			// saveInteractor = new LoadoutInteractor(true);
			// SaveCluster = saveInteractor.Cluster;
			// SaveCluster.transform.SetParent(GameObject.Find("ShiftstoneCabinet").transform, false);
			// SaveCluster.transform.localPosition = new Vector3(-0.07f, 1.334f, -1.016f);
			// SaveCluster.transform.localRotation = Quaternion.Euler(0, 90, 0);
			// SaveCluster.SetActive(true);


			// titleText.transform.localRotation = Quaternion.Euler(0f, 90f, 90f);
			// titleText.transform.localPosition = new Vector3(-0.09f, 0.736f, 0.0002f);
			// titleText.transform.localScale = new Vector3(6.8f, 4.9f, 1f);
			// titleText.transform.SetParent(titleBar.transform, false);

			// GameObject blackListButton = GameObject.Instantiate(Button);
			// blackListButton.transform.parent = GameObject.Find("ShiftstoneCabinet").transform;
			// blackListButton.transform.localPosition = new Vector3(-0.0509f, 1.6473f, 0.6836f);
			// blackListButton.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
			// blackListButton.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().isToggleButton = false;

			// GameObject blackListLabel = GameObject.Instantiate(titleBar);
			// blackListLabel.transform.SetParent(blackListButton.transform, false);
			// blackListLabel.transform.localScale = new Vector3(0.09f, 0.3f, 0.3f);
			// blackListLabel.transform.localPosition = new Vector3(0.118f, 0f, 0.212f);
			// blackListLabel.transform.localRotation = Quaternion.Euler(0.3f, 83.491f, 275.6874f);
			// blackListLabel.transform.GetChild(0).gameObject.GetComponent<TextMeshPro>().text = "Blacklist";


			// GameObject keepHandButton = GameObject.Instantiate(Button);
			// keepHandButton.transform.parent = GameObject.Find("ShiftstoneCabinet").transform;
			// keepHandButton.transform.localPosition = new Vector3(-0.0509f, 1.3537f, 0.6836f);
			// keepHandButton.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
			// keepHandButton.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().isToggleButton = false;

			// GameObject keepHandLabel = GameObject.Instantiate(titleBar);
			// keepHandLabel.transform.SetParent(keepHandButton.transform, false);
			// keepHandLabel.transform.localScale = new Vector3(0.09f, 0.3f, 0.3f);
			// keepHandLabel.transform.localPosition = new Vector3(0.168f, 0.02f, 0.212f);
			// keepHandLabel.transform.localRotation = Quaternion.Euler(1.9346f, 97f, 268.269f);
			// keepHandLabel.transform.GetChild(0).gameObject.GetComponent<TextMeshPro>().text = "Lock Hand";

			/*leftHand = GameObject.Instantiate(IndicatorsBase.transform.GetChild(2).gameObject);

			rightHand = GameObject.Instantiate(IndicatorsBase.transform.GetChild(1).gameObject);

			leftHand.transform.SetParent(keepHandButton.transform.GetChild(0), false);
			leftHand.transform.localPosition = new Vector3(-0.14f, 0.01f, 0.07f);
			leftHand.transform.localRotation = Quaternion.Euler(90f, 90f, 0f);
			leftHand.transform.localScale = new Vector3(0.0003f, 0.0003f, 0.0003f);

			rightHand.transform.SetParent(keepHandButton.transform.GetChild(0), false);
			rightHand.transform.localPosition = new Vector3(-0.14f, 0.01f, -0.07f);
			rightHand.transform.localRotation = Quaternion.Euler(90f, 90f, 0f);
			rightHand.transform.localScale = new Vector3(0.0003f, 0.0003f, 0.0003f);*/



			// GameObject saveLoadOutButton = GameObject.Instantiate(Button);
			// saveLoadOutButton.transform.parent = GameObject.Find("ShiftstoneCabinet").transform;
			// saveLoadOutButton.transform.localPosition = new Vector3(-0.0509f, 1.6473f, -1.0164f);
			// saveLoadOutButton.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
			// saveLoadOutButton.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().isToggleButton = false;

			// GameObject saveLoadOutLabel = GameObject.Instantiate(titleBar);
			// saveLoadOutLabel.transform.SetParent(saveLoadOutButton.transform, false);
			// saveLoadOutLabel.transform.localPosition = new Vector3(0.128f, 0.025f, 0.212f);
			// saveLoadOutLabel.transform.localScale = new Vector3(0.09f, 0.3f, 0.3f);
			// saveLoadOutLabel.transform.localRotation = Quaternion.Euler(5.0254f, 87.0001f, 271.7236f);
			// saveLoadOutLabel.transform.GetChild(0).gameObject.GetComponent<TextMeshPro>().text = "Loadout";

			// GameObject saveLabel = GameObject.Instantiate(titleBar);
			// saveLabel.transform.SetParent(saveLoadOutButton.transform.GetChild(0), false);
			// saveLabel.transform.localPosition = new Vector3(0.208f, -0.05f, 0.212f);
			// saveLabel.transform.localScale = new Vector3(0.09f, 0.2f, 0.3f);
			// saveLabel.transform.localRotation = Quaternion.Euler(5.0254f, 82.3274f, 274.8562f);
			// saveLabel.transform.GetChild(0).gameObject.GetComponent<TextMeshPro>().text = "Save";
			// saveLabel.transform.GetChild(0).gameObject.transform.localScale = new Vector3(9.7f, 4.9f, 1f);

			// dropSign = saveLabel;


			// blackListButton.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().onPressed.AddListener((System.Action)delegate
			// {
			// 	ToggleStones(Player0.GetComponent<PlayerShiftstoneSystem>().GetCurrentShiftStoneConfiguration());
			// 	ActivateEffect(true, true);
			// });

			// keepHandButton.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().onPressed.AddListener((System.Action)delegate
			// {
			// 	CycleHandLock();
			// });

			// saveLoadOutButton.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().onPressed.AddListener((System.Action)delegate
			// {
			// 	SaveLoadOut(Player0.GetComponent<PlayerShiftstoneSystem>().GetCurrentShiftStoneConfiguration());
			// });

		}

		private void CreateQuickSwapButtons(GameObject swapper)
		{
			GameObject Button = swapper.transform.GetChild(0).GetChild(2).gameObject;

			//RandomizedHandButton
			GameObject keepHandButton = GameObject.Instantiate(Button);
			keepHandButton.transform.SetParent(swapper.transform.GetChild(0), false);

			keepHandButton.transform.localPosition = new Vector3(0.1835f, -0.001f, -0.02f);
			keepHandButton.transform.localRotation = Quaternion.Euler(43.108f, 348.0498f, 275.3816f);
			keepHandButton.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().isToggleButton = false;

			keepHandButton.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().onPressed.AddListener((System.Action)delegate
			{
				ShiftStoneRandomizer.Instance.CycleHandLock();
			});

			GameObject handHolder = new GameObject("Hand Holder");
			handHolder.transform.SetParent(keepHandButton.transform, false);
			//-0.04 0.25 - 0.16
			handHolder.transform.localPosition = new Vector3(-0.04f, 0.25f, -0.16f);
			//284.9999 315.0001 150
			handHolder.transform.localRotation = Quaternion.Euler(285f, 315f, 150f);

			leftHand = GameObject.Instantiate(ShiftStoneRandomizer.IndicatorsBase.transform.GetChild(2).gameObject);

			rightHand = GameObject.Instantiate(ShiftStoneRandomizer.IndicatorsBase.transform.GetChild(1).gameObject);

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

			GameObject RandomButton = GameObject.Instantiate(Button);
			RandomButton.transform.parent = swapper.transform.GetChild(0);
			RandomButton.transform.localPosition = new Vector3(-0.096f, 0.064f, -0.025f);
			RandomButton.transform.localRotation = Quaternion.Euler(298.0022f, 83.3369f, 359.8999f);
			RandomButton.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().isToggleButton = false;

			RandomButton.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().onPressed.AddListener((System.Action)delegate
			{
				RandomizeStones(Player0.GetComponent<PlayerShiftstoneSystem>().GetCurrentShiftStoneConfiguration(), EnabledHand);
			});


			//Stone clear button
			// GameObject Button3 = swapper.transform.GetChild(0).GetChild(2).gameObject;
			// GameObject ClearStones = GameObject.Instantiate(Button3);
			// ClearStones.transform.parent = swapper.transform.GetChild(0);
			// ClearStones.transform.localPosition = new Vector3(0.0531f, 0.054f, -0.1023f);
			// ClearStones.transform.localRotation = Quaternion.Euler(292.25f, 6.781f, 350.6181f);
			// ClearStones.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().isToggleButton = false;
			// ClearStones.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().onPressed.AddListener((System.Action)delegate
			// {
			// 	EquipStones(new StoneItem(), new StoneItem());
			// });

			PortableCabinet portaStones = new PortableCabinet(swapper);

			Button.SetActive(false);
		}


		private void GrabBoxSource()
		{

			ShiftStoneBoxSource = GameObject.Instantiate(Calls.GameObjects.Gym.LOGIC.Heinhouserproducts.ShiftstoneCabinet.Cabinet.ShiftstoneBox___________.GetGameObject());
			ShiftStoneBoxSource.SetActive(false);
			ShiftStoneBoxSource.name = "ShiftStoneBoxSource";
			ShiftStoneBoxSource.transform.SetParent(DDOLParent.transform, false);
			SmallButtonSource = GameObject.Instantiate(Calls.GameObjects.Gym.LOGIC.Heinhouserproducts.Telephone20REDUXspecialedition.FriendScreen.FriendScrollBar.PageDownButton.GetGameObject());
		}

		private void GrabTemplates()
		{

		}

		#endregion
		#region UI Logic
		Hands FindCulprit(GameObject selectedButton)
		{
			float leftDist = Vector3.Distance(leftPoint.transform.position, selectedButton.transform.position);
			float rightDist = Vector3.Distance(rightPoint.transform.position, selectedButton.transform.position);

			Debug.Log($"LeftDist: {leftDist} RightDist: {rightDist}", true);

			return leftDist < rightDist ? Hands.Left : Hands.Right;
		}


		#endregion
	}
}