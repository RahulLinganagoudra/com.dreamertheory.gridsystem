using DT.GridSystem;
using UnityEngine;

public class MouseTest : MonoBehaviour
{
	Vector3 mousePos;
	[SerializeField] GridSystem<GameObject> gridSystem;
	private void Update()
	{
		if ((Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit)))
		{
			mousePos = hit.point;
		}
	}
	private void OnDrawGizmos()
	{
		Gizmos.DrawSphere(mousePos, 0.1f);
		if (gridSystem != null)
		{
			gridSystem.GetGridPosition(mousePos, out int x, out int y);
			Vector3 worldPos = gridSystem.GetWorldPosition(x, y, true);
			Gizmos.DrawSphere(worldPos, 0.1f);
		}
	}
}