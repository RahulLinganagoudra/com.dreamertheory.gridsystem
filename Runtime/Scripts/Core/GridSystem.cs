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
		/// The 2D array storing grid objects.
		/// </summary>
		protected TGridObject[,] gridArray;

		/// <summary>
		/// Event invoked whenever the grid is updated.
		/// </summary>
		public UnityEvent OnGridUpdated = new();

		/// <summary>
		/// Gets the current grid size.
		/// </summary>
		public Vector2Int GridSize => gridSize;

		/// <summary>
		/// Gets or sets the size of each grid cell.
		/// </summary>
		public float CellSize { get => cellSize; protected set => cellSize = value; }

		/// <summary>
		/// Initializes the grid by creating default grid objects.
		/// </summary>
		protected virtual void Awake()
		{
			gridArray = new TGridObject[gridSize.x, gridSize.y];
			for (int x = 0; x < gridSize.x; x++)
			{
				for (int y = 0; y < gridSize.y; y++)
				{
					gridArray[x, y] = CreateGridObject(this, x, y);
				}
			}
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
			gridArray = new TGridObject[gridSize.x, gridSize.y];
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
		public int GetRowCount() => gridArray.GetLength(0);

		/// <summary>
		/// Returns the number of columns in the grid.
		/// </summary>
		public int GetColumnCount() => gridArray.GetLength(1);

		/// <summary>
		/// Adds or replaces a grid object at the specified coordinates.
		/// </summary>
		/// <param name="x">X coordinate.</param>
		/// <param name="y">Y coordinate.</param>
		/// <param name="value">The grid object to add.</param>
		/// <param name="snapToGrid">If true, adjusts object to snap to grid (not implemented here).</param>
		public virtual void AddGridObject(int x, int y, TGridObject value, bool snapToGrid = false)
		{
			if (x >= 0 && y >= 0 && x < gridSize.x && y < gridSize.y)
			{
				gridArray[x, y] = value;
			}
			OnGridUpdated?.Invoke();
		}

		/// <summary>
		/// Removes and returns the grid object at the specified coordinates.
		/// </summary>
		public virtual TGridObject RemoveGridObject(int x, int y)
		{
			if (x >= 0 && y >= 0 && x < gridSize.x && y < gridSize.y)
			{
				TGridObject removedObject = gridArray[x, y];
				gridArray[x, y] = default;
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
			if (x >= 0 && y >= 0 && x < gridSize.x && y < gridSize.y)
			{
				return gridArray[x, y];
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

			gridArray ??= new TGridObject[gridSize.x, gridSize.y];

			if (gridArray.GetLength(0) != gridSize.x || gridArray.GetLength(1) != gridSize.y)
			{
				gridArray = new TGridObject[gridSize.x, gridSize.y];
			}

			for (int x = 0; x < gridSize.x; x++)
			{
				for (int y = 0; y < gridSize.y; y++)
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
}
