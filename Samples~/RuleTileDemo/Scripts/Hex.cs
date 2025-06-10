using DT.GridSystem;
using System.Collections.Generic;
using UnityEngine;

public class Hex : HexGridSystem3D<GameObject>
{
	[SerializeField] GameObject prefab;
	[SerializeField, Range(1, 5)] int x;


	protected override void Awake()
	{
		base.Awake();
		// gridsize x/2 & y/2 to get center point of the grid
		DrawBFS((GridSize.x >> 1), (GridSize.y >> 1), x);
	}
	void DrawBFS(int startX, int startY, int maxRing)
	{
		Vector2Int start = new(startX, startY);
		HashSet<Vector2Int> visited = new();
		Queue<(Vector2Int pos, int depth)> queue = new();

		queue.Enqueue((start, 0));
		visited.Add(start);

		while (queue.Count > 0)
		{
			var (pos, depth) = queue.Dequeue();
			if (depth >= maxRing) continue;

			var hex = Instantiate(prefab, GetWorldPosition(pos.x, pos.y, true), Quaternion.identity);
			AddGridObject(pos.x, pos.y, hex, true);

			foreach (var neighbor in GetNeighbors(pos))
			{
				if (!visited.Contains(neighbor) && !TryGetGridObject(neighbor.x, neighbor.y, out _))
				{
					queue.Enqueue((neighbor, depth + 1));
					visited.Add(neighbor);
				}
			}
		}
	}
}
