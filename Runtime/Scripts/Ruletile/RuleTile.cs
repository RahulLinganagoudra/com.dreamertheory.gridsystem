using UnityEngine;
using System.Collections.Generic;

namespace DT.GridSystem.Ruletile
{

	public abstract class BaseRuleTile : ScriptableObject
	{
		// This class can be extended for more complex rule tiles if needed
		// Currently, it serves as a base for RuleTile
		public abstract GameObject GetPrefabForPosition(int x, int y, Dictionary<Vector2Int, GameObject> placedTiles, HashSet<Vector2Int> selectedCells);
	}

	[CreateAssetMenu]
	public class RuleTile : BaseRuleTile
	{
		[Header("Tile Prefabs")]
		public GameObject single;
		public GameObject centerPrefab;
		public GameObject topPrefab;
		public GameObject bottomPrefab;
		public GameObject leftPrefab;
		public GameObject rightPrefab;

		public GameObject topLeftPrefab;
		public GameObject topRightPrefab;
		public GameObject bottomLeftPrefab;
		public GameObject bottomRightPrefab;

		public GameObject invertedTopLeftPrefab;
		public GameObject invertedTopRightPrefab;
		public GameObject invertedBottomLeftPrefab;
		public GameObject invertedBottomRightPrefab;

		public GameObject forwardSlashPrefab;
		public GameObject backwardSlashPrefab;

		public GameObject singleTopPrefab;
		public GameObject singleBottomPrefab;
		public GameObject singleLeftPrefab;
		public GameObject singleRightPrefab;

		public override GameObject GetPrefabForPosition(
			int x, int y,
			Dictionary<Vector2Int, GameObject> placedTiles,
			HashSet<Vector2Int> selectedCells)
		{
			bool HasTile(Vector2Int pos) =>
				placedTiles.ContainsKey(pos) || selectedCells.Contains(pos);

			bool top = HasTile(new Vector2Int(x, y + 1));
			bool bottom = HasTile(new Vector2Int(x, y - 1));
			bool left = HasTile(new Vector2Int(x - 1, y));
			bool right = HasTile(new Vector2Int(x + 1, y));

			bool topLeft = HasTile(new Vector2Int(x - 1, y + 1));
			bool topRight = HasTile(new Vector2Int(x + 1, y + 1));
			bool bottomLeft = HasTile(new Vector2Int(x - 1, y - 1));
			bool bottomRight = HasTile(new Vector2Int(x + 1, y - 1));

			int cardinalCount = (top ? 1 : 0) + (bottom ? 1 : 0) + (left ? 1 : 0) + (right ? 1 : 0);

			// 1. Isolated
			if (cardinalCount == 0)
				return single;

			// 2. Single neighbor
			if (cardinalCount == 1)
			{
				if (top) return singleBottomPrefab;
				if (bottom) return singleTopPrefab;
				if (left) return singleRightPrefab;
				if (right) return singleLeftPrefab;
			}

			// 3. Fully surrounded but with one missing diagonal -> inverted corners
			if (top && bottom && left && right)
			{
				if (!topRight && !bottomLeft) return backwardSlashPrefab;
				if (!topLeft && !bottomRight) return forwardSlashPrefab;
				if (!topLeft) return invertedTopLeftPrefab;
				if (!topRight) return invertedTopRightPrefab;
				if (!bottomLeft) return invertedBottomLeftPrefab;
				if (!bottomRight) return invertedBottomRightPrefab;
			}

			// 4. L-shaped corners (missing two adjacent sides)
			if (!top && !left) return topLeftPrefab;
			if (!top && !right) return topRightPrefab;
			if (!bottom && !left) return bottomLeftPrefab;
			if (!bottom && !right) return bottomRightPrefab;

			// 5. Edges (only one side missing)
			if (!top) return topPrefab;
			if (!bottom) return bottomPrefab;
			if (!left) return leftPrefab;
			if (!right) return rightPrefab;

			// 6. Surrounded or middle
			return centerPrefab;
		}
	}

}
