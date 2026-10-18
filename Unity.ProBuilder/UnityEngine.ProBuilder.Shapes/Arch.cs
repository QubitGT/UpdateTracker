using System;
using System.Collections.Generic;

namespace UnityEngine.ProBuilder.Shapes;

[Shape("Arch")]
public class Arch : Shape
{
	[Min(0.01f)]
	[SerializeField]
	private float m_Thickness = 0.1f;

	[Range(2f, 200f)]
	[SerializeField]
	private int m_NumberOfSides = 5;

	[Range(1f, 360f)]
	[SerializeField]
	private float m_ArchDegrees = 180f;

	[SerializeField]
	private bool m_EndCaps = true;

	[SerializeField]
	private bool m_Smooth = true;

	internal override void SetParametersToBuiltInShape()
	{
		m_Thickness = 1f;
		m_NumberOfSides = 8;
		m_ArchDegrees = 180f;
		m_EndCaps = true;
		m_Smooth = false;
	}

	public override void CopyShape(Shape shape)
	{
		if (shape is Arch)
		{
			Arch arch = (Arch)shape;
			m_Thickness = arch.m_Thickness;
			m_NumberOfSides = arch.m_NumberOfSides;
			m_ArchDegrees = arch.m_ArchDegrees;
			m_EndCaps = arch.m_EndCaps;
			m_Smooth = arch.m_Smooth;
		}
	}

	private Vector3[] GetFace(Vector2 vertex1, Vector2 vertex2, float depth)
	{
		return new Vector3[4]
		{
			new Vector3(vertex1.x, vertex1.y, depth),
			new Vector3(vertex2.x, vertex2.y, depth),
			new Vector3(vertex1.x, vertex1.y, 0f - depth),
			new Vector3(vertex2.x, vertex2.y, 0f - depth)
		};
	}

	public override Bounds RebuildMesh(ProBuilderMesh mesh, Vector3 size, Quaternion rotation)
	{
		Vector3 vector = Vector3.Scale(rotation * Vector3.up, size);
		Vector3 vector2 = Vector3.Scale(rotation * Vector3.right, size);
		Vector3 vector3 = Vector3.Scale(rotation * Vector3.forward, size);
		float num = vector2.magnitude / 2f;
		float num2 = vector.magnitude;
		float num3 = vector3.magnitude / 2f;
		int num4 = m_NumberOfSides + 1;
		float archDegrees = m_ArchDegrees;
		Vector2[] array = new Vector2[num4];
		Vector2[] array2 = new Vector2[num4];
		if (archDegrees < 90f)
		{
			num *= 2f;
		}
		else if (archDegrees < 180f)
		{
			num *= 1f + Mathf.Lerp(1f, 0f, Mathf.Abs(Mathf.Cos(archDegrees * (MathF.PI / 180f))));
		}
		else if (archDegrees > 180f)
		{
			num2 /= 1f + Mathf.Lerp(0f, 1f, (archDegrees - 180f) / 90f);
		}
		for (int i = 0; i < num4; i++)
		{
			float angleInDegrees = (float)i * (archDegrees / (float)(num4 - 1));
			array[i] = Math.PointInEllipseCircumference(num, num2, angleInDegrees, Vector2.zero, out var tangent);
			array2[i] = Math.PointInEllipseCircumference(num - m_Thickness, num2 - m_Thickness, angleInDegrees, Vector2.zero, out tangent);
		}
		List<Vector3> list = new List<Vector3>();
		float z = 0f - num3;
		int num5 = 0;
		for (int j = 0; j < num4 - 1; j++)
		{
			Vector2 vertex = array[j];
			Vector2 vertex2 = ((j < num4 - 1) ? array[j + 1] : array[j]);
			Vector3[] face = GetFace(vertex, vertex2, 0f - num3);
			vertex = array2[j];
			vertex2 = ((j < num4 - 1) ? array2[j + 1] : array2[j]);
			Vector3[] face2 = GetFace(vertex2, vertex, 0f - num3);
			if (archDegrees < 360f && m_EndCaps && j == 0)
			{
				list.AddRange(GetFace(array[j], array2[j], num3));
			}
			list.AddRange(face);
			list.AddRange(face2);
			num5 += 2;
			if (archDegrees < 360f && m_EndCaps && j == num4 - 2)
			{
				list.AddRange(GetFace(array2[j + 1], array[j + 1], num3));
			}
		}
		for (int k = 0; k < num4 - 1; k++)
		{
			Vector2 vertex = array[k];
			Vector2 vertex2 = ((k < num4 - 1) ? array[k + 1] : array[k]);
			Vector2 vector4 = array2[k];
			Vector2 vector5 = ((k < num4 - 1) ? array2[k + 1] : array2[k]);
			Vector3[] collection = new Vector3[4]
			{
				new Vector3(vertex.x, vertex.y, num3),
				new Vector3(vertex2.x, vertex2.y, num3),
				new Vector3(vector4.x, vector4.y, num3),
				new Vector3(vector5.x, vector5.y, num3)
			};
			Vector3[] collection2 = new Vector3[4]
			{
				new Vector3(vertex2.x, vertex2.y, z),
				new Vector3(vertex.x, vertex.y, z),
				new Vector3(vector5.x, vector5.y, z),
				new Vector3(vector4.x, vector4.y, z)
			};
			list.AddRange(collection);
			list.AddRange(collection2);
		}
		Vector3 b = size.Sign();
		for (int l = 0; l < list.Count; l++)
		{
			list[l] = Vector3.Scale(rotation * list[l], b);
		}
		mesh.GeometryWithPoints(list.ToArray());
		if (m_Smooth)
		{
			for (int m = ((archDegrees < 360f && m_EndCaps) ? 1 : 0); m < num5; m++)
			{
				mesh.facesInternal[m].smoothingGroup = 1;
			}
		}
		if (b.x * b.y * b.z < 0f)
		{
			Face[] facesInternal = mesh.facesInternal;
			for (int n = 0; n < facesInternal.Length; n++)
			{
				facesInternal[n].Reverse();
			}
		}
		mesh.TranslateVerticesInWorldSpace(mesh.mesh.triangles, mesh.transform.TransformDirection(-mesh.mesh.bounds.center));
		mesh.Refresh();
		return UpdateBounds(mesh, size, rotation, default(Bounds));
	}
}
