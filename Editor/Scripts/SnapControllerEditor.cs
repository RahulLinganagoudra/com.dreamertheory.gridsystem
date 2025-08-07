#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

[InitializeOnLoad]
public static class SnapControllerEditor
{
    private static bool isDragging = false;
    
    static SnapControllerEditor()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    [MenuItem("DT/Toggle Grid Snapping %&s")] // Ctrl+Alt+S
    public static void ToggleGridSnapping()
    {
        var gridSystems = GameObject.FindObjectsOfType<MonoBehaviour>()
            .Where(mb => IsDerivedFromGridSystem(mb.GetType()))
            .ToArray();

        foreach (var gridSystem in gridSystems)
        {
            var snapField = gridSystem.GetType().GetField("snap", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            var snapProp = gridSystem.GetType().GetProperty("snap", BindingFlags.Public | BindingFlags.Instance);

            bool snapEnabled = false;
            if (snapField != null)
                snapEnabled = (bool)snapField.GetValue(gridSystem);
            else if (snapProp != null)
                snapEnabled = (bool)snapProp.GetValue(gridSystem);

            bool newSnap = !snapEnabled;

            if (snapField != null)
                snapField.SetValue(gridSystem, newSnap);
            else if (snapProp != null && snapProp.CanWrite)
                snapProp.SetValue(gridSystem, newSnap);

            EditorUtility.SetDirty(gridSystem);
        }

        Debug.Log("[SnapControllerEditor] Toggled grid snapping for all grid systems.");
    }

    static void OnSceneGUI(SceneView sceneView)
    {
        if (Tools.current != Tool.Move)
            return;

        Event currentEvent = Event.current;

        // Track dragging state
        if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
        {
            isDragging = true;
        }
        else if (currentEvent.type == EventType.MouseUp && currentEvent.button == 0)
        {
            isDragging = false;
        }

        // Only process during drag or on mouse up
        if (currentEvent.type != EventType.MouseDrag && currentEvent.type != EventType.MouseUp)
            return;

        // Skip if no objects selected
        if (Selection.gameObjects == null || Selection.gameObjects.Length == 0)
            return;

        var gridSystems = GameObject.FindObjectsOfType<MonoBehaviour>()
            .Where(mb => IsDerivedFromGridSystem(mb.GetType()))
            .ToArray();

        foreach (var gridSystem in gridSystems)
        {
            var snapField = gridSystem.GetType().GetField("snap", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            var snapProp = gridSystem.GetType().GetProperty("snap", BindingFlags.Public | BindingFlags.Instance);
            bool snapEnabled = false;
            if (snapField != null)
                snapEnabled = (bool)snapField.GetValue(gridSystem);
            else if (snapProp != null)
                snapEnabled = (bool)snapProp.GetValue(gridSystem);

            if (!snapEnabled)
                continue;

            var getWorldPos = gridSystem.GetType().GetMethod(
                "GetWorldPosition",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new[] { typeof(int), typeof(int), typeof(bool) },
                null);

            var getGridCoords = gridSystem.GetType().GetMethod(
                "GetGridPosition",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new[] { typeof(Vector3) },
                null);

            var getGridCoordsOut = getGridCoords == null
                ? gridSystem.GetType().GetMethod(
                    "GetGridPosition",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    new[] { typeof(Vector3), typeof(int).MakeByRefType(), typeof(int).MakeByRefType() },
                    null)
                : null;

            if (getWorldPos == null || (getGridCoords == null && getGridCoordsOut == null))
                continue;

            // Process all selected objects
            foreach (var obj in Selection.gameObjects)
            {
                if (obj == null || obj == ((MonoBehaviour)gridSystem).gameObject)
                    continue;

                Vector3 currentPos = obj.transform.position;
                
                // Calculate snapped position
                Vector2Int gridCoords;
                if (getGridCoords != null)
                {
                    gridCoords = (Vector2Int)getGridCoords.Invoke(gridSystem, new object[] { currentPos });
                }
                else
                {
                    object[] parameters = new object[] { currentPos, 0, 0 };
                    getGridCoordsOut.Invoke(gridSystem, parameters);
                    gridCoords = new Vector2Int((int)parameters[1], (int)parameters[2]);
                }

                var snappedPos = (Vector3)getWorldPos.Invoke(gridSystem, new object[] { gridCoords.x, gridCoords.y, true });

                // Only update if position actually changed
                if (Vector3.Distance(currentPos, snappedPos) > 0.001f)
                {
                    Undo.RecordObject(obj.transform, "Snap Move Multiple Objects");
                    obj.transform.position = snappedPos;
                }
            }

            // Only process first valid grid system
            break;
        }

        // Repaint scene view to show updates
        SceneView.RepaintAll();
    }

    static bool IsDerivedFromGridSystem(Type type)
    {
        while (type != null)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition().Name.StartsWith("GridSystem"))
                return true;
            if (type.Name.StartsWith("GridSystem")) // fallback for non-generic base
                return true;
            type = type.BaseType;
        }
        return false;
    }
}
#endif
