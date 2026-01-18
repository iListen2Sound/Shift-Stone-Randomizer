
using MelonLoader;
using Il2CppRUMBLE.Combat.ShiftStones;
using Il2CppRUMBLE.Interactions.InteractionBase;
using Il2CppTMPro;
using UnityEngine;

using RumbleModdingAPI;
using System.Collections.Generic;

using System;




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
				new Vector3(-0.04f, -0.06f, 0.07f),
				new Vector3(0.04f, 0.06f, 0.07f),
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
				Button.transform.localPosition = Sections[(int)Quadrant];
				if (isSaveButton)
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
					ActualButton.GetComponent<InteractionButton>().onPressed.AddListener((System.Action)delegate
					{
						SaveSelectedToLoadout();
						Debug.Log("Slot: Event handler assigned for save", true);
					});
				}
				else
				{
					ActualButton.GetComponent<InteractionButton>().onPressed.AddListener((System.Action)delegate
					{

						ApplyLoadOut();
						LoadoutInteractor.IsNextSelectionPrimed = false;
					});
				}
			}

			private void SaveSelectedToLoadout()
			{
				Debug.Log("Saving Loadout...");
				int[] stonesInHand = ShiftStoneRandomizer.Player0.GetComponent<PlayerShiftstoneSystem>().GetCurrentShiftStoneConfiguration();

				//Templogic for not adding listeners to stone case yet
				for (int i = 0; i < 2; i++)
				{
					//Selecting a command stone will have the shift stone socket empty
					//If a command isn't selected, then assign the equivalent enum to the shift stone or lack of it to selected
					if (Selection[i] >= ShiftStonePrefs.Empty)
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


			public void ApplyLoadOut()
			{
				Debug.Log("Applying Loadout...");
				ShiftStonePrefs left;
				if (!Enum.TryParse<ShiftStonePrefs>(LeftHandPref.Value, out left))
					Debug.Log($"AppyloLoadout Failed to parse: {LeftHandPref.Value}");
				ShiftStonePrefs right;
				if (!Enum.TryParse<ShiftStonePrefs>(RightHandPref.Value, out right))
					Debug.Log($"AppyloLoadout Failed to parse: {RightHandPref.Value}");

				ShiftStonePrefs[] eachHand = { left, right };

				int[] currentEquipped = ShiftStoneRandomizer.Player0.GetComponent<PlayerShiftstoneSystem>().GetCurrentShiftStoneConfiguration();
				//If both are random, use standard randomization method
				if (left == ShiftStonePrefs.Random && right == ShiftStonePrefs.Random)
				{
					ShiftStoneRandomizer.RandomizeStones(currentEquipped);

				}
				else
				{
					ShiftStoneRandomizer.EquipStones(new StoneItem(), new StoneItem());


					for (int i = 0; i < 2; i++)
					{
						Hands currentHand = (Hands)i;
						LoadoutInteractor.ClearSlot(LoadoutInteractor.Sockets[i]);
						switch (eachHand[i])
						{
							case ShiftStonePrefs.Random:
								ShiftStoneRandomizer.RandomizeStones(currentEquipped, (Hands)i);
								break;

							case ShiftStonePrefs.Mirror:
								if (ShiftStoneRandomizer.IsInMatch)
								{
									if (ShiftStoneRandomizer.Player1 != null)
									{
										/*int[] opponentEquipped = ShiftStoneRandomizer.Player1.GetComponent<PlayerShiftstoneSystem>().GetCurrentShiftStoneConfiguration();
										NextSelection[0] = (ShiftStonePrefs)opponentEquipped[0];
										NextSelection[1] = (ShiftStonePrefs)opponentEquipped[1];*/
										int[] opponentEquipped = ShiftStoneRandomizer.Player1.GetComponent<PlayerShiftstoneSystem>().GetCurrentShiftStoneConfiguration();
										

										if (currentHand == Hands.Left)
										{
											ShiftStoneRandomizer.EquipStones(opponentEquipped[(int)Hands.Left] >= 0 ? StoneItem.AllStones[opponentEquipped[(int)Hands.Left]] : new StoneItem(), null);
										}
										else if (currentHand == Hands.Right)
										{
											ShiftStoneRandomizer.EquipStones(null, opponentEquipped[(int)Hands.Right] >= 0 ? StoneItem.AllStones[opponentEquipped[(int)Hands.Right]] : new StoneItem());
										}
									}
								}
								break;

							case ShiftStonePrefs.Stay:
								ShiftStoneRandomizer.EquipStones(
									currentHand == Hands.Left ? StoneItem.AllStones[(int)currentEquipped[i]] : null,
									currentHand == Hands.Right ? StoneItem.AllStones[(int)currentEquipped[i]] : null);
								break;
							case ShiftStonePrefs.Empty:
								ShiftStoneRandomizer.EquipStones(new StoneItem(), new StoneItem());
								break;
							default:
								if (i == 0)

									ShiftStoneRandomizer.EquipStones(StoneItem.AllStones[(int)eachHand[i]], null);
								else if (i == 1)
									ShiftStoneRandomizer.EquipStones(null, StoneItem.AllStones[(int)eachHand[i]]);
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
				if (quadrant != this.Quadrant)
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

				if (!Enum.TryParse<ShiftStonePrefs>(RightHandPref.Value, out right))
				{
					Debug.Log($"Failed to parse right: {RightHandPref.Value}, defaulting to Empty", false, 1);
				}

				GameObject leftItem = StoneItem.GetDisplayObject(left);
				if (left == ShiftStonePrefs.Charge)
				{
					leftItem.transform.localRotation = Quaternion.Euler(0f, 0f, 270f);

				}
				leftItem.transform.SetParent(LeftStoneSlot.transform, false);
				leftItem.SetActive(true);

				GameObject rightItem = StoneItem.GetDisplayObject(right);
				if (right == ShiftStonePrefs.Charge)
				{
					rightItem.transform.localRotation = Quaternion.Euler(0f, 0f, 270f);

				}
				rightItem.transform.SetParent(RightStoneSlot.transform, false);
				rightItem.SetActive(true);

			}

			public static void Infanticide(GameObject parent)
			{

				//Debug.Log($"Infanticide: parent is null? {parent == null}", true);
				//if(parent == null)
				//return;
				for (int i = 0; i < parent.transform.childCount; i++)
				{
					try
					{
						UnityEngine.Object.Destroy(parent.transform.GetChild(i).gameObject);
					}
					catch (Exception ex)
					{
						Debug.Log(ex.Message);
					}
				}
			}


		}
		#endregion


		#region static Members



		#region Game Flow 
		public static bool IsNextSelectionPrimed { get; set; } = true;
		public static AutomationPrefs AutomationMode { get { return ShiftStoneRandomizer.AutomationMode; } set { ShiftStoneRandomizer.AutomationMode = value; } }
		public static void OnMatchLoad()
		{
			//Debug override. Remove on release
			//if (AutomationMode == AutomationPrefs.None && Debug.debugMode)
			//{
			//	AutomationMode = AutomationPrefs.Auto;
			//	IsNextSelectionPrimed = true;
			//	Debug.Log("Overriding automation mode for debug. ");
			//}
			//only do matchload shift stone apply on first load into match
			Debug.Log("LoadoutInteractor: OnMatchLoad called", true);
			if (ShiftStoneRandomizer.IsFirstMatchLoad)
			{
				AutoApply(true);
				Debug.Log("LoadoutInteractor: OnMatchLoad applying stones for first match load", true);
				if (AutomationMode == AutomationPrefs.Mirror && IsNextSelectionPrimed)
				{
					AutoApply(false);
				}
			}

		}

		//Runs in between matches called onMatchEnded()
		public static void ReplayPrep()
		{
			IsNextSelectionPrimed = AutomationMode != AutomationPrefs.None; //reset nextselection primed every match end
			AutoApply(false);
		}

		public static void AutoApply(bool isFirstMatchLoad)
		{
			if (!IsNextSelectionPrimed)
				return;

			if (AutomationMode == AutomationPrefs.Auto)
			{
				int hostStartIndex = ShiftStoneRandomizer.IsHost ? 2 : 0; //Invert host/client selection when in between matches to prepare for next match
				if (isFirstMatchLoad) //Reverse selection if call is for first mapload
					hostStartIndex = ShiftStoneRandomizer.IsHost ? 0 : 2;

				int mapIndex = ShiftStoneRandomizer.CurrentLoadedScene == "map0" ? 0 : 1;

				MainInteractor.SlotList[hostStartIndex + mapIndex].ApplyLoadOut();
			}
			else if (AutomationMode == AutomationPrefs.Mirror)
			{
				if (ShiftStoneRandomizer.Player1 != null)
				{
					int[] opponentEquipped = ShiftStoneRandomizer.Player1.GetComponent<PlayerShiftstoneSystem>().GetCurrentShiftStoneConfiguration();
					ShiftStoneRandomizer.EquipStones(opponentEquipped[0] >= 0 ? StoneItem.AllStones[opponentEquipped[0]] : new StoneItem(), opponentEquipped[1] >= 0 ? StoneItem.AllStones[opponentEquipped[1]] : new StoneItem());
				}
			}
			else if (AutomationMode == AutomationPrefs.Random)
			{
				ShiftStoneRandomizer.RandomizeStones(ShiftStoneRandomizer.Player0.GetComponent<PlayerShiftstoneSystem>().GetCurrentShiftStoneConfiguration(), ShiftStoneRandomizer.EnabledHand);
			}
		}


		#endregion

		public static List<ShiftStonePrefs> Selection = new List<ShiftStonePrefs> {
			ShiftStonePrefs.Stay,
			ShiftStonePrefs.Stay,
		};

		public static List<ShiftStonePrefs> NextSelection = new List<ShiftStonePrefs> {
			ShiftStonePrefs.Invalid,
			ShiftStonePrefs.Invalid,
		};



		public enum Quadrants { TopLeft, TopRight, BottomLeft, BottomRight };

		public static GameObject ButtonSource;

		private static GameObject[] Sockets = new GameObject[2];
		public static GameObject LeftSocket { get { return Sockets[0]; } set { Sockets[0] = value; } }
		public static GameObject RightSocket { get { return Sockets[1]; } set { Sockets[1] = value; } }

		public static GameObject[] DisplayedItem = new GameObject[2];


		public static void ClearAllSlots()
		{
			ClearSlot(Sockets[0]);
			ClearSlot(Sockets[1]);
		}

		public static void ClearSlot(GameObject parent)
		{
			for (int i = 0; i < parent.transform.childCount; i++)
			{
				GameObject child = parent.transform.GetChild(i).gameObject;
				if (child.name.Contains("Indicator"))
				{
					try
					{
						Debug.Log($"Clearing slot: {child.name}", true);
						UnityEngine.Object.Destroy(parent.transform.GetChild(i).gameObject);
					}
					catch (Exception ex)
					{
						Debug.Log(ex.Message, false, 1);
					}

				}
			}
		}

		public static void HighlightItem(ShiftStonePrefs item, Hands hand)
		{
			int handIndex = (int)hand;
			int otherHandIndex = hand == Hands.Left ? (int)Hands.Right : (int)Hands.Left;
			int itemIndex = (int)item;

			//destroy previously selected item indicators
			ClearSlot(Sockets[handIndex]);
			//When an indicator is selected
			if (item <= ShiftStonePrefs.Empty)
			{
				//Unequip shift stones when selecting indicators
				ShiftStoneRandomizer.EquipStones(
					hand == Hands.Left ? new StoneItem() : null,
					hand == Hands.Right ? new StoneItem() : null,
					false);

				Selection[handIndex] = Selection[handIndex] == item ? ShiftStonePrefs.Empty : item;

				if (Selection[handIndex] != ShiftStonePrefs.Empty)
				{
					/*GameObject displayItem = StoneItem.GetDisplayObject(Selection[handIndex]);
					displayItem.transform.localPosition = new Vector3(0.0f, 0.025f, 0f);
					displayItem.transform.SetParent(Sockets[handIndex].transform, false);
					displayItem.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
					displayItem.SetActive(true);*/

					DisplayedItem[handIndex] = StoneItem.GetDisplayObject(Selection[handIndex]);
					DisplayedItem[handIndex].transform.localPosition = new Vector3(0.0f, 0.025f, 0f);
					DisplayedItem[handIndex].transform.SetParent(Sockets[handIndex].transform, false);
					DisplayedItem[handIndex].transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
					DisplayedItem[handIndex].SetActive(true);

				}

			}
			//When standard shift stones are selected
			else
			{


				int[] currentEquipped = Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().GetCurrentShiftStoneConfiguration();

				if (currentEquipped[otherHandIndex] == itemIndex)
					return;

				StoneItem toEquip = currentEquipped[handIndex] == itemIndex ? new StoneItem() : StoneItem.AllStones[itemIndex];


				ShiftStoneRandomizer.EquipStones(
					hand == Hands.Left ? toEquip : null,
					hand == Hands.Right ? toEquip : null,
					false);

				Selection[handIndex] = toEquip.GetEnum();


				//HighlightCurrentEquippedStones();
			}
		}


		public static void HighlightCurrentEquippedStones()
		{
			int[] equipped = Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().GetCurrentShiftStoneConfiguration();
			Selection[0] = (ShiftStonePrefs)equipped[0];
			Selection[1] = (ShiftStonePrefs)equipped[1];
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

			LoadoutInteractor.Display = null;
		}

		public static GameObject ClusterSource;

		//Static constructor. Make sure to make no reference to this class before first load
		static LoadoutInteractor()
		{
			ButtonSource = ShiftStoneRandomizer.SmallButtonSource;
			ButtonSource.transform.GetChild(0).GetComponent<InteractionButton>().enabled = true;

			ButtonSource.transform.localRotation = Quaternion.Euler(0f, 270f, 90f);
			ButtonSource.transform.localPosition = Vector3.zero;
			ButtonSource.SetActive(false);
			ButtonSource.transform.SetParent(ShiftStoneRandomizer.DDOLParent.transform, false);

			ClusterSource = new GameObject("Loadout Cluster");
			ClusterSource.transform.SetParent(ShiftStoneRandomizer.DDOLParent.transform, false);

			//Assign delegates for match flow

			Calls.onMatchEnded += ReplayPrep;

		}

		//Add this as a listener to the shift stone interaction for the existing base stones
		public static void SelectBaseStone()
		{
			int[] stonesInHand = Calls.Managers.GetPlayerManager().LocalPlayer.Controller.GetComponent<PlayerShiftstoneSystem>().GetCurrentShiftStoneConfiguration();
			for (int i = 0; i < 2; i++)
			{
				Selection[i] = (ShiftStonePrefs)stonesInHand[i];
			}
		}

		public static void SelectCommandStone(ShiftStonePrefs commandStone)
		{
			Hands handTrigger = Hands.Left; /*= determine which hand pressed the button*/

			//Toggle between commandStone and empty when selecting a command stone.
			if (Selection[(int)handTrigger] == commandStone)
			{
				Selection[(int)handTrigger] = ShiftStonePrefs.Empty;
			}
			else
			{
				Selection[(int)handTrigger] = commandStone;
			}


		}

		public static void DisplayCommandOnHand(ShiftStonePrefs command, Hands hand)
		{

		}

		//public static List<GameObject> AllDisplaySlots = new List<GameObject>();

		public static LoadoutInteractor MainInteractor = null;


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

		public GameObject Cluster { get; private set; }

		public static List<MelonPreferences_Entry<string>> PrefList = new List<MelonPreferences_Entry<string>>()
		{
			ShiftStoneRandomizer.PrefMap0HostLeft,
			ShiftStoneRandomizer.PrefMap0HostRight,
			ShiftStoneRandomizer.PrefMap1HostLeft,
			ShiftStoneRandomizer.PrefMap1HostRight,
			ShiftStoneRandomizer.PrefMap0ClientLeft,
			ShiftStoneRandomizer.PrefMap0ClientRight,
			ShiftStoneRandomizer.PrefMap1ClientLeft,
			ShiftStoneRandomizer.PrefMap1ClientRight,
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
			for (int i = 0; i < 4; i++)
			{
				SlotList[i] = new Slot((Quadrants)i, PrefList[i * 2], PrefList[(i * 2) + 1], isForSaving);
				SlotList[i].Button.transform.SetParent(Cluster.transform, false);
			}
			Cluster.SetActive(false);

			//Select one loadaout applyer as main interactor. 
			//TODO: Might not work as game objects get destroyed. 
			// Either create an interactor agnostic way to apply shift stones (and might as well pivot all interactors to call on that) 
			// or create a DDOL interactor that is enabled but placed far under the map.
			if (MainInteractor == null && !isForSaving)
			{
				MainInteractor = this;
			}

			GameObject functionLabel = Calls.Create.NewText(
				isForSaving ? "Save Loadout" : "Apply Loadout",
				0.4f,
				Color.white,
				new Vector3(0f, 0f, 0f),
				Quaternion.Euler(0f, 0f, 0f));
			functionLabel.transform.SetParent(Cluster.transform, false);
			functionLabel.transform.localPosition = new Vector3(0f, -0.12f, 0f);
			functionLabel.SetActive(true);
			functionLabel.GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.Center;

			GameObject posHost = Calls.Create.NewText("Host", 0.2f, Color.white, new Vector3(0f, 0f, 0f), Quaternion.Euler(0f, 0f, 0f));
			posHost.transform.SetParent(Cluster.transform, false);
			posHost.name = "Position Label";
			posHost.GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.Center;
			posHost.transform.localPosition = new Vector3(-0.04f, 0.14f, 0f);

			GameObject posClient = Calls.Create.NewText("Client", 0.2f, Color.white, new Vector3(0f, 0f, 0f), Quaternion.Euler(0f, 0f, 0f));
			posClient.transform.SetParent(Cluster.transform, false);
			posClient.name = "Position Label";
			posClient.GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.Center;
			posClient.transform.localPosition = new Vector3(0.04f, 0.14f, 0f);

			GameObject posRing = Calls.Create.NewText("Ring", 0.2f, Color.white, new Vector3(0f, 0f, 0f), Quaternion.Euler(0f, 0f, 0f));
			posRing.transform.SetParent(Cluster.transform, false);
			posRing.name = "Position Label";
			posRing.GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.Center;
			posRing.transform.localPosition = new Vector3(0.1f, 0.06f, 0.0f);

			GameObject posPit = Calls.Create.NewText("Pit", 0.2f, Color.white, new Vector3(0f, 0f, 0f), Quaternion.Euler(0f, 0f, 0f));
			posPit.transform.SetParent(Cluster.transform, false);
			posPit.name = "Position Label";
			posPit.GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.Center;
			posPit.transform.localPosition = new Vector3(0.1f, -0.06f, 0.0f);

		}


	}

		#endregion
}
