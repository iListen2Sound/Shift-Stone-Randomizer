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
			public MelonPreferences_Entry<string> LeftHandPref { get; set; }


			private GameObject _rightStoneSlot = new GameObject();
			public GameObject RightStoneSlot { get { return _rightStoneSlot; } }
			public MelonPreferences_Entry<string> RightHandPref { get; set; }
		}


#region static
		public enum Quadrant { TopLeft, TopRight, BottomLeft, BottomRight };

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


		public GameObject Cluster { get; set; } = 
		
		public Slot Map0Host {get; set;}
		public Slot Map1Host {get; set;}
		public Slot Map0Client {get; set; }
		public Slot Map1Client {get; set; }

		public readonly List<Slot> SlotList = new List<Slot>() { Map0Host, Map1Host, Map0Client, Map1Client };

		



	}
}
