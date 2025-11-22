using Il2CppTMPro;
using MelonLoader;
using Il2CppRUMBLE.Combat.ShiftStones;
using Il2CppRUMBLE.Interactions.InteractionBase;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using RumbleModdingAPI;
using System.Collections.Generic;
using System.Linq;
using RumbleModdingAPI;

namespace ShiftStoneRandomizer
{
	internal class LoadoutInteractor
	{
		public class Slot
		{

			public GameObject Button { get; set; }


			private GameObject _leftStoneSlot = new GameObject();
			public GameObject LeftStoneSlot { get { return _leftStoneSlot; } }

			private StoneItem _leftStoneItem;
			public StoneItem LeftStoneItem
			{
				get { return _leftStoneItem; }
				set
				{
					_leftStoneItem = value;
					GameObject miniStone = GameObject.Instantiate(value.ShiftStone.gameObject);
					miniStone.transform.SetParent(_leftStoneSlot.transform, false);

					if (value.Name == "Charge")
					{
						miniStone.transform.localRotation = Quaternion.Euler(0f, 0f, 270f);
					}
					else
					{
						miniStone.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
					}
				}
			}
			public MelonPreferences_Entry<string> LeftHandPref { get; set; }


			private GameObject _rightStoneSlot = new GameObject();
			public GameObject RightStoneSlot { get { return _rightStoneSlot; } }

			private StoneItem _rightStoneItem;
			public StoneItem RightStoneItem
			{
				get { return _rightStoneItem; }
				set
				{
					_rightStoneItem = value;
					GameObject miniStone = GameObject.Instantiate(value.ShiftStone.gameObject);
					miniStone.transform.SetParent(_rightStoneSlot.transform, false);

					if (value.Name == "Charge")
					{
						miniStone.transform.localRotation = Quaternion.Euler(0f, 0f, 270f);
					}
					else
					{
						miniStone.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
					}
				}
			}
			public MelonPreferences_Entry<string> RightHandPref { get; set; }

			private Quadrants _quadrant;
			public Quadrants Quadrant;

			public Slot(Vector3 position, MelonPreferences_Entry<string> leftPref, MelonPreferences_Entry<string> rightPref, bool isSaveButton)
			{
				//Quadrant = quad;
				LeftHandPref = leftPref;
				RightHandPref = rightPref;

				Button = GameObject.Instantiate(ButtonSource);
				Button.name = "Loadout Button";
				Button.transform.localPosition = position;

				_leftStoneSlot.transform.localPosition = new Vector3(0.05f, 0.07f, 0.01f);
				_leftStoneSlot.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
				_leftStoneSlot.transform.SetParent(Button.transform, false);

				_rightStoneSlot.transform.localPosition = new Vector3(0.05f, 0.07f, -0.01f);
				_rightStoneSlot.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
				_rightStoneSlot.transform.SetParent(Button.transform, false);
			}

			public void DisplayShiftStones(ShiftStonePrefs left, ShiftStonePrefs right)
			{
				LeftStoneItem = AllStones.GetStoneItem(left);
				RightStoneItem = AllStones.GetStoneItem(right);
			}


		}

		public enum Quadrant { TopLeft, TopRight, BottomLeft, BottomRight };

		#region static Members
		public enum Quadrants { TopLeft, TopRight, BottomLeft, BottomRight };

		public static GameObject ButtonSource;


		public static GameObject ClusterSource;
		static LoadoutInteractor()
		{
			ButtonSource = GameObject.Instantiate(Calls.GameObjects.Gym.LOGIC.Heinhouserproducts.Telephone20REDUXspecialedition.FriendScreen.FriendScrollBar.PageDownButton.GetGameObject());
			ButtonSource.transform.GetChild(0).GetComponent<InteractionButton>().enabled = true;
			ButtonSource.transform.localRotation = Quaternion.Euler(0f, 270f, 90f);
			ButtonSource.transform.localPosition = Vector3.zero;
			ButtonSource.SetActive(false);
			GameObject.DontDestroyOnLoad(ButtonSource);

			ClusterSource = new GameObject("Loadout Cluster");
			GameObject.DontDestroyOnLoad(ClusterSource);

		}
		#endregion

		public static Vector3[] Sections = new Vector3[]
		{
			new Vector3(-0.04f, 0.06f, 0.07f),
			new Vector3(0.04f, 0.06f, 0.07f),
			new Vector3(-0.04f, -0.06f, 0.07f),
			new Vector3(0.04f, -0.06f, 0.07f),
		};


		private Slot _map0Host;
		public Slot Map0Host { get { return _map0Host; } }

		private Slot _map1Host;
		public Slot Map1Host { get { return _map1Host; } }

		private Slot _map0Client;
		public Slot Map0Client { get { return _map0Client; } }

		private Slot _map1Client;
		public Slot Map1Client { get { return _map1Client; } }

		public readonly List<Slot> SlotList;

		public LoadoutInteractor(bool isForSaving)
		{
			SlotList = new List<Slot>() { Map0Host, Map1Host, Map0Client, Map1Client };
		}
	}

	public static class AllStones
	{
		public static StoneItem[] StoneHandles = new StoneItem[] {
				new StoneItem(Calls.Managers.GetPoolManager().GetPooledObject("AdamantStone").gameObject.GetComponent<UnyieldingStone>()),
				new StoneItem(Calls.Managers.GetPoolManager().GetPooledObject("ChargeStone").gameObject.GetComponent<ChargeStone>()),
				new StoneItem(Calls.Managers.GetPoolManager().GetPooledObject("FlowStone").gameObject.GetComponent<FlowStone>()),
				new StoneItem(Calls.Managers.GetPoolManager().GetPooledObject("GuardStone").gameObject.GetComponent<GuardStone>()),
				new StoneItem(Calls.Managers.GetPoolManager().GetPooledObject("StubbornStone").gameObject.GetComponent<StubbornStone>()),
				new StoneItem(Calls.Managers.GetPoolManager().GetPooledObject("SurgeStone").gameObject.GetComponent<CounterStone>()),
				new StoneItem(Calls.Managers.GetPoolManager().GetPooledObject("VigorStone").gameObject.GetComponent<VigorStone>()),
				new StoneItem(Calls.Managers.GetPoolManager().GetPooledObject("VolatileStone").gameObject.GetComponent<VolatileStone>())
			};

		public static StoneItem GetStoneItem(ShiftStonePrefs stoneEnum)
		{
			if ((int)stoneEnum >= 0 || (int)stoneEnum <= 7)
			{
				return StoneHandles[(int)stoneEnum];
			}
			else if (stoneEnum == ShiftStonePrefs.Empty)
			{
				return new StoneItem();
			}
			else
			{

				return null;
			}
		}
	}
}
