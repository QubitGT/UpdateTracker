using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Rendering.Universal.UTess;
using UnityEngine.U2D;

namespace UnityEngine.Rendering.Universal;

internal static class LightUtility
{
	private enum PivotType
	{
		PivotBase,
		PivotCurve,
		PivotIntersect,
		PivotSkip,
		PivotClip
	}

	[Serializable]
	internal struct LightMeshVertex
	{
		public Vector3 position;

		public Color color;

		public Vector2 uv;

		public static readonly VertexAttributeDescriptor[] VertexLayout = new VertexAttributeDescriptor[3]
		{
			new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0),
			new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 4),
			new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2)
		};
	}

	public static bool CheckForChange(Light2D.LightType a, ref Light2D.LightType b)
	{
		bool result = a != b;
		b = a;
		return result;
	}

	public static bool CheckForChange(Component a, ref Component b)
	{
		bool result = a != b;
		b = a;
		return result;
	}

	public static bool CheckForChange(int a, ref int b)
	{
		bool result = a != b;
		b = a;
		return result;
	}

	public static bool CheckForChange(float a, ref float b)
	{
		bool result = a != b;
		b = a;
		return result;
	}

	public static bool CheckForChange(bool a, ref bool b)
	{
		bool result = a != b;
		b = a;
		return result;
	}

	private static bool TestPivot(List<IntPoint> path, int activePoint, long lastPoint)
	{
		for (int i = activePoint; i < path.Count; i++)
		{
			if (path[i].N > lastPoint)
			{
				return true;
			}
		}
		return path[activePoint].N == -1;
	}

	private static List<IntPoint> DegeneratePivots(List<IntPoint> path, List<IntPoint> inPath, ref int interiorStart)
	{
		List<IntPoint> list = new List<IntPoint>();
		long num = path[0].N;
		long num2 = path[0].N;
		for (int i = 1; i < path.Count; i++)
		{
			if (path[i].N != -1)
			{
				num = Math.Min(num, path[i].N);
				num2 = Math.Max(num2, path[i].N);
			}
		}
		for (long num3 = 0L; num3 < num; num3++)
		{
			IntPoint item = path[(int)num];
			item.N = num3;
			list.Add(item);
		}
		list.AddRange(path.GetRange(0, path.Count));
		interiorStart = list.Count;
		for (long num4 = num2 + 1; num4 < inPath.Count; num4++)
		{
			IntPoint item2 = inPath[(int)num4];
			item2.N = num4;
			list.Add(item2);
		}
		return list;
	}

	private static List<IntPoint> SortPivots(List<IntPoint> outPath, List<IntPoint> inPath)
	{
		List<IntPoint> list = new List<IntPoint>();
		_ = outPath[0];
		long n = outPath[0].N;
		int num = 0;
		bool flag = true;
		for (int i = 1; i < outPath.Count; i++)
		{
			if (n > outPath[i].N && flag && outPath[i].N != -1)
			{
				n = outPath[i].N;
				num = i;
				flag = false;
			}
			else if (outPath[i].N >= n)
			{
				n = outPath[i].N;
				flag = true;
			}
		}
		list.AddRange(outPath.GetRange(num, outPath.Count - num));
		list.AddRange(outPath.GetRange(0, num));
		return list;
	}

	private static List<IntPoint> FixPivots(List<IntPoint> outPath, List<IntPoint> inPath, ref int interiorStart)
	{
		List<IntPoint> list = SortPivots(outPath, inPath);
		long n = list[0].N;
		for (int i = 1; i < list.Count; i++)
		{
			int index = ((i != list.Count - 1) ? (i + 1) : 0);
			IntPoint intPoint = list[i - 1];
			IntPoint value = list[i];
			IntPoint intPoint2 = list[index];
			if (intPoint.N > value.N && TestPivot(list, i, n))
			{
				if (intPoint.N == intPoint2.N)
				{
					value.N = intPoint.N;
				}
				else
				{
					value.N = ((n + 1 < inPath.Count) ? (n + 1) : 0);
				}
				value.D = 3L;
				list[i] = value;
			}
			n = list[i].N;
		}
		int num = 1;
		while (num < list.Count - 1)
		{
			IntPoint intPoint3 = list[num - 1];
			IntPoint intPoint4 = list[num];
			IntPoint intPoint5 = list[num + 1];
			if (intPoint4.N - intPoint3.N > 1)
			{
				if (intPoint4.N == intPoint5.N)
				{
					IntPoint value2 = intPoint4;
					value2.N--;
					list[num] = value2;
				}
				else
				{
					IntPoint item = intPoint4;
					item.N--;
					list.Insert(num, item);
				}
			}
			else
			{
				num++;
			}
		}
		return DegeneratePivots(list, inPath, ref interiorStart);
	}

	internal static List<Vector2> GetOutlinePath(Vector3[] shapePath, float offsetDistance)
	{
		List<IntPoint> list = new List<IntPoint>();
		List<Vector2> list2 = new List<Vector2>();
		for (int i = 0; i < shapePath.Length; i++)
		{
			Vector2 vector = new Vector2(shapePath[i].x, shapePath[i].y) * 10000f;
			list.Add(new IntPoint((long)vector.x, (long)vector.y));
		}
		List<List<IntPoint>> solution = new List<List<IntPoint>>();
		ClipperOffset clipperOffset = new ClipperOffset(24.0);
		clipperOffset.AddPath(list, JoinTypes.jtRound, EndTypes.etClosedPolygon);
		clipperOffset.Execute(ref solution, 10000f * offsetDistance, list.Count);
		if (solution.Count > 0)
		{
			int interiorStart = 0;
			List<IntPoint> outPath = solution[0];
			outPath = FixPivots(outPath, list, ref interiorStart);
			for (int j = 0; j < outPath.Count; j++)
			{
				list2.Add(new Vector2((float)outPath[j].X / 10000f, (float)outPath[j].Y / 10000f));
			}
		}
		return list2;
	}

	private static void TransferToMesh(NativeArray<LightMeshVertex> vertices, int vertexCount, NativeArray<ushort> indices, int indexCount, Light2D light)
	{
		Mesh lightMesh = light.lightMesh;
		lightMesh.SetVertexBufferParams(vertexCount, LightMeshVertex.VertexLayout);
		lightMesh.SetVertexBufferData(vertices, 0, 0, vertexCount);
		lightMesh.SetIndices(indices, 0, indexCount, MeshTopology.Triangles, 0);
		light.vertices = new LightMeshVertex[vertexCount];
		NativeArray<LightMeshVertex>.Copy(vertices, light.vertices, vertexCount);
		light.indices = new ushort[indexCount];
		NativeArray<ushort>.Copy(indices, light.indices, indexCount);
	}

	public static Bounds GenerateShapeMesh(Light2D light, Vector3[] shapePath, float falloffDistance, float batchColor)
	{
		Random.State state = Random.state;
		Random.InitState(123456);
		Color color = new Color(0f, 0f, batchColor, 1f);
		Color color2 = new Color(0f, 0f, batchColor, 0f);
		int num = shapePath.Length;
		NativeArray<int2> edges = new NativeArray<int2>(num, Allocator.Temp);
		NativeArray<float2> points = new NativeArray<float2>(num, Allocator.Temp);
		for (int i = 0; i < num; i++)
		{
			int num2 = i + 1;
			if (num2 == num)
			{
				num2 = 0;
			}
			int x = (edges[i] = new int2(i, num2)).x;
			points[x] = new float2(shapePath[x].x, shapePath[x].y);
		}
		NativeArray<int> outIndices = new NativeArray<int>(edges.Length * 8, Allocator.Temp);
		NativeArray<float2> outVertices = new NativeArray<float2>(edges.Length * 8, Allocator.Temp);
		NativeArray<int2> outEdges = new NativeArray<int2>(edges.Length * 8, Allocator.Temp);
		int outVertexCount = 0;
		int outIndexCount = 0;
		int outEdgeCount = 0;
		UnityEngine.Rendering.Universal.UTess.ModuleHandle.Tessellate(Allocator.Temp, points, edges, ref outVertices, ref outVertexCount, ref outIndices, ref outIndexCount, ref outEdges, ref outEdgeCount);
		int num3 = shapePath.Length;
		List<IntPoint> list = new List<IntPoint>();
		for (int j = 0; j < num3; j++)
		{
			long num4 = (long)((double)shapePath[j].x * 10000.0);
			long num5 = (long)((double)shapePath[j].y * 10000.0);
			IntPoint item = new IntPoint(num4 + Random.Range(-10, 10), num5 + Random.Range(-10, 10));
			item.N = j;
			item.D = -1L;
			list.Add(item);
		}
		int num6 = num3 - 1;
		int interiorStart = 0;
		List<List<IntPoint>> solution = new List<List<IntPoint>>();
		ClipperOffset clipperOffset = new ClipperOffset(24.0);
		clipperOffset.AddPath(list, JoinTypes.jtRound, EndTypes.etClosedPolygon);
		clipperOffset.Execute(ref solution, 10000f * falloffDistance, list.Count);
		if (solution.Count > 0)
		{
			List<IntPoint> list2 = solution[0];
			long num7 = num3;
			for (int k = 0; k < list2.Count; k++)
			{
				num7 = ((list2[k].N != -1) ? Math.Min(num7, list2[k].N) : num7);
			}
			bool flag = num7 == 0;
			list2 = FixPivots(list2, list, ref interiorStart);
			int length = outVertexCount + list2.Count + num3;
			int length2 = outIndexCount + list2.Count * 6 + 6;
			NativeArray<LightMeshVertex> vertices = new NativeArray<LightMeshVertex>(length, Allocator.Temp);
			NativeArray<ushort> indices = new NativeArray<ushort>(length2, Allocator.Temp);
			for (int l = 0; l < outIndexCount; l++)
			{
				indices[l] = (ushort)outIndices[l];
			}
			for (int m = 0; m < outVertexCount; m++)
			{
				vertices[m] = new LightMeshVertex
				{
					position = new float3(outVertices[m].x, outVertices[m].y, 0f),
					color = color
				};
			}
			int num8 = outVertexCount;
			int num9 = outIndexCount;
			ushort[] array = new ushort[num3];
			for (int n = 0; n < num3; n++)
			{
				vertices[num8++] = new LightMeshVertex
				{
					position = new float3(shapePath[n].x, shapePath[n].y, 0f),
					color = color
				};
				array[n] = (ushort)(num8 - 1);
			}
			ushort num10 = (ushort)num8;
			ushort num11 = num10;
			long num12 = ((list2[0].N == -1) ? 0 : list2[0].N);
			for (int num13 = 0; num13 < list2.Count; num13++)
			{
				IntPoint intPoint = list2[num13];
				float2 @float = new float2((float)intPoint.X / 10000f, (float)intPoint.Y / 10000f);
				long num14 = ((intPoint.N == -1) ? 0 : intPoint.N);
				vertices[num8++] = new LightMeshVertex
				{
					position = new float3(@float.x, @float.y, 0f),
					color = ((interiorStart > num13) ? color2 : color)
				};
				if (num12 != num14)
				{
					indices[num9++] = array[num12];
					indices[num9++] = array[num14];
					indices[num9++] = (ushort)(num8 - 1);
				}
				indices[num9++] = array[num12];
				indices[num9++] = num10;
				num10 = (indices[num9++] = (ushort)(num8 - 1));
				num12 = num14;
			}
			indices[num9++] = num11;
			indices[num9++] = array[num7];
			indices[num9++] = (flag ? array[num6] : num10);
			indices[num9++] = (flag ? num11 : num10);
			indices[num9++] = (flag ? num10 : array[num7]);
			if (flag)
			{
				float num16 = 0.001f;
				ushort num17 = array[num6];
				bool num18 = MathF.Abs(vertices[num17].position.x - vertices[indices[num9 - 1]].position.x) > num16 || MathF.Abs(vertices[num17].position.y - vertices[indices[num9 - 1]].position.y) > num16;
				bool flag2 = MathF.Abs(vertices[num17].position.x - vertices[indices[num9 - 2]].position.x) > num16 || MathF.Abs(vertices[num17].position.y - vertices[indices[num9 - 2]].position.y) > num16;
				if (!num18 || !flag2)
				{
					num17 = (ushort)(interiorStart + num3 + outVertexCount - 1);
				}
				indices[num9++] = num17;
			}
			else
			{
				indices[num9++] = array[num7 - 1];
			}
			TransferToMesh(vertices, num8, indices, num9, light);
		}
		Random.state = state;
		return light.lightMesh.GetSubMesh(0).bounds;
	}

	public static Bounds GenerateParametricMesh(Light2D light, float radius, float falloffDistance, float angle, int sides, float batchColor)
	{
		float num = MathF.PI / 2f + MathF.PI / 180f * angle;
		if (sides < 3)
		{
			radius = 0.70710677f * radius;
			sides = 4;
		}
		if (sides == 4)
		{
			num = MathF.PI / 4f + MathF.PI / 180f * angle;
		}
		int num2 = 1 + 2 * sides;
		int num3 = 9 * sides;
		NativeArray<LightMeshVertex> nativeArray = new NativeArray<LightMeshVertex>(num2, Allocator.Temp);
		NativeArray<ushort> nativeArray2 = new NativeArray<ushort>(num3, Allocator.Temp);
		ushort num4 = (ushort)(2 * sides);
		Mesh lightMesh = light.lightMesh;
		Color color = new Color(0f, 0f, batchColor, 1f);
		nativeArray[num4] = new LightMeshVertex
		{
			position = float3.zero,
			color = color
		};
		float num5 = MathF.PI * 2f / (float)sides;
		float3 @float = new float3(float.MaxValue, float.MaxValue, 0f);
		float3 float2 = new float3(float.MinValue, float.MinValue, 0f);
		for (int i = 0; i < sides; i++)
		{
			float num6 = (float)(i + 1) * num5;
			float3 float3 = new float3(math.cos(num6 + num), math.sin(num6 + num), 0f);
			float3 float4 = radius * float3;
			int num7 = (2 * i + 2) % (2 * sides);
			nativeArray[num7] = new LightMeshVertex
			{
				position = float4,
				color = new Color(float3.x, float3.y, batchColor, 0f)
			};
			nativeArray[num7 + 1] = new LightMeshVertex
			{
				position = float4,
				color = color
			};
			int num8 = 9 * i;
			nativeArray2[num8] = (ushort)(num7 + 1);
			nativeArray2[num8 + 1] = (ushort)(2 * i + 1);
			nativeArray2[num8 + 2] = num4;
			nativeArray2[num8 + 3] = (ushort)num7;
			nativeArray2[num8 + 4] = (ushort)(2 * i);
			nativeArray2[num8 + 5] = (ushort)(2 * i + 1);
			nativeArray2[num8 + 6] = (ushort)(num7 + 1);
			nativeArray2[num8 + 7] = (ushort)num7;
			nativeArray2[num8 + 8] = (ushort)(2 * i + 1);
			@float = math.min(@float, float4 + float3 * falloffDistance);
			float2 = math.max(float2, float4 + float3 * falloffDistance);
		}
		lightMesh.SetVertexBufferParams(num2, LightMeshVertex.VertexLayout);
		lightMesh.SetVertexBufferData(nativeArray, 0, 0, num2);
		lightMesh.SetIndices(nativeArray2, MeshTopology.Triangles, 0, calculateBounds: false);
		light.vertices = new LightMeshVertex[num2];
		NativeArray<LightMeshVertex>.Copy(nativeArray, light.vertices, num2);
		light.indices = new ushort[num3];
		NativeArray<ushort>.Copy(nativeArray2, light.indices, num3);
		Bounds result = default(Bounds);
		result.min = @float;
		result.max = float2;
		return result;
	}

	public static Bounds GenerateSpriteMesh(Light2D light, Sprite sprite, float batchColor)
	{
		Mesh lightMesh = light.lightMesh;
		if (sprite == null)
		{
			lightMesh.Clear();
			return new Bounds(Vector3.zero, Vector3.zero);
		}
		_ = sprite.uv;
		NativeSlice<Vector3> vertexAttribute = sprite.GetVertexAttribute<Vector3>(VertexAttribute.Position);
		NativeSlice<Vector2> vertexAttribute2 = sprite.GetVertexAttribute<Vector2>(VertexAttribute.TexCoord0);
		NativeArray<ushort> indices = sprite.GetIndices();
		_ = 0.5f * (sprite.bounds.min + sprite.bounds.max);
		NativeArray<LightMeshVertex> nativeArray = new NativeArray<LightMeshVertex>(indices.Length, Allocator.Temp);
		Color color = new Color(0f, 0f, batchColor, 1f);
		for (int i = 0; i < vertexAttribute.Length; i++)
		{
			nativeArray[i] = new LightMeshVertex
			{
				position = new Vector3(vertexAttribute[i].x, vertexAttribute[i].y, 0f),
				color = color,
				uv = vertexAttribute2[i]
			};
		}
		lightMesh.SetVertexBufferParams(nativeArray.Length, LightMeshVertex.VertexLayout);
		lightMesh.SetVertexBufferData(nativeArray, 0, 0, nativeArray.Length);
		lightMesh.SetIndices(indices, MeshTopology.Triangles, 0);
		light.vertices = new LightMeshVertex[nativeArray.Length];
		NativeArray<LightMeshVertex>.Copy(nativeArray, light.vertices, nativeArray.Length);
		light.indices = new ushort[indices.Length];
		NativeArray<ushort>.Copy(indices, light.indices, indices.Length);
		return lightMesh.GetSubMesh(0).bounds;
	}

	public static int GetShapePathHash(Vector3[] path)
	{
		int num = -2128831035;
		if (path != null)
		{
			for (int i = 0; i < path.Length; i++)
			{
				Vector3 vector = path[i];
				num = (num * 16777619) ^ vector.GetHashCode();
			}
		}
		else
		{
			num = 0;
		}
		return num;
	}
}
