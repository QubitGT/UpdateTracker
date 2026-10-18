using System;
using System.Collections.Generic;

namespace UnityEngine.ProBuilder.Shapes;

[Shape("Cone")]
public class Cone : Shape
{
	[Range(3f, 64f)]
	[SerializeField]
	internal int m_NumberOfSides = 6;

	private float m_Radius;

	[SerializeField]
	private bool m_Smooth = true;

	internal override void SetParametersToBuiltInShape()
	{
		m_NumberOfSides = 8;
		m_Radius = 0.5f;
		m_Smooth = false;
	}

	public override void CopyShape(Shape shape)
	{
		if (shape is Cone)
		{
			Cone cone = (Cone)shape;
			m_NumberOfSides = cone.m_NumberOfSides;
			m_Radius = cone.m_Radius;
			m_Smooth = cone.m_Smooth;
		}
	}

	public override Bounds UpdateBounds(ProBuilderMesh mesh, Vector3 size, Quaternion rotation, Bounds bounds)
	{
		Vector3 v = rotation * Vector3.up;
		v = v.Abs();
		Vector3 size2 = mesh.mesh.bounds.size;
		size2.x = Mathf.Lerp(m_Radius * 2f, size2.x, v.x);
		size2.y = Mathf.Lerp(m_Radius * 2f, size2.y, v.y);
		size2.z = Mathf.Lerp(m_Radius * 2f, size2.z, v.z);
		bounds.size = size2;
		return bounds;
	}

	public override Bounds RebuildMesh(ProBuilderMesh mesh, Vector3 size, Quaternion rotation)
	{
		Vector3 vector = size.Abs();
		m_Radius = System.Math.Min(vector.x, vector.z);
		float y = vector.y;
		int numberOfSides = m_NumberOfSides;
		Vector3[] array = new Vector3[numberOfSides];
		for (int i = 0; i < numberOfSides; i++)
		{
			Vector2 vector2 = Math.PointInCircumference(m_Radius, (float)i * (360f / (float)numberOfSides), Vector2.zero);
			array[i] = new Vector3(vector2.x, (0f - y) / 2f, vector2.y);
		}
		List<Vector3> list = new List<Vector3>();
		List<Face> list2 = new List<Face>();
		for (int j = 0; j < numberOfSides; j++)
		{
			list.Add(array[j]);
			list.Add((j < numberOfSides - 1) ? array[j + 1] : array[0]);
			list.Add(Vector3.up * y / 2f);
			list.Add(array[j]);
			list.Add((j < numberOfSides - 1) ? array[j + 1] : array[0]);
			list.Add(Vector3.down * y / 2f);
		}
		List<Face> list3 = new List<Face>();
		for (int k = 0; k < numberOfSides * 6; k += 6)
		{
			Face face = new Face(new int[3]
			{
				k + 2,
				k + 1,
				k
			});
			face.smoothingGroup = (m_Smooth ? 1 : 0);
			list2.Add(face);
			list3.Add(face);
			list2.Add(new Face(new int[3]
			{
				k + 3,
				k + 4,
				k + 5
			}));
		}
		Vector3 b = size.Sign();
		for (int l = 0; l < list.Count; l++)
		{
			list[l] = Vector3.Scale(rotation * list[l], b);
		}
		if (Mathf.Sign(size.x) * Mathf.Sign(size.y) * Mathf.Sign(size.z) < 0f)
		{
			foreach (Face item in list2)
			{
				item.Reverse();
			}
		}
		mesh.RebuildWithPositionsAndFaces(list, list2);
		mesh.unwrapParameters = new UnwrapParameters
		{
			packMargin = 30f
		};
		Face face2 = list3[0];
		AutoUnwrapSettings uv = face2.uv;
		uv.anchor = AutoUnwrapSettings.Anchor.LowerLeft;
		face2.uv = uv;
		face2.manualUV = true;
		UvUnwrapping.Unwrap(mesh, face2, Vector3.up);
		for (int m = 1; m < list3.Count; m++)
		{
			Face face3 = list3[m];
			face3.manualUV = true;
			UvUnwrapping.CopyUVs(mesh, face2, face3);
		}
		mesh.RefreshUV(list3);
		return UpdateBounds(mesh, size, rotation, default(Bounds));
	}
}
