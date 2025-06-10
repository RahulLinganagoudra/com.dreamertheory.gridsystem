using DT.GridSystem;
using UnityEngine;

//public class DTGrid : GridSystem3D<GameObject>
//{
//	// Start is called once before the first execution of Update after the MonoBehaviour is created
//	void Start()
//	{
//		Vector2Int _2d = new(2, 5);
//		Vector2Int bounds = new(30, 10);

//		int message = _2d.ConvertTo1DIndex(bounds);
//		print(message);
//		print(message.ConvertTo2DIndex(bounds));
//	}

//	// Update is called once per frame
//	public override void OnDrawGizmos()
//	{
//		base.OnDrawGizmos();
//	}
//	Color GetColorForRule(RuleState rule)
//	{
//		switch (rule)
//		{
//			case RuleState.Tile_Exist:
//				return Color.green;

//			case RuleState.No_Tile:
//				return Color.red;

//			case RuleState.No_Mention:
//			default:
//				return Color.gray;
//		}
//	}
//}