namespace DT.GridSystem.Pathfinding
{
	using UnityEngine;

	public class PathfindingModifier : MonoBehaviour
	{
		[SerializeField] NavMeshArea navMeshArea = NavMeshArea.WALKABLE;

		public NavMeshArea NavMeshArea { get => navMeshArea; set => navMeshArea = value; }
	}
}