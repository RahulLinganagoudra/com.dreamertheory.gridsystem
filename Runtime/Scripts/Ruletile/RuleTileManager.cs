using UnityEngine;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DT.GridSystem.Ruletile
{

	[ExecuteAlways]
	public class RuleTileManger : GridSystem3D<GameObject>
	{
		[System.Serializable]
		public struct PlacedTile
		{
			public Vector2Int position;
			public GameObject tile;
		}

		public Transform container;
		[SerializeField] RuleTile ruleTile;

		[SerializeField]
		private List<PlacedTile> placedTileList = new List<PlacedTile>();
		private HashSet<Vector2Int> selectedCells = new HashSet<Vector2Int>();

		[SerializeField] private Color selectedColor = new Color(1f, 0.6f, 0.2f, 0.4f); // orange highlight
		private Dictionary<Vector2Int, GameObject> placedTiles = new();

		bool enableEditing;



#if UNITY_EDITOR
		private void OnEnable()
		{
			if (!Application.isPlaying)
				SceneView.duringSceneGui += OnSceneGUI;
			RebuildDictionary();
		}

		private void OnDisable()
		{
			SceneView.duringSceneGui -= OnSceneGUI;
		}

		private void OnSceneGUI(SceneView sceneView)
		{
			Handles.color = Color.gray;

			for (int x = 0; x < gridSize.x; x++)
			{
				for (int y = 0; y < gridSize.y; y++)
				{
					Vector3 center = GetWorldPosition(x, y, true);
					Vector3 size = Vector3.one * CellSize;

					if (selectedCells.Contains(new Vector2Int(x, y)))
					{
						Handles.DrawSolidRectangleWithOutline(new[]
						{
					center + new Vector3(-CellSize/2, 0,-CellSize/2),
					center + new Vector3(-CellSize/2, 0, CellSize/2),
					center + new Vector3( CellSize/2, 0, CellSize/2),
					center + new Vector3( CellSize/2, 0,-CellSize/2)
				}, selectedColor, Color.white);
					}
				}
			}

			HandleClick(sceneView);
		}
		private void HandleClick(SceneView sceneView)
		{
			if (!enableEditing) return;
			Event e = Event.current;
			if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
			{
				Ray worldRay = HandleUtility.GUIPointToWorldRay(e.mousePosition);
				if (Physics.Raycast(worldRay, out RaycastHit hit)) return;

				Plane plane = new Plane(Vector3.up, transform.position);
				if (plane.Raycast(worldRay, out float distance))
				{
					Vector3 hitPoint = worldRay.GetPoint(distance);
					GetGridPosition(hitPoint, out int x, out int y);

					if (x >= 0 && x < gridSize.x && y >= 0 && y < gridSize.y)
					{
						Vector2Int cell = new Vector2Int(x, y);

						if (Event.current.control || Event.current.command)
						{
							if (selectedCells.Contains(cell))
								selectedCells.Remove(cell);
							else
								selectedCells.Add(cell);
						}
						else
						{
							selectedCells.Clear();
							selectedCells.Add(cell);
						}

						e.Use();
						SceneView.RepaintAll();
					}
				}
			}
		}
		private void RebuildDictionary()
		{
			placedTiles.Clear();
			foreach (var pt in placedTileList)
			{
				if (pt.tile != null)
					placedTiles[pt.position] = pt.tile;
			}
		}
		private void SyncPlacedTileList()
		{
			placedTileList = placedTiles
				.Select(kvp => new PlacedTile { position = kvp.Key, tile = kvp.Value })
				.ToList();
		}


		public void GenerateGrid()
		{
			if (container == null)
			{
				Debug.LogWarning("No container assigned.");
				return;
			}

			if (selectedCells.Count == 0) return;

			HashSet<Vector2Int> toUpdate = new HashSet<Vector2Int>();

			// Collect all selected + their neighbors
			foreach (var cell in selectedCells)
			{
				toUpdate.Add(cell);
				toUpdate.Add(new Vector2Int(cell.x + 1, cell.y));
				toUpdate.Add(new Vector2Int(cell.x - 1, cell.y));
				toUpdate.Add(new Vector2Int(cell.x, cell.y + 1));
				toUpdate.Add(new Vector2Int(cell.x, cell.y - 1));
				toUpdate.Add(new Vector2Int(cell.x - 1, cell.y - 1));
				toUpdate.Add(new Vector2Int(cell.x - 1, cell.y + 1));
				toUpdate.Add(new Vector2Int(cell.x + 1, cell.y - 1));
				toUpdate.Add(new Vector2Int(cell.x + 1, cell.y + 1));
			}

			foreach (var pos in toUpdate)
			{
				if (pos.x < 0 || pos.x >= gridSize.x || pos.y < 0 || pos.y >= gridSize.y)
					continue;

				CreateOrUpdateTile(pos.x, pos.y);
			}
			SyncPlacedTileList();
		}
		public void DeleteSelectedTiles()
		{
			HashSet<Vector2Int> neighborsToUpdate = new HashSet<Vector2Int>();

			// Step 1: Collect neighbors BEFORE destroying
			foreach (var pos in selectedCells)
			{
				neighborsToUpdate.Add(new Vector2Int(pos.x + 1, pos.y));
				neighborsToUpdate.Add(new Vector2Int(pos.x - 1, pos.y));
				neighborsToUpdate.Add(new Vector2Int(pos.x, pos.y + 1));
				neighborsToUpdate.Add(new Vector2Int(pos.x, pos.y - 1));
				neighborsToUpdate.Add(new Vector2Int(pos.x + 1, pos.y + 1));
				neighborsToUpdate.Add(new Vector2Int(pos.x + 1, pos.y - 1));
				neighborsToUpdate.Add(new Vector2Int(pos.x - 1, pos.y + 1));
				neighborsToUpdate.Add(new Vector2Int(pos.x - 1, pos.y - 1));
			}

			// Step 2: Remove from placedTiles AND selectedCells BEFORE updating
			foreach (var pos in selectedCells)
			{
				if (placedTiles.TryGetValue(pos, out var tile))
				{
					DestroyImmediate(tile);
					placedTiles.Remove(pos);
				}
			}

			selectedCells.Clear(); // <- IMPORTANT: clear before updating neighbors

			// Step 3: Now update neighbors properly
			foreach (var neighbor in neighborsToUpdate)
			{
				if (neighbor.x < 0 || neighbor.x >= gridSize.x || neighbor.y < 0 || neighbor.y >= gridSize.y)
					continue;

				CreateOrUpdateTile(neighbor.x, neighbor.y);
			}
			SyncPlacedTileList();

			SceneView.RepaintAll();
		}

		private void CreateOrUpdateTile(int x, int y)
		{
			Vector2Int pos = new(x, y);

			bool isActiveTile = selectedCells.Contains(pos);

			if (!isActiveTile && !placedTiles.ContainsKey(pos))
				return; // Not selected and nothing placed, do nothing

			// Destroy existing tile (we're going to re-create or update it)
			if (placedTiles.TryGetValue(pos, out GameObject existing))
			{
				DestroyImmediate(existing);
				placedTiles.Remove(pos);
			}

			// Only recreate tile if it's part of the active selection
			//if (!isActiveTile) return;

			GameObject prefab = ruleTile.GetPrefabForPosition(x, y, placedTiles, selectedCells);
			if (prefab == null) return;

			GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, container);
			instance.transform.position = GetWorldPosition(x, y, true);
			//instance.transform.localScale = Vector3.one * CellSize;
			instance.name = $"Tile_{x}_{y}";

			placedTiles[pos] = instance;
		}


		public void ClearSelection()
		{
			selectedCells.Clear();
			SceneView.RepaintAll();
			SyncPlacedTileList();
		}

		public void ToggleEditing()
		{
			enableEditing = !enableEditing;
			if (!enableEditing)
			{
				ClearSelection();
			}
		}
		public bool IsEditing()
		{
			return enableEditing;
		}


#endif
		public void DeleteAllChildren()
		{
			foreach (var go in placedTiles.Values)
			{
				if (go != null)
					DestroyImmediate(go);
			}
			placedTileList.Clear();
			placedTiles.Clear();
		}
	}
}
