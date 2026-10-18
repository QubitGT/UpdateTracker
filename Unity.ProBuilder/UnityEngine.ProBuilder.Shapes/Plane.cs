namespace UnityEngine.ProBuilder.Shapes;

[Shape("Plane")]
public class Plane : Shape
{
	[Min(0f)]
	[SerializeField]
	private int m_HeightSegments = 1;

	[Min(0f)]
	[SerializeField]
	private int m_WidthSegments = 1;

	internal override void SetParametersToBuiltInShape()
	{
		m_HeightSegments = 5;
		m_WidthSegments = 5;
	}

	public override void CopyShape(Shape shape)
	{
		if (shape is Plane)
		{
			m_HeightSegments = ((Plane)shape).m_HeightSegments;
			m_WidthSegments = ((Plane)shape).m_WidthSegments;
		}
	}

	public override Bounds RebuildMesh(ProBuilderMesh mesh, Vector3 size, Quaternion rotation)
	{
		int num = m_WidthSegments + 1;
		int num2 = m_HeightSegments + 1;
		Vector2[] array = new Vector2[num * num2 * 4];
		Vector3[] array2 = new Vector3[num * num2 * 4];
		float num3 = 1f;
		float num4 = 1f;
		int num5 = 0;
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				float x = (float)j * (num3 / (float)num) - num3 / 2f;
				float x2 = (float)(j + 1) * (num3 / (float)num) - num3 / 2f;
				float y = (float)i * (num4 / (float)num2) - num4 / 2f;
				float y2 = (float)(i + 1) * (num4 / (float)num2) - num4 / 2f;
				array[num5] = new Vector2(x, y);
				array[num5 + 1] = new Vector2(x2, y);
				array[num5 + 2] = new Vector2(x, y2);
				array[num5 + 3] = new Vector2(x2, y2);
				num5 += 4;
			}
		}
		for (num5 = 0; num5 < array2.Length; num5++)
		{
			array2[num5] = new Vector3(array[num5].y, 0f, array[num5].x);
		}
		mesh.GeometryWithPoints(array2);
		return mesh.mesh.bounds;
	}
}
