using System.Collections.Generic;
using UnityEngine;

namespace DT.GridSystem.Pathfinding
{
	public class PathfindingLayerData : ScriptableObject
	{
		public List<string> layerNames = new List<string> { "Ground", "Obstacle", "Water" };

		private static PathfindingLayerData _instance;
		private Dictionary<string, int> nameToIndexMap;
		private bool isInitialized = false;

		public static PathfindingLayerData Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = Resources.Load<PathfindingLayerData>("PathfindingLayerData");
					if (_instance == null)
					{
						Debug.LogError("PathfindingLayerData asset not found in Resources folder! Please create it using DT Tools -> Pathfinding Layers Manager");
					}
				}
				return _instance;
			}
		}

		private void Initialize()
		{
			if (isInitialized && nameToIndexMap != null) return;

			nameToIndexMap = new Dictionary<string, int>();
			for (int i = 0; i < layerNames.Count; i++)
			{
				if (!string.IsNullOrEmpty(layerNames[i]))
				{
					nameToIndexMap[layerNames[i]] = i;
				}
			}

			isInitialized = true;
		}
		private void Awake()
		{
			RefreshData();
		}

		/// <summary>
		/// Forces reinitialization (useful when data changes in editor)
		/// </summary>
		public void RefreshData()
		{
			isInitialized = false;
			Initialize();
		}

		/// <summary>
		/// Get custom Pathfinding layer index (0-based for bitmask shift).
		/// Returns -1 if not found.
		/// </summary>
		public int GetCustomLayerIndex(string layerName)
		{
			Initialize();
			return nameToIndexMap.TryGetValue(layerName, out var index) ? index : -1;
		}

		public PathfindingCullingMask GetMask(params string[] layerNames)
		{
			return PathfindingCullingMask.FromLayerNames(layerNames);
		}

		public PathfindingCullingMask GetFullMask()
		{
			return PathfindingCullingMask.FromLayerNames(layerNames.ToArray());
		}

		/// <summary>
		/// Get all layer names (for debugging or UI purposes)
		/// </summary>
		public string[] GetAllLayerNames()
		{
			return layerNames.ToArray();
		}
	}
}
