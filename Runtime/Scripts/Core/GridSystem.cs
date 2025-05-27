using System;
using UnityEngine;
using UnityEngine.Events;
namespace DT.GridSystem
{
	/// <summary>
	/// this is a base class for grid systems that can be used to create 2D or 3D grids.
	/// </summary>
	/// <typeparam name="TGridObject">The objects that are stored in the grid</typeparam>
	public abstract class GridSystem<TGridObject> : MonoBehaviour
	{
		[SerializeField] protected Vector2Int gridSize;
		[SerializeField] private float cellSize;
		protected TGridObject[,] gridArray;
		public UnityEvent OnGridUpdated = new();
		public Vector2Int GridSize => gridSize;

		public float CellSize { get => cellSize; protected set => cellSize = value; }

		[SerializeField] protected bool drawGizmos = true;
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
		public void SetUpGrid(Vector2Int size, float cellsize)
		{
			gridSize = size;
			CellSize = cellsize;
			gridArray = new TGridObject[gridSize.x, gridSize.y];
		}
		public virtual TGridObject CreateGridObject(GridSystem<TGridObject> gridSystem, int x, int y)
		{
			return default;
		}
		public int GetRowCount() => gridArray.GetLength(0);
		public int GetColumnCount() => gridArray.GetLength(1);

		public virtual void AddGridObject(int x, int y, TGridObject value, bool snapToGrid = false)
		{
			if (x >= 0 && y >= 0 && x < gridSize.x && y < gridSize.y)
			{
				gridArray[x, y] = value;
			}
			OnGridUpdated?.Invoke();
		}
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
		public virtual TGridObject GetGridObject(int x, int y)
		{
			if (x >= 0 && y >= 0 && x < gridSize.x && y < gridSize.y)
			{
				return gridArray[x, y];
			}
			return default;
		}
		public virtual bool TryGetGridObject(int x, int y, out TGridObject gridObject)
		{
			gridObject = GetGridObject(x, y);
			if (gridObject != null)
			{
				return true;
			}
			return false;
		}
		public virtual bool TryGetGridObject(Vector3 worldPosition, out TGridObject gridObject)
		{
			gridObject = GetGridObject(worldPosition);
			if (gridObject != null)
			{
				return true;
			}
			return false;
		}
		public virtual TGridObject GetGridObject(Vector3 worldPosition)
		{
			GetGridPosition(worldPosition, out int x, out int y);
			return GetGridObject(x, y);
		}

		public virtual Vector3 SnapWorldPosition(Vector3 worldPosition)
		{
			int x, y;
			GetGridPosition(worldPosition, out x, out y);
			return GetWorldPosition(x, y, true);
		}
		public Vector2Int GetGridPosition(Vector3 worldPosition)
		{
			GetGridPosition(worldPosition, out int x, out int y);
			return new Vector2Int(x, y);
		}

		public abstract Vector3 GetWorldPosition(int x, int y, bool snapToGrid = false);
		public abstract void GetGridPosition(Vector3 worldPosition, out int x, out int y);
		public virtual TGridObject GetGridObject(Vector2Int pos)
		{
			return GetGridObject(pos.x, pos.y);
		}
		public virtual bool IsInBounds(Vector2Int pos)
		{
			return pos.x >= 0 && pos.y >= 0 && pos.x < GridSize.x && pos.y < GridSize.y;
		}
		public virtual void OnDrawGizmos()
		{
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
					Vector3 start = GetWorldPosition(x, y);
					Vector3 endX = GetWorldPosition(x + 1, y);
					Vector3 endY = GetWorldPosition(x, y + 1);
					Gizmos.DrawLine(start, endX);
					Gizmos.DrawLine(start, endY);

				}
			}
		}

		internal Vector3 GetWorldPosition(Vector2Int vector2Int)
		{
			throw new NotImplementedException();
		}
	}
}