using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.ProBuilder;

public static class Projection
{
	public static Vector2[] PlanarProject(IList<Vector3> positions, IList<int> indexes = null)
	{
		return PlanarProject(positions, indexes, FindBestPlane(positions, indexes).normal);
	}

	public static Vector2[] PlanarProject(IList<Vector3> positions, IList<int> indexes, Vector3 direction)
	{
		List<Vector2> list = new List<Vector2>(indexes?.Count ?? positions.Count);
		PlanarProject(positions, indexes, direction, list);
		return list.ToArray();
	}

	internal static void PlanarProject(IList<Vector3> positions, IList<int> indexes, Vector3 direction, List<Vector2> results)
	{
		if (positions == null)
		{
			throw new ArgumentNullException("positions");
		}
		if (results == null)
		{
			throw new ArgumentNullException("results");
		}
		Vector3 vector = Math.EnsureUnitVector(direction);
		Vector3 tangentToAxis = GetTangentToAxis(VectorToProjectionAxis(vector));
		int num = indexes?.Count ?? positions.Count;
		results.Clear();
		Vector3 lhs = Vector3.Cross(vector, tangentToAxis);
		Vector3 lhs2 = Vector3.Cross(lhs, vector);
		lhs.Normalize();
		lhs2.Normalize();
		if (indexes != null)
		{
			int i = 0;
			for (int num2 = num; i < num2; i++)
			{
				results.Add(new Vector2(Vector3.Dot(lhs, positions[indexes[i]]), Vector3.Dot(lhs2, positions[indexes[i]])));
			}
		}
		else
		{
			int j = 0;
			for (int num3 = num; j < num3; j++)
			{
				results.Add(new Vector2(Vector3.Dot(lhs, positions[j]), Vector3.Dot(lhs2, positions[j])));
			}
		}
	}

	internal static void PlanarProject(ProBuilderMesh mesh, int textureGroup, AutoUnwrapSettings unwrapSettings)
	{
		bool useWorldSpace = unwrapSettings.useWorldSpace;
		Transform transform = null;
		Face[] facesInternal = mesh.facesInternal;
		Vector3 vector = Vector3.zero;
		int i = 0;
		for (int num = facesInternal.Length; i < num; i++)
		{
			if (facesInternal[i].textureGroup == textureGroup)
			{
				Vector3 vector2 = Math.Normal(mesh, facesInternal[i]);
				vector += vector2;
			}
		}
		if (useWorldSpace)
		{
			transform = mesh.transform;
			vector = transform.TransformDirection(vector);
		}
		Vector3 tangentToAxis = GetTangentToAxis(VectorToProjectionAxis(vector));
		Vector3 lhs = Vector3.Cross(vector, tangentToAxis);
		Vector3 lhs2 = Vector3.Cross(lhs, vector);
		lhs.Normalize();
		lhs2.Normalize();
		Vector3[] positionsInternal = mesh.positionsInternal;
		Vector2[] texturesInternal = mesh.texturesInternal;
		int j = 0;
		for (int num2 = facesInternal.Length; j < num2; j++)
		{
			if (facesInternal[j].textureGroup == textureGroup)
			{
				int[] distinctIndexesInternal = facesInternal[j].distinctIndexesInternal;
				int k = 0;
				for (int num3 = distinctIndexesInternal.Length; k < num3; k++)
				{
					Vector3 rhs = (useWorldSpace ? transform.TransformPoint(positionsInternal[distinctIndexesInternal[k]]) : positionsInternal[distinctIndexesInternal[k]]);
					texturesInternal[distinctIndexesInternal[k]].x = Vector3.Dot(lhs, rhs);
					texturesInternal[distinctIndexesInternal[k]].y = Vector3.Dot(lhs2, rhs);
				}
			}
		}
	}

	internal static void PlanarProject(ProBuilderMesh mesh, Face face, Vector3 projection = default(Vector3))
	{
		Vector3 vector = Math.EnsureUnitVector(Math.Normal(mesh, face));
		Transform transform = null;
		bool useWorldSpace = face.uv.useWorldSpace;
		if (useWorldSpace)
		{
			transform = mesh.transform;
			vector = transform.TransformDirection(vector);
		}
		Vector3 vector2 = projection;
		if (vector2 == Vector3.zero)
		{
			vector2 = GetTangentToAxis(VectorToProjectionAxis(vector));
		}
		Vector3 lhs = Vector3.Cross(vector, vector2);
		Vector3 lhs2 = Vector3.Cross(lhs, vector);
		lhs.Normalize();
		lhs2.Normalize();
		Vector3[] positionsInternal = mesh.positionsInternal;
		Vector2[] texturesInternal = mesh.texturesInternal;
		int[] distinctIndexesInternal = face.distinctIndexesInternal;
		int i = 0;
		for (int num = distinctIndexesInternal.Length; i < num; i++)
		{
			Vector3 rhs = (useWorldSpace ? transform.TransformPoint(positionsInternal[distinctIndexesInternal[i]]) : positionsInternal[distinctIndexesInternal[i]]);
			texturesInternal[distinctIndexesInternal[i]].x = Vector3.Dot(lhs, rhs);
			texturesInternal[distinctIndexesInternal[i]].y = Vector3.Dot(lhs2, rhs);
		}
	}

	internal static Vector2[] SphericalProject(IList<Vector3> vertices, IList<int> indexes = null)
	{
		int num = indexes?.Count ?? vertices.Count;
		Vector2[] array = new Vector2[num];
		Vector3 vector = Math.Average(vertices, indexes);
		for (int i = 0; i < num; i++)
		{
			int index = indexes?[i] ?? i;
			Vector3 vector2 = vertices[index] - vector;
			vector2.Normalize();
			array[i].x = 0.5f + Mathf.Atan2(vector2.z, vector2.x) / (MathF.PI * 2f);
			array[i].y = 0.5f - Mathf.Asin(vector2.y) / MathF.PI;
		}
		return array;
	}

	internal static IList<Vector2> Sort(IList<Vector2> verts, SortMethod method = SortMethod.CounterClockwise)
	{
		Vector2 vector = Math.Average(verts);
		Vector2 up = Vector2.up;
		int count = verts.Count;
		List<SimpleTuple<float, Vector2>> list = new List<SimpleTuple<float, Vector2>>(count);
		for (int i = 0; i < count; i++)
		{
			list.Add(new SimpleTuple<float, Vector2>(Math.SignedAngle(up, verts[i] - vector), verts[i]));
		}
		list.Sort((SimpleTuple<float, Vector2> a, SimpleTuple<float, Vector2> b) => (!(a.item1 < b.item1)) ? 1 : (-1));
		IList<Vector2> list2 = list.Select((SimpleTuple<float, Vector2> x) => x.item2).ToList();
		if (method == SortMethod.Clockwise)
		{
			list2 = list2.Reverse().ToList();
		}
		return list2;
	}

	internal static Vector3 GetTangentToAxis(ProjectionAxis axis)
	{
		switch (axis)
		{
		case ProjectionAxis.X:
		case ProjectionAxis.XNegative:
			return Vector3.up;
		case ProjectionAxis.Y:
		case ProjectionAxis.YNegative:
			return Vector3.forward;
		case ProjectionAxis.Z:
		case ProjectionAxis.ZNegative:
			return Vector3.up;
		default:
			return Vector3.up;
		}
	}

	internal static Vector3 ProjectionAxisToVector(ProjectionAxis axis)
	{
		return axis switch
		{
			ProjectionAxis.X => Vector3.right, 
			ProjectionAxis.Y => Vector3.up, 
			ProjectionAxis.Z => Vector3.forward, 
			ProjectionAxis.XNegative => -Vector3.right, 
			ProjectionAxis.YNegative => -Vector3.up, 
			ProjectionAxis.ZNegative => -Vector3.forward, 
			_ => Vector3.forward, 
		};
	}

	internal static ProjectionAxis VectorToProjectionAxis(Vector3 direction)
	{
		float num = System.Math.Abs(direction.x);
		float num2 = System.Math.Abs(direction.y);
		float num3 = System.Math.Abs(direction.z);
		if (!num.Approx(num2) && num > num2 && !num.Approx(num3) && num > num3)
		{
			if (!(direction.x > 0f))
			{
				return ProjectionAxis.XNegative;
			}
			return ProjectionAxis.X;
		}
		if (!num2.Approx(num3) && num2 > num3)
		{
			if (!(direction.y > 0f))
			{
				return ProjectionAxis.YNegative;
			}
			return ProjectionAxis.Y;
		}
		if (!(direction.z > 0f))
		{
			return ProjectionAxis.ZNegative;
		}
		return ProjectionAxis.Z;
	}

	public static Plane FindBestPlane(IList<Vector3> points, IList<int> indexes = null)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		float num5 = 0f;
		float num6 = 0f;
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		bool flag = indexes != null && indexes.Count > 0;
		int num7 = (flag ? indexes.Count : points.Count);
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		for (int i = 0; i < num7; i++)
		{
			zero.x += points[flag ? indexes[i] : i].x;
			zero.y += points[flag ? indexes[i] : i].y;
			zero.z += points[flag ? indexes[i] : i].z;
		}
		zero.x /= num7;
		zero.y /= num7;
		zero.z /= num7;
		for (int j = 0; j < num7; j++)
		{
			Vector3 vector = points[flag ? indexes[j] : j] - zero;
			num += vector.x * vector.x;
			num2 += vector.x * vector.y;
			num3 += vector.x * vector.z;
			num4 += vector.y * vector.y;
			num5 += vector.y * vector.z;
			num6 += vector.z * vector.z;
		}
		float num8 = num4 * num6 - num5 * num5;
		float num9 = num * num6 - num3 * num3;
		float num10 = num * num4 - num2 * num2;
		if (num8 > num9 && num8 > num10)
		{
			zero2.x = 1f;
			zero2.y = (num3 * num5 - num2 * num6) / num8;
			zero2.z = (num2 * num5 - num3 * num4) / num8;
		}
		else if (num9 > num10)
		{
			zero2.x = (num5 * num3 - num2 * num6) / num9;
			zero2.y = 1f;
			zero2.z = (num2 * num3 - num5 * num) / num9;
		}
		else
		{
			zero2.x = (num5 * num2 - num3 * num4) / num10;
			zero2.y = (num3 * num2 - num5 * num) / num10;
			zero2.z = 1f;
		}
		zero2.Normalize();
		return new Plane(zero2, zero);
	}

	internal static Plane FindBestPlane(ProBuilderMesh mesh, int textureGroup)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		float num5 = 0f;
		float num6 = 0f;
		if (mesh == null)
		{
			throw new ArgumentNullException("mesh");
		}
		Vector3 zero = Vector3.zero;
		int num7 = 0;
		Vector3[] positionsInternal = mesh.positionsInternal;
		int faceCount = mesh.faceCount;
		Face[] facesInternal = mesh.facesInternal;
		for (int i = 0; i < faceCount; i++)
		{
			if (facesInternal[i].textureGroup == textureGroup)
			{
				int[] indexesInternal = facesInternal[i].indexesInternal;
				int j = 0;
				for (int num8 = indexesInternal.Length; j < num8; j++)
				{
					zero.x += positionsInternal[indexesInternal[j]].x;
					zero.y += positionsInternal[indexesInternal[j]].y;
					zero.z += positionsInternal[indexesInternal[j]].z;
					num7++;
				}
			}
		}
		zero.x /= num7;
		zero.y /= num7;
		zero.z /= num7;
		for (int k = 0; k < faceCount; k++)
		{
			if (facesInternal[k].textureGroup == textureGroup)
			{
				int[] indexesInternal2 = facesInternal[k].indexesInternal;
				int l = 0;
				for (int num9 = indexesInternal2.Length; l < num9; l++)
				{
					Vector3 vector = positionsInternal[indexesInternal2[l]] - zero;
					num += vector.x * vector.x;
					num2 += vector.x * vector.y;
					num3 += vector.x * vector.z;
					num4 += vector.y * vector.y;
					num5 += vector.y * vector.z;
					num6 += vector.z * vector.z;
				}
			}
		}
		float num10 = num4 * num6 - num5 * num5;
		float num11 = num * num6 - num3 * num3;
		float num12 = num * num4 - num2 * num2;
		Vector3 zero2 = Vector3.zero;
		if (num10 > num11 && num10 > num12)
		{
			zero2.x = 1f;
			zero2.y = (num3 * num5 - num2 * num6) / num10;
			zero2.z = (num2 * num5 - num3 * num4) / num10;
		}
		else if (num11 > num12)
		{
			zero2.x = (num5 * num3 - num2 * num6) / num11;
			zero2.y = 1f;
			zero2.z = (num2 * num3 - num5 * num) / num11;
		}
		else
		{
			zero2.x = (num5 * num2 - num3 * num4) / num12;
			zero2.y = (num3 * num2 - num5 * num) / num12;
			zero2.z = 1f;
		}
		zero2.Normalize();
		return new Plane(zero2, zero);
	}
}
