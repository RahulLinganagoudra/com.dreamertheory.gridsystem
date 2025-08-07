using UnityEngine;
namespace DT.GridSystem.Pathfinding.Samples
{
	public class PathfindingGridSample : GridSystem3D<GameObject>    // This class is a sample
	{
		AStarPathfinding pathfinding;
		[SerializeField] NavmeshData navmeshData;
		[SerializeField] PathfindingObstacle obstaclePrefab;
		[SerializeField] PathfinderAgent3D pathfinderAgent3D;
		Camera Camera;

		private void Start()
		{
			Camera = Camera.main;
			if (navmeshData == null)
			{
				navmeshData = ScriptableObject.CreateInstance<NavmeshData>();
			}
			//Initialization
			pathfinding = new RectGridPathfinding<GameObject>(this,navmeshData, false);
			navmeshData.BakeNavmesh(this);
			pathfinderAgent3D.Initialize(this, pathfinding);
		}


		public override GameObject CreateGridObject(GridSystem<GameObject> gridSystem, int x, int y)
		{
			//10% change of Obstacle
			if (obstaclePrefab != null && Random.Range(0, 100) < 10)
			{
				var obstacle = Instantiate(obstaclePrefab).gameObject;
				obstacle.transform.position = GetWorldPosition(x, y, true);
				return obstacle;
			}

			return base.CreateGridObject(gridSystem, x, y);
		}
		private void Update()
		{
			if (Input.GetMouseButtonDown(0) && Physics.Raycast(Camera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
			{
				Vector3 worldPos = hit.point;
				if (pathfinderAgent3D != null)
				{
					pathfinderAgent3D.SetDestination(worldPos);
				}
				else
				{
					Debug.LogWarning("PathfinderAgent3D is not assigned.");
				}
			}
		}
	}
}