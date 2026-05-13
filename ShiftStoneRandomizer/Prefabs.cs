using RumbleModdingAPI.RMAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShiftStoneManager
{
	internal class Prefabs
	{
		private static GameObject PrefabParent;
		private static GameObject IndicatorAssets;
		
		internal static void InitializePrefabs()
		{
			PrefabParent = new GameObject("ShiftStoneManager");
			GameObject.DontDestroyOnLoad(PrefabParent);

			IndicatorAssets = LoadAssetBundle();
			IndicatorAssets.transform.SetParent(PrefabParent.transform, false);
		}

		private static GameObject LoadAssetBundle()
		{
			GameObject _indicatorsBase;
			_indicatorsBase = GameObject.Instantiate(AssetBundles.LoadAssetFromStream<GameObject>(Core.Instance, "ShiftStoneManager.assets.randomizer", "ShiftStoneRandomizer"));
			_indicatorsBase.transform.SetParent(IndicatorAssets.transform);
			//GameObject.DontDestroyOnLoad(_indicatorsBase);
			_indicatorsBase.SetActive(false);

			return _indicatorsBase;
		}

		private static void CreateClusterSource()
		{

		}
		private static void CreatePortableCabinetSource()
		{

		}
	}
}
