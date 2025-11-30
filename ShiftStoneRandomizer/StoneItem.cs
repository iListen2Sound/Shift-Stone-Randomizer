using AsmResolver.PE.DotNet.Cil;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppInterop.Runtime.Runtime.VersionSpecific.Class;
using Il2CppPhoton.Realtime;
using Il2CppRootMotion;
using Il2CppRUMBLE.CharacterCreation.Interactable;
using Il2CppRUMBLE.Combat.ShiftStones;
using Il2CppRUMBLE.Interactions.InteractionBase;
using Il2CppRUMBLE.Managers;
using Il2CppRUMBLE.Players.Subsystems;
using Il2CppSystem;
using Il2CppSystem.Data;
using Il2CppTMPro;
using MelonLoader;
using MelonLoader.TinyJSON;
using MelonLoader.Utils;
using RumbleModdingAPI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Action = System.Action;
using Type = Il2CppSystem.Type;

namespace ShiftStoneRandomizer
{
	/// <summary>
	/// Represents an item associated with a shift stone, providing functionality to manage its state and behavior.
	/// </summary>
	/// <remarks>A <see cref="StoneItem"/> can either represent a valid shift stone or an empty slot.  It provides
	/// properties to manage the stone's state, such as enabling or disabling it,  and tracks the number of disabled stones
	/// globally through <see cref="BlackListCount"/>.</remarks>
	public class StoneItem
	{
		private static GameObject ObjMirror = GameObject.Instantiate(ShiftStoneRandomizer.IndicatorsBase.transform.GetChild(2).gameObject);
		private static GameObject ObjStay = GameObject.Instantiate(ShiftStoneRandomizer.IndicatorsBase.transform.GetChild(2).gameObject);
		private static GameObject ObjRandom = GameObject.Instantiate(ShiftStoneRandomizer.IndicatorsBase.transform.GetChild(2).gameObject);
		private static GameObject ObjEmpty = GameObject.Instantiate(ShiftStoneRandomizer.IndicatorsBase.transform.GetChild(2).gameObject);

		public readonly static GameObject[] Specials = new GameObject[] {
			//TODO: Create source templates and prefabs
			ObjMirror, //Possible icon: https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQpFLoTk1WnuNjDSu5WeOFhxRwb50LfWqaBHA&s
			ObjStay, //Cube
			ObjRandom,
			ObjEmpty //Either actually empty or disc
		};

		static StoneItem()
		{
			ObjMirror.name = "Indicator_Mirror";
			ObjMirror.transform.GetChild(0).GetComponent<RawImage>().color = Color.blue;
			ObjStay.name = "Indicator_Stay";
			ObjStay.transform.GetChild(0).GetComponent<RawImage>().color = Color.red;
			ObjRandom.name = "Indicator_Random";
			ObjRandom.transform.GetChild(0).GetComponent<RawImage>().color = Color.green;
			ObjEmpty.name = "Indicator_Empty";
			ObjEmpty.transform.GetChild(0).GetComponent<RawImage>().color = Color.black;

			foreach (GameObject obj in Specials)
			{
				obj.transform.localScale = Vector3.one * 0.00015f;
				obj.transform.localRotation = Quaternion.Euler(90f, 90f, 0);
				GameObject.DontDestroyOnLoad(obj);
				obj.SetActive(false);

			}
		}

		public static StoneItem[] AllStones = new StoneItem[] {
				new StoneItem(Calls.Managers.GetPoolManager().GetPooledObject("AdamantStone").gameObject.GetComponent<UnyieldingStone>()),
				new StoneItem(Calls.Managers.GetPoolManager().GetPooledObject("ChargeStone").gameObject.GetComponent<ChargeStone>()),
				new StoneItem(Calls.Managers.GetPoolManager().GetPooledObject("FlowStone").gameObject.GetComponent<FlowStone>()),
				new StoneItem(Calls.Managers.GetPoolManager().GetPooledObject("GuardStone").gameObject.GetComponent<GuardStone>()),
				new StoneItem(Calls.Managers.GetPoolManager().GetPooledObject("StubbornStone").gameObject.GetComponent<StubbornStone>()),
				new StoneItem(Calls.Managers.GetPoolManager().GetPooledObject("SurgeStone").gameObject.GetComponent<CounterStone>()),
				new StoneItem(Calls.Managers.GetPoolManager().GetPooledObject("VigorStone").gameObject.GetComponent<VigorStone>()),
				new StoneItem(Calls.Managers.GetPoolManager().GetPooledObject("VolatileStone").gameObject.GetComponent<VolatileStone>())
			};
		
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="stoneEnum"></param>
		/// <returns></returns>
		public static GameObject GetDisplayObject (ShiftStonePrefs stoneEnum)
		{
			Debug.Log($"GetDisplayObject called for {stoneEnum} Index: {(int) stoneEnum}", true);
			if ((int)stoneEnum >= 0 && (int)stoneEnum <= 7)
			{
				//null ref
				try
				{
					return GameObject.Instantiate(AllStones[(int)stoneEnum].ShiftStone.gameObject);
				}
				catch (System.Exception ex)
				{
					Debug.Log($"Error instantiating stone object for {stoneEnum}: {ex.Message}", false);
					return GameObject.Instantiate(RecreateStoneItems()[(int)stoneEnum].ShiftStone.gameObject);
					
				}
			}
			else
			{
				return GameObject.Instantiate(Specials[((int)stoneEnum + 4)]);
			}	
		}

		public static StoneItem[] RecreateStoneItems()
		{
			StoneItem[] NewStones = new StoneItem[] {
				new StoneItem(Calls.Managers.GetPoolManager().GetPooledObject("AdamantStone").gameObject.GetComponent<UnyieldingStone>()),
				new StoneItem(Calls.Managers.GetPoolManager().GetPooledObject("ChargeStone").gameObject.GetComponent<ChargeStone>()),
				new StoneItem(Calls.Managers.GetPoolManager().GetPooledObject("FlowStone").gameObject.GetComponent<FlowStone>()),
				new StoneItem(Calls.Managers.GetPoolManager().GetPooledObject("GuardStone").gameObject.GetComponent<GuardStone>()),
				new StoneItem(Calls.Managers.GetPoolManager().GetPooledObject("StubbornStone").gameObject.GetComponent<StubbornStone>()),
				new StoneItem(Calls.Managers.GetPoolManager().GetPooledObject("SurgeStone").gameObject.GetComponent<CounterStone>()),
				new StoneItem(Calls.Managers.GetPoolManager().GetPooledObject("VigorStone").gameObject.GetComponent<VigorStone>()),
				new StoneItem(Calls.Managers.GetPoolManager().GetPooledObject("VolatileStone").gameObject.GetComponent<VolatileStone>())
			};
			AllStones = NewStones;
			return NewStones;
		}


		private static int _blacklistCount = 0;
		public static int BlackListCount
		{
			get { return _blacklistCount; }
		}
		public ShiftStone ShiftStone { get; private set; }
		/// <summary>
		/// Returns "None" if the stone is null.
		/// </summary>
		public string Name
		{
			get
			{
				return ShiftStone != null ? ShiftStone.name.Replace("Stone", "") : "Empty";
			}
		}
		private bool _isEnabled;
		public bool IsEnabled
		{
			get { return _isEnabled; }
			set
			{
				_isEnabled = value;

				//MelonLogger.Msg($"{Name} Icon: {_icon.active}");
				if(ShiftStone is null)
					return;

				if (value)
					_blacklistCount++;

				else
					_blacklistCount--;
				if (_icon == null)
					return;
				_icon.SetActive(!_isEnabled);
			}
		}

		public ShiftStonePrefs GetEnum()
		{
			ShiftStonePrefs result;
			if(!System.Enum.TryParse<ShiftStonePrefs>(Name, out result))
			{
				Debug.Log("Error: Failed to parse stone item enum");
				return ShiftStonePrefs.Invalid;
			}

			return result;
		}

		private GameObject _icon;
		public GameObject Icon { set { _icon = value; _icon.SetActive(false); } }
		public StoneItem(ShiftStone shiftStone)
		{
			shiftStone.gameObject.SetActive(false);// Disable the stone so it doesn't show up in the game
			ShiftStone = shiftStone;
			_isEnabled = true;
		}
		
		public StoneItem(ShiftStonePrefs stoneSelection)
		{
			ShiftStone stone = AllStones[(int) stoneSelection].ShiftStone;
			_isEnabled = true;
		}
		/// <summary>
		/// No arguments to indicate empty stone slot
		/// </summary>
		public StoneItem()
		{
			ShiftStone = null;
			_isEnabled = false;
		}

		
	}
}