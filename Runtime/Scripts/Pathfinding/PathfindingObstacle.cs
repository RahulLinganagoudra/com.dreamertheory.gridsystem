namespace DT.GridSystem.Pathfinding
{
	using UnityEngine;

	public class PathfindingObstacle : MonoBehaviour
	{
		[SerializeField] PathfindingCullingMask cullingMask;

		public PathfindingCullingMask CullingMask { get => cullingMask; set => cullingMask = value; }

	}
}