
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

		#endregion
	}
}