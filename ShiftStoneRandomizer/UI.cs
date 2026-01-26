
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
	/// <summary>
	/// 
	/// </summary>
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
		/// <summary>
		/// 
		/// </summary>
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

		private void CreateQuickSwapButtons(GameObject swapper)
		{
			GameObject Button = swapper.transform.GetChild(0).GetChild(2).gameObject;

			//RandomizedHandButton
			/*GameObject keepHandButton = GameObject.Instantiate(Button);
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

			ShowRandomedHand();
			*/
			GameObject RandomButton = GameObject.Instantiate(Button);
			RandomButton.transform.parent = swapper.transform.GetChild(0);
			RandomButton.transform.localPosition = new Vector3(-0.096f, 0.064f, -0.025f);
			RandomButton.transform.localRotation = Quaternion.Euler(298.0022f, 83.3369f, 359.8999f);
			RandomButton.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().isToggleButton = false;
			
			RandomButton.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().onPressed.AddListener((System.Action)delegate
			{
				RandomizeStones(Player0.GetComponent<PlayerShiftstoneSystem>().GetCurrentShiftStoneConfiguration(), EnabledHand);
			});
			

			PortableCabinet portaStones = new PortableCabinet(swapper);

			Button.SetActive(false);
		}

		/// <summary>
		/// 
		/// </summary>
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