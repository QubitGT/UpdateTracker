using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace g3;

public abstract class MeshGenerator
{
	public struct CircularSection
	{
		public float Radius;

		public float SectionY;

		public CircularSection(float r, float y)
		{
			Radius = r;
			SectionY = y;
		}
	}

	public VectorArray3d vertices;

	public VectorArray2f uv;

	public VectorArray3f normals;

	public IndexArray3i triangles;

	public int[] groups;

	public bool WantUVs = true;

	public bool WantNormals = true;

	public bool WantGroups = true;

	public bool Clockwise;

	public abstract MeshGenerator Generate();

	public virtual void MakeMesh(SimpleMesh m)
	{
		m.AppendVertices(vertices, WantNormals ? normals : null, null, WantUVs ? uv : null);
		m.AppendTriangles(triangles);
	}

	public virtual SimpleMesh MakeSimpleMesh()
	{
		SimpleMesh simpleMesh = new SimpleMesh();
		MakeMesh(simpleMesh);
		return simpleMesh;
	}

	public virtual void MakeMesh(DMesh3 m)
	{
		int count = vertices.Count;
		bool flag = WantNormals && normals != null && normals.Count == vertices.Count;
		if (flag)
		{
			m.EnableVertexNormals(Vector3f.AxisY);
		}
		bool flag2 = WantUVs && uv != null && uv.Count == vertices.Count;
		if (flag2)
		{
			m.EnableVertexUVs(Vector2f.Zero);
		}
		for (int i = 0; i < count; i++)
		{
			NewVertexInfo newVertexInfo = default(NewVertexInfo);
			newVertexInfo.v = vertices[i];
			NewVertexInfo info = newVertexInfo;
			if (flag)
			{
				info.bHaveN = true;
				info.n = normals[i];
			}
			if (flag2)
			{
				info.bHaveUV = true;
				info.uv = uv[i];
			}
			m.AppendVertex(info);
		}
		int count2 = triangles.Count;
		if (WantGroups && groups != null && groups.Length == count2)
		{
			m.EnableTriangleGroups();
			for (int j = 0; j < count2; j++)
			{
				m.AppendTriangle(triangles[j], groups[j]);
			}
		}
		else
		{
			for (int k = 0; k < count2; k++)
			{
				m.AppendTriangle(triangles[k]);
			}
		}
	}

	public virtual DMesh3 MakeDMesh()
	{
		DMesh3 dMesh = new DMesh3();
		MakeMesh(dMesh);
		return dMesh;
	}

	public virtual void MakeMesh(NTMesh3 m)
	{
		int count = vertices.Count;
		for (int i = 0; i < count; i++)
		{
			m.AppendVertex(vertices[i]);
		}
		int count2 = triangles.Count;
		if (WantGroups && groups != null && groups.Length == count2)
		{
			m.EnableTriangleGroups();
			for (int j = 0; j < count2; j++)
			{
				m.AppendTriangle(triangles[j], groups[j]);
			}
		}
		else
		{
			for (int k = 0; k < count2; k++)
			{
				m.AppendTriangle(triangles[k]);
			}
		}
	}

	public virtual NTMesh3 MakeNTMesh()
	{
		NTMesh3 nTMesh = new NTMesh3();
		MakeMesh(nTMesh);
		return nTMesh;
	}

	protected void duplicate_vertex_span(int nStart, int nCount)
	{
		for (int i = 0; i < nCount; i++)
		{
			vertices[nStart + nCount + i] = vertices[nStart + i];
			normals[nStart + nCount + i] = normals[nStart + i];
			uv[nStart + nCount + i] = uv[nStart + i];
		}
	}

	protected void append_disc(int Slices, int nCenterV, int nRingStart, bool bClosed, bool bCycle, ref int tri_counter, int groupid = -1)
	{
		int num = nRingStart + Slices;
		for (int i = nRingStart; i < num - 1; i++)
		{
			if (groupid >= 0)
			{
				groups[tri_counter] = groupid;
			}
			triangles.Set(tri_counter++, i, nCenterV, i + 1, bCycle);
		}
		if (bClosed)
		{
			if (groupid >= 0)
			{
				groups[tri_counter] = groupid;
			}
			triangles.Set(tri_counter++, num - 1, nCenterV, nRingStart, bCycle);
		}
	}

	protected void append_rectangle(int v0, int v1, int v2, int v3, bool bCycle, ref int tri_counter, int groupid = -1)
	{
		if (groupid >= 0)
		{
			groups[tri_counter] = groupid;
		}
		triangles.Set(tri_counter++, v0, v1, v2, bCycle);
		if (groupid >= 0)
		{
			groups[tri_counter] = groupid;
		}
		triangles.Set(tri_counter++, v0, v2, v3, bCycle);
	}

	protected void append_2d_disc_segment(int iCenter, int iEnd1, int iEnd2, int nSteps, bool bCycle, ref int vtx_counter, ref int tri_counter, int groupid = -1, double force_r = 0.0)
	{
		Vector3d vector3d = vertices[iCenter];
		Vector3d vector3d2 = vertices[iEnd1];
		Vector3d vector3d3 = vertices[iEnd2];
		Vector3d vector3d4 = vector3d2 - vector3d;
		double num = vector3d4.Normalize();
		if (force_r > 0.0)
		{
			num = force_r;
		}
		double num2 = Math.Atan2(vector3d4.z, vector3d4.x);
		Vector3d vector3d5 = vector3d3 - vector3d;
		double num3 = vector3d5.Normalize();
		if (force_r > 0.0)
		{
			num3 = force_r;
		}
		double num4 = Math.Atan2(vector3d5.z, vector3d5.x);
		if (num2 < 0.0)
		{
			num2 += Math.PI * 2.0;
		}
		if (num4 < 0.0)
		{
			num4 += Math.PI * 2.0;
		}
		if (num4 < num2)
		{
			num4 += Math.PI * 2.0;
		}
		int b = iEnd1;
		for (int i = 0; i < nSteps; i++)
		{
			double num5 = (double)(i + 1) / (double)(nSteps + 1);
			double num6 = (1.0 - num5) * num2 + num5 * num4;
			Vector3d vector3d6 = vector3d + new Vector3d(num * Math.Cos(num6), 0.0, num3 * Math.Sin(num6));
			vertices.Set(vtx_counter, vector3d6.x, vector3d6.y, vector3d6.z);
			if (groupid >= 0)
			{
				groups[tri_counter] = groupid;
			}
			triangles.Set(tri_counter++, iCenter, b, vtx_counter, bCycle);
			b = vtx_counter++;
		}
		if (groupid >= 0)
		{
			groups[tri_counter] = groupid;
		}
		triangles.Set(tri_counter++, iCenter, b, iEnd2, bCycle);
	}

	protected Vector3f estimate_normal(int v0, int v1, int v2)
	{
		Vector3d vector3d = vertices[v0];
		Vector3d vector3d2 = vertices[v1];
		Vector3d vector3d3 = vertices[v2];
		Vector3d normalized = (vector3d2 - vector3d).Normalized;
		Vector3d normalized2 = (vector3d3 - vector3d).Normalized;
		return new Vector3f(normalized.Cross(normalized2));
	}

	protected Vector3d bilerp(ref Vector3d v00, ref Vector3d v10, ref Vector3d v11, ref Vector3d v01, double tx, double ty)
	{
		Vector3d a = Vector3d.Lerp(ref v00, ref v01, ty);
		Vector3d b = Vector3d.Lerp(ref v10, ref v11, ty);
		return Vector3d.Lerp(a, b, tx);
	}

	protected Vector2d bilerp(ref Vector2d v00, ref Vector2d v10, ref Vector2d v11, ref Vector2d v01, double tx, double ty)
	{
		Vector2d a = Vector2d.Lerp(ref v00, ref v01, ty);
		Vector2d b = Vector2d.Lerp(ref v10, ref v11, ty);
		return Vector2d.Lerp(a, b, tx);
	}

	protected Vector2f bilerp(ref Vector2f v00, ref Vector2f v10, ref Vector2f v11, ref Vector2f v01, float tx, float ty)
	{
		Vector2f a = Vector2f.Lerp(ref v00, ref v01, ty);
		Vector2f b = Vector2f.Lerp(ref v10, ref v11, ty);
		return Vector2f.Lerp(a, b, tx);
	}

	protected Vector3i bilerp(ref Vector3i v00, ref Vector3i v10, ref Vector3i v11, ref Vector3i v01, double tx, double ty)
	{
		Vector3d a = Vector3d.Lerp((Vector3d)v00, (Vector3d)v01, ty);
		Vector3d b = Vector3d.Lerp((Vector3d)v10, (Vector3d)v11, ty);
		Vector3d vector3d = Vector3d.Lerp(a, b, tx);
		return new Vector3i((int)Math.Round(vector3d.x), (int)Math.Round(vector3d.y), (int)Math.Round(vector3d.z));
	}

	protected Vector3i lerp(ref Vector3i a, ref Vector3i b, double t)
	{
		Vector3d vector3d = Vector3d.Lerp((Vector3d)a, (Vector3d)b, t);
		return new Vector3i((int)Math.Round(vector3d.x), (int)Math.Round(vector3d.y), (int)Math.Round(vector3d.z));
	}

	private static Vector3[] ToUnityVector3(VectorArray3f a, bool bFlipLR = false)
	{
		Vector3[] array = new Vector3[a.Count];
		float num = ((!bFlipLR) ? 1 : (-1));
		for (int i = 0; i < a.Count; i++)
		{
			array[i].x = a.array[3 * i];
			array[i].y = a.array[3 * i + 1];
			array[i].z = num * a.array[3 * i + 2];
		}
		return array;
	}

	private static Vector3[] ToUnityVector3(VectorArray3d a, bool bFlipLR = false)
	{
		Vector3[] array = new Vector3[a.Count];
		float num = ((!bFlipLR) ? 1 : (-1));
		for (int i = 0; i < a.Count; i++)
		{
			array[i].x = (float)a.array[3 * i];
			array[i].y = (float)a.array[3 * i + 1];
			array[i].z = num * (float)a.array[3 * i + 2];
		}
		return array;
	}

	private static Vector2[] ToUnityVector2(VectorArray2f a)
	{
		Vector2[] array = new Vector2[a.Count];
		for (int i = 0; i < a.Count; i++)
		{
			array[i].x = a.array[2 * i];
			array[i].y = a.array[2 * i + 1];
		}
		return array;
	}

	public void MakeMesh(Mesh m, bool bRecalcNormals = false, bool bFlipLR = false)
	{
		m.vertices = ToUnityVector3(vertices, bFlipLR);
		if (uv != null && WantUVs)
		{
			m.uv = ToUnityVector2(uv);
		}
		if (normals != null && WantNormals)
		{
			m.normals = ToUnityVector3(normals, bFlipLR);
		}
		if (m.vertexCount > 64000 || triangles.Count > 64000)
		{
			m.indexFormat = IndexFormat.UInt32;
		}
		m.triangles = triangles.array;
		if (bRecalcNormals)
		{
			m.RecalculateNormals();
		}
	}
}
