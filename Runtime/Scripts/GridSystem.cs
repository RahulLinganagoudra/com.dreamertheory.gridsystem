using UnityEngine;
using UnityEngine.Events;
namespace DT.GridSystem
{
	public abstract class GridSystem<TGridObject> : MonoBehaviour
	{
		[SerializeField] private protected Vector2Int gridSize;
		[SerializeField] private protected float cellSize;
		private protected TGridObject[,] gridArray;
		public UnityEvent OnGridUpdated = new();
		public Vector2Int GridSize() => gridSize;
		[SerializeField] private protected bool drawGizmos = true;
		private protected virtual void Awake()
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
			cellSize = cellsize;
			gridArray = new TGridObject[gridSize.x, gridSize.y];
		}
		public virtual TGridObject CreateGridObject(GridSystem<TGridObject> gridSystem, int x, int y)
		{
			return default;
		}
		public int GetRowCount() => gridArray.GetLength(0);
		public int GetColumnCount() => gridArray.GetLength(1);

		public void AddGridObject(int x, int y, TGridObject value, bool snapToGrid = false)
		{
			if (x >= 0 && y >= 0 && x < gridSize.x && y < gridSize.y)
			{
				gridArray[x, y] = value;
			}
			OnGridUpdated?.Invoke();
		}
		public TGridObject RemoveGridObject(int x, int y)
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
		public TGridObject GetGridObject(int x, int y)
		{
			if (x >= 0 && y >= 0 && x < gridSize.x && y < gridSize.y)
			{
				return gridArray[x, y];
			}
			return default;
		}
		public bool TryGetGridObject(int x, int y, out TGridObject gridObject)
		{
			gridObject = GetGridObject(x, y);
			if (gridObject != null)
			{
				return true;
			}
			return false;
		}
		public bool TryGetGridObject(Vector3 worldPosition, out TGridObject gridObject)
		{
			gridObject = GetGridObject(worldPosition);
			if (gridObject != null)
			{
				return true;
			}
			return false;
		}
		public TGridObject GetGridObject(Vector3 worldPosition)
		{
			GetGridPosition(worldPosition, out int x, out int y);
			return GetGridObject(x, y);
		}


		public abstract Vector3 GetWorldPosition(int x, int y, bool snapToGrid = false);
		public abstract void GetGridPosition(Vector3 worldPosition, out int x, out int y);

		public virtual void OnDrawGizmos()
		{
			if(!drawGizmos)return;
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
	}
	public class GridSystem3D<TGridObject> : GridSystem<TGridObject>
	{
		public override Vector3 GetWorldPosition(int x, int y, bool snapToGrid = false)
		{
			if (snapToGrid)
			{
				return (new Vector3(x - gridSize.x / 2f, 0, y - gridSize.y / 2f) * cellSize + transform.position) + new Vector3(cellSize, 0, cellSize) * 0.5f;
			}
			else
			{
				return new Vector3(x - gridSize.x / 2f, 0, y - gridSize.y / 2f) * cellSize + transform.position;
			}
		}

		public override void GetGridPosition(Vector3 worldPosition, out int x, out int y)
		{
			//x = Mathf.FloorToInt((worldPosition.x - transform.position.x) / cellSize + gridSize.x / 2f);
			//y = Mathf.FloorToInt((worldPosition.z - transform.position.z) / cellSize + gridSize.y / 2f);


			//x = Mathf.FloorToInt((worldPosition.x - transform.position.x) / cellSize + gridSize.x / 2f);
			//y = Mathf.FloorToInt((worldPosition.z - transform.position.z) / cellSize + gridSize.y / 2f);

			// Snap world position to grid
			float snappedX = Mathf.FloorToInt((worldPosition.x - transform.position.x) / cellSize);
			float snappedY = Mathf.FloorToInt((worldPosition.z - transform.position.z) / cellSize);

			// Convert snapped position to grid indices
			x = Mathf.FloorToInt(snappedX + gridSize.x / 2f);
			y = Mathf.FloorToInt(snappedY + gridSize.y / 2f);

			// Clamp the grid indices to ensure they are within bounds
			x = Mathf.Clamp(x, 0, gridSize.x - 1);
			y = Mathf.Clamp(y, 0, gridSize.y - 1);
		}

		public override void OnDrawGizmos()
		{
			if(!drawGizmos)return;
			base.OnDrawGizmos();
			Gizmos.DrawWireCube(transform.position, new Vector3(gridSize.x, 0, gridSize.y) * cellSize);

			for (int x = 0; x < gridSize.x; x++)
			{
				for (int y = 0; y < gridSize.y; y++)
				{
					// Draw the grid object at the center of the cell
					TGridObject gridObject = GetGridObject(x, y);
					if (gridObject != null)
					{
						Vector3 objectPosition = GetWorldPosition(x, y, true);
						Gizmos.DrawSphere(objectPosition, cellSize * 0.1f);
					}
				}
			}
		}
	}
	public class GridSystem2D<TGridObject> : GridSystem<TGridObject>
	{
		public override Vector3 GetWorldPosition(int x, int y, bool snapToGrid = false)
		{
			if (!snapToGrid)
				return new Vector3(x - gridSize.x / 2f, y - gridSize.y / 2f, 0) * cellSize + transform.position;
			else
				return (new Vector3(x - gridSize.x / 2f, y - gridSize.y / 2f, 0) * cellSize + transform.position) + new Vector3(cellSize, cellSize, 0) * 0.5f;
		}

		public override void GetGridPosition(Vector3 worldPosition, out int x, out int y)
		{
			//x = Mathf.FloorToInt((worldPosition.x - transform.position.x) / cellSize + gridSize.x / 2f);
			//y = Mathf.FloorToInt((worldPosition.y - transform.position.y) / cellSize + gridSize.y / 2f);


			float snappedX = Mathf.FloorToInt((worldPosition.x - transform.position.x) / cellSize);
			float snappedY = Mathf.FloorToInt((worldPosition.y - transform.position.y) / cellSize);

			// Convert snapped position to grid indices
			x = Mathf.FloorToInt(snappedX + gridSize.x / 2f);
			y = Mathf.FloorToInt(snappedY + gridSize.y / 2f);

			// Clamp the grid indices to ensure they are within bounds
			x = Mathf.Clamp(x, 0, gridSize.x - 1);
			y = Mathf.Clamp(y, 0, gridSize.y - 1);
		}

		public override void OnDrawGizmos()
		{
			if(!drawGizmos)return;

			base.OnDrawGizmos();
			Gizmos.DrawWireCube(transform.position, new Vector3(gridSize.x, gridSize.y, 0) * cellSize);

			for (int x = 0; x < gridSize.x; x++)
			{
				for (int y = 0; y < gridSize.y; y++)
				{
					// Draw the grid object at the center of the cell
					TGridObject gridObject = GetGridObject(x, y);
					if (gridObject != null)
					{
						Vector3 objectPosition = GetWorldPosition(x, y, true);
						Gizmos.DrawSphere(objectPosition, cellSize * 0.1f);
					}
				}
			}
		}
	}
}