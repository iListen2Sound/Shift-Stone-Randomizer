
using Il2CppRUMBLE.Combat.ShiftStones;
using Il2CppRUMBLE.Managers;
using RumbleModdingAPI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ShiftStoneManager
{
	/// <summary>
	/// Represents an item associated with a shift stone, providing functionality to manage its state and behavior.
	/// </summary>
	/// <remarks>A <see cref="StoneItem"/> can either represent a valid shift stone or an empty slot.  It provides
	/// properties to manage the stone's state, such as enabling or disabling it,  and tracks the number of disabled stones
	/// globally through <see cref="BlackListCount"/>.</remarks>
	public class StoneItem
	{
		private static GameObject ObjMirror = GameObject.Instantiate(ShiftStoneManager.IndicatorsBase.transform.GetChild(3).gameObject);
		private static GameObject ObjStay = GameObject.Instantiate(ShiftStoneManager.IndicatorsBase.transform.GetChild(4).gameObject);
		private static GameObject ObjRandom = GameObject.Instantiate(ShiftStoneManager.IndicatorsBase.transform.GetChild(5).gameObject);
		private static GameObject ObjEmpty = GameObject.Instantiate(ShiftStoneManager.IndicatorsBase.transform.GetChild(6).gameObject);

		public readonly static GameObject[] Specials = new GameObject[] {
			ObjMirror, 
			ObjStay, 
			ObjRandom,
			ObjEmpty
		};
		/// <summary>
		/// 
		/// </summary>
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
				obj.transform.SetParent(ShiftStoneManager.DDOLParent.transform);
				//GameObject.D=ontDestroyOnLoad(obj);
				obj.SetActive(false);

			}

			foreach (StoneItem stone in AllStones)
			{
				stone.ShiftStone.gameObject.SetActive(false);
			}
		}

		public static StoneItem[] AllStones = new StoneItem[] {
				new StoneItem(PoolManager.Instance.GetPooledObject("AdamantStone").gameObject.GetComponent<UnyieldingStone>()),
				new StoneItem(PoolManager.Instance.GetPooledObject("ChargeStone").gameObject.GetComponent<ChargeStone>()),
				new StoneItem(PoolManager.Instance.GetPooledObject("FlowStone").gameObject.GetComponent<FlowStone>()),
				new StoneItem(PoolManager.Instance.GetPooledObject("GuardStone").gameObject.GetComponent<GuardStone>()),
				new StoneItem(PoolManager.Instance.GetPooledObject("StubbornStone").gameObject.GetComponent<StubbornStone>()),
				new StoneItem(PoolManager.Instance.GetPooledObject("SurgeStone").gameObject.GetComponent<CounterStone>()),
				new StoneItem(PoolManager.Instance.GetPooledObject("VigorStone").gameObject.GetComponent<VigorStone>()),
				new StoneItem(PoolManager.Instance.GetPooledObject("VolatileStone").gameObject.GetComponent<VolatileStone>())
			};


		/// <summary>
		/// 
		/// </summary>
		/// <param name="stoneEnum"></param>
		/// <returns></returns>
		public static GameObject GetDisplayObject(ShiftStonePrefs stoneEnum)
		{
			Debug.Log($"GetDisplayObject called for {stoneEnum} Index: {(int)stoneEnum}", true);
			if ((int)stoneEnum >= 0 && (int)stoneEnum <= 7)
			{
				//null ref
				try
				{
					GameObject stone = GameObject.Instantiate(AllStones[(int)stoneEnum].ShiftStone.gameObject);
					MeshRenderer mesh = stone.transform.GetChild(0).GetComponent<MeshRenderer>();
					mesh.forceRenderingOff = false;
					mesh.enabled = true;

					return stone;
				}
				catch (System.Exception ex)
				{
					Debug.Log($"Error instantiating stone object for {stoneEnum}: {ex.Message}", false, 2);
					return GameObject.Instantiate(RecreateStoneItems()[(int)stoneEnum].ShiftStone.gameObject);

				}
			}
			else if ((int)stoneEnum >= -4 && (int)stoneEnum <= -1)
			{
				return GameObject.Instantiate(Specials[((int)stoneEnum + 4)]);
			}
			else 			{
				Debug.Log($"GetDisplayObject: Invalid stone enum {stoneEnum} index for GetDisplayObject {(int) stoneEnum}", false, 2);
				return null;
			}
		}
		public static void ResetAllIcons()
		{
			foreach (StoneItem stone in AllStones)
			{
				stone.ResetIcons();
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public static StoneItem[] RecreateStoneItems()
		{
			StoneItem[] NewStones = new StoneItem[] {
				new StoneItem(PoolManager.Instance.GetPooledObject("AdamantStone").gameObject.GetComponent<UnyieldingStone>()),
				new StoneItem(PoolManager.Instance.GetPooledObject("ChargeStone").gameObject.GetComponent<ChargeStone>()),
				new StoneItem(PoolManager.Instance.GetPooledObject("FlowStone").gameObject.GetComponent<FlowStone>()),
				new StoneItem(PoolManager.Instance.GetPooledObject("GuardStone").gameObject.GetComponent<GuardStone>()),
				new StoneItem(PoolManager.Instance.GetPooledObject("StubbornStone").gameObject.GetComponent<StubbornStone>()),
				new StoneItem(PoolManager.Instance.GetPooledObject("SurgeStone").gameObject.GetComponent<CounterStone>()),
				new StoneItem(PoolManager.Instance.GetPooledObject("VigorStone").gameObject.GetComponent<VigorStone>()),
				new StoneItem(PoolManager.Instance.GetPooledObject("VolatileStone").gameObject.GetComponent<VolatileStone>())
			};
			AllStones = NewStones;
			return NewStones;
		}


		private static int _blacklistCount = 0;
		/// <summary>
		/// 
		/// </summary>
		public static int BlackListCount
		{
			get { return _blacklistCount; }
		}
		/// <summary>
		/// 
		/// </summary>
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
		/// <summary>
		/// 
		/// </summary>
		public bool IsEnabled
		{
			get { return _isEnabled; }
			set
			{
				_isEnabled = value;

				//MelonLogger.Msg($"{Name} Icon: {_icon.active}");
				if (ShiftStone is null)
					return;

				if (value)
					_blacklistCount++;

				else
					_blacklistCount--;

				if (_iconList == null)
					return;

				foreach (GameObject icon in _iconList)
				{
					try
					{
						icon.SetActive(!_isEnabled);
					}
					catch (System.Exception ex)
					{
						Debug.Log(ex.Message, false, 1);
					}
				}
				/*if (_icon == null)
					return;
				_icon.SetActive(!_isEnabled);*/
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public ShiftStonePrefs GetEnum()
		{
			ShiftStonePrefs result;
			if (!System.Enum.TryParse<ShiftStonePrefs>(Name, out result))
			{
				Debug.Log("Error: Failed to parse stone item enum");
				return ShiftStonePrefs.Invalid;
			}

			return result;
		}
		/// <summary>
		/// 
		/// </summary>
		private List<GameObject> _iconList = new List<GameObject>();
		/// <summary>
		/// 
		/// </summary>
		public List<GameObject> IconList { get { return _iconList; } set { _iconList = value; } }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="shiftStone"></param>
		public StoneItem(ShiftStone shiftStone)
		{
			//shiftStone.gameObject.SetActive(false);// Disable the stone so it doesn't show up in the game
			ShiftStone = shiftStone;
			_isEnabled = true;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="stoneSelection"></param>
		public StoneItem(ShiftStonePrefs stoneSelection)
		{
			ShiftStone stone = AllStones[(int)stoneSelection].ShiftStone;
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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="icon"></param>
		public void AddIcon(GameObject icon)
		{
			_iconList.Add(icon);

			icon.SetActive(!_isEnabled);
		}
		/// <summary>
		/// 
		/// </summary>
		public void ResetIcons()
		{
			_iconList.Clear();
			_iconList = new List<GameObject>();
		}


	}
}