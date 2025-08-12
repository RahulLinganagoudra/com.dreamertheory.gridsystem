using UnityEngine;

namespace DT.GridSystem
{
	/// <summary>
	/// A 2D grid system that supports mapping between grid and world positions.
	/// Provides visualization and runtime access to grid objects.
	/// </summary>
	/// <typeparam name="TGridObject">The type of object stored in each grid cell.</typeparam>
	/// <remarks>
	/// This class extends the generic <see cref="GridSystem{TGridObject}"/> class to implement 
	/// functionality specific to 2D grid layouts, such as 2D position snapping and coordinate translation.
	/// </remarks>
	public class GridSystem2D<TGridObject> : RectGridSystem<TGridObject>
	{
		/// <summary>
		/// Converts grid coordinates (x, y) to a world position.
		/// </summary>
		/// <param name="x">The x-coordinate in the grid.</param>
		/// <param name="y">The y-coordinate in the grid.</param>
		/// <param name="snapToGrid">
		/// If true, returns the center of the grid cell; if false, returns the bottom-left corner.
		/// </param>
		/// <returns>The corresponding world position as a <see cref="Vector3"/>.</returns>
		public override Vector3 GetWorldPosition(int x, int y, bool snapToGrid = true)
		{
			if (!snapToGrid)
				return new Vector3(x - GridSize.x / 2f, y - GridSize.y / 2f, 0) * CellSize + transform.position;
			else
				return (new Vector3(x - GridSize.x / 2f, y - GridSize.y / 2f, 0) * CellSize + transform.position) + new Vector3(CellSize, CellSize, 0) * 0.5f;
		}

		/// <summary>
		/// Converts a world position to grid coordinates (x, y).
		/// </summary>
		/// <param name="worldPosition">The world position to convert.</param>
		/// <param name="x">The resulting x-coordinate in the grid.</param>
		/// <param name="y">The resulting y-coordinate in the grid.</param>
		public override void GetGridPosition(Vector3 worldPosition, out int x, out int y)
		{
			float relativeX = (worldPosition.x - transform.position.x) / CellSize;
			float relativeY = (worldPosition.y - transform.position.y) / CellSize;

			x = Mathf.FloorToInt(relativeX + GridSize.x / 2f);
			y = Mathf.FloorToInt(relativeY + GridSize.y / 2f);

			x = Mathf.Clamp(x, 0, GridSize.x - 1);
			y = Mathf.Clamp(y, 0, GridSize.y - 1);
		}


		/// <summary>
		/// Draws gizmos in the Unity Editor to visualize the grid layout and the objects stored in the grid.
		/// </summary>
		/// <remarks>
		/// Each cell is drawn as a wireframe cube, and grid objects are represented as small spheres at cell centers.
		/// </remarks>
		public override void OnDrawGizmos()
		{
			if (!drawGizmos) return;

			base.OnDrawGizmos();
			Gizmos.DrawWireCube(transform.position, new Vector3(GridSize.x, GridSize.y, 0) * CellSize);

			for (int x = 0; x < GridSize.x; x++)
			{
				for (int y = 0; y < GridSize.y; y++)
				{
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
