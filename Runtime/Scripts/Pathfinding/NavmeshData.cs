namespace DT.GridSystem.Pathfinding
{
	using System.Collections.Generic;
	using UnityEngine;
	[CreateAssetMenu(fileName = "NavmeshData", menuName = "Pathfinding/NavmeshData")]
	public class NavmeshData : ScriptableObject
	{
		public enum NavMeshArea
		{
			WALKABLE,
			NOT_WALKABLE
		}

		public class NodeCache
		{
			public NavMeshArea area;
			public PathfindingCullingMask cullingMask;
		}

		[HideInInspector] public Vector2Int gridSize;
		[HideInInspector] public float cellSize;
		[HideInInspector] public bool allowDiagonals;
		[HideInInspector] private Dictionary<Vector2Int, bool> walkableCells = new();
		[HideInInspector] private readonly Dictionary<Vector2Int, NodeCache> cellData = new();

		public bool IsWalkable(Vector2Int position, PathfindingCullingMask cullingMask)
		{
			return cellData.ContainsKey(position) && (cellData[position].cullingMask == cullingMask || walkableCells[position]);
		}


		public void BakeNavmesh(GridSystem<GameObject> grid)
		{
			gridSize = new Vector2Int(grid.GridSize.x, grid.GridSize.y);
			cellSize = grid.CellSize;
			walkableCells.Clear();
			cellData.Clear();

			for (int x = 0; x < grid.GridSize.x; x++)
			{
				for (int y = 0; y < grid.GridSize.y; y++)
				{
					Vector2Int pos = new(x, y);
					GameObject obj = grid.GetGridObject(x, y);
					bool isWalkable = obj == null;
					PathfindingCullingMask cullingMask = null;
					if (obj != null)
					{
						if (obj.TryGetComponent(out PathfinderAgent<GameObject> agent))
						{
							cullingMask = agent.CullingMask;
						}
						else if (obj.TryGetComponent(out PathfindingObstacle obstacle))
						{
							cullingMask = obstacle.CullingMask;
						}
					}
					walkableCells[pos] = isWalkable;
					cellData[pos] = new()
					{
						area = isWalkable ? NavMeshArea.WALKABLE : NavMeshArea.NOT_WALKABLE,
						cullingMask = cullingMask
					};
				}
			}
		}
	}
}