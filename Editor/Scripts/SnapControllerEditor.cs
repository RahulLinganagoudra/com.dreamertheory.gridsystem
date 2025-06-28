#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;

[InitializeOnLoad]
public static class SnapControllerEditor
{
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

        // Only process on mouse drag or mouse up (Unity 6+ best practice)
        if (Event.current.type != EventType.MouseDrag && Event.current.type != EventType.MouseUp)
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

            foreach (var obj in Selection.gameObjects)
            {
                if (obj == null || obj == ((MonoBehaviour)gridSystem).gameObject)
                    continue;

                if (Selection.activeGameObject != obj)
                    continue;

                Handles.color = Color.green;
                Vector3 oldPos = obj.transform.position;
                Vector3 newPos = Handles.PositionHandle(oldPos, Quaternion.identity);

                // Unity 6+: Compare directly, don't rely on BeginChangeCheck/EndChangeCheck
                if (oldPos != newPos)
                {
                    Undo.RecordObject(obj.transform, "Snap Move");
                    Vector2Int gridCoords;
                    if (getGridCoords != null)
                    {
                        gridCoords = (Vector2Int)getGridCoords.Invoke(gridSystem, new object[] { newPos });
                    }
                    else
                    {
                        object[] parameters = new object[] { newPos, 0, 0 };
                        getGridCoordsOut.Invoke(gridSystem, parameters);
                        gridCoords = new Vector2Int((int)parameters[1], (int)parameters[2]);
                    }
                    var snappedPos = getWorldPos.Invoke(gridSystem, new object[] { gridCoords.x, gridCoords.y, true });
                    obj.transform.position = (Vector3)snappedPos;
                }
            }
        }
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