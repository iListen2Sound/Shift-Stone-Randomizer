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

namespace ShiftStoneRandomizer
{
	internal class LoadoutInteractor
	{
		//TODO: Create logic for all loadout interactors in the world to display the same shiftstones
#region Slot
		public class Slot
		{
			public static Vector3[] Sections = new Vector3[]
			{
				new Vector3(-0.04f, 0.06f, 0.07f),
				new Vector3(0.04f, 0.06f, 0.07f),
				new Vector3(-0.04f, -0.06f, 0.07f),
				new Vector3(0.04f, -0.06f, 0.07f),
			};

			public GameObject Button { get; private set; }
			public GameObject ActualButton 
			{
				get 
				{
					return Button.transform.GetChild(0).gameObject;
				}
			}

			private GameObject _leftStoneSlot = new GameObject("LeftSlot");
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


			private GameObject _rightStoneSlot = new GameObject("Right Slot");
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

			/// <summary>
			/// 
			/// </summary>
			/// <param name="quadrant"></param>
			/// <param name="leftPref"></param>
			/// <param name="rightPref"></param>
			/// <param name="isSaveButton"></param>
			public Slot(LoadoutInteractor.Quadrants quadrant, MelonPreferences_Entry<string> leftPref, MelonPreferences_Entry<string> rightPref, bool isSaveButton)
			{
				//Quadrant = quad;
				LeftHandPref = leftPref;
				RightHandPref = rightPref;
				Quadrant = quadrant;

				Button = GameObject.Instantiate(ButtonSource);
				Button.name = "Loadout Button";
				Button.transform.localPosition = Sections[(int) Quadrant];
				Button.SetActive(true);

				_leftStoneSlot.transform.localPosition = new Vector3(0.05f, 0.07f, 0.01f);
				_leftStoneSlot.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
				_leftStoneSlot.transform.SetParent(Button.transform, false);
				_leftStoneSlot.SetActive(true);

				_rightStoneSlot.transform.localPosition = new Vector3(0.05f, 0.07f, -0.01f);
				_rightStoneSlot.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
				_rightStoneSlot.transform.SetParent(Button.transform, false);
				_rightStoneSlot.SetActive(true);

				LoadoutInteractor.Display += DisplayShiftStones;
				//UpdateAllDisplays(Quadrant, Selection[0], Selection[1]);
				DisplayShiftStones(Quadrant, Selection[0], Selection[1]);
				//TODO: Define save vs load event handlers
				if (isSaveButton)
				{
					ActualButton.GetComponent<InteractionButton>().onPressed.AddListener((System.Action) delegate
					{
						SaveSelectedToLoadout();
					});
				}
				else
				{
					ActualButton.GetComponent<InteractionButton>().onPressed.AddListener((System.Action) delegate
					{
						//TODO: Implement load logic
					});
				}
			}

			private void SaveSelectedToLoadout()
			{
				int[] stonesInHand = Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().GetCurrentShiftStoneConfiguration();
				
				foreach (int i in stonesInHand)
				{
					//Make sure to make an event handler for selecting a shiftstone from the shiftstone case that sets the selection to the selected stone
					if(Selection[i] >= ShiftStonePrefs.Empty)
					{
						Selection[i] = (ShiftStonePrefs) i;
						
					}
				}

				//DisplayShiftStones(LoadoutInteractor.Selection[0], LoadoutInteractor.Selection[1]);
				LeftHandPref.Value = Selection[0].ToString();
				RightHandPref.Value = Selection[1].ToString();
				ShiftStoneRandomizer.Instance.SavePrefs();
				UpdateAllDisplays(Quadrant, Selection[0], Selection[1]);
			}

			private void ApplyLoadOut()
			{
				ShiftStonePrefs left;
				if(!Enum.TryParse<ShiftStonePrefs>(LeftHandPref.Value, out left))
					Degug.Log($"AppyloLoadout Failed to parse: {LeftHandPref.Value}");
				ShiftStonePrefs right;
				if(!Enum.TryParse<ShiftStonePrefs>(RightHandPref.Value, out right))
					Degug.Log($"AppyloLoadout Failed to parse: {RightHandPref.Value}");
				
				ShiftStonePrefs[] both = {left, right};

				int[] currentEquipped = Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().GetCurrentShiftStoneConfiguration();
				//If both are random, use standard randomization method
				if(left == ShiftStonePrefs.Random && right == ShiftStonePrefs.Random)
				{
					RandomizeShiftStone(currentEquipped);
					
				}
				else
				{
					
					for(int i = 0; i < 2; i++)
					{
						switch (both[i])
						{
							
						}
						if(both[i] == ShiftStonePrefs.Random)
						{
							RandomizeStones(currentEquipped, (Hands) i);
						}
					}
				}
				

				
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="quadrant"></param>
			/// <param name="left"></param>
			/// <param name="right"></param>
			internal void DisplayShiftStones(Quadrants quadrant, ShiftStonePrefs left, ShiftStonePrefs right)
			{
				if(quadrant != this.Quadrant)
					return;
				//clear existing children to replace with new ones
				Infanticide(LeftStoneSlot);
				Infanticide(RightStoneSlot);

				if(!Enum.TryParse<ShiftStonePrefs>(LeftHandPref.Value, out left))
				{
					Debug.Log($"Failed to parse left: {LeftHandPref.Value}, defaulting to Empty");
				}

				if(!Enum.TryParse<ShiftStonePrefs>(RightHandPref.Value, out right))
				{
					Debug.Log($"Failed to parse right: {RightHandPref.Value}, defaulting to Empty");
				}

				GameObject leftItem = StoneItem.GetDisplayObject(left);
				if(left == ShiftStonePrefs.Charge)
				{
					leftItem.transform.localRotation = Quaternion.Euler(0f, 0f, 270f);
					
				}
				leftItem.transform.SetParent(LeftStoneSlot.transform, false);
				leftItem.SetActive(true);

				GameObject rightItem = StoneItem.GetDisplayObject(right);
				if(right == ShiftStonePrefs.Charge)
				{
					rightItem.transform.localRotation = Quaternion.Euler(0f, 0f, 270f);
					
				}
				rightItem.transform.SetParent(RightStoneSlot.transform, false);
				rightItem.SetActive(true);

			}
			
			private void Infanticide(GameObject parent)
			{
				for(int i = 0; i < parent.transform.childCount; i++)
				{
					UnityEngine.Object.Destroy(parent.transform.GetChild(i).gameObject);
				}
			}


		}
#endregion


#region static Members
		public static List<ShiftStonePrefs> Selection = new List<ShiftStonePrefs> {
			ShiftStonePrefs.Stay,
			ShiftStonePrefs.Stay,
		};

		public enum Quadrants { TopLeft, TopRight, BottomLeft, BottomRight };

		public static GameObject ButtonSource;

		public static event Action<Quadrants, ShiftStonePrefs, ShiftStonePrefs> Display;
		public static void UpdateAllDisplays(Quadrants quadrant, ShiftStonePrefs left, ShiftStonePrefs right)
		{
			Display?.Invoke(quadrant, left, right);
		}

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

		

		//public static List<GameObject> AllDisplaySlots = new List<GameObject>();

		
#endregion

#region Instance Members

		private Slot _map0Host;
		public Slot Map0Host { get { return _map0Host; } }

		private Slot _map1Host;
		public Slot Map1Host { get { return _map1Host; } }

		private Slot _map0Client;
		public Slot Map0Client { get { return _map0Client; } }

		private Slot _map1Client;
		public Slot Map1Client { get { return _map1Client; } }

		public GameObject Cluster {get; private set;}

		public static List<MelonPreferences_Entry<string>> PrefList = new List<MelonPreferences_Entry<string>>()
		{
			ShiftStoneRandomizer.Instance.PrefMap0HostLeft,
			ShiftStoneRandomizer.Instance.PrefMap0HostRight,
			ShiftStoneRandomizer.Instance.PrefMap1HostLeft,
			ShiftStoneRandomizer.Instance.PrefMap1HostRight,
			ShiftStoneRandomizer.Instance.PrefMap0ClientLeft,
			ShiftStoneRandomizer.Instance.PrefMap0ClientRight,
			ShiftStoneRandomizer.Instance.PrefMap1ClientLeft,
			ShiftStoneRandomizer.Instance.PrefMap1ClientRight,
		};

		public readonly List<Slot> SlotList;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="isForSaving"></param>
		public LoadoutInteractor(bool isForSaving)
		{
			SlotList = new List<Slot>() { Map0Host, Map1Host, Map0Client, Map1Client };
			Cluster = GameObject.Instantiate(ClusterSource);
			for(int i = 0; i < 4; i++)
			{
				SlotList[i] = new Slot((Quadrants) i, PrefList[i * 2], PrefList[(i * 2) + 1], isForSaving);
				SlotList[i].Button.transform.SetParent(Cluster.transform, false);
			}
			Cluster.SetActive(false);
		}

		
	}

	#endregion
}
