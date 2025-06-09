using System.Collections.Generic;
using UnityEngine;
namespace DT.GridSystem.Pathfinding
{
	public interface IPathfindingGrid
	{
		List<Vector2Int> GetNeighbors(Vector2Int position);
		int GetHeuristic(Vector2Int from, Vector2Int to);
		int GetMoveCost(Vector2Int from, Vector2Int to); // Optional, default to 10
		bool IsWalkable(Vector2Int position, PathfindingCullingMask mask = null);
		Vector2Int GridSize { get; }
	}

	public class AStarPathfinding
	{
		private readonly IPathfindingGrid grid;

		public AStarPathfinding(IPathfindingGrid grid)
		{
			this.grid = grid;
		}

		public Path FindPath(Vector2Int start, Vector2Int end, PathfindingCullingMask cullingMask = null)
		{
			if (!grid.IsWalkable(start, cullingMask) || !grid.IsWalkable(end, cullingMask))
				return new Path { hasPath = false, pathDistance = 0 };


			List<PathNode> openSet = new();
			HashSet<Vector2Int> closedSet = new();

			PathNode startNode = new(start, null, 0, grid.GetHeuristic(start, end));
			openSet.Add(startNode);

			while (openSet.Count > 0)
			{
				openSet.Sort((a, b) => a.F.CompareTo(b.F));
				PathNode current = openSet[0];
				openSet.RemoveAt(0);
				closedSet.Add(current.Position);

				if (current.Position == end)
					return RetracePath(current);

				foreach (var neighbor in grid.GetNeighbors(current.Position))
				{
					if (closedSet.Contains(neighbor) || !grid.IsWalkable(neighbor, cullingMask))
						continue;

					int moveCost = grid.GetMoveCost(current.Position, neighbor);
					int newG = current.G + moveCost;
					int h = grid.GetHeuristic(neighbor, end);

					PathNode neighborNode = new(neighbor, current, newG, h);
					var existing = openSet.Find(n => n.Position == neighbor);
					if (existing == null)
						openSet.Add(neighborNode);
					else if (newG < existing.G)
					{
						existing.G = newG;
						existing.Parent = current;
						existing.F = existing.G + existing.H;
					}
				}
			}

			return new Path { hasPath = false, pathDistance = 0 };
		}

		private Path RetracePath(PathNode end)
		{
			List<Vector2Int> waypoints = new();
			float distance = 0;
			var current = end;
			while (current != null)
			{
				if (current.Parent != null)
					distance += Vector2Int.Distance(current.Position, current.Parent.Position);
				waypoints.Add(current.Position);
				current = current.Parent;
			}
			waypoints.Reverse();
			return new Path { hasPath = true, pathDistance = distance, wayPoints = waypoints };
		}
	}

	public class PathNode
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
	public class Path
	{
		public bool hasPath;
		public float pathDistance;
		public List<Vector2Int> wayPoints;
	}
	public class HexGrid : IPathfindingGrid
	{
		private readonly Vector2Int gridSize;
		private readonly int cost;
		private NavmeshData NavmeshData;


		private static readonly Vector2Int[] evenQ = new Vector2Int[]
		{
			new(+1, 0), new(0, -1), new(-1, -1), new(-1, 0), new(-1, +1), new(0, +1)
		};
		private static readonly Vector2Int[] oddQ = new Vector2Int[]
		{
			new(+1, 0), new(+1, -1), new(0, -1), new(-1, 0), new(0, +1), new(+1, +1)
		};
		private static readonly Vector2Int[] evenR = new Vector2Int[]
		{
			new(+0, 1), new(-1, 0), new(-1, -1),
			new(0, -1), new(+1, -1), new(1, 0)
		};
		private static readonly Vector2Int[] oddR = new Vector2Int[]
		{
			new(0, 1), new(-1, +1), new(-1, 0),
			new(0, -1), new(1, 0), new(+1, +1)
		};
		public HexGrid(Vector2Int gridSize, int cost, NavmeshData navmeshData)
		{
			this.gridSize = gridSize;
			this.cost = cost;
			NavmeshData = navmeshData;
		}

		public Vector2Int GridSize => gridSize;

		public List<Vector2Int> GetNeighbors(Vector2Int pos)
		{
			Vector2Int[] directions;
			if (hexOrientation == HexOrientation.PointyTop)
			{
				if (pos.y % 2 == 0)
				{
					directions = evenQ;
				}
				else
				{
					directions = oddQ;
				}

			}
			else
			{
				if (pos.x % 2 == 0)
				{
					directions = evenR;
				}
				else
				{
					directions = oddR;
				}
			}
			List<Vector2Int> result = new();
			foreach (var dir in directions)
			{
				var neighbor = pos + dir;
				if (neighbor.x >= 0 && neighbor.y >= 0 && neighbor.x < GridSize.x && neighbor.y < GridSize.y)
					result.Add(neighbor);
			}
			return result;
		}

		public int GetHeuristic(Vector2Int a, Vector2Int b)
		{
			Vector3Int ac = OffsetToCube(a);
			Vector3Int bc = OffsetToCube(b);
			return Mathf.Max(Mathf.Abs(ac.x - bc.x), Mathf.Abs(ac.y - bc.y), Mathf.Abs(ac.z - bc.z)) * cost;
		}

		public int GetMoveCost(Vector2Int from, Vector2Int to) => cost;

		public bool IsWalkable(Vector2Int pos, PathfindingCullingMask mask = null) => NavmeshData.IsWalkable(pos, mask);
		private Vector3Int OffsetToCube(Vector2Int offset)
		{
			int x = offset.x - (offset.y - (offset.y & 1)) / 2;
			int z = offset.y;
			int y = -x - z;
			return new Vector3Int(x, y, z);
		}
	}
	public class RectGridPathfinding : IPathfindingGrid
	{
		private readonly NavmeshData navmesh;
		private readonly bool allowDiagonals;
		private readonly int straightCost, diagonalCost;

		public RectGridPathfinding(NavmeshData navmesh, bool allowDiagonals, int straightCost = 10, int diagonalCost = 14)
		{
			this.navmesh = navmesh;
			this.allowDiagonals = allowDiagonals;
			this.straightCost = straightCost;
			this.diagonalCost = diagonalCost;
		}

		public List<Vector2Int> GetNeighbors(Vector2Int pos)
		{
			Vector2Int[] directions = allowDiagonals ?
				new Vector2Int[] { new(-1, 0), new(1, 0), new(0, -1), new(0, 1), new(-1, -1), new(-1, 1), new(1, -1), new(1, 1) } :
				new Vector2Int[] { new(-1, 0), new(1, 0), new(0, -1), new(0, 1) };

			List<Vector2Int> result = new();
			foreach (var dir in directions)
			{
				var neighbor = pos + dir;
				if (neighbor.x >= 0 && neighbor.y >= 0 && neighbor.x < navmesh.gridSize.x && neighbor.y < navmesh.gridSize.y)
					result.Add(neighbor);
			}
			return result;
		}

		public int GetHeuristic(Vector2Int from, Vector2Int to)
		{
			int dx = Mathf.Abs(from.x - to.x);
			int dy = Mathf.Abs(from.y - to.y);
			if (allowDiagonals)
				return diagonalCost * Mathf.Min(dx, dy) + straightCost * Mathf.Abs(dx - dy);
			else
				return (dx + dy) * straightCost;
		}

		public int GetMoveCost(Vector2Int from, Vector2Int to)
		{
			int dx = Mathf.Abs(from.x - to.x);
			int dy = Mathf.Abs(from.y - to.y);
			return (dx == 1 && dy == 1) ? diagonalCost : straightCost;
		}

		public bool IsWalkable(Vector2Int pos, PathfindingCullingMask mask = null) => navmesh.IsWalkable(pos, mask);
		public Vector2Int GridSize => navmesh.gridSize;
	}

}
