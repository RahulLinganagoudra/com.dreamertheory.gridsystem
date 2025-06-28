using UnityEngine;
using System.Collections.Generic;

namespace DT.GridSystem.Ruletile
{
	public struct RuleTilePrefabResult
	{
		public GameObject prefab;
		public Quaternion rotation;
		public RuleTilePrefabResult(GameObject prefab, Quaternion rotation)
		{
			this.prefab = prefab;
			this.rotation = rotation;
		}
	}

	public abstract class BaseRuleTile : ScriptableObject
	{
		public abstract RuleTilePrefabResult GetPrefabForPosition(int x, int y, Dictionary<Vector2Int, GameObject> placedTiles, HashSet<Vector2Int> selectedCells);
	}

	[CreateAssetMenu]
	public class RuleTile : BaseRuleTile
	{
		[Header("Tile Prefabs")]

		[Tooltip("Prefab for a single, isolated tile (no neighbors). Forward (Z+) should face up in grid.")]
		public GameObject singlePrefab;

		[Tooltip("Prefab for a tile completely surrounded by other tiles (center tile). Forward (Z+) should face up in grid.")]
		public GameObject centerPrefab;

		[Tooltip("Prefab for straight edges. Forward (Z+) should face UP. Rotation will be applied automatically for each direction.")]
		public GameObject edgePrefab;
		[Tooltip("Prefab for straight edges. Forward (Z+) should face UP. Rotation will be applied automatically for each direction.")]
		public GameObject edgeWith2SidePrefab;

		[Tooltip("Prefab for straight edges with one diagonal. Forward (Z+) should face UP.")]
		public GameObject edgePrefabWith1DiagonalL;
		[Tooltip("Prefab for straight edges with one diagonal. Forward (Z+) should face UP.")]
		public GameObject edgePrefabWith1DiagonalR;

		[Tooltip("Prefab for straight edges with two diagonals. Forward (Z+) should face UP.")]
		public GameObject edgePrefabWith2Diagonal;

		[Tooltip("Prefab for L-shaped corners. Forward (Z+) should face UP and RIGHT (open corner points up and right).")]
		public GameObject cornerPrefab;
		[Tooltip("Prefab for L-shaped corners. Forward (Z+) should face UP and RIGHT (open corner points up and right).")]
		public GameObject cornerPrefabWithCorner;

		[Tooltip("Prefab for inverted corners (missing only one diagonal neighbor). Forward (Z+) should face UP and RIGHT (filled corner points up and right).")]
		public GameObject invertedCornerPrefab;

		[Tooltip("Prefab for inverted corners (missing two adjacent diagonals). Forward (Z+) should face UP and RIGHT.")]
		public GameObject invertedTopPrefab;

		[Tooltip("Prefab for inverted corners (missing three diagonals). Forward (Z+) should face UP and RIGHT.")]
		public GameObject invertedTopThreePrefab;

		[Tooltip("Prefab for a cross (all 8 neighbors present except diagonals). Forward (Z+) should face up in grid.")]
		public GameObject crossPrefab;

		[Tooltip("Prefab for slash tiles (\\ or /). Forward (Z+) should face UP and RIGHT for '/'.")]
		public GameObject slashPrefab;

		[Tooltip("Prefab for a tile with only one neighbor (single edge). Forward (Z+) should face UP.")]
		public GameObject singleEdgePrefab;

		public override RuleTilePrefabResult GetPrefabForPosition(
			int x, int y,
			Dictionary<Vector2Int, GameObject> placedTiles,
			HashSet<Vector2Int> selectedCells)
		{
			bool HasTile(Vector2Int pos) =>
				placedTiles.ContainsKey(pos) || selectedCells.Contains(pos);

			bool hasTop = HasTile(new Vector2Int(x, y + 1));
			bool hasBottom = HasTile(new Vector2Int(x, y - 1));
			bool hasLeft = HasTile(new Vector2Int(x - 1, y));
			bool hasRight = HasTile(new Vector2Int(x + 1, y));

			bool hasTopLeft = HasTile(new Vector2Int(x - 1, y + 1));
			bool hasTopRight = HasTile(new Vector2Int(x + 1, y + 1));
			bool hasBottomLeft = HasTile(new Vector2Int(x - 1, y - 1));
			bool hasBottomRight = HasTile(new Vector2Int(x + 1, y - 1));

			int cardinalCount = (hasTop ? 1 : 0) + (hasBottom ? 1 : 0) + (hasLeft ? 1 : 0) + (hasRight ? 1 : 0);

			// 1. Isolated
			if (cardinalCount == 0)
				return new RuleTilePrefabResult(singlePrefab, Quaternion.identity);

			// 2. Single neighbor (edge)
			if (cardinalCount == 1)
			{
				if (hasTop)
					return new RuleTilePrefabResult(singleEdgePrefab, Quaternion.Euler(0, 180, 0)); // faces up (+Z)
				if (hasBottom)
					return new RuleTilePrefabResult(singleEdgePrefab, Quaternion.identity); // faces down (-Z)
				if (hasLeft)
					return new RuleTilePrefabResult(singleEdgePrefab, Quaternion.Euler(0, 90, 0)); // faces left (-X)
				if (hasRight)
					return new RuleTilePrefabResult(singleEdgePrefab, Quaternion.Euler(0, -90, 0)); // faces right (+X)
			}

			// 3. Fully surrounded but with one or more missing diagonals
			if (hasTop && hasBottom && hasLeft && hasRight)
			{
				// All diagonals missing
				if (!hasTopLeft && !hasTopRight && !hasBottomLeft && !hasBottomRight)
					return new RuleTilePrefabResult(crossPrefab, Quaternion.identity);

				// Three diagonals missing
				if (!hasTopLeft && !hasTopRight && !hasBottomLeft)
					return new RuleTilePrefabResult(invertedTopThreePrefab, Quaternion.identity);
				if (!hasTopLeft && !hasTopRight && !hasBottomRight)
					return new RuleTilePrefabResult(invertedTopThreePrefab, Quaternion.Euler(0, 90, 0));
				if (!hasTopRight && !hasBottomLeft && !hasBottomRight)
					return new RuleTilePrefabResult(invertedTopThreePrefab, Quaternion.Euler(0, 180, 0));
				if (!hasTopLeft && !hasBottomLeft && !hasBottomRight)
					return new RuleTilePrefabResult(invertedTopThreePrefab, Quaternion.Euler(0, -90, 0));

				// Two diagonals missing (adjacent)
				if (!hasTopLeft && !hasTopRight)
					return new RuleTilePrefabResult(invertedTopPrefab, Quaternion.identity);
				if (!hasBottomLeft && !hasBottomRight)
					return new RuleTilePrefabResult(invertedTopPrefab, Quaternion.Euler(0, 180, 0));
				if (!hasTopRight && !hasBottomRight)
					return new RuleTilePrefabResult(invertedTopPrefab, Quaternion.Euler(0, 90, 0));
				if (!hasTopLeft && !hasBottomLeft)
					return new RuleTilePrefabResult(invertedTopPrefab, Quaternion.Euler(0, -90, 0));

				// Diagonal slashes
				if (!hasTopRight && !hasBottomLeft)
					return new RuleTilePrefabResult(slashPrefab, Quaternion.Euler(0, 90, 0)); // "\"
				if (!hasTopLeft && !hasBottomRight)
					return new RuleTilePrefabResult(slashPrefab, Quaternion.identity); // "/"

				// Single inverted corners (only one diagonal missing)
				if (!hasTopLeft)
					return new RuleTilePrefabResult(invertedCornerPrefab, Quaternion.identity); // open up/right
				if (!hasTopRight)
					return new RuleTilePrefabResult(invertedCornerPrefab, Quaternion.Euler(0, 90, 0)); // open up/left
				if (!hasBottomLeft)
					return new RuleTilePrefabResult(invertedCornerPrefab, Quaternion.Euler(0, -90, 0)); // open down/right
				if (!hasBottomRight)
					return new RuleTilePrefabResult(invertedCornerPrefab, Quaternion.Euler(0, 180, 0)); // open down/left
			}

			// 4. L-shaped corners (missing two adjacent sides)
			if (!hasTop && !hasLeft)
			{
				if (!hasBottomRight)
					return new RuleTilePrefabResult(cornerPrefabWithCorner, Quaternion.identity); // missing top & left & topLeft
				return new RuleTilePrefabResult(cornerPrefab, Quaternion.identity); // missing top & left
			}
			if (!hasTop && !hasRight)
			{
				if (!hasBottomLeft)
					return new RuleTilePrefabResult(cornerPrefabWithCorner, Quaternion.Euler(0, 90, 0)); // missing top & right & topRight
				return new RuleTilePrefabResult(cornerPrefab, Quaternion.Euler(0, 90, 0)); // missing top & right
			}
			if (!hasBottom && !hasLeft)
			{
				if (!hasTopRight)
					return new RuleTilePrefabResult(cornerPrefabWithCorner, Quaternion.Euler(0, -90, 0)); // missing bottom & left & bottomLeft
				return new RuleTilePrefabResult(cornerPrefab, Quaternion.Euler(0, -90, 0)); // missing bottom & left
			}
			if (!hasBottom && !hasRight)
			{
				if (!hasTopLeft)
					return new RuleTilePrefabResult(cornerPrefabWithCorner, Quaternion.Euler(0, 180, 0)); // missing bottom & right & bottomRight
				return new RuleTilePrefabResult(cornerPrefab, Quaternion.Euler(0, 180, 0)); // missing bottom & right
			}

			// 5. Straight edges (missing two opposite sides)
			if (!hasBottom && !hasTop)
				return new RuleTilePrefabResult(edgeWith2SidePrefab, Quaternion.identity); // horizontal (left-right)
			if (!hasLeft && !hasRight)
				return new RuleTilePrefabResult(edgeWith2SidePrefab, Quaternion.Euler(0, 90, 0)); // vertical (up-down)

			// 6. Edges (only one side missing)
			if (!hasTop)
			{
				if (!hasBottomLeft && !hasBottomRight)
					return new RuleTilePrefabResult(edgePrefabWith2Diagonal, Quaternion.Euler(0, 180, 0)); // faces down
				else if (!hasBottomRight)
					return new RuleTilePrefabResult(edgePrefabWith1DiagonalL, Quaternion.Euler(0, 180, 0)); // faces down, missing bottom-right (L)
				else if (!hasBottomLeft)
					return new RuleTilePrefabResult(edgePrefabWith1DiagonalR, Quaternion.Euler(0, 180, 0)); // faces down, missing bottom-left (R)
				return new RuleTilePrefabResult(edgePrefab, Quaternion.Euler(0, 180, 0)); // faces down
			}
			if (!hasBottom)
			{
				if (!hasTopLeft && !hasTopRight)
					return new RuleTilePrefabResult(edgePrefabWith2Diagonal, Quaternion.identity); // faces up
				else if (!hasTopLeft)
					return new RuleTilePrefabResult(edgePrefabWith1DiagonalL, Quaternion.identity); // faces up, missing top-left (L)
				else if (!hasTopRight)
					return new RuleTilePrefabResult(edgePrefabWith1DiagonalR, Quaternion.identity); // faces up, missing top-right (R)
				return new RuleTilePrefabResult(edgePrefab, Quaternion.identity); // faces up
			}
			if (!hasLeft)
			{
				if (!hasTopRight && !hasBottomRight)
					return new RuleTilePrefabResult(edgePrefabWith2Diagonal, Quaternion.Euler(0, 90, 0)); // faces left
				else if (!hasTopRight)
					return new RuleTilePrefabResult(edgePrefabWith1DiagonalL, Quaternion.Euler(0, 90, 0)); // faces left, missing top-right (L)
				else if (!hasBottomRight)
					return new RuleTilePrefabResult(edgePrefabWith1DiagonalR, Quaternion.Euler(0, 90, 0)); // faces left, missing bottom-right (R)
				return new RuleTilePrefabResult(edgePrefab, Quaternion.Euler(0, 90, 0)); // faces left
			}
			if (!hasRight)
			{
				if (!hasTopLeft && !hasBottomLeft)
					return new RuleTilePrefabResult(edgePrefabWith2Diagonal, Quaternion.Euler(0, -90, 0)); // faces right
				else if (!hasBottomLeft)
					return new RuleTilePrefabResult(edgePrefabWith1DiagonalL, Quaternion.Euler(0, -90, 0)); // faces right, missing bottom-left (L)
				else if (!hasTopLeft)
					return new RuleTilePrefabResult(edgePrefabWith1DiagonalR, Quaternion.Euler(0, -90, 0)); // faces right, missing top-left (R)
				return new RuleTilePrefabResult(edgePrefab, Quaternion.Euler(0, -90, 0)); // faces right
			}

			// 7. Surrounded or middle
			return new RuleTilePrefabResult(centerPrefab, Quaternion.identity);
		}
	}
	[CreateAssetMenu]
	public class ObjectBrush : BaseRuleTile
	{
		[SerializeField] GameObject objectToPaint;
		override public RuleTilePrefabResult GetPrefabForPosition(int x, int y, Dictionary<Vector2Int, GameObject> placedTiles, HashSet<Vector2Int> selectedCells)
		{
			return new RuleTilePrefabResult(objectToPaint, Quaternion.identity);
		}
	}
}
