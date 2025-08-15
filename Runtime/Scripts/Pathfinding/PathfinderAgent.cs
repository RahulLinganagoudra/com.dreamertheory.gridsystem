using UnityEngine;

namespace DT.GridSystem.Pathfinding
{
	public class PathfinderAgent : MonoBehaviour
	{
		private GridSystem<GameObject> grid;
		private AStarPathfinding pathfinding;
		private Path path;
		[SerializeField] PathfindingLayer pathfindingLayer;
		private int pathIndex;
		public float moveSpeed = 5f;
		[SerializeField] private Vector2Int agentSize = new Vector2Int(1, 1);

		public PathfindingLayer Layer => pathfindingLayer;
		public Vector2Int AgentSize { get => agentSize; set => agentSize = value; }
		public PathfindingCullingMask CullingMask => pathfindingLayer.GetMask();

		public void Initialize(GridSystem<GameObject> grid, AStarPathfinding pathfinding)
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

		// Static pathfinding methods (unchanged)
		public Path GetPath(Vector3 target) => GetPath(target, CullingMask);

		public Path GetPath(Vector3 target, PathfindingCullingMask cullingMask)
		{
			grid.GetGridPosition(transform.position, out int x, out int y);
			grid.GetGridPosition(target, out int X, out int Y);

			if (agentSize.x > 1 || agentSize.y > 1)
				return pathfinding.FindPathMultiCell(new(x, y), new(X, Y), agentSize, cullingMask);
			else
				return pathfinding.FindPath(new(x, y), new(X, Y), cullingMask);
		}

		private bool PathsAreEqual(Path path1, Path path2)
		{
			if (path1.wayPoints.Count != path2.wayPoints.Count) return false;
			for (int i = 0; i < path1.wayPoints.Count; i++)
			{
				if (path1.wayPoints[i] != path2.wayPoints[i]) return false;
			}
			return true;
		}

		// Rest of your existing methods...
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

#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			if (path == null || !path.hasPath) return;
			for (int i = 1; i < path.wayPoints.Count; i++)
			{
				var start = path.wayPoints[i - 1];
				var end = path.wayPoints[i];
				Gizmos.DrawLine(grid.GetWorldPosition(start.x, start.y, true), grid.GetWorldPosition(end.x, end.y, true));
			}
		}
#endif
	}
}