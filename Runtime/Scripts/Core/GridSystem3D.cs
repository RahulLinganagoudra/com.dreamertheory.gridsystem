using UnityEngine;

namespace DT.GridSystem
{
	/// <summary>
	/// A 3D grid system implementation that maps grid coordinates to world positions on the XZ plane.
	/// Supports visualization and interaction with grid elements in 3D space.
	/// </summary>
	/// <typeparam name="TGridObject">The type of object stored in each grid cell.</typeparam>
	/// <remarks>
	/// This class extends the <see cref="GridSystem{TGridObject}"/> base class to work in a 3D environment,
	/// using the X and Z axes for the grid plane, with Y typically representing elevation.
	/// </remarks>
	public class GridSystem3D<TGridObject> : RectGridSystem<TGridObject>
	{
		/// <summary>
		/// Converts grid coordinates (x, y) to a world position on the XZ plane.
		/// </summary>
		/// <param name="x">The x-coordinate in the grid.</param>
		/// <param name="y">The y-coordinate in the grid.</param>
		/// <param name="snapToGrid">
		/// If true, returns the center of the cell; otherwise, returns the corner.
		/// </param>
		/// <returns>The corresponding world position as a <see cref="Vector3"/>.</returns>
		public override Vector3 GetWorldPosition(int x, int y, bool snapToGrid = true)
		{
			if (snapToGrid)
			{
				return (new Vector3(x - GridSize.x / 2f, 0, y - GridSize.y / 2f) * CellSize + transform.position) + new Vector3(CellSize, 0, CellSize) * 0.5f;
			}
			else
			{
				return new Vector3(x - GridSize.x / 2f, 0, y - GridSize.y / 2f) * CellSize + transform.position;
			}
		}

		/// <summary>
		/// Converts a world position to grid coordinates (x, y), assuming grid lies on the XZ plane.
		/// </summary>
		/// <param name="worldPosition">The world position to convert.</param>
		/// <param name="x">The resulting x-coordinate in the grid.</param>
		/// <param name="y">The resulting y-coordinate in the grid.</param>
		public override void GetGridPosition(Vector3 worldPosition, out int x, out int y)
		{
			float relativeX = (worldPosition.x - transform.position.x) / CellSize;
			float relativeY = (worldPosition.z - transform.position.z) / CellSize;

			x = Mathf.FloorToInt(relativeX + GridSize.x / 2f);
			y = Mathf.FloorToInt(relativeY + GridSize.y / 2f);

			x = Mathf.Clamp(x, 0, GridSize.x - 1);
			y = Mathf.Clamp(y, 0, GridSize.y - 1);
		}

		/// <summary>
		/// Draws gizmos in the Unity Editor to visualize the 3D grid and its objects on the XZ plane.
		/// </summary>
		/// <remarks>
		/// Cells are drawn as lines, and any populated cells have their object centers shown with spheres.
		/// </remarks>
		public override void OnDrawGizmos()
		{
			if (!drawGizmos) return;
			base.OnDrawGizmos();

			Gizmos.DrawWireCube(transform.position, new Vector3(GridSize.x, 0, GridSize.y) * CellSize);

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
