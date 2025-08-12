using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace DT.GridSystem
{
	/// <summary>
	/// A base class for grid systems that can be used to create 2D or 3D grids.
	/// </summary>
	/// <typeparam name="TGridObject">The type of objects stored in the grid.</typeparam>
	/// <remarks>
	/// This class is intended to be inherited and extended for specific grid implementations.
	/// </remarks>
	public abstract class GridSystem<TGridObject> : MonoBehaviour
	{
		[SerializeField] protected Vector2Int gridSize = new Vector2Int(5, 5);
		[SerializeField] private float cellSize = 1;
		[SerializeField] public bool snap = false;
		[SerializeField] protected bool drawGizmos = true;
		[SerializeField] protected bool showGridIndex = false;
		/// <summary>
		/// The 1D array storing grid objects.
		/// </summary>
		[SerializeField] protected TGridObject[] gridArray;

		/// <summary>
		/// Event invoked whenever the grid is updated.
		/// </summary>
		public UnityEvent OnGridUpdated = new();

		/// <summary>
		/// Gets the current grid size.
		/// </summary>
		public virtual Vector2Int GridSize => gridSize;

		/// <summary>
		/// Gets or sets the size of each grid cell.
		/// </summary>
		public virtual float CellSize { get => cellSize; protected set => cellSize = value; }

		/// <summary>
		/// Initializes the grid by creating default grid objects.
		/// </summary>
		protected virtual void Awake()
		{
			int size = GridSize.x * GridSize.y;

			gridArray = new TGridObject[size];
			for (int x = 0; x < GridSize.x; x++)
			{
				for (int y = 0; y < GridSize.y; y++)
				{
					int index = ToIndex(x, y);
					gridArray[index] = CreateGridObject(this, x, y);
				}
			}
		}


		/// <summary>
		/// Converts 2D grid coordinates to a flat 1D index (row-major order).
		/// </summary>
		/// <param name="x">X coordinate (row)</param>
		/// <param name="y">Y coordinate (column)</param>
		/// <returns>The 1D index</returns>
		public int ToIndex(int x, int y)
		{
			return x * GridSize.y + y;
		}

		/// <summary>
		/// Converts a flat 1D index to 2D grid coordinates (row-major order).
		/// </summary>
		/// <param name="index">The 1D index</param>
		/// <param name="x">Output X coordinate (row)</param>
		/// <param name="y">Output Y coordinate (column)</param>
		public void FromIndex(int index, out int x, out int y)
		{
			x = index / GridSize.y;
			y = index % GridSize.y;
		}

		/// <summary>
		/// Sets up the grid with a new size and cell size, reinitializing the grid array.
		/// </summary>
		/// <param name="size">The new size of the grid.</param>
		/// <param name="cellsize">The size of each cell.</param>
		public void SetUpGrid(Vector2Int size, float cellsize)
		{
			gridSize = size;
			CellSize = cellsize;

			gridArray = new TGridObject[GridSize.x * GridSize.y];
		}

		/// <summary>
		/// Creates a new grid object at the given coordinates.
		/// Should be overridden in derived classes.
		/// </summary>
		/// <param name="gridSystem">The grid system reference.</param>
		/// <param name="x">The X coordinate.</param>
		/// <param name="y">The Y coordinate.</param>
		public virtual TGridObject CreateGridObject(GridSystem<TGridObject> gridSystem, int x, int y)
		{
			return default;
		}

		/// <summary>
		/// Returns the number of rows in the grid.
		/// </summary>
		public int GetRowCount() => GridSize.x;

		/// <summary>
		/// Returns the number of columns in the grid.
		/// </summary>
		public int GetColumnCount() => GridSize.y;

		/// <summary>
		/// Adds or replaces a grid object at the specified coordinates.
		/// </summary>
		/// <param name="x">X coordinate.</param>
		/// <param name="y">Y coordinate.</param>
		/// <param name="value">The grid object to add.</param>
		/// <param name="snapToGrid">If true, adjusts object to snap to grid (not implemented here).</param>
		public virtual void AddGridObject(int x, int y, TGridObject value, bool snapToGrid = false)
		{
			if (x >= 0 && y >= 0 && x < GridSize.x && y < GridSize.y)
			{
				int index = ToIndex(x, y);
				gridArray[index] = value;
			}
			OnGridUpdated?.Invoke();
		}

		/// <summary>
		/// Removes and returns the grid object at the specified coordinates.
		/// </summary>
		public virtual TGridObject RemoveGridObject(int x, int y)
		{
			if (x >= 0 && y >= 0 && x < GridSize.x && y < GridSize.y)
			{
				int index = ToIndex(x, y);

				TGridObject removedObject = gridArray[index];
				gridArray[index] = default;
				OnGridUpdated?.Invoke();
				return removedObject;
			}
			return default;
		}

		/// <summary>
		/// Gets the grid object at the specified coordinates.
		/// </summary>
		public virtual TGridObject GetGridObject(int x, int y)
		{
			if (x >= 0 && y >= 0 && x < GridSize.x && y < GridSize.y)
			{
				int index = ToIndex(x, y);
				return gridArray[index];
			}
			return default;
		}

		/// <summary>
		/// Attempts to get a grid object at the specified coordinates.
		/// </summary>
		public virtual bool TryGetGridObject(int x, int y, out TGridObject gridObject)
		{
			gridObject = GetGridObject(x, y);
			return gridObject != null;
		}

		/// <summary>
		/// Attempts to get a grid object at the world position.
		/// </summary>
		public virtual bool TryGetGridObject(Vector3 worldPosition, out TGridObject gridObject)
		{
			gridObject = GetGridObject(worldPosition);
			return gridObject != null;
		}

		/// <summary>
		/// Gets a grid object using a world position.
		/// </summary>
		public virtual TGridObject GetGridObject(Vector3 worldPosition)
		{
			GetGridPosition(worldPosition, out int x, out int y);
			return GetGridObject(x, y);
		}

		/// <summary>
		/// Snaps a world position to the nearest grid cell center.
		/// </summary>
		public virtual Vector3 SnapWorldPosition(Vector3 worldPosition)
		{
			GetGridPosition(worldPosition, out int x, out int y);
			return GetWorldPosition(x, y, true);
		}

		/// <summary>
		/// Converts a world position to grid coordinates.
		/// </summary>
		public Vector2Int GetGridPosition(Vector3 worldPosition)
		{
			GetGridPosition(worldPosition, out int x, out int y);
			return new Vector2Int(x, y);
		}

		/// <summary>
		/// Converts grid coordinates to a world position.
		/// Must be implemented by derived classes.
		/// </summary>
		public abstract Vector3 GetWorldPosition(int x, int y, bool snapToGrid = false);

		public abstract List<Vector2Int> GetNeighbors(Vector2Int pos);

		/// <summary>
		/// Converts a world position to grid coordinates.
		/// Must be implemented by derived classes.
		/// </summary>
		public abstract void GetGridPosition(Vector3 worldPosition, out int x, out int y);

		/// <summary>
		/// Gets the grid object at a grid position.
		/// </summary>
		public virtual TGridObject GetGridObject(Vector2Int pos)
		{
			return GetGridObject(pos.x, pos.y);
		}

		/// <summary>
		/// Checks whether the specified position is within grid bounds.
		/// </summary>
		public virtual bool IsInBounds(Vector2Int pos)
		{
			return pos.x >= 0 && pos.y >= 0 && pos.x < GridSize.x && pos.y < GridSize.y;
		}

		/// <summary>
		/// Draws grid lines in the Unity editor using Gizmos.
		/// </summary>
		public virtual void OnDrawGizmos()
		{
#if UNITY_EDITOR
			if (!drawGizmos) return;

			int expectedSize = GridSize.x * GridSize.y;
			gridArray ??= new TGridObject[expectedSize];

			if (gridArray.Length != expectedSize)
			{
				gridArray = new TGridObject[expectedSize];
			}

			for (int x = 0; x < GridSize.x; x++)
			{
				for (int y = 0; y < GridSize.y; y++)
				{
					Vector3 start = GetWorldPosition(x, y, false);
					Vector3 mid = GetWorldPosition(x, y, true);
					Vector3 endX = GetWorldPosition(x + 1, y, false);
					Vector3 endY = GetWorldPosition(x, y + 1, false);
					Gizmos.DrawLine(start, endX);
					Gizmos.DrawLine(start, endY);
					if (showGridIndex)
						UnityEditor.Handles.Label(mid, $"{x},{y}");

				}
			}
#endif
		}

		/// <summary>
		/// Converts a Vector2Int grid position to a world position.
		/// This method must be implemented in derived classes.
		/// </summary>
		internal Vector3 GetWorldPosition(Vector2Int vector2Int)
		{
			return GetWorldPosition(vector2Int.x, vector2Int.y, true);
		}
	}
	public abstract class RectGridSystem<T> : GridSystem<T>
	{

		private static readonly Vector2Int[] directionsWithNeighbours = new Vector2Int[]
		{
			new(+0, 1),//right
			new(0, -1),//left
			new(1, 0),//top
			new(-1, 0),//bottom
			new(1, 1),//1st Quadrant
			new(-1, 1),//2nd Quadrant
			new(-1, -1),//3rd Quadrant
			new(1, -1)//4th Quadrant
		};
		private static readonly Vector2Int[] directions = new Vector2Int[]
		{
			new(+0, 1),//right
			new(0, -1),//left
			new(1, 0),//top
			new(-1, 0)
		};
		public override List<Vector2Int> GetNeighbors(Vector2Int pos)
		{
			List<Vector2Int> result = new();
			foreach (var dir in directionsWithNeighbours)
			{
				var neighbor = pos + dir;
				if (neighbor.x >= 0 && neighbor.y >= 0 && neighbor.x < GridSize.x && neighbor.y < GridSize.y)
					result.Add(neighbor);
			}
			return result;
		}

		public virtual List<Vector2Int> GetNeighbors(Vector2Int pos, bool includeDiagonals)
		{
			List<Vector2Int> result = new();
			if (includeDiagonals)
			{
				foreach (var dir in directionsWithNeighbours)
				{
					var neighbor = pos + dir;
					if (neighbor.x >= 0 && neighbor.y >= 0 && neighbor.x < GridSize.x && neighbor.y < GridSize.y)
						result.Add(neighbor);
				}
			}
			else
			{
				foreach (var dir in directions)
				{
					var neighbor = pos + dir;
					if (neighbor.x >= 0 && neighbor.y >= 0 && neighbor.x < GridSize.x && neighbor.y < GridSize.y)
						result.Add(neighbor);
				}
			}
			return result;
		}
	}
}
