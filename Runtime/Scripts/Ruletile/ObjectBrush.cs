using System.Collections.Generic;
using UnityEngine;

namespace DT.GridSystem.Ruletile
{
    [CreateAssetMenu(fileName = "ObjectBrush", menuName = "DT/GridSystem/ObjectBrush")]
	public class ObjectBrush : BaseRuleTile
	{
		[SerializeField] GameObject objectToPaint;
		override public RuleTilePrefabResult GetPrefabForPosition(int x, int y, Dictionary<Vector2Int, GameObject> placedTiles, HashSet<Vector2Int> selectedCells)
		{
			return new RuleTilePrefabResult(objectToPaint, Quaternion.identity);
		}
	}
}