using UnityEngine;

namespace DT.GridSystem.Pathfinding
{
	[System.Serializable]
	public struct PathfindingLayer
	{
		[SerializeField] string layerName;

		public readonly string LayerName => layerName;

		public readonly PathfindingCullingMask GetMask()
		{
			return new PathfindingCullingMask(this);
		}
	}
}
