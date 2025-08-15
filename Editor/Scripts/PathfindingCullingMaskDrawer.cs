using DT.GridSystem.Pathfinding;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(PathfindingCullingMask),true)]
public class PathfindingCullingMaskDrawer : PropertyDrawer
{
	// Draws the pathfinding culling mask using the custom layers
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		// Fetch the 'mask' int field inside PathfindingCullingMask
		SerializedProperty maskProperty = property.FindPropertyRelative("mask");
		if (maskProperty == null)
		{
			EditorGUI.LabelField(position, label.text, "Error: 'mask' field not found.");
			return;
		}

		// Fetch your custom layer names; replace this with your own layer provider
		string[] layerNames = GetPathfindingLayerNames();
		if (layerNames.Length == 0)
		{
			EditorGUI.LabelField(position, label.text, "No pathfinding layers defined.");
			return;
		}
		// Draw the mask field, passing in the current mask value and layer names
		maskProperty.intValue = EditorGUI.MaskField(position, label, maskProperty.intValue, layerNames);
	}

	// Optional: adjust height if needed
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return EditorGUI.GetPropertyHeight(property, label, true);
	}

	// Example method to get your custom pathfinding layers
	private string[] GetPathfindingLayerNames()
	{
		// TODO: Replace with your real layer system
		// Example layer names for demonstration
		return PathfindingLayerData.Instance.GetAllLayerNames();
	}
}
