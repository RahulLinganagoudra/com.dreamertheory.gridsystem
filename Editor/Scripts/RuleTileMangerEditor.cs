#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
namespace DT.GridSystem.Ruletile
{
	[CustomEditor(typeof(RuleTileManger))]
	public class RuleTileMangerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if (GUILayout.Button(((RuleTileManger)target).IsEditing() ? "Disable Editing" : "Enable editing"))
			{
				var generator = (RuleTileManger)target;
				generator.ToggleEditing();
			}
			if (GUILayout.Button("Generate Grid"))
			{
				var generator = (RuleTileManger)target;
				generator.GenerateGrid();
			}

			if (GUILayout.Button("Clear Selection"))
			{
				var generator = (RuleTileManger)target;
				generator.ClearSelection();
			}
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Delete Selected"))
			{
				var generator = (RuleTileManger)target;
				generator.DeleteSelectedTiles();
			}
			if (GUILayout.Button("Destroy All"))
			{
				var generator = (RuleTileManger)target;
				generator.DeleteAllChildren();
			}

			GUILayout.EndHorizontal();
		}
	}
}
#endif
