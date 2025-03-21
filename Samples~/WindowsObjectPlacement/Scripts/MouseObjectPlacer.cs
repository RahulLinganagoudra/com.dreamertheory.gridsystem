using UnityEngine;


namespace DT.GridSystem.Samples
{
	public class MouseObjectPlacer : MonoBehaviour
	{
		public GameObject objectToPlace; // Assign the prefab to be placed
		public LayerMask gridLayer; // Assign the layer of the grid
		public LayerMask objectLayer; // Assign the layer of objects that can be deleted
									  //public float gridSize = 1f; // Grid snapping size
		[SerializeField] Grid grid;
		void Update()
		{
			if (Input.GetMouseButtonDown(0)) // Left Click (Place Object)
			{
				PlaceObject();
			}
			else if (Input.GetMouseButtonDown(1)) // Right Click (Delete Object)
			{
				DeleteObject();
			}
		}

		void PlaceObject()
		{
			Vector3? gridPosition = GetMouseWorldPosition(gridLayer);
			if (gridPosition.HasValue)
			{
				grid.GetGridPosition(gridPosition.Value, out int x, out int y);
				if (grid.GetGridObject(x, y) == null) // Check if there is no object at the grid position
				{
					var obj = Instantiate(objectToPlace, grid.GetWorldPosition(x, y, true), Quaternion.identity);
					grid.AddGridObject(x, y, obj);
				}
			}
		}

		void DeleteObject()
		{
			Vector3? hitPosition = GetMouseWorldPosition(objectLayer);
			if (hitPosition.HasValue)
			{
				grid.GetGridPosition(hitPosition.Value, out int x, out int y);
				if (grid.TryGetGridObject(x, y, out var gridObject))
				{
					grid.RemoveGridObject(x, y);
					Destroy(gridObject);
				}
			}
		}

		Vector3? GetMouseWorldPosition(LayerMask layer)
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layer))
			{
				return hit.point;
			}
			return null;
		}
	}
}