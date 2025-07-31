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
        public Transform container;
        [SerializeField] RuleTile ruleTile;

        // Only keep this runtime reference for generated tiles
        protected Dictionary<Vector2Int, GameObject> placedTiles = new();

#if UNITY_EDITOR
        [System.Serializable]
        public struct PlacedTile
        {
            public Vector2Int position;
            public GameObject tile;
        }

        [SerializeField]
        protected List<PlacedTile> placedTileList = new List<PlacedTile>();
        protected HashSet<Vector2Int> selectedCells = new();
        [SerializeField] private Color selectedColor = new(1f, 0.6f, 0.2f, 0.4f); // orange highlight
        protected bool enableEditing;

        protected virtual void OnEnable()
        {
            if (!Application.isPlaying)
                SceneView.duringSceneGui += OnSceneGUI;
            RebuildDictionary();
        }

        protected virtual void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

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

            HandleClick(sceneView);
        }

        protected virtual void HandleClick(SceneView sceneView)
        {
            if (!enableEditing) return;
            Event e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
            {
                Ray worldRay = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                //if (Physics.Raycast(worldRay, out RaycastHit hit)) return;

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

        public virtual void DeleteSelectedTiles()
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

            selectedCells.Clear();

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
                return;

            if (placedTiles.TryGetValue(pos, out GameObject existing))
            {
                DestroyImmediate(existing);
                placedTiles.Remove(pos);
            }

            var result = ruleTile.GetPrefabForPosition(x, y, placedTiles, selectedCells);
            if (result.prefab == null) return;
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(result.prefab, container);
            instance.transform.position = GetWorldPosition(x, y, true);
            instance.transform.rotation = result.rotation;

            placedTiles[pos] = instance;
        }

        public virtual void ClearSelection()
        {
            selectedCells.Clear();
            SceneView.RepaintAll();
            SyncPlacedTileList();
        }

        public virtual void ToggleEditing()
        {
            enableEditing = !enableEditing;
            if (!enableEditing)
            {
                ClearSelection();
            }
        }
        public virtual bool IsEditing()
        {
            return enableEditing;
        }
#endif

        // RUNTIME: Only keep reference for generated tiles
        public void DeleteAllChildren()
        {
            foreach (var go in placedTiles.Values)
            {
                if (go != null)
                    DestroyImmediate(go);
            }
#if UNITY_EDITOR
            placedTileList.Clear();
#endif
            placedTiles.Clear();
        }
    }
}
