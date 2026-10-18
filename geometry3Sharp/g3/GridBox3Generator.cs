using System;
using System.Collections.Generic;

namespace g3;

public class GridBox3Generator : MeshGenerator
{
	public Box3d Box = Box3d.UnitZeroCentered;

	public int EdgeVertices = 8;

	public bool NoSharedVertices;

	public override MeshGenerator Generate()
	{
		int N = ((EdgeVertices > 1) ? EdgeVertices : 2);
		int num = N - 2;
		int num2 = N - 1;
		int num3 = N * N;
		vertices = new VectorArray3d(NoSharedVertices ? (num3 * 6) : (8 + num * 12 + num * num * 6));
		uv = new VectorArray2f(vertices.Count);
		normals = new VectorArray3f(vertices.Count);
		triangles = new IndexArray3i(2 * num2 * num2 * 6);
		groups = new int[triangles.Count];
		Vector3d[] array = Box.ComputeVertices();
		int vi = 0;
		int num4 = 0;
		if (NoSharedVertices)
		{
			for (int i = 0; i < 6; i++)
			{
				Vector3d v = array[gIndices.BoxFaces[i, 0]];
				Vector3d v2 = array[gIndices.BoxFaces[i, 1]];
				Vector3d v3 = array[gIndices.BoxFaces[i, 2]];
				Vector3d v4 = array[gIndices.BoxFaces[i, 3]];
				Vector3f value = Math.Sign(gIndices.BoxFaceNormals[i]) * (Vector3f)Box.Axis(Math.Abs(gIndices.BoxFaceNormals[i]) - 1);
				int num5 = vi;
				for (int j = 0; j < N; j++)
				{
					double num6 = (double)j / (double)(N - 1);
					for (int k = 0; k < N; k++)
					{
						double num7 = (double)k / (double)(N - 1);
						normals[vi] = value;
						uv[vi] = new Vector2f(num7, num6);
						vertices[vi++] = bilerp(ref v, ref v2, ref v3, ref v4, num7, num6);
					}
				}
				for (int l = 0; l < num2; l++)
				{
					for (int m = 0; m < num2; m++)
					{
						int num8 = num5 + l * N + m;
						int num9 = num5 + (l + 1) * N + m;
						int b2 = num8 + 1;
						int num10 = num9 + 1;
						groups[num4] = i;
						triangles.Set(num4++, num8, b2, num10, Clockwise);
						groups[num4] = i;
						triangles.Set(num4++, num8, num10, num9, Clockwise);
					}
				}
			}
		}
		else
		{
			Vector3i[] array2 = new Vector3i[array.Length];
			for (int n = 0; n < array.Length; n++)
			{
				Vector3d vector3d = array[n] - Box.Center;
				array2[n] = new Vector3i((!(vector3d.x < 0.0)) ? (N - 1) : 0, (!(vector3d.y < 0.0)) ? (N - 1) : 0, (!(vector3d.z < 0.0)) ? (N - 1) : 0);
			}
			int[] array3 = new int[num3];
			Dictionary<Vector3i, int> edgeVerts = new Dictionary<Vector3i, int>();
			for (int num11 = 0; num11 < 6; num11++)
			{
				int num12 = gIndices.BoxFaces[num11, 0];
				int num13 = gIndices.BoxFaces[num11, 1];
				int num14 = gIndices.BoxFaces[num11, 2];
				int num15 = gIndices.BoxFaces[num11, 3];
				Vector3d vector3d2 = array[num12];
				Vector3i vector3i = array2[num12];
				Vector3d vector3d3 = array[num13];
				Vector3i vector3i2 = array2[num13];
				Vector3d vector3d4 = array[num14];
				Vector3i vector3i3 = array2[num14];
				Vector3d vector3d5 = array[num15];
				Vector3i vector3i4 = array2[num15];
				Action<Vector3d, Vector3d, Vector3i, Vector3i> obj = delegate(Vector3d a, Vector3d b, Vector3i ai, Vector3i bi)
				{
					for (int num30 = 0; num30 < N; num30++)
					{
						double t = (double)num30 / (double)(N - 1);
						Vector3i key2 = lerp(ref ai, ref bi, t);
						if (!edgeVerts.ContainsKey(key2))
						{
							Vector3d value5 = Vector3d.Lerp(ref a, ref b, t);
							normals[vi] = (Vector3f)value5.Normalized;
							uv[vi] = Vector2f.Zero;
							edgeVerts[key2] = vi;
							vertices[vi++] = value5;
						}
					}
				};
				obj(vector3d2, vector3d3, vector3i, vector3i2);
				obj(vector3d3, vector3d4, vector3i2, vector3i3);
				obj(vector3d4, vector3d5, vector3i3, vector3i4);
				obj(vector3d5, vector3d2, vector3i4, vector3i);
			}
			for (int num16 = 0; num16 < 6; num16++)
			{
				int num17 = gIndices.BoxFaces[num16, 0];
				int num18 = gIndices.BoxFaces[num16, 1];
				int num19 = gIndices.BoxFaces[num16, 2];
				int num20 = gIndices.BoxFaces[num16, 3];
				Vector3d v5 = array[num17];
				Vector3i v6 = array2[num17];
				Vector3d v7 = array[num18];
				Vector3i v8 = array2[num18];
				Vector3d v9 = array[num19];
				Vector3i v10 = array2[num19];
				Vector3d v11 = array[num20];
				Vector3i v12 = array2[num20];
				Vector3f value2 = Math.Sign(gIndices.BoxFaceNormals[num16]) * (Vector3f)Box.Axis(Math.Abs(gIndices.BoxFaceNormals[num16]) - 1);
				for (int num21 = 0; num21 < N; num21++)
				{
					double num22 = (double)num21 / (double)(N - 1);
					for (int num23 = 0; num23 < N; num23++)
					{
						double num24 = (double)num23 / (double)(N - 1);
						Vector3i key = bilerp(ref v6, ref v8, ref v10, ref v12, num24, num22);
						if (!edgeVerts.TryGetValue(key, out var value3))
						{
							Vector3d value4 = bilerp(ref v5, ref v7, ref v9, ref v11, num24, num22);
							value3 = vi++;
							normals[value3] = value2;
							uv[value3] = new Vector2f(num24, num22);
							vertices[value3] = value4;
						}
						array3[num21 * N + num23] = value3;
					}
				}
				for (int num25 = 0; num25 < num2; num25++)
				{
					int num26 = num25 + 1;
					for (int num27 = 0; num27 < num2; num27++)
					{
						int num28 = num27 + 1;
						int a2 = array3[num25 * N + num27];
						int b3 = array3[num25 * N + num28];
						int num29 = array3[num26 * N + num28];
						int c = array3[num26 * N + num27];
						groups[num4] = num16;
						triangles.Set(num4++, a2, b3, num29, Clockwise);
						groups[num4] = num16;
						triangles.Set(num4++, a2, num29, c, Clockwise);
					}
				}
			}
		}
		return this;
	}
}
