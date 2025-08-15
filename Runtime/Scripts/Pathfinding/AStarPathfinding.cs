using System.Collections.Generic;
using UnityEngine;

namespace DT.GridSystem.Pathfinding
{
	public abstract class AStarPathfinding
	{
		protected GridSystem<GameObject> gridSystem;
		private NavmeshData navmeshData;

		public abstract Vector2Int GridSize { get; }
		public NavmeshData NavmeshData { get => navmeshData; protected set => navmeshData = value; }

		public abstract List<Vector2Int> GetNeighbors(Vector2Int pos);
		public abstract int GetHeuristic(Vector2Int from, Vector2Int to);
		public abstract int GetMoveCost(Vector2Int from, Vector2Int to);
		public abstract bool IsWalkable(Vector2Int position, PathfindingCullingMask mask);


		protected AStarPathfinding(GridSystem<GameObject> gridSystem)
		{
			this.gridSystem = gridSystem;
		}


		public Path FindPath(Vector2Int start, Vector2Int end, PathfindingCullingMask cullingMask)
		{
			if (!IsWalkable(end, cullingMask))
				return new Path { hasPath = false, pathDistance = 0 };

			List<PathNode> openSet = new();
			HashSet<Vector2Int> closedSet = new();
			PathNode startNode = new(start, null, 0, GetHeuristic(start, end));
			openSet.Add(startNode);

			while (openSet.Count > 0)
			{
				openSet.Sort((a, b) => a.F.CompareTo(b.F));
				PathNode current = openSet[0];
				openSet.RemoveAt(0);
				closedSet.Add(current.Position);

				if (current.Position == end)
					return RetracePath(current);

				foreach (var neighbor in GetNeighbors(current.Position))
				{
					if (closedSet.Contains(neighbor) || !IsWalkable(neighbor, cullingMask))
						continue;

					int moveCost = GetMoveCost(current.Position, neighbor);
					int newG = current.G + moveCost;
					int h = GetHeuristic(neighbor, end);
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
		public Path FindPath(Vector2Int start, Vector2Int end)
		{
			return FindClosestPath(start, end, PathfindingCullingMask.AllMask);
		}

		public Path FindClosestPath(Vector2Int start, Vector2Int end, int searchLimit = 100)
		{
			return FindClosestPath(start, end, PathfindingCullingMask.AllMask, searchLimit);
		}
		public Path FindClosestPath(Vector2Int start, Vector2Int end, PathfindingCullingMask cullingMask, int searchLimit = 100)
		{
			Path normalPath = FindPath(start, end, cullingMask);
			if (normalPath.hasPath)
				return normalPath;

			List<PathNode> openSet = new();
			HashSet<Vector2Int> closedSet = new();
			PathNode startNode = new(start, null, 0, GetHeuristic(start, end));
			PathNode closestNode = startNode;
			int closestDistance = GetHeuristic(start, end);
			int searchedNodes = 0;

			openSet.Add(startNode);

			while (openSet.Count > 0 && searchedNodes < searchLimit)
			{
				openSet.Sort((a, b) => a.F.CompareTo(b.F));
				PathNode current = openSet[0];
				openSet.RemoveAt(0);
				closedSet.Add(current.Position);
				searchedNodes++;

				int distanceToTarget = GetHeuristic(current.Position, end);
				if (distanceToTarget < closestDistance)
				{
					closestDistance = distanceToTarget;
					closestNode = current;
				}

				if (distanceToTarget <= 1)
					break;

				foreach (var neighbor in GetNeighbors(current.Position))
				{
					if (closedSet.Contains(neighbor) || !IsWalkable(neighbor, cullingMask))
						continue;

					int moveCost = GetMoveCost(current.Position, neighbor);
					int newG = current.G + moveCost;
					int h = GetHeuristic(neighbor, end);
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
			return RetracePath(closestNode);
		}

		public Path FindPathMultiCell(Vector2Int start, Vector2Int end, Vector2Int agentSize)
		{
			return FindClosestPathMultiCell(start, end, agentSize, PathfindingCullingMask.AllMask);
		}
		public Path FindPathMultiCell(Vector2Int start, Vector2Int end, Vector2Int agentSize, PathfindingCullingMask cullingMask)
		{
			if (!CanAgentFit(end, agentSize, cullingMask))
				return new Path { hasPath = false, pathDistance = 0 };

			List<PathNode> openSet = new();
			HashSet<Vector2Int> closedSet = new();
			PathNode startNode = new(start, null, 0, GetHeuristic(start, end));
			openSet.Add(startNode);

			while (openSet.Count > 0)
			{
				openSet.Sort((a, b) => a.F.CompareTo(b.F));
				PathNode current = openSet[0];
				openSet.RemoveAt(0);
				closedSet.Add(current.Position);

				if (current.Position == end)
					return RetracePath(current);

				foreach (var neighbor in GetNeighbors(current.Position))
				{
					if (closedSet.Contains(neighbor) || !CanAgentFit(neighbor, agentSize, cullingMask, current.Position))
						continue;

					int moveCost = GetMoveCost(current.Position, neighbor);
					int newG = current.G + moveCost;
					int h = GetHeuristic(neighbor, end);
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

		public Path FindClosestPathMultiCell(Vector2Int start, Vector2Int end, Vector2Int agentSize, int searchLimit = 100)
		{
			return FindClosestPathMultiCell(start, end, agentSize, PathfindingCullingMask.AllMask, searchLimit);
		}
		public Path FindClosestPathMultiCell(Vector2Int start, Vector2Int end, Vector2Int agentSize, PathfindingCullingMask cullingMask, int searchLimit = 100)
		{
			Path normalPath = FindPathMultiCell(start, end, agentSize, cullingMask);
			if (normalPath.hasPath)
				return normalPath;

			List<PathNode> openSet = new();
			HashSet<Vector2Int> closedSet = new();
			PathNode startNode = new(start, null, 0, GetHeuristic(start, end));
			PathNode closestNode = startNode;
			int closestDistance = GetHeuristic(start, end);
			int searchedNodes = 0;

			openSet.Add(startNode);

			while (openSet.Count > 0 && searchedNodes < searchLimit)
			{
				openSet.Sort((a, b) => a.F.CompareTo(b.F));
				PathNode current = openSet[0];
				openSet.RemoveAt(0);
				closedSet.Add(current.Position);
				searchedNodes++;

				int distanceToTarget = GetHeuristic(current.Position, end);
				if (distanceToTarget < closestDistance)
				{
					closestDistance = distanceToTarget;
					closestNode = current;
				}

				if (distanceToTarget <= 1)
					break;

				foreach (var neighbor in GetNeighbors(current.Position))
				{
					if (closedSet.Contains(neighbor) || !CanAgentFit(neighbor, agentSize, cullingMask, current.Position))
						continue;

					int moveCost = GetMoveCost(current.Position, neighbor);
					int newG = current.G + moveCost;
					int h = GetHeuristic(neighbor, end);
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
			return RetracePath(closestNode);
		}

		public bool CanAgentFit(Vector2Int centerPosition, Vector2Int agentSize, Vector2Int? ignoreCenter = null)
		{
			return CanAgentFit(centerPosition, agentSize, PathfindingCullingMask.AllMask, ignoreCenter);
		}
		public bool CanAgentFit(Vector2Int centerPosition, Vector2Int agentSize, PathfindingCullingMask cullingMask, Vector2Int? ignoreCenter = null)
		{
			int halfX = (agentSize.x - 1) / 2;
			int halfY = (agentSize.y - 1) / 2;

			for (int x = -halfX; x <= halfX; x++)
			{
				for (int y = -halfY; y <= halfY; y++)
				{
					Vector2Int checkPos = centerPosition + new Vector2Int(x, y);

					if (ignoreCenter.HasValue && IsPositionInAgentFootprint(checkPos, ignoreCenter.Value, agentSize))
						continue;

					if (!navmeshData.IsWalkable(checkPos, cullingMask))
						return false;
				}
			}
			return true;
		}

		private bool IsPositionInAgentFootprint(Vector2Int position, Vector2Int agentCenter, Vector2Int agentSize)
		{
			int halfX = (agentSize.x - 1) / 2;
			int halfY = (agentSize.y - 1) / 2;
			return position.x >= agentCenter.x - halfX && position.x <= agentCenter.x + halfX &&
				   position.y >= agentCenter.y - halfY && position.y <= agentCenter.y + halfY;
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


	public class HexGrid : AStarPathfinding
	{
		HexGridSystem3D<GameObject> hexGridSystem;
		private readonly int cost;

		public HexGrid(HexGridSystem3D<GameObject> hexGridSystem, int cost, NavmeshData navmeshData) : base(hexGridSystem)
		{
			this.hexGridSystem = hexGridSystem;
			this.cost = cost;
			NavmeshData = navmeshData;
		}

		public override Vector2Int GridSize => NavmeshData.gridSize;

		public override List<Vector2Int> GetNeighbors(Vector2Int pos)
		{
			return hexGridSystem.GetNeighbors(pos);
		}

		public override int GetHeuristic(Vector2Int a, Vector2Int b)
		{
			Vector3Int ac = OffsetToCube(a);
			Vector3Int bc = OffsetToCube(b);
			return Mathf.Max(Mathf.Abs(ac.x - bc.x), Mathf.Abs(ac.y - bc.y), Mathf.Abs(ac.z - bc.z)) * cost;
		}

		public override int GetMoveCost(Vector2Int from, Vector2Int to) => cost;

		public override bool IsWalkable(Vector2Int pos, PathfindingCullingMask mask) => NavmeshData.IsWalkable(pos, mask);

		private Vector3Int OffsetToCube(Vector2Int offset)
		{
			int x = offset.x - (offset.y - (offset.y & 1)) / 2;
			int z = offset.y;
			int y = -x - z;
			return new Vector3Int(x, y, z);
		}
	}

	public class RectGridPathfinding : AStarPathfinding
	{
		private readonly RectGridSystem<GameObject> rectGridSystem;
		private readonly bool allowDiagonals;
		private readonly int straightCost, diagonalCost;

		public RectGridPathfinding(RectGridSystem<GameObject> rectGridSystem, NavmeshData navmesh, bool allowDiagonals, int straightCost = 10, int diagonalCost = 14) : base(rectGridSystem)
		{
			this.rectGridSystem = rectGridSystem;
			this.NavmeshData = navmesh;
			this.allowDiagonals = allowDiagonals;
			this.straightCost = straightCost;
			this.diagonalCost = diagonalCost;
		}

		public override List<Vector2Int> GetNeighbors(Vector2Int pos)
		{
			return rectGridSystem.GetNeighbors(pos, allowDiagonals);
		}

		public override int GetHeuristic(Vector2Int from, Vector2Int to)
		{
			int dx = Mathf.Abs(from.x - to.x);
			int dy = Mathf.Abs(from.y - to.y);
			if (allowDiagonals)
				return diagonalCost * Mathf.Min(dx, dy) + straightCost * Mathf.Abs(dx - dy);
			else
				return (dx + dy) * straightCost;
		}

		public override int GetMoveCost(Vector2Int from, Vector2Int to)
		{
			int dx = Mathf.Abs(from.x - to.x);
			int dy = Mathf.Abs(from.y - to.y);
			return (dx == 1 && dy == 1) ? diagonalCost : straightCost;
		}

		public override bool IsWalkable(Vector2Int pos, PathfindingCullingMask mask) => NavmeshData.IsWalkable(pos, mask);

		public override Vector2Int GridSize => NavmeshData.gridSize;
	}

}
