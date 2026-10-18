using System.Collections.Generic;

namespace UnityEngine.ProBuilder.Shapes;

[Shape("Pipe")]
public class Pipe : Shape
{
	[Min(0.01f)]
	[SerializeField]
	private float m_Thickness = 0.25f;

	[Range(3f, 64f)]
	[SerializeField]
	private int m_NumberOfSides = 6;

	[Range(0f, 31f)]
	[SerializeField]
	private int m_HeightCuts;

	[SerializeField]
	private bool m_Smooth = true;

	internal override void SetParametersToBuiltInShape()
	{
		m_Thickness = 0.25f;
		m_NumberOfSides = 8;
		m_HeightCuts = 2;
		m_Smooth = false;
	}

	public override void CopyShape(Shape shape)
	{
		if (shape is Pipe)
		{
			Pipe pipe = (Pipe)shape;
			m_Thickness = pipe.m_Thickness;
			m_NumberOfSides = pipe.m_NumberOfSides;
			m_HeightCuts = pipe.m_HeightCuts;
			m_Smooth = pipe.m_Smooth;
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
		Vector2[] array = new Vector2[m_NumberOfSides];
		Vector2[] array2 = new Vector2[m_NumberOfSides];
		for (int i = 0; i < m_NumberOfSides; i++)
		{
			float angleInDegrees = (float)i * (360f / (float)m_NumberOfSides);
			array[i] = Math.PointInEllipseCircumference(xRadius, yRadius, angleInDegrees, Vector2.zero, out var tangent);
			Vector2 vector4 = new Vector2(0f - tangent.y, tangent.x);
			array2[i] = array[i] + m_Thickness * vector4;
		}
		List<Vector3> list = new List<Vector3>();
		float num = magnitude / 2f;
		int num2 = m_HeightCuts + 1;
		for (int j = 0; j < num2; j++)
		{
			float y = (float)j * (magnitude / (float)num2) - num;
			float y2 = (float)(j + 1) * (magnitude / (float)num2) - num;
			for (int k = 0; k < m_NumberOfSides; k++)
			{
				Vector2 vector5 = array[k];
				Vector2 vector6 = ((k < m_NumberOfSides - 1) ? array[k + 1] : array[0]);
				Vector3[] collection = new Vector3[4]
				{
					new Vector3(vector6.x, y, vector6.y),
					new Vector3(vector5.x, y, vector5.y),
					new Vector3(vector6.x, y2, vector6.y),
					new Vector3(vector5.x, y2, vector5.y)
				};
				vector5 = array2[k];
				vector6 = ((k < m_NumberOfSides - 1) ? array2[k + 1] : array2[0]);
				Vector3[] collection2 = new Vector3[4]
				{
					new Vector3(vector5.x, y, vector5.y),
					new Vector3(vector6.x, y, vector6.y),
					new Vector3(vector5.x, y2, vector5.y),
					new Vector3(vector6.x, y2, vector6.y)
				};
				list.AddRange(collection);
				list.AddRange(collection2);
			}
		}
		for (int l = 0; l < m_NumberOfSides; l++)
		{
			Vector2 vector5 = array[l];
			Vector2 vector6 = ((l < m_NumberOfSides - 1) ? array[l + 1] : array[0]);
			Vector2 vector7 = array2[l];
			Vector2 vector8 = ((l < m_NumberOfSides - 1) ? array2[l + 1] : array2[0]);
			Vector3[] collection3 = new Vector3[4]
			{
				new Vector3(vector6.x, magnitude - num, vector6.y),
				new Vector3(vector5.x, magnitude - num, vector5.y),
				new Vector3(vector8.x, magnitude - num, vector8.y),
				new Vector3(vector7.x, magnitude - num, vector7.y)
			};
			Vector3[] collection4 = new Vector3[4]
			{
				new Vector3(vector5.x, 0f - num, vector5.y),
				new Vector3(vector6.x, 0f - num, vector6.y),
				new Vector3(vector7.x, 0f - num, vector7.y),
				new Vector3(vector8.x, 0f - num, vector8.y)
			};
			list.AddRange(collection4);
			list.AddRange(collection3);
		}
		for (int m = 0; m < list.Count; m++)
		{
			list[m] = rotation * list[m];
		}
		mesh.GeometryWithPoints(list.ToArray());
		if (m_Smooth)
		{
			int num3 = 2 * num2 * m_NumberOfSides;
			for (int n = 0; n < num3; n++)
			{
				mesh.facesInternal[n].smoothingGroup = 1;
			}
		}
		return UpdateBounds(mesh, size, rotation, default(Bounds));
	}
}
