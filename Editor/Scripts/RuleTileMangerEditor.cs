#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
namespace DT.GridSystem.Ruletile
{
	[CustomEditor(typeof(RuleTileManager),true)]
	public class RuleTileMangerEditor : Editor
	{
		static RuleTileManager Target;

		private void Awake()
		{
			if (Target == null)
			{
				Target = (RuleTileManager)target;
			}
		}
		public override void OnInspectorGUI()
		{
			if (Target == null)
			{
				Target = (RuleTileManager)target;
			}
			DrawDefaultInspector();

			if (GUILayout.Button(((RuleTileManager)target).IsEditing() ? "Disable Editing" : "Enable editing"))
			{
				var generator = (RuleTileManager)target;
				generator.ToggleEditing();
			}
			if (GUILayout.Button("Generate Grid"))
			{
				var generator = (RuleTileManager)target;
				generator.GenerateGrid();
			}

			if (GUILayout.Button("Clear Selection"))
			{
				var generator = (RuleTileManager)target;
				generator.ClearSelection();
			}
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Delete Selected"))
			{
				var generator = (RuleTileManager)target;
				generator.DeleteSelectedTilesEditor();
			}
			if (GUILayout.Button("Destroy All"))
			{
				var generator = (RuleTileManager)target;
				generator.DeleteAllChildren();
			}

			GUILayout.EndHorizontal();
		}

		[MenuItem("DT/DeleteAllRuleTiles %&d")]
		public static void DeleteAll()
		{
			if (Target == null) return;
			var generator = Target;
			generator.DeleteAllChildren();
		}

		[MenuItem("DT/Delete selected ruleTiles &d")]
		public static void Delete()
		{
			if (Target == null) return;
			var generator = Target;
			generator.DeleteSelectedTilesEditor();
		}

		[MenuItem("DT/Generate ruletile &g")]
		public static void Generate()
		{
			if (Target == null) return;
			var generator = Target;
			generator.GenerateGrid();
		}

		[MenuItem("DT/Clear grid select &s")]
		public static void ClearSelection()
		{
			if (Target == null) return;
			var generator = Target;
			generator.ClearSelection();
		}

		[MenuItem("DT/Toggle editing &t")]
		public static void ToggleEdit()
		{
			if (Target == null) return;
			var generator = Target;
			generator.ToggleEditing();
			EditorUtility.SetDirty(generator);
		}
		[MenuItem("DT/Select all tiles while editing %&a")]
		public static void SelectAll()
		{
			if (Target == null) return;
			var generator = Target;
			generator.SelectAllCells();
		}
	}
}
#endif
