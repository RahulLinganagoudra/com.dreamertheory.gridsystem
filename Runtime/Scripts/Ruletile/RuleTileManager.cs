using UnityEngine;
using System.Collections.Generic;
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
		[SerializeField] protected RuleTile ruleTile;

		// Only keep this runtime reference for generated tiles
		protected Dictionary<Vector2Int, GameObject> placedTiles = new();

		[SerializeField]
		protected List<PlacedTile> placedTileList = new List<PlacedTile>();

#if UNITY_EDITOR
		protected HashSet<Vector2Int> selectedCells = new();
		[SerializeField] private Color selectedColor = new(1f, 0.6f, 0.2f, 0.4f); // orange highlight
		protected bool enableEditing;
		private Vector2 boxSelectStart;
		private Vector2 boxSelectEnd;
		private bool isBoxSelecting = false;
#endif

		protected virtual void OnEnable()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				SceneView.duringSceneGui += OnSceneGUI;
#endif
			RebuildDictionary();
		}

		protected virtual void OnDisable()
		{
#if UNITY_EDITOR
			SceneView.duringSceneGui -= OnSceneGUI;
#endif
		}

#if UNITY_EDITOR
		protected virtual void OnSceneGUI(SceneView sceneView)
		{
			Handles.color = Color.gray;

			for (int x = 0; x < gridSize.x; x++)
			{
				for (int y = 0; y < gridSize.y; y++)
				{
					Vector3 center = GetWorldPosition(x, y, true);

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

			// Handle both box selection and single clicks
			HandleBoxSelection(sceneView);

			// Only handle single clicks if we're not box selecting
			if (!isBoxSelecting)
			{
				HandleClick(sceneView);
			}
		}

		protected void HandleBoxSelection(SceneView sceneView)
		{
			Event e = Event.current;

			if (!enableEditing) return;

			// Start box selection with Shift+Click
			if (e.type == EventType.MouseDown && e.button == 0 && e.shift && !e.alt)
			{
				// Record selection state for undo
				Undo.RecordObject(this, "Box Select Tiles");

				isBoxSelecting = true;
				boxSelectStart = e.mousePosition;
				boxSelectEnd = e.mousePosition;

				// Clear selection if not holding Ctrl/Cmd (for additive selection)
				if (!e.control && !e.command)
				{
					selectedCells.Clear();
				}

				e.Use();
			}
			else if (e.type == EventType.MouseDrag && isBoxSelecting)
			{
				boxSelectEnd = e.mousePosition;
				e.Use();
				SceneView.RepaintAll();
			}
			else if (e.type == EventType.MouseUp && isBoxSelecting)
			{
				isBoxSelecting = false;
				boxSelectEnd = e.mousePosition;

				SelectCellsInBox(boxSelectStart, boxSelectEnd, sceneView.camera);

				EditorUtility.SetDirty(this);
				e.Use();
				SceneView.RepaintAll();
			}

			// Draw the selection box
			if (isBoxSelecting)
			{
				Handles.BeginGUI();
				var rect = new Rect(
					Mathf.Min(boxSelectStart.x, boxSelectEnd.x),
					Mathf.Min(boxSelectStart.y, boxSelectEnd.y),
					Mathf.Abs(boxSelectStart.x - boxSelectEnd.x),
					Mathf.Abs(boxSelectStart.y - boxSelectEnd.y)
				);
				EditorGUI.DrawRect(rect, new Color(1f, 0.8f, 0.2f, 0.2f));
				Handles.EndGUI();
			}
		}

		protected virtual void HandleClick(SceneView sceneView)
		{
			if (!enableEditing) return;
			Event e = Event.current;

			// Only handle single clicks (not when shift is held for box select)
			if (e.type == EventType.MouseDown && e.button == 0 && !e.alt && !e.shift)
			{
				Ray worldRay = HandleUtility.GUIPointToWorldRay(e.mousePosition);
				Plane plane = new Plane(Vector3.up, transform.position);

				if (plane.Raycast(worldRay, out float distance))
				{
					Vector3 hitPoint = worldRay.GetPoint(distance);
					GetGridPosition(hitPoint, out int x, out int y);

					if (x >= 0 && x < gridSize.x && y >= 0 && y < gridSize.y)
					{
						Vector2Int cell = new Vector2Int(x, y);

						// Record selection state for undo
						Undo.RecordObject(this, "Select Tile");

						if (Event.current.control || Event.current.command)
						{
							// Toggle selection with Ctrl/Cmd
							if (selectedCells.Contains(cell))
								selectedCells.Remove(cell);
							else
								selectedCells.Add(cell);
						}
						else
						{
							// Replace selection
							selectedCells.Clear();
							selectedCells.Add(cell);
						}

						EditorUtility.SetDirty(this);
						e.Use();
						SceneView.RepaintAll();
					}
				}
			}
		}

		private void SyncPlacedTileList()
		{
			placedTileList = new List<PlacedTile>();
			foreach (var kvp in placedTiles)
			{
				placedTileList.Add(new PlacedTile { position = kvp.Key, tile = kvp.Value });
			}
		}

		public virtual void GenerateGrid()
		{
			if (container == null)
			{
				Debug.LogWarning("No container assigned.");
				return;
			}

			if (selectedCells.Count == 0) return;

			// Group operations for better undo experience
			Undo.IncrementCurrentGroup();
			int undoGroupIndex = Undo.GetCurrentGroup();
			Undo.SetCurrentGroupName("Generate Rule Tiles");

			// Record manager state
			Undo.RecordObject(this, "Generate Rule Tiles");

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
			EditorUtility.SetDirty(this);

			// Collapse undo operations into one step
			Undo.CollapseUndoOperations(undoGroupIndex);
		}

		public virtual void DeleteSelectedTiles()
		{
			if (selectedCells.Count == 0) return;

			// Group operations for better undo experience
			Undo.IncrementCurrentGroup();
			int undoGroupIndex = Undo.GetCurrentGroup();
			Undo.SetCurrentGroupName("Delete Rule Tiles");

			// Record manager state
			Undo.RecordObject(this, "Delete Rule Tiles");

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

			// Step 2: Delete selected tiles
			foreach (var pos in selectedCells)
			{
				if (placedTiles.TryGetValue(pos, out var tile))
				{
					Undo.DestroyObjectImmediate(tile);
					placedTiles.Remove(pos);
				}
			}

			selectedCells.Clear();

			// Step 3: Update neighbors
			foreach (var neighbor in neighborsToUpdate)
			{
				if (neighbor.x < 0 || neighbor.x >= gridSize.x || neighbor.y < 0 || neighbor.y >= gridSize.y)
					continue;

				CreateOrUpdateTile(neighbor.x, neighbor.y);
			}

			SyncPlacedTileList();
			EditorUtility.SetDirty(this);

			// Collapse undo operations into one step
			Undo.CollapseUndoOperations(undoGroupIndex);
			SceneView.RepaintAll();
		}

		private void CreateOrUpdateTile(int x, int y)
		{
			Vector2Int pos = new(x, y);
			bool isActiveTile = selectedCells.Contains(pos);

			if (!isActiveTile && !placedTiles.ContainsKey(pos))
				return;

			// Destroy existing tile if present
			if (placedTiles.TryGetValue(pos, out GameObject existing))
			{
				Undo.DestroyObjectImmediate(existing);
				placedTiles.Remove(pos);
			}

			var result = ruleTile.GetPrefabForPosition(x, y, placedTiles, selectedCells);
			if (result.prefab == null) return;

			GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(result.prefab, container == null ? transform : container);
			Undo.RegisterCreatedObjectUndo(instance, "Create Rule Tile");

			instance.transform.position = GetWorldPosition(x, y, true);
			instance.transform.rotation = result.rotation;

			placedTiles[pos] = instance;
		}

		public virtual void ClearSelection()
		{
			Undo.RecordObject(this, "Clear Selection");
			selectedCells.Clear();
			EditorUtility.SetDirty(this);
			SceneView.RepaintAll();
		}

		public virtual void ToggleEditing()
		{
			enableEditing = !enableEditing;
			if (!enableEditing)
			{
				ClearSelection();
			}
#if UNITY_EDITOR
    		UnityEditor.EditorUtility.SetDirty(this);
#endif
		}

		public virtual bool IsEditing()
		{
			return enableEditing;
		}
		public virtual void SelectAllCells()
		{
			selectedCells.Clear();
			for (int x = 0; x < gridSize.x; x++)
			{
				for (int y = 0; y < gridSize.y; y++)
				{
					selectedCells.Add(new Vector2Int(x, y));
				}
			}
		}

		private void SelectCellsInBox(Vector2 start, Vector2 end, Camera sceneCamera)
		{
			Vector2 min = Vector2.Min(start, end);
			Vector2 max = Vector2.Max(start, end);

			// Only select if the box has some minimum size to avoid accidental selections
			if (Mathf.Abs(max.x - min.x) < 5 || Mathf.Abs(max.y - min.y) < 5) return;

			for (int x = 0; x < gridSize.x; x++)
			{
				for (int y = 0; y < gridSize.y; y++)
				{
					Vector3 worldPos = GetWorldPosition(x, y, true);
					Vector2 screenPoint = HandleUtility.WorldToGUIPoint(worldPos);

					if (screenPoint.x >= min.x && screenPoint.x <= max.x &&
						screenPoint.y >= min.y && screenPoint.y <= max.y)
					{
						selectedCells.Add(new Vector2Int(x, y));
					}
				}
			}
		}

#endif

		protected void RebuildDictionary()
		{
			placedTiles.Clear();
			foreach (var pt in placedTileList)
			{
				if (pt.tile != null)
					placedTiles[pt.position] = pt.tile;
			}
		}

		// RUNTIME: Only keep reference for generated tiles
		public virtual void DeleteAllChildren()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				Undo.RecordObject(this, "Delete All Tiles");
				foreach (var go in placedTiles.Values)
				{
					if (go != null)
						Undo.DestroyObjectImmediate(go);
				}
				placedTileList.Clear();
				EditorUtility.SetDirty(this);
			}
			else
#endif
			{
				foreach (var go in placedTiles.Values)
				{
					if (go != null)
						DestroyImmediate(go);
				}
#if UNITY_EDITOR
				placedTileList.Clear();
#endif
			}

			placedTiles.Clear();
		}
	}
}