using UnityEngine;
using UnityEditor;

namespace DT.GridSystem.Pathfinding.Editor
{
	[CustomPropertyDrawer(typeof(PathfindingLayer),true)]
	public class PathfindingLayerDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			// Finds the serialized field inside PathfindingLayer struct
			SerializedProperty nameProp = property.FindPropertyRelative("layerName");

			if (nameProp == null)
			{
				EditorGUI.LabelField(position, label.text, "Error: 'layerName' field not found.");
				return;
			}

			// Get available layers from singleton
			var layerData = PathfindingLayerData.Instance;
			if (layerData == null)
			{
				EditorGUI.LabelField(position, label.text, "PathfindingLayerData not found");
				return;
			}

			string[] layerNames = layerData.GetAllLayerNames();
			if (layerNames.Length == 0)
			{
				EditorGUI.LabelField(position, label.text, "No pathfinding layers defined.");
				return;
			}

			// Find current selection index
			int selectedIndex = Mathf.Max(0, System.Array.IndexOf(layerNames, nameProp.stringValue));

			// Draw property as a popup
			EditorGUI.BeginProperty(position, label, property);
			int newIndex = EditorGUI.Popup(position, label.text, selectedIndex, layerNames);

			if (newIndex != selectedIndex && newIndex >= 0 && newIndex < layerNames.Length)
			{
				nameProp.stringValue = layerNames[newIndex];
			}
			EditorGUI.EndProperty();
		}
	}
}
