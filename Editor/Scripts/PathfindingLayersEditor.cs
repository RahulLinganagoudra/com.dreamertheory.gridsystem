using UnityEngine;
using UnityEditor;
using System.IO;

namespace DT.GridSystem.Pathfinding.Editor
{
	public class PathfindingLayersEditor : EditorWindow
	{
		private PathfindingLayerData layerData;
		private string newLayerName = "";
		private const string AssetPath = "Assets/Resources/PathfindingLayerData.asset";
		private const string ResourcesPath = "Assets/Resources";
		private const int MaxLayers = 32;

		// Scroll position
		private Vector2 scrollPos;

		// Split view variables
		private float listHeight = 300f; // starting height
		private bool isResizing;
		private Rect resizeHandleRect;

		[MenuItem("DT/Pathfinding Layers &_0")]
		public static void ShowWindow()
		{
			GetWindow<PathfindingLayersEditor>("Pathfinding Layers");
		}

		private void OnEnable()
		{
			LoadOrCreateLayerData();
		}

		private void LoadOrCreateLayerData()
		{
			layerData = AssetDatabase.LoadAssetAtPath<PathfindingLayerData>(AssetPath);

			if (layerData == null)
			{
				if (!Directory.Exists(ResourcesPath))
				{
					Directory.CreateDirectory(ResourcesPath);
					AssetDatabase.Refresh();
				}
				layerData = CreateInstance<PathfindingLayerData>();
				AssetDatabase.CreateAsset(layerData, AssetPath);
				AssetDatabase.SaveAssets();

				Debug.Log("Created new PathfindingLayerData asset at: " + AssetPath);
			}
		}

		private void OnGUI()
		{
			if (layerData == null)
			{
				GUILayout.Label("Loading layer data...", EditorStyles.boldLabel);
				return;
			}

			GUILayout.Label("Pathfinding Layers", EditorStyles.boldLabel);
			GUILayout.Label($"Asset: {AssetPath}", EditorStyles.miniLabel);
			GUILayout.Space(5);

			int indexToRemove = -1;

			// Scrollable list area
			scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(listHeight));
			for (int i = 0; i < layerData.layerNames.Count; i++)
			{
				EditorGUILayout.BeginHorizontal();

				EditorGUI.BeginChangeCheck();
				string newValue = EditorGUILayout.TextField($"Layer {i}:", layerData.layerNames[i]);
				if (EditorGUI.EndChangeCheck())
				{
					layerData.layerNames[i] = newValue;
					SaveAndRefresh();
				}
				if (GUILayout.Button("X", GUILayout.Width(25)))
				{
					indexToRemove = i;
				}

				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndScrollView();

			// Resizable handle UI
			resizeHandleRect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 5f, GUIStyle.none);
			EditorGUI.DrawRect(resizeHandleRect, new Color(0.2f, 0.2f, 0.2f));

			EditorGUIUtility.AddCursorRect(resizeHandleRect, MouseCursor.ResizeVertical);

			HandleResize();

			if (indexToRemove >= 0)
			{
				layerData.layerNames.RemoveAt(indexToRemove);
				SaveAndRefresh();
			}

			GUILayout.Space(10);

			// Add new layer
			EditorGUILayout.BeginHorizontal();
			newLayerName = EditorGUILayout.TextField("New Layer:", newLayerName);

			GUI.enabled = layerData.layerNames.Count < MaxLayers;
			if (GUILayout.Button("Add") && !string.IsNullOrWhiteSpace(newLayerName))
			{
				if (!layerData.layerNames.Contains(newLayerName))
				{
					layerData.layerNames.Add(newLayerName);
					SaveAndRefresh();
					newLayerName = "";
				}
				else
				{
					Debug.LogWarning("Layer already exists in list.");
				}
			}
			GUI.enabled = true;

			EditorGUILayout.EndHorizontal();

			GUILayout.Space(15);
			GUILayout.Label($"Total Layers: {layerData.layerNames.Count} / {MaxLayers}", EditorStyles.miniLabel);

			EditorGUILayout.BeginHorizontal();

			if (GUILayout.Button("Highlight Asset", GUILayout.Height(25)))
			{
				Selection.activeObject = layerData;
				EditorGUIUtility.PingObject(layerData);
			}

			// Red color for "Clear All" button
			Color prevColor = GUI.color;
			GUI.color = Color.red;

			if (GUILayout.Button("Clear All Layers", GUILayout.Height(25)))
			{
				if (EditorUtility.DisplayDialog("Clear All Layers?",
						"Are you sure you want to remove ALL layers? This cannot be undone!",
						"Yes", "Cancel"))
				{
					layerData.layerNames.Clear();
					SaveAndRefresh();
				}
			}

			// Restore GUI color
			GUI.color = prevColor;

			EditorGUILayout.EndHorizontal();
		}

		// Handles mouse drag for resizing the scroll section
		private void HandleResize()
		{
			Event e = Event.current;

			if (e.type == EventType.MouseDown && resizeHandleRect.Contains(e.mousePosition))
			{
				isResizing = true;
				e.Use();
			}

			if (isResizing && e.type == EventType.MouseDrag)
			{
				listHeight = Mathf.Clamp(listHeight + e.delta.y, 100f, position.height - 100f);
				Repaint();
			}

			if (e.type == EventType.MouseUp)
			{
				isResizing = false;
			}
		}

		private void SaveAndRefresh()
		{
			if (layerData != null)
			{
				EditorUtility.SetDirty(layerData);
				AssetDatabase.SaveAssets();
				layerData.RefreshData();
			}
		}
	}
}
