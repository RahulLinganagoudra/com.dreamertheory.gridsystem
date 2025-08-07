using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

namespace DT.GridSystem
{
	/// <summary>
	/// A 3D hexagonal grid system supporting flat-topped and pointy-topped layouts on the XZ plane.
	/// Provides coordinate conversion, world positioning, and visualization tools.
	/// </summary>
	/// <typeparam name="TGridObject">The type of object stored in the grid cells.</typeparam>
	public class HexGridSystem3D<TGridObject> : GridSystem<TGridObject>
	{
		/// <summary>
		/// Orientation of the hex tiles: FlatTop or PointyTop.
		/// </summary>
		public enum HexOrientation
		{
			FlatTop,
			PointyTop
		}

		[SerializeField] protected HexOrientation hexOrientation = HexOrientation.FlatTop;

		protected float sqrt3;
		protected float sqrt3Over2;


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




		/// <summary>
		/// Initializes mathematical constants for hexagonal calculations.
		/// </summary>
		protected override void Awake()
		{
			sqrt3 = Mathf.Sqrt(3f);
			sqrt3Over2 = sqrt3 / 2f;
			base.Awake();
		}

		/// <summary>
		/// Converts grid coordinates to a world position for the specified orientation.
		/// </summary>
		/// <param name="x">The x-coordinate in the grid.</param>
		/// <param name="y">The y-coordinate in the grid.</param>
		/// <param name="snapToGrid">If true, returns the center of the hex; otherwise, the corner position.</param>
		/// <returns>The calculated world position in 3D space.</returns>
		public override Vector3 GetWorldPosition(int x, int y, bool snapToGrid = false)
		{
			float size = CellSize;

			switch (hexOrientation)
			{
				case HexOrientation.PointyTop:
					float width = size;
					float height = sqrt3Over2 * size;
					float offsetX = (y % 2 == 0) ? 0 : width * 0.5f;
					float xPos = x * width + offsetX - gridSize.x * width * 0.5f;
					float yPos = y * height - gridSize.y * height * 0.5f;
					return new Vector3(xPos, 0, yPos) + transform.position + new Vector3(CellSize, 0, CellSize) * 0.5f;

				case HexOrientation.FlatTop:
					float heightP = size;
					float widthP = sqrt3Over2 * size;
					float offsetY = (x % 2 == 0) ? 0 : heightP * 0.5f;
					return new Vector3(x * widthP - gridSize.x * widthP * 0.5f, 0, y * heightP + offsetY - gridSize.y * heightP * 0.5f) + transform.position + new Vector3(CellSize, 0, CellSize) * 0.5f;

				default:
					return Vector3.zero;
			}
		}
		/// <summary>
		/// Returns a list of valid neighboring cell positions for a given hex cell in the grid,
		/// taking into account the current hex orientation (FlatTop or PointyTop) and the grid boundaries.
		/// </summary>
		/// <param name="pos">The grid position (as a Vector2Int) for which to find neighbors.</param>
		/// <returns>
		/// A list of Vector2Int positions representing the adjacent cells that are within the grid bounds.
		/// </returns>
		/// <remarks>
		/// The method automatically selects the correct neighbor offset pattern based on the grid's orientation
		/// and the parity (even/odd) of the relevant coordinate, ensuring accurate neighbor calculation for both
		/// flat-topped and pointy-topped hex layouts.
		/// </remarks>
		public override List<Vector2Int> GetNeighbors(Vector2Int pos)
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


		/// <summary>
		/// Converts a world position into hex grid coordinates based on the selected orientation.
		/// </summary>
		/// <param name="worldPosition">The world position to convert.</param>
		/// <param name="x">The resulting x grid index.</param>
		/// <param name="y">The resulting y grid index.</param>
		public override void GetGridPosition(Vector3 worldPosition, out int x, out int y)
		{
			Vector3 offsetCenter = new Vector3(CellSize, 0, CellSize) * 0.5f;
			Vector3 local = worldPosition - transform.position - offsetCenter;

			float size = CellSize;

			switch (hexOrientation)
			{
				case HexOrientation.PointyTop:
					float width = size;
					float height = sqrt3Over2 * size;

					float q = (local.x + gridSize.x * width * 0.5f) / width;
					float r = (local.z + gridSize.y * height * 0.5f) / height;

					int row = Mathf.RoundToInt(r);
					float offsetX = (row % 2 == 0) ? 0 : 0.5f;
					int col = Mathf.RoundToInt(q - offsetX);

					x = Mathf.Clamp(col, 0, gridSize.x - 1);
					y = Mathf.Clamp(row, 0, gridSize.y - 1);
					break;

				case HexOrientation.FlatTop:
					float heightP = size;
					float widthP = sqrt3Over2 * size;

					float colF = (local.x + gridSize.x * widthP * 0.5f) / widthP;
					float rowF = (local.z + gridSize.y * heightP * 0.5f) / heightP;

					int colP = Mathf.RoundToInt(colF);
					float offsetY = (colP % 2 == 0) ? 0 : 0.5f;
					int rowP = Mathf.RoundToInt(rowF - offsetY);

					x = Mathf.Clamp(colP, 0, gridSize.x - 1);
					y = Mathf.Clamp(rowP, 0, gridSize.y - 1);
					break;

				default:
					x = y = 0;
					break;
			}
		}

		/// <summary>
		/// Visualizes the hex grid using Unity Gizmos, including cell centers and outlines.
		/// </summary>
		public override void OnDrawGizmos()
		{
#if UNITY_EDITOR
			if (!drawGizmos) return;
			sqrt3 = Mathf.Sqrt(3f);
			sqrt3Over2 = sqrt3 / 2f;

			for (int i = 0; i < gridSize.x; i++)
			{
				for (int j = 0; j < gridSize.y; j++)
				{
					Vector3 position = GetWorldPosition(i, j, true);
					//Gizmos.color = Color.green;
					//Gizmos.DrawWireSphere(position, CellSize * 0.1f);
					Gizmos.color = Color.white;
					DrawHexOutline(position, CellSize * 0.5f);
					if (showGridIndex)
						UnityEditor.Handles.Label(position, $"{i},{j}");
				}
			}
			    
#endif
		}

		/// <summary>
		/// Draws the outline of a single hexagon for debugging in the Unity editor.
		/// </summary>
		/// <param name="center">The center of the hex cell.</param>
		/// <param name="size">The radius from the center to a corner.</param>
		protected void DrawHexOutline(Vector3 center, float size)
		{
			Vector3[] corners = new Vector3[7];

			for (int i = 0; i < 7; i++)
			{
				float angleDeg = 0;
				if (hexOrientation == HexOrientation.PointyTop)
				{
					angleDeg = 60f * i - 30f;
				}
				else
				{
					angleDeg = 60f * i;
				}

				float angleRad = Mathf.Deg2Rad * angleDeg;
				corners[i] = center + new Vector3(Mathf.Cos(angleRad), 0f, Mathf.Sin(angleRad)) * size;
			}

			for (int i = 0; i < 6; i++)
			{
				Gizmos.DrawLine(corners[i], corners[i + 1]);
			}
		}
	}
}
