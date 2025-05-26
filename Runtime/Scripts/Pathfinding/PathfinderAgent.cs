using UnityEngine;

namespace DT.GridSystem.Pathfinding
{
	public class PathfinderAgent<TGridObject> : MonoBehaviour
	{
		private GridSystem<TGridObject> grid;
		private AStarPathfinding pathfinding;
		private Path path;
		[SerializeField] PathfindingCullingMask cullingMask;
		private int pathIndex;
		public float moveSpeed = 5f;

		public PathfindingCullingMask CullingMask { get => cullingMask; set => cullingMask = value; }

		public void Initialize(GridSystem<TGridObject> grid, AStarPathfinding pathfinding)
		{
			this.grid = grid;
			this.pathfinding = pathfinding;
		}

		public bool SetDestination(Vector3 target)
		{
			path = GetPath(target, CullingMask);
			FollowPath(path);
			return path != null && path.hasPath;
		}
		public Path GetPath(Vector3 target, PathfindingCullingMask cullingMask = null)
		{
			grid.GetGridPosition(transform.position, out int x, out int y);
			grid.GetGridPosition(target, out int X, out int Y);

			return pathfinding.FindPath(new(x, y), new(X, Y), cullingMask);
		}
		public Path GetPath(Vector3 startPoint, Vector3 endPoint, PathfindingCullingMask cullingMask = null)
		{
			grid.GetGridPosition(startPoint, out int x, out int y);
			grid.GetGridPosition(endPoint, out int X, out int Y);
			return pathfinding.FindPath(new(x, y), new(X, Y), cullingMask);
		}
		public void FollowPath(Path path)
		{
			if (path != null && path.hasPath)
			{
				pathIndex = 0;
				StopAllCoroutines();
				StartCoroutine(FollowPath());
			}
		}
		private System.Collections.IEnumerator FollowPath()
		{
			while (path != null && path.hasPath && pathIndex < path.wayPoints.Count)
			{
				Vector3 targetPos = grid.GetWorldPosition(path.wayPoints[pathIndex].x, path.wayPoints[pathIndex].y, true);
				while (Vector3.Distance(transform.position, targetPos) > 0.1f)
				{
					transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
					yield return null;
				}
				pathIndex++;
			}
		}
	}

}