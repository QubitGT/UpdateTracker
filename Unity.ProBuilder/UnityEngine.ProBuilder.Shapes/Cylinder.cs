namespace UnityEngine.ProBuilder.Shapes;

[Shape("Cylinder")]
public class Cylinder : Shape
{
	[SerializeField]
	[Range(3f, 64f)]
	private int m_AxisDivisions = 6;

	[Min(0f)]
	[SerializeField]
	private int m_HeightCuts;

	[SerializeField]
	private bool m_Smooth = true;

	internal override void SetParametersToBuiltInShape()
	{
		m_AxisDivisions = 8;
		m_HeightCuts = 2;
		m_Smooth = false;
	}

	public override void CopyShape(Shape shape)
	{
		if (shape is Cylinder)
		{
			m_AxisDivisions = ((Cylinder)shape).m_AxisDivisions;
			m_HeightCuts = ((Cylinder)shape).m_HeightCuts;
			m_Smooth = ((Cylinder)shape).m_Smooth;
		}
	}

	public override Bounds UpdateBounds(ProBuilderMesh mesh, Vector3 size, Quaternion rotation, Bounds bounds)
	{
		bounds.size = size;
		return bounds;
	}

	public override Bounds RebuildMesh(ProBuilderMesh mesh, Vector3 size, Quaternion rotation)
	{
		Vector3 vector = Vector3.Scale(rotation * Vector3.up, size);
		Vector3 vector2 = Vector3.Scale(rotation * Vector3.right, size);
		Vector3 vector3 = Vector3.Scale(rotation * Vector3.forward, size);
		float magnitude = vector.magnitude;
		float xRadius = vector2.magnitude / 2f;
		float yRadius = vector3.magnitude / 2f;
		float num = magnitude / (float)(m_HeightCuts + 1);
		Vector2[] array = new Vector2[m_AxisDivisions];
		for (int i = 0; i < m_AxisDivisions; i++)
		{
			float angleInDegrees = (float)i * 360f / (float)m_AxisDivisions;
			array[i] = Math.PointInEllipseCircumference(xRadius, yRadius, angleInDegrees, Vector2.zero, out var _);
		}
		Vector3[] array2 = new Vector3[m_AxisDivisions * (m_HeightCuts + 1) * 4 + m_AxisDivisions * 6];
		Face[] array3 = new Face[m_AxisDivisions * (m_HeightCuts + 1) + m_AxisDivisions * 2];
		int num2 = 0;
		for (int j = 0; j < m_HeightCuts + 1; j++)
		{
			float y = (float)j * num - magnitude * 0.5f;
			float y2 = (float)(j + 1) * num - magnitude * 0.5f;
			for (int k = 0; k < m_AxisDivisions; k++)
			{
				array2[num2] = new Vector3(array[k].x, y, array[k].y);
				array2[num2 + 1] = new Vector3(array[k].x, y2, array[k].y);
				if (k != m_AxisDivisions - 1)
				{
					array2[num2 + 2] = new Vector3(array[k + 1].x, y, array[k + 1].y);
					array2[num2 + 3] = new Vector3(array[k + 1].x, y2, array[k + 1].y);
				}
				else
				{
					array2[num2 + 2] = new Vector3(array[0].x, y, array[0].y);
					array2[num2 + 3] = new Vector3(array[0].x, y2, array[0].y);
				}
				num2 += 4;
			}
		}
		int num3 = 0;
		for (int l = 0; l < m_HeightCuts + 1; l++)
		{
			for (int m = 0; m < m_AxisDivisions * 4; m += 4)
			{
				int num4;
				int num5 = (num4 = l * (m_AxisDivisions * 4) + m);
				int num6 = num5 + 1;
				int num7 = num5 + 2;
				int num8 = num5 + 3;
				array3[num3++] = new Face(new int[6] { num4, num6, num7, num6, num8, num7 }, 0, AutoUnwrapSettings.tile, m_Smooth ? 1 : (-1), -1, -1, manualUVs: false);
			}
		}
		int num9 = m_AxisDivisions * (m_HeightCuts + 1) * 4;
		int num10 = m_AxisDivisions * (m_HeightCuts + 1);
		for (int n = 0; n < m_AxisDivisions; n++)
		{
			float y3 = (0f - magnitude) * 0.5f;
			array2[num9] = new Vector3(array[n].x, y3, array[n].y);
			array2[num9 + 1] = new Vector3(0f, y3, 0f);
			if (n != m_AxisDivisions - 1)
			{
				array2[num9 + 2] = new Vector3(array[n + 1].x, y3, array[n + 1].y);
			}
			else
			{
				array2[num9 + 2] = new Vector3(array[0].x, y3, array[0].y);
			}
			array3[num10 + n] = new Face(new int[3]
			{
				num9 + 2,
				num9 + 1,
				num9
			});
			num9 += 3;
			float y4 = magnitude * 0.5f;
			array2[num9] = new Vector3(array[n].x, y4, array[n].y);
			array2[num9 + 1] = new Vector3(0f, y4, 0f);
			if (n != m_AxisDivisions - 1)
			{
				array2[num9 + 2] = new Vector3(array[n + 1].x, y4, array[n + 1].y);
			}
			else
			{
				array2[num9 + 2] = new Vector3(array[0].x, y4, array[0].y);
			}
			array3[num10 + (n + m_AxisDivisions)] = new Face(new int[3]
			{
				num9,
				num9 + 1,
				num9 + 2
			});
			num9 += 3;
		}
		for (int num11 = 0; num11 < array2.Length; num11++)
		{
			array2[num11] = rotation * array2[num11];
		}
		mesh.RebuildWithPositionsAndFaces(array2, array3);
		return UpdateBounds(mesh, size, rotation, default(Bounds));
	}
}
