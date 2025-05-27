using UnityEngine;

namespace DT.GridSystem.Pathfinding.Samples
{
	/// <summary>
	/// HexGrid3d is a 3D hexagonal grid system that supports pathfinding using A* algorithm.
	/// It allows for both flat-top and pointy-top hexagonal grids.
	/// </summary>
	public class HexGrid3d : HexGridSystem3D<GameObject>
	{
		[SerializeField] GameObject flatTopPrefab;
		[SerializeField] GameObject pointyTopPrefab;
		[SerializeField] PathfinderAgent3D pathfinderAgent;
		AStarPathfinding pathfinding;
		[SerializeField] NavmeshData NavmeshData;
		[SerializeField] int cost = 10; // Cost for moving to adjacent hexes, can be adjusted as needed
		Camera Camera;
		void Start()
		{
			if (NavmeshData == null)
			{
				NavmeshData = ScriptableObject.CreateInstance<NavmeshData>();
			}

			pathfinding = new AStarPathfinding(new HexGrid(GridSize, 1, NavmeshData));
			NavmeshData.BakeNavmesh(this);

			pathfinderAgent.Initialize(this, pathfinding);

			Camera = Camera.main;
		}
		public override GameObject CreateGridObject(GridSystem<GameObject> gridSystem, int x, int y)
		{
			if (hexOrientation == HexOrientation.FlatTop)
			{
				return Instantiate(flatTopPrefab, GetWorldPosition(x, y, true), Quaternion.identity);
			}
			else
			{
				return Instantiate(pointyTopPrefab, GetWorldPosition(x, y, true), Quaternion.identity);
			}
		}
		private void Update()
		{
			if (Input.GetMouseButtonDown(0) && Physics.Raycast(Camera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
			{
				Vector3 worldPos = hit.point;
				if (pathfinderAgent != null)
				{
					pathfinderAgent.SetDestination(worldPos);
				}
				else
				{
					Debug.LogWarning("PathfinderAgent3D is not assigned.");
				}
			}
		}
	}
}
