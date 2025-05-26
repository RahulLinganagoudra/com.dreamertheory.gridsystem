namespace DT.GridSystem.Pathfinding
{
	using System.Collections.Generic;
	using UnityEngine;

	public class AStarPathfinding
	{
		private NavmeshData navmesh;
		private bool allowDiagonals;

		public AStarPathfinding(NavmeshData navmesh, bool allowDiagonals = false)
		{
			this.navmesh = navmesh;
			this.allowDiagonals = navmesh.allowDiagonals;
		}

		public Path FindPath(Vector2Int start, Vector2Int end, PathfindingCullingMask cullingMask = null)
		{
			List<PathNode> openSet = new();
			HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();

			PathNode startNode = new PathNode(start, null, 0, GetHeuristic(start, end));
			openSet.Add(startNode);

			while (openSet.Count > 0)
			{
				openSet.Sort((a, b) => a.F.CompareTo(b.F));
				PathNode currentNode = openSet[0];
				openSet.RemoveAt(0);
				closedSet.Add(currentNode.Position);

				if (currentNode.Position == end)
				{
					return RetracePath(currentNode);
				}

				foreach (Vector2Int neighbor in GetNeighbors(currentNode.Position))
				{
					if (closedSet.Contains(neighbor) || !navmesh.IsWalkable(neighbor,cullingMask))
						continue;

					int newGCost = currentNode.G + 1;
					PathNode neighborNode = new PathNode(neighbor, currentNode, newGCost, GetHeuristic(neighbor, end));

					PathNode existingNode = openSet.Find(n => n.Position == neighbor);
					if (existingNode == null)
					{
						openSet.Add(neighborNode);
					}
					else if (newGCost < existingNode.G)
					{
						existingNode.Parent = currentNode;
						existingNode.G = newGCost;
						existingNode.F = existingNode.G + existingNode.H;
					}
				}
			}
			return new Path() { hasPath = false, pathDistance = 0 };
		}

		private Path RetracePath(PathNode endNode)
		{
			float distance = 0;
			List<Vector2Int> pathWayPoints = new List<Vector2Int>();
			PathNode currentNode = endNode;
			while (currentNode != null)
			{
				if (currentNode.Parent != null)
				{
					distance += (currentNode.Position - currentNode.Parent.Position).magnitude;
				}
				pathWayPoints.Add(currentNode.Position);
				currentNode = currentNode.Parent;
			}
			pathWayPoints.Reverse();

			return new Path() { hasPath = pathWayPoints.Count > 0, pathDistance = distance, wayPoints = pathWayPoints };
		}

		private List<Vector2Int> GetNeighbors(Vector2Int position)
		{
			List<Vector2Int> neighbors = new List<Vector2Int>();
			Vector2Int[] directions = allowDiagonals ?
				new Vector2Int[] { new(-1, 0), new(1, 0), new(0, -1), new(0, 1), new(-1, -1), new(-1, 1), new(1, -1), new(1, 1) } :
				new Vector2Int[] { new(-1, 0), new(1, 0), new(0, -1), new(0, 1) };

			foreach (Vector2Int dir in directions)
			{
				Vector2Int neighborPos = position + dir;
				if (neighborPos.x >= 0 && neighborPos.y >= 0 && neighborPos.x < navmesh.gridSize.x && neighborPos.y < navmesh.gridSize.y)
				{
					neighbors.Add(neighborPos);
				}
			}
			return neighbors;
		}

		private int GetHeuristic(Vector2Int a, Vector2Int b)
		{
			return allowDiagonals ? Mathf.RoundToInt(Vector2Int.Distance(a, b) * 10) : Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
		}



		private class PathNode
		{
			public Vector2Int Position;
			public PathNode Parent;
			public int G, H, F;

			public PathNode(Vector2Int position, PathNode parent, int g, int h)
			{
				Position = position;
				Parent = parent;
				G = g;
				H = h;
				F = G + H;
			}
		}
	}
	public class Path
	{
		public bool hasPath;
		public float pathDistance;
		public List<Vector2Int> wayPoints;
	}
}