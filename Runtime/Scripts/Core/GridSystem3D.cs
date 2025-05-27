using UnityEngine;
namespace DT.GridSystem
{
	public class GridSystem3D<TGridObject> : GridSystem<TGridObject>
	{
		public override Vector3 GetWorldPosition(int x, int y, bool snapToGrid = false)
		{
			if (snapToGrid)
			{
				return (new Vector3(x - gridSize.x / 2f, 0, y - gridSize.y / 2f) * CellSize + transform.position) + new Vector3(CellSize, 0, CellSize) * 0.5f;
			}
			else
			{
				return new Vector3(x - gridSize.x / 2f, 0, y - gridSize.y / 2f) * CellSize + transform.position;
			}
		}

		public override void GetGridPosition(Vector3 worldPosition, out int x, out int y)
		{
			float relativeX = (worldPosition.x - transform.position.x) / CellSize;
			float relativeY = (worldPosition.z - transform.position.z) / CellSize;

			x = Mathf.FloorToInt(relativeX + gridSize.x / 2f);
			y = Mathf.FloorToInt(relativeY + gridSize.y / 2f);

			x = Mathf.Clamp(x, 0, gridSize.x - 1);
			y = Mathf.Clamp(y, 0, gridSize.y - 1);
		}


		public override void OnDrawGizmos()
		{
			if (!drawGizmos) return;
			base.OnDrawGizmos();
			Gizmos.DrawWireCube(transform.position, new Vector3(gridSize.x, 0, gridSize.y) * CellSize);

			for (int x = 0; x < gridSize.x; x++)
			{
				for (int y = 0; y < gridSize.y; y++)
				{
					// Draw the grid object at the center of the cell
					TGridObject gridObject = GetGridObject(x, y);
					if (gridObject != null)
					{
						Vector3 objectPosition = GetWorldPosition(x, y, true);
						Gizmos.DrawSphere(objectPosition, CellSize * 0.1f);
					}
				}
			}
		}
	}
}