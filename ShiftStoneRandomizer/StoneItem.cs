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
	class StoneItem
	{
		private static int _blacklistCount = 0;
		public static int BlackListCount
		{
			get { return _blacklistCount; }
		}
		public ShiftStone ShiftStone { get; set; }
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

				if (value)
					_blacklistCount++;

				else
					_blacklistCount--;
				if (_icon == null)
					return;
				_icon.SetActive(!_isEnabled);
			}
		}

		private ShiftStonePrefs GetEnum()
		{
			ShiftStonePrefs result;
			if(!System.Enum.TryParse<ShiftStonePrefs>(Name, out result))
			{
				Log("Error: Failed to parse stone item enum");
				return null;
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
		/// <summary>
		/// No arguments to indicate empty stone slot
		/// </summary>
		public StoneItem()
		{
			ShiftStone = null;
		}
	}
}