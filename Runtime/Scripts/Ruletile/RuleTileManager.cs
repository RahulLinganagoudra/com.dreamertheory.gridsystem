using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DT.GridSystem.Ruletile
{
	[ExecuteAlways]
	public class RuleTileManager : GridSystem3D<GameObject>
	{
		[System.Serializable]
		public struct PlacedTile
		{
			public Vector2Int position;
			public GameObject tile;
		}

		[SerializeField] protected RuleTile ruleTile;
		[SerializeField] protected List<PlacedTile> placedTileList = new();

#if UNITY_EDITOR
		[Header("Editor - Selection")]
		[SerializeField]private Transform container;
		
		[SerializeField] private Color selectedColor = new(1f, 0.6f, 0.2f, 0.4f);

		protected readonly HashSet<Vector2Int> selectedCells = new();
		private bool enableEditing = false;
		private bool isBoxSelecting = false;
		private Vector2 boxSelectStart, boxSelectEnd;
#endif

		// Runtime dictionary - rebuilt from serialized list
		protected readonly Dictionary<Vector2Int, GameObject> placedTiles = new();

#if UNITY_EDITOR
		public Transform Container
		{
			get => container == null ? transform : container;
			set => container = value;
		}
#else
        public Transform Container => transform;
#endif

		protected virtual void OnEnable()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				SceneView.duringSceneGui += OnSceneGUI;
				Undo.undoRedoPerformed += OnUndoRedoPerformed;
			}
#endif
			RebuildDictionary();
		}

		protected virtual void OnDisable()
		{
#if UNITY_EDITOR
			SceneView.duringSceneGui -= OnSceneGUI;
			Undo.undoRedoPerformed -= OnUndoRedoPerformed;
#endif
		}

#if UNITY_EDITOR
		private void OnSceneGUI(SceneView sceneView)
		{
			DrawGridSelection();
			HandleBoxSelection();
			if (!isBoxSelecting) HandleClick();
		}

		private void DrawGridSelection()
		{
			if (selectedCells.Count == 0) return;

			Handles.color = Color.gray;
			float halfCell = CellSize * 0.5f;

			foreach (var cell in selectedCells)
			{
				Vector3 center = GetWorldPosition(cell.x, cell.y, true);
				Vector3[] corners = {
					center + new Vector3(-halfCell, 0, -halfCell),
					center + new Vector3(-halfCell, 0,  halfCell),
					center + new Vector3( halfCell, 0,  halfCell),
					center + new Vector3( halfCell, 0, -halfCell)
				};
				Handles.DrawSolidRectangleWithOutline(corners, selectedColor, Color.white);
			}
		}

		private void HandleBoxSelection()
		{
			Event e = Event.current;
			if (!enableEditing) return;

			switch (e.type)
			{
				case EventType.MouseDown when e.button == 0 && e.shift && !e.alt:
					StartBoxSelection(e);
					break;
				case EventType.MouseDrag when isBoxSelecting:
					UpdateBoxSelection(e);
					break;
				case EventType.MouseUp when isBoxSelecting:
					EndBoxSelection(e);
					break;
			}

			if (isBoxSelecting) DrawSelectionBox();
		}

		private void StartBoxSelection(Event e)
		{
			Undo.RegisterCompleteObjectUndo(this, "Box Select Tiles");
			isBoxSelecting = true;
			boxSelectStart = boxSelectEnd = e.mousePosition;
			if (!e.control && !e.command) selectedCells.Clear();
			e.Use();
		}

		private void UpdateBoxSelection(Event e)
		{
			boxSelectEnd = e.mousePosition;
			e.Use();
			SceneView.RepaintAll();
		}

		private void EndBoxSelection(Event e)
		{
			isBoxSelecting = false;
			boxSelectEnd = e.mousePosition;
			SelectCellsInBox(boxSelectStart, boxSelectEnd);
			SetDirtyAndRepaint();
			e.Use();
		}

		private void DrawSelectionBox()
		{
			Handles.BeginGUI();
			Rect rect = new(
				Mathf.Min(boxSelectStart.x, boxSelectEnd.x),
				Mathf.Min(boxSelectStart.y, boxSelectEnd.y),
				Mathf.Abs(boxSelectStart.x - boxSelectEnd.x),
				Mathf.Abs(boxSelectStart.y - boxSelectEnd.y)
			);
			EditorGUI.DrawRect(rect, new Color(1f, 0.8f, 0.2f, 0.2f));
			Handles.EndGUI();
		}

		private void HandleClick()
		{
			if (!enableEditing) return;

			Event e = Event.current;
			if (e.type != EventType.MouseDown || e.button != 0 || e.alt || e.shift) return;

			if (!GetClickedCell(e.mousePosition, out Vector2Int cell)) return;

			Undo.RegisterCompleteObjectUndo(this, "Select Tile");
			Undo.FlushUndoRecordObjects(); 
			if (e.control || e.command)
			{
				if (!selectedCells.Remove(cell)) selectedCells.Add(cell);
			}
			else
			{
				selectedCells.Clear();
				selectedCells.Add(cell);
			}

			SetDirtyAndRepaint();
			e.Use();
		}

		private bool GetClickedCell(Vector2 mousePos, out Vector2Int cell)
		{
			cell = default;
			Ray worldRay = HandleUtility.GUIPointToWorldRay(mousePos);
			Plane plane = new(Vector3.up, transform.position);

			if (!plane.Raycast(worldRay, out float dist)) return false;

			Vector3 hitPoint = worldRay.GetPoint(dist);
			GetGridPosition(hitPoint, out int x, out int y);

			if (x < 0 || x >= gridSize.x || y < 0 || y >= gridSize.y) return false;

			cell = new Vector2Int(x, y);
			return true;
		}

		private void SelectCellsInBox(Vector2 start, Vector2 end)
		{
			Vector2 min = Vector2.Min(start, end);
			Vector2 max = Vector2.Max(start, end);

			// Minimum drag distance to avoid accidental selections
			if (Mathf.Abs(max.x - min.x) < 5 || Mathf.Abs(max.y - min.y) < 5) return;

			for (int x = 0; x < gridSize.x; x++)
			{
				for (int y = 0; y < gridSize.y; y++)
				{
					Vector3 worldPos = GetWorldPosition(x, y, true);
					Vector2 screenPos = HandleUtility.WorldToGUIPoint(worldPos);

					if (screenPos.x >= min.x && screenPos.x <= max.x &&
						screenPos.y >= min.y && screenPos.y <= max.y)
					{
						selectedCells.Add(new Vector2Int(x, y));
					}
				}
			}
		}

		// Optimized helper for common operations
		private void SetDirtyAndRepaint()
		{
			EditorUtility.SetDirty(this);
			SceneView.RepaintAll();
		}

		public void ToggleEditing()
		{
			enableEditing = !enableEditing;
			if (!enableEditing) ClearSelection();
			EditorUtility.SetDirty(this);
		}

		public bool IsEditing() => enableEditing;

		public void ClearSelection()
		{
			Undo.RegisterCompleteObjectUndo(this, "Clear Selection");
			selectedCells.Clear();
			SetDirtyAndRepaint();
		}

		public void SelectAllCells()
		{
			Undo.RegisterCompleteObjectUndo(this, "Select All Cells");
			selectedCells.Clear();

			for (int x = 0; x < gridSize.x; x++)
				for (int y = 0; y < gridSize.y; y++)
					selectedCells.Add(new Vector2Int(x, y));

			SetDirtyAndRepaint();
		}

		public virtual void GenerateGrid()
		{
			if (Container == null || selectedCells.Count == 0) return;

			var undoGroup = BeginUndoGroup("Generate Rule Tiles");
			Undo.SetCurrentGroupName("Generate Rule Tiles");
			Undo.FlushUndoRecordObjects();
			// Get all positions that need updating (selected + neighbors)
			HashSet<Vector2Int> toUpdate = GetPositionsToUpdate(selectedCells);

			foreach (var pos in toUpdate)
			{
				if (IsValidGridPosition(pos))
					CreateOrUpdateTile(pos);
			}

			SyncPlacedTileList();
			EndUndoGroup(undoGroup);
		}

		public virtual void DeleteSelectedTiles()
		{
			if (selectedCells.Count == 0) return;

			var undoGroup = BeginUndoGroup("Delete Rule Tiles");

			List<Vector2Int> toDelete = new(selectedCells);
			HashSet<Vector2Int> neighborsToUpdate = GetNeighbors(toDelete);

			// Delete selected tiles
			foreach (var pos in toDelete)
			{
				if (placedTiles.TryGetValue(pos, out GameObject go) && go != null)
					Undo.DestroyObjectImmediate(go);

				placedTiles.Remove(pos);
				RemoveFromPlacedTileList(pos);
			}

			selectedCells.Clear();

			// Update neighbors
			foreach (var neighbor in neighborsToUpdate)
			{
				if (IsValidGridPosition(neighbor))
					CreateOrUpdateTile(neighbor);
			}

			EndUndoGroup(undoGroup);
		}

		private int BeginUndoGroup(string name)
		{
			Undo.IncrementCurrentGroup();
			int group = Undo.GetCurrentGroup();
			Undo.SetCurrentGroupName(name);
			Undo.RegisterCompleteObjectUndo(this, name);
			return group;
		}	

		private void EndUndoGroup(int group)
		{
			Undo.FlushUndoRecordObjects();
			EditorUtility.SetDirty(this);
			Undo.CollapseUndoOperations(group);
			SceneView.RepaintAll();
		}

		private HashSet<Vector2Int> GetPositionsToUpdate(IEnumerable<Vector2Int> positions)
		{
			HashSet<Vector2Int> result = new();
			foreach (var pos in positions)
			{
				// Add position and all 8 neighbors
				for (int dx = -1; dx <= 1; dx++)
					for (int dy = -1; dy <= 1; dy++)
						result.Add(new Vector2Int(pos.x + dx, pos.y + dy));
			}
			return result;
		}

		private HashSet<Vector2Int> GetNeighbors(IEnumerable<Vector2Int> positions)
		{
			HashSet<Vector2Int> neighbors = new();
			foreach (var pos in positions)
			{
				for (int dx = -1; dx <= 1; dx++)
					for (int dy = -1; dy <= 1; dy++)
					{
						if (dx == 0 && dy == 0) continue;
						neighbors.Add(new Vector2Int(pos.x + dx, pos.y + dy));
					}
			}
			return neighbors;
		}

		private bool IsValidGridPosition(Vector2Int pos) =>
			pos.x >= 0 && pos.x < gridSize.x && pos.y >= 0 && pos.y < gridSize.y;

		protected void RemoveFromPlacedTileList(Vector2Int position)
		{
			for (int i = placedTileList.Count - 1; i >= 0; i--)
			{
				if (placedTileList[i].position == position)
				{
					placedTileList.RemoveAt(i);
					break;
				}
			}
		}

		private void OnUndoRedoPerformed()
		{
			RebuildDictionary();
			SceneView.RepaintAll();
		}
#endif

		private void CreateOrUpdateTile(Vector2Int pos)
		{
#if UNITY_EDITOR
			bool isActiveTile = selectedCells.Contains(pos);
#else
            const bool isActiveTile = true;
#endif
			if (!isActiveTile && !placedTiles.ContainsKey(pos)) return;

			// Remove existing tile if present
			if (placedTiles.TryGetValue(pos, out GameObject existing) && existing != null)
			{
#if UNITY_EDITOR
				Undo.DestroyObjectImmediate(existing);
#else
                DestroyImmediate(existing);
#endif
				placedTiles.Remove(pos);
				RemoveFromPlacedTileList(pos);
			}

			// Get new tile from rule system
			var result = ruleTile.GetPrefabForPosition(pos.x, pos.y, placedTiles,
#if UNITY_EDITOR
				selectedCells
#else
                null
#endif
			);

			if (result.prefab == null) return;

			// Create new tile
#if UNITY_EDITOR
			GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(result.prefab, Container);
			Undo.RegisterCreatedObjectUndo(instance, "Create Rule Tile");
#else
			GameObject instance = GameObject.Instantiate(result.prefab, Container);
#endif

			instance.transform.position = GetWorldPosition(pos.x, pos.y, true);
			instance.transform.rotation = result.rotation;

			placedTiles[pos] = instance;
			placedTileList.Add(new PlacedTile { position = pos, tile = instance });
		}

		private void SyncPlacedTileList()
		{
			placedTileList.Clear();
			foreach (var kvp in placedTiles)
			{
				if (kvp.Value != null)
					placedTileList.Add(new PlacedTile { position = kvp.Key, tile = kvp.Value });
			}
		}

		public void RebuildDictionary()
		{
			placedTiles.Clear();
			for (int i = placedTileList.Count - 1; i >= 0; i--)
			{
				PlacedTile pt = placedTileList[i];
				if (pt.tile == null)
					placedTileList.RemoveAt(i);
				else
					placedTiles[pt.position] = pt.tile;
			}
		}

		public virtual void DeleteAllChildren()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				Undo.RegisterCompleteObjectUndo(this, "Delete All Tiles");
				foreach (var go in placedTiles.Values)
					if (go != null) Undo.DestroyObjectImmediate(go);
				placedTileList.Clear();
				EditorUtility.SetDirty(this);
			}
			else
#endif
			{
				foreach (var go in placedTiles.Values)
					if (go != null) DestroyImmediate(go);
#if UNITY_EDITOR
				placedTileList.Clear();
#endif
			}
			placedTiles.Clear();
		}
	}
}
