#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
namespace DT.GridSystem.Ruletile
{
	[CustomEditor(typeof(RuleTileManger))]
	public class RuleTileMangerEditor : Editor
	{
		static RuleTileManger Target;

		private void Awake()
		{
			if (Target == null)
			{
				Target = (RuleTileManger)target;
			}
		}
		public override void OnInspectorGUI()
		{
			if (Target == null)
			{
				Target = (RuleTileManger)target;
			}
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
			generator.DeleteSelectedTiles();
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
		[MenuItem("DT/Select all tiles while editing &a")]
		public static void SelectAll()
		{
			if (Target == null) return;
			var generator = Target;
			generator.SelectAllCells();
		}
	}
}
#endif
