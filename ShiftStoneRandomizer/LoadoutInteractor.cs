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
	public class LoadoutInteractor
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

			private GameObject _leftStoneSlot;// = new GameObject("LeftSlot");
			public GameObject LeftStoneSlot { get { return _leftStoneSlot; } private set { _leftStoneSlot = value; } }

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


			private GameObject _rightStoneSlot;// = new GameObject("Right Slot");
			public GameObject RightStoneSlot { get { return _rightStoneSlot; } private set { _rightStoneSlot = value; } }

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
				if(isSaveButton)
				{
					ActualButton.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
				}
				
				Button.SetActive(true);

				_leftStoneSlot = new GameObject("LeftSlot");
				_leftStoneSlot.transform.localPosition = new Vector3(0.05f, 0.07f, 0.01f);
				_leftStoneSlot.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
				_leftStoneSlot.transform.SetParent(Button.transform, false);
				_leftStoneSlot.SetActive(true);

				_rightStoneSlot = new GameObject("Right Slot");
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
						Debug.Log("Slot: Event handler assigned for save", true);
					});
				}
				else
				{
					ActualButton.GetComponent<InteractionButton>().onPressed.AddListener((System.Action) delegate
					{
						ApplyLoadOut();
					});
				}
			}

			private void SaveSelectedToLoadout()
			{
				Debug.Log("Saving Loadout...");
				int[] stonesInHand = Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().GetCurrentShiftStoneConfiguration();
				
				//Templogic for not adding listeners to stone case yet
				for(int i = 0; i < 2; i++)
				{
					//Selecting a command stone will have the shift stone socket empty
					//If a command isn't selected, then assign the equivalent enum to the shift stone or lack of it to selected
					if (Selection[i] >= ShiftStonePrefs.Empty || true)
					{
						Selection[i] = (ShiftStonePrefs)stonesInHand[i];

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
				Debug.Log("Applying Loadout...");
				ShiftStonePrefs left;
				if(!Enum.TryParse<ShiftStonePrefs>(LeftHandPref.Value, out left))
					Debug.Log($"AppyloLoadout Failed to parse: {LeftHandPref.Value}");
				ShiftStonePrefs right;
				if(!Enum.TryParse<ShiftStonePrefs>(RightHandPref.Value, out right))
					Debug.Log($"AppyloLoadout Failed to parse: {RightHandPref.Value}");
				
				ShiftStonePrefs[] eachHand = {left, right};

				int[] currentEquipped = Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().GetCurrentShiftStoneConfiguration();
				//If both are random, use standard randomization method
				if(left == ShiftStonePrefs.Random && right == ShiftStonePrefs.Random)
				{
					ShiftStoneRandomizer.RandomizeStones(currentEquipped);
					
				}
				else
				{
					ShiftStoneRandomizer.EquipStones(new StoneItem(), new StoneItem());
					for(int i = 0; i < 2; i++)
					{
						switch (eachHand[i])
						{
							case ShiftStonePrefs.Random:
								ShiftStoneRandomizer.RandomizeStones(currentEquipped, (Hands) i);
								break;
							
							case ShiftStonePrefs.Mirror:
								if(ShiftStoneRandomizer.CurrentLoadedScene.Contains("map") && PlayerManager.instance.AllPlayers.Count >= 2)
								{
									//Copy other player's shift stone
								}
								else 
								{
									//ignore
								}
								break;
							
							case ShiftStonePrefs.Stay: 
								//Don't do anything really
								break;
							case ShiftStonePrefs.Empty: 
								ShiftStoneRandomizer.EquipStones(new StoneItem(), new StoneItem());
								break;
							default:
								if(i == 0)
									ShiftStoneRandomizer.EquipStones(StoneItem.AllStones[(int) eachHand[i]], null);
								else if(i == 1)
									ShiftStoneRandomizer.EquipStones(null, StoneItem.AllStones[(int) eachHand[i]]);
								break;
								
						}
					}
				}

				UpdateAllDisplays(Quadrant, left, right);
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
				if (LeftStoneSlot == null || RightStoneSlot == null)
				{
					Debug.Log("DisplayShiftStones: One or both stone slots are null!", true, 2);
					return;
				}
				//clear existing children to replace with new ones
				Infanticide(LeftStoneSlot);
				Infanticide(RightStoneSlot);


				if (!Enum.TryParse<ShiftStonePrefs>(LeftHandPref.Value, out left))
				{
					Debug.Log($"Failed to parse left: {LeftHandPref.Value}, defaulting to Empty", false, 1);
				}

				if(!Enum.TryParse<ShiftStonePrefs>(RightHandPref.Value, out right))
				{
					Debug.Log($"Failed to parse right: {RightHandPref.Value}, defaulting to Empty", false, 1);
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
			
			public static void Infanticide(GameObject parent)
			{
				
				Debug.Log($"Infanticide: parent is null? {parent == null}", true);
				//if(parent == null)
					//return;
				for (int i = 0; i < parent.transform.childCount; i++)
				{
					try
					{
						UnityEngine.Object.Destroy(parent.transform.GetChild(i).gameObject);
					}catch(Exception ex)
					{
						Debug.Log(ex.Message);
					}
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

		private static GameObject[] Sockets = new GameObject[2];
		public static GameObject LeftSocket {get {return Sockets[0];} set{Sockets[0] = value;}}
		public static GameObject RightSocket {get {return Sockets[1];} set{Sockets[1] = value;}}
		
		public static void HighlightItem(ShiftStonePrefs item, Hands hand)
		{
			if(item < ShiftStonePrefs.Empty)
			{
				Slot.Infanticide(Sockets[(int) hand]);
				Selection[(int) hand] = Selection[(int) hand] == item ? ShiftStonePrefs.Empty : item;

				if (Selection[(int)hand] != ShiftStonePrefs.Empty)
				{
					GameObject displayItem = StoneItem.GetDisplayObject(Selection[(int)hand]);
					displayItem.transform.SetParent(Sockets[(int)hand].transform, false);
					displayItem.SetActive(true);

				}//Unequip current shift stone from selected hand;

				//Highlight should be on top of empty shift stone socket
				if (hand == Hands.Left)
					ShiftStoneRandomizer.EquipStones(new StoneItem(), null, false);
				else 
					ShiftStoneRandomizer.EquipStones(null, new StoneItem(), false);
			}
			else 
			{
				int[] currentEquipped = Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().GetCurrentShiftStoneConfiguration();
				bool itemIsEquipped = false;
				foreach (int stoneInHand in currentEquipped)
				{
					if((ShiftStonePrefs) stoneInHand == item)
					{
						itemIsEquipped == true;
					}
				}

				if(!itemIsEquipped)
				{
					
					if (hand == Hands.Left)
					{
						ShiftStoneRandomizer.EquipStones(currentEquipped[0] == (int)item ? new StoneItem() : StoneItem.AllStones[(int) item], null, false);
						
					}
					else
					{
						ShiftStoneRandomizer.EquipStones(null, currentEquipped[1] == (int) item ? new StoneItem() : StoneItem.AllStones[(int)item], false);
					}
				}
				//HighlightCurrentEquippedStones();
			}
		}

		public static void HighlightCurrentEquippedStones()
		{
			int[] equipped = Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().GetCurrentShiftStoneConfiguration();
			Selection[0] = (ShiftStonePrefs) equipped[0];
			Selection[1] = (ShiftStonePrefs) equipped[1];
		}

		//Create an action slots could subscribe to. This lets slots from every interactor know that the display should be updated without having to keep a reference to them
		//Currently, this means that slots are left till listening even after the scene unloads. They are now null safe but it is technically a memory leak
		public static event Action<Quadrants, ShiftStonePrefs, ShiftStonePrefs> Display;
		public static void UpdateAllDisplays(Quadrants quadrant, ShiftStonePrefs left, ShiftStonePrefs right)
		{
			Display?.Invoke(quadrant, left, right);
		}

		public static void UnsubAll()
		{
			lock (typeof(LoadoutInteractor))
			{
				LoadoutInteractor.Display = null;
			}
		}

		public static GameObject ClusterSource;

		//Static constructor. Make sure to make no reference to this class before first load
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

		//Add this as a listener to the shift stone interaction for the existing base stones
		public static void SelectBaseStone()
		{
			int[] stonesInHand = Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().GetCurrentShiftStoneConfiguration();
			for(int i = 0; i < 2; i++)
			{
				Selection[i] = (ShiftStonePrefs) stonesInHand[i];
			}
		}

		public static void SelectCommandStone(ShiftStonePrefs commandStone)
		{
			Hands handTrigger = Hands.Left; /*= determine which hand pressed the button*/

			//Toggle between commandStone and empty when selecting a command stone.
			if(Selection[(int) handTrigger] == commandStone)
			{
				Selection[(int) handTrigger] = ShiftStonePrefs.Empty;
			}
			else 
			{
				Selection[(int) handTrigger] = commandStone;
			}
			 
			
		}

		public static void DisplayCommandOnHand(ShiftStonePrefs command, Hands hand)
		{
			
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
