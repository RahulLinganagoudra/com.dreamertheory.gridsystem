using UnityEngine;

namespace DT.GridSystem
{
    public class HexGridSystem3D<TGridObject> : GridSystem<TGridObject>
    {
        public enum HexOrientation
        {
            FlatTop,
            PointyTop
        }

        [SerializeField] protected HexOrientation hexOrientation = HexOrientation.FlatTop;
        float sqrt3;
        float sqrt3Over2;

        protected override void Awake()
        {
            sqrt3 = Mathf.Sqrt(3f);
            sqrt3Over2 = sqrt3 / 2f;
            base.Awake();
        }

        public override Vector3 GetWorldPosition(int x, int y, bool snapToGrid = false)
        {
            float size = CellSize;

            switch (hexOrientation)
            {
                case HexOrientation.PointyTop:
                    float width = size;
                    float height = sqrt3Over2 * size;
                    float offsetX = (y % 2 == 0) ? 0 : width * 0.5f;
                    float xPos = x * width + offsetX - gridSize.x * width * 0.5f;
                    float yPos = y * height - gridSize.y * height * 0.5f;
                    return new Vector3(
                        xPos ,
                        0,
                        yPos

                    ) + transform.position
                    + new Vector3(CellSize, 0, CellSize) * 0.5f;

                case HexOrientation.FlatTop:
                    float heightP = size;
                    float widthP = sqrt3Over2 * size;
                    float offsetY = (x % 2 == 0) ? 0 : heightP * 0.5f;

                    return new Vector3(
                        x * widthP - gridSize.x * widthP * 0.5f,
                        0,
                        y * heightP + offsetY - gridSize.y * heightP * 0.5f
                    ) + transform.position
                    + new Vector3(CellSize, 0, CellSize) * 0.5f;

                default:
                    return Vector3.zero;
            }
        }

		public override void GetGridPosition(Vector3 worldPosition, out int x, out int y)
		{
			Vector3 offsetCenter = new Vector3(CellSize, 0, CellSize) * 0.5f;
			Vector3 local = worldPosition - transform.position - offsetCenter;

			float size = CellSize;

			switch (hexOrientation)
			{
				case HexOrientation.PointyTop:
					float width = size;
					float height = sqrt3Over2 * size;

					float q = (local.x + gridSize.x * width * 0.5f) / width;
					float r = (local.z + gridSize.y * height * 0.5f) / height;

					int row = Mathf.RoundToInt(r);
					float offsetX = (row % 2 == 0) ? 0 : 0.5f;
					int col = Mathf.RoundToInt(q - offsetX);

					x = Mathf.Clamp(col, 0, gridSize.x - 1);
					y = Mathf.Clamp(row, 0, gridSize.y - 1);
					break;

				case HexOrientation.FlatTop:
					float heightP = size;
					float widthP = sqrt3Over2 * size;

					float colF = (local.x + gridSize.x * widthP * 0.5f) / widthP;
					float rowF = (local.z + gridSize.y * heightP * 0.5f) / heightP;

					int colP = Mathf.RoundToInt(colF);
					float offsetY = (colP % 2 == 0) ? 0 : 0.5f;
					int rowP = Mathf.RoundToInt(rowF - offsetY);

					x = Mathf.Clamp(colP, 0, gridSize.x - 1);
					y = Mathf.Clamp(rowP, 0, gridSize.y - 1);
					break;

				default:
					x = y = 0;
					break;
			}
		}


		public override void OnDrawGizmos()
        {
            sqrt3 = Mathf.Sqrt(3f);
            sqrt3Over2 = sqrt3 / 2f;
            for (int i = 0; i < gridSize.x; i++)
            {
                for (int j = 0; j < gridSize.y; j++)
                {
                    Vector3 position = GetWorldPosition(i, j, true);
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(position, CellSize * 0.1f);
                }
            }
        }
    }
}
