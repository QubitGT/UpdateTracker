using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Rendering;
using UnityEngine.Splines.ExtrusionShapes;

namespace UnityEngine.Splines;

public static class SplineMesh
{
	public interface ISplineVertexData
	{
		Vector3 position { get; set; }

		Vector3 normal { get; set; }

		Vector2 texture { get; set; }
	}

	private struct VertexData : ISplineVertexData
	{
		public Vector3 position { get; set; }

		public Vector3 normal { get; set; }

		public Vector2 texture { get; set; }
	}

	private const int k_SidesMin = 2;

	private const int k_SidesMax = 2084;

	private static readonly VertexAttributeDescriptor[] k_PipeVertexAttribs = new VertexAttributeDescriptor[3]
	{
		new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0),
		new VertexAttributeDescriptor(VertexAttribute.Normal),
		new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2)
	};

	private static readonly Circle s_DefaultShape = new Circle();

	internal static bool s_IsConvex;

	private static bool s_IsConvexComputed;

	private static void ExtrudeRing<TSpline, TShape, TVertex>(TSpline spline, ExtrudeSettings<TShape> settings, int segment, NativeArray<TVertex> data, int start, bool uvsAreCaps = false) where TSpline : ISpline where TShape : IExtrudeShape where TVertex : struct, ISplineVertexData
	{
		TShape shape = settings.Shape;
		int sides = settings.sides;
		float radius = settings.Radius;
		bool wrapped = settings.wrapped;
		float num = math.lerp(settings.Range.x, settings.Range.y, (float)segment / ((float)settings.SegmentCount - 1f));
		float num2 = (spline.Closed ? math.frac(num) : math.clamp(num, 0f, 1f));
		spline.Evaluate(num2, out var position, out var tangent, out var upVector);
		float num3 = math.lengthsq(tangent);
		if (num3 == 0f || float.IsNaN(num3))
		{
			float t = math.clamp(num2 + 0.0001f * ((num < 1f) ? 1f : (-1f)), 0f, 1f);
			spline.Evaluate(t, out var _, out tangent, out upVector);
		}
		tangent = math.normalize(tangent);
		quaternion q = quaternion.LookRotationSafe(tangent, upVector);
		shape.SetSegment(segment, num, position, tangent, upVector);
		bool flipNormals = settings.FlipNormals;
		for (int i = 0; i < sides; i++)
		{
			TVertex value = new TVertex();
			int num4 = (flipNormals ? (sides - i - 1) : i);
			float t2 = (float)num4 / ((float)sides - 1f);
			float2 xy = shape.GetPosition(t2, num4) * radius;
			value.position = position + math.rotate(q, new float3(xy, 0f));
			value.normal = (value.position - (Vector3)position).normalized * (flipNormals ? (-1f) : 1f);
			if (uvsAreCaps)
			{
				value.texture = xy.xy / radius / 2f;
			}
			else if (wrapped)
			{
				float num5 = (float)num4 / ((float)sides + (float)(sides % 2));
				float num6 = math.abs(num5 - math.floor(num5 + 0.5f)) * 2f;
				value.texture = new Vector2(1f - num6, num * spline.GetLength());
			}
			else
			{
				value.texture = new Vector2(1f - (float)num4 / ((float)sides - 1f), num * spline.GetLength());
			}
			data[start + i] = value;
		}
		if (!s_IsConvexComputed)
		{
			ComputeIsConvex(data, tangent, start, sides);
		}
	}

	private static void ComputeIsConvex<TVertex>(NativeArray<TVertex> data, float3 normal, int start, int sideCount) where TVertex : struct, ISplineVertexData
	{
		s_IsConvexComputed = true;
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < sideCount; i++)
		{
			int num = start + i;
			int num2 = (num + 1) % (sideCount - 1);
			int index = (num2 + 1) % (sideCount - 1);
			Vector3 position = data[num].position;
			Vector3 position2 = data[num2].position;
			Vector3 position3 = data[index].position;
			Vector3 vector = position2 - position;
			Vector3 vector2 = position3 - position2;
			float3 y = math.normalizesafe(math.cross(vector, vector2));
			float num3 = math.dot(normal, y);
			if (num3 < 0f)
			{
				flag = true;
			}
			else if (num3 > 0f)
			{
				flag2 = true;
			}
			if (flag && flag2)
			{
				s_IsConvex = false;
				return;
			}
		}
		s_IsConvex = true;
	}

	public static bool GetVertexAndIndexCount(int sides, int segments, bool capped, bool closed, bool closeRing, out int vertexCount, out int indexCount)
	{
		vertexCount = sides * (segments + (capped ? 2 : 0));
		indexCount = (closeRing ? sides : (sides - 1)) * 6 * (segments - ((!closed) ? 1 : 0)) + (capped ? ((sides - 2) * 3 * 2) : 0);
		if (vertexCount > 3)
		{
			return indexCount > 5;
		}
		return false;
	}

	public static void GetVertexAndIndexCount(int sides, int segments, bool capped, bool closed, Vector2 range, out int vertexCount, out int indexCount)
	{
		GetVertexAndIndexCount(sides, segments, capped, closed, closeRing: true, out vertexCount, out indexCount);
	}

	private static bool GetVertexAndIndexCount<T, K>(T spline, ExtrudeSettings<K> settings, out int vertexCount, out int indexCount) where T : ISpline where K : IExtrudeShape
	{
		return GetVertexAndIndexCount(settings.sides, settings.SegmentCount, settings.DoCapEnds(spline), settings.DoCloseSpline(spline), settings.wrapped, out vertexCount, out indexCount);
	}

	public static void Extrude<T>(T spline, Mesh mesh, float radius, int sides, int segments, bool capped = true) where T : ISpline
	{
		Extrude(spline, mesh, radius, sides, segments, capped, new float2(0f, 1f));
	}

	public static void Extrude<T, K>(T spline, Mesh mesh, float radius, int segments, bool capped, K shape) where T : ISpline where K : IExtrudeShape
	{
		Extrude(spline, mesh, radius, segments, capped, new float2(0f, 1f), shape);
	}

	public static void Extrude<T>(T spline, Mesh mesh, float radius, int sides, int segments, bool capped, float2 range) where T : ISpline
	{
		s_DefaultShape.SideCount = sides;
		Extrude(spline, mesh, radius, segments, capped, range, s_DefaultShape);
	}

	public static void Extrude<T, K>(T spline, Mesh mesh, float radius, int segments, bool capped, float2 range, K shape) where T : ISpline where K : IExtrudeShape
	{
		ExtrudeSettings<K> extrudeSettings = default(ExtrudeSettings<K>);
		extrudeSettings.Radius = radius;
		extrudeSettings.CapEnds = capped;
		extrudeSettings.Range = range;
		extrudeSettings.SegmentCount = segments;
		extrudeSettings.Shape = shape;
		ExtrudeSettings<K> settings = extrudeSettings;
		Extrude(spline, mesh, settings);
	}

	public static bool Extrude<T, K>(T spline, Mesh mesh, ExtrudeSettings<K> settings) where T : ISpline where K : IExtrudeShape
	{
		if (!GetVertexAndIndexCount(spline, settings, out var vertexCount, out var indexCount))
		{
			return false;
		}
		Mesh.MeshDataArray data = Mesh.AllocateWritableMeshData(1);
		Mesh.MeshData meshData = data[0];
		IndexFormat indexFormat = ((vertexCount >= 65535) ? IndexFormat.UInt32 : IndexFormat.UInt16);
		meshData.SetIndexBufferParams(indexCount, indexFormat);
		meshData.SetVertexBufferParams(vertexCount, k_PipeVertexAttribs);
		NativeArray<VertexData> vertexData = meshData.GetVertexData<VertexData>();
		if (indexFormat == IndexFormat.UInt16)
		{
			NativeArray<ushort> indexData = meshData.GetIndexData<ushort>();
			Extrude(spline, vertexData, indexData, settings);
		}
		else
		{
			NativeArray<uint> indexData2 = meshData.GetIndexData<uint>();
			Extrude(spline, vertexData, indexData2, settings);
		}
		mesh.Clear();
		meshData.subMeshCount = 1;
		meshData.SetSubMesh(0, new SubMeshDescriptor(0, indexCount));
		Mesh.ApplyAndDisposeWritableMeshData(data, mesh);
		mesh.RecalculateBounds();
		return true;
	}

	public static void Extrude<T>(IReadOnlyList<T> splines, Mesh mesh, float radius, int sides, float segmentsPerUnit, bool capped, float2 range) where T : ISpline
	{
		s_DefaultShape.SideCount = sides;
		ExtrudeSettings<Circle> extrudeSettings = new ExtrudeSettings<Circle>(s_DefaultShape);
		extrudeSettings.Radius = radius;
		extrudeSettings.SegmentCount = (int)segmentsPerUnit;
		extrudeSettings.CapEnds = capped;
		extrudeSettings.Range = range;
		ExtrudeSettings<Circle> settings = extrudeSettings;
		Extrude(splines, mesh, settings, segmentsPerUnit);
	}

	internal static void Extrude<T, K>(IReadOnlyList<T> splines, Mesh mesh, ExtrudeSettings<K> settings, float segmentsPerUnit) where T : ISpline where K : IExtrudeShape
	{
		mesh.Clear();
		if (splines == null)
		{
			if (Application.isPlaying)
			{
				Debug.LogError("Trying to extrude a spline mesh with no valid splines.");
			}
			return;
		}
		Mesh.MeshDataArray data = Mesh.AllocateWritableMeshData(1);
		Mesh.MeshData meshData = data[0];
		meshData.subMeshCount = 1;
		int num = 0;
		int num2 = 0;
		(int, int)[] array = new(int, int)[splines.Count];
		for (int i = 0; i < splines.Count; i++)
		{
			if (splines[i].Count >= 2)
			{
				settings.SegmentCount = GetSegmentCount(splines[i]);
				GetVertexAndIndexCount(splines[i], settings, out var vertexCount, out var indexCount);
				array[i] = (num2, num);
				num += vertexCount;
				num2 += indexCount;
			}
		}
		IndexFormat indexFormat = ((num >= 65535) ? IndexFormat.UInt32 : IndexFormat.UInt16);
		meshData.SetIndexBufferParams(num2, indexFormat);
		meshData.SetVertexBufferParams(num, k_PipeVertexAttribs);
		NativeArray<VertexData> vertexData = meshData.GetVertexData<VertexData>();
		if (indexFormat == IndexFormat.UInt16)
		{
			NativeArray<ushort> indexData = meshData.GetIndexData<ushort>();
			for (int j = 0; j < splines.Count; j++)
			{
				if (splines[j].Count >= 2)
				{
					settings.SegmentCount = GetSegmentCount(splines[j]);
					Extrude(splines[j], vertexData, indexData, settings, array[j].Item2, array[j].Item1);
				}
			}
		}
		else
		{
			NativeArray<uint> indexData2 = meshData.GetIndexData<uint>();
			for (int k = 0; k < splines.Count; k++)
			{
				if (splines[k].Count >= 2)
				{
					settings.SegmentCount = GetSegmentCount(splines[k]);
					Extrude(splines[k], vertexData, indexData2, settings, array[k].Item2, array[k].Item1);
				}
			}
		}
		meshData.SetSubMesh(0, new SubMeshDescriptor(0, num2));
		Mesh.ApplyAndDisposeWritableMeshData(data, mesh);
		mesh.RecalculateBounds();
		int GetSegmentCount(T spline)
		{
			float num3 = Mathf.Abs(settings.Range.y - settings.Range.x);
			return Mathf.Max((int)Mathf.Ceil(spline.GetLength() * num3 * segmentsPerUnit), 1);
		}
	}

	public static void Extrude<TSplineType, TVertexType, TIndexType>(TSplineType spline, NativeArray<TVertexType> vertices, NativeArray<TIndexType> indices, float radius, int sides, int segments, bool capped, float2 range) where TSplineType : ISpline where TVertexType : struct, ISplineVertexData where TIndexType : struct
	{
		s_DefaultShape.SideCount = math.clamp(sides, 2, 2084);
		Extrude(spline, vertices, indices, new ExtrudeSettings<Circle>(segments, capped, range, radius, s_DefaultShape));
	}

	private static void Extrude<TSplineType, TVertexType, TIndexType, TShapeType>(TSplineType spline, NativeArray<TVertexType> vertices, NativeArray<TIndexType> indices, ExtrudeSettings<TShapeType> settings, int vertexArrayOffset = 0, int indicesArrayOffset = 0) where TSplineType : ISpline where TVertexType : struct, ISplineVertexData where TIndexType : struct where TShapeType : IExtrudeShape
	{
		int sides = settings.sides;
		int segmentCount = settings.SegmentCount;
		float2 range = settings.Range;
		bool flag = settings.DoCapEnds(spline);
		if (!GetVertexAndIndexCount(spline, settings, out var vertexCount, out var indexCount))
		{
			return;
		}
		if (settings.Shape == null)
		{
			throw new ArgumentNullException("Shape", "Shape template is null.");
		}
		if (sides < 2)
		{
			throw new ArgumentOutOfRangeException("sides", "Sides must be greater than 2");
		}
		if (segmentCount < 2)
		{
			throw new ArgumentOutOfRangeException("segments", "Segments must be greater than 2");
		}
		if (vertices.Length < vertexCount)
		{
			throw new ArgumentOutOfRangeException($"Vertex array is incorrect size. Expected {vertexCount} or more, but received {vertices.Length}.");
		}
		if (indices.Length < indexCount)
		{
			throw new ArgumentOutOfRangeException($"Index array is incorrect size. Expected {indexCount} or more, but received {indices.Length}.");
		}
		if (typeof(TIndexType) == typeof(ushort))
		{
			WindTris(indices.Reinterpret<ushort>(), spline, settings, vertexArrayOffset, indicesArrayOffset);
		}
		else
		{
			if (!(typeof(TIndexType) == typeof(uint)))
			{
				throw new ArgumentException("Indices must be UInt16 or UInt32", "indices");
			}
			WindTris(indices.Reinterpret<uint>(), spline, settings, vertexArrayOffset, indicesArrayOffset);
		}
		settings.Shape.Setup(spline, segmentCount);
		s_IsConvexComputed = false;
		for (int i = 0; i < segmentCount; i++)
		{
			ExtrudeRing(spline, settings, i, vertices, vertexArrayOffset + i * sides);
		}
		if (flag)
		{
			int num = vertexArrayOffset + segmentCount * sides;
			int num2 = vertexArrayOffset + (segmentCount + 1) * sides;
			float2 @float = (spline.Closed ? math.frac(range) : math.clamp(range, 0f, 1f));
			ExtrudeRing(spline, settings, 0, vertices, num, uvsAreCaps: true);
			ExtrudeRing(spline, settings, segmentCount - 1, vertices, num2, uvsAreCaps: true);
			float3 float2 = math.normalize(spline.EvaluateTangent(@float.x));
			float num3 = math.lengthsq(float2);
			if (num3 == 0f || float.IsNaN(num3))
			{
				float2 = math.normalize(spline.EvaluateTangent(@float.x + 0.0001f));
			}
			float3 float3 = math.normalize(spline.EvaluateTangent(@float.y));
			num3 = math.lengthsq(float3);
			if (num3 == 0f || float.IsNaN(num3))
			{
				float3 = math.normalize(spline.EvaluateTangent(@float.y - 0.0001f));
			}
			for (int j = 0; j < sides; j++)
			{
				TVertexType value = vertices[num + j];
				TVertexType value2 = vertices[num2 + j];
				value.normal = -float2;
				value2.normal = float3;
				vertices[num + j] = value;
				vertices[num2 + j] = value2;
			}
		}
	}

	private static void WindTris<T, K>(NativeArray<ushort> indices, T spline, ExtrudeSettings<K> settings, int vertexArrayOffset = 0, int indexArrayOffset = 0) where T : ISpline where K : IExtrudeShape
	{
		bool flag = settings.DoCloseSpline(spline);
		int segmentCount = settings.SegmentCount;
		int sides = settings.sides;
		bool wrapped = settings.wrapped;
		bool flag2 = settings.DoCapEnds(spline);
		int num = (wrapped ? sides : (sides - 1));
		for (int i = 0; i < (flag ? segmentCount : (segmentCount - 1)); i++)
		{
			for (int j = 0; j < (wrapped ? sides : (sides - 1)); j++)
			{
				int num2 = vertexArrayOffset + i * sides + j;
				int num3 = vertexArrayOffset + i * sides + (j + 1) % sides;
				int num4 = vertexArrayOffset + (i + 1) % segmentCount * sides + j;
				int num5 = vertexArrayOffset + (i + 1) % segmentCount * sides + (j + 1) % sides;
				indices[indexArrayOffset + i * num * 6 + j * 6] = (ushort)num2;
				indices[indexArrayOffset + i * num * 6 + j * 6 + 1] = (ushort)num3;
				indices[indexArrayOffset + i * num * 6 + j * 6 + 2] = (ushort)num4;
				indices[indexArrayOffset + i * num * 6 + j * 6 + 3] = (ushort)num3;
				indices[indexArrayOffset + i * num * 6 + j * 6 + 4] = (ushort)num5;
				indices[indexArrayOffset + i * num * 6 + j * 6 + 5] = (ushort)num4;
			}
		}
		if (flag2)
		{
			int num6 = vertexArrayOffset + segmentCount * sides;
			int num7 = indexArrayOffset + num * 6 * (segmentCount - 1);
			int num8 = vertexArrayOffset + (segmentCount + 1) * sides;
			int num9 = indexArrayOffset + (segmentCount - 1) * 6 * num + (sides - 2) * 3;
			for (ushort num10 = 0; num10 < sides - 2; num10++)
			{
				indices[num7 + num10 * 3] = (ushort)num6;
				indices[num7 + num10 * 3 + 1] = (ushort)(num6 + num10 + 2);
				indices[num7 + num10 * 3 + 2] = (ushort)(num6 + num10 + 1);
				indices[num9 + num10 * 3] = (ushort)num8;
				indices[num9 + num10 * 3 + 1] = (ushort)(num8 + num10 + 1);
				indices[num9 + num10 * 3 + 2] = (ushort)(num8 + num10 + 2);
			}
		}
	}

	private static void WindTris<T, K>(NativeArray<uint> indices, T spline, ExtrudeSettings<K> settings, int vertexArrayOffset = 0, int indexArrayOffset = 0) where T : ISpline where K : IExtrudeShape
	{
		bool flag = settings.DoCloseSpline(spline);
		int segmentCount = settings.SegmentCount;
		int sides = settings.sides;
		bool wrapped = settings.wrapped;
		bool flag2 = settings.DoCapEnds(spline);
		int num = (wrapped ? sides : (sides - 1));
		for (int i = 0; i < (flag ? segmentCount : (segmentCount - 1)); i++)
		{
			for (int j = 0; j < (wrapped ? sides : (sides - 1)); j++)
			{
				int num2 = vertexArrayOffset + i * sides + j;
				int num3 = vertexArrayOffset + i * sides + (j + 1) % sides;
				int num4 = vertexArrayOffset + (i + 1) % segmentCount * sides + j;
				int num5 = vertexArrayOffset + (i + 1) % segmentCount * sides + (j + 1) % sides;
				indices[indexArrayOffset + i * num * 6 + j * 6] = (ushort)num2;
				indices[indexArrayOffset + i * num * 6 + j * 6 + 1] = (ushort)num3;
				indices[indexArrayOffset + i * num * 6 + j * 6 + 2] = (ushort)num4;
				indices[indexArrayOffset + i * num * 6 + j * 6 + 3] = (ushort)num3;
				indices[indexArrayOffset + i * num * 6 + j * 6 + 4] = (ushort)num5;
				indices[indexArrayOffset + i * num * 6 + j * 6 + 5] = (ushort)num4;
			}
		}
		if (flag2)
		{
			int num6 = vertexArrayOffset + segmentCount * sides;
			int num7 = indexArrayOffset + num * 6 * (segmentCount - 1);
			int num8 = vertexArrayOffset + (segmentCount + 1) * sides;
			int num9 = indexArrayOffset + (segmentCount - 1) * 6 * num + (sides - 2) * 3;
			for (ushort num10 = 0; num10 < sides - 2; num10++)
			{
				indices[num7 + num10 * 3] = (ushort)num6;
				indices[num7 + num10 * 3 + 1] = (ushort)(num6 + num10 + 2);
				indices[num7 + num10 * 3 + 2] = (ushort)(num6 + num10 + 1);
				indices[num9 + num10 * 3] = (ushort)num8;
				indices[num9 + num10 * 3 + 1] = (ushort)(num8 + num10 + 1);
				indices[num9 + num10 * 3 + 2] = (ushort)(num8 + num10 + 2);
			}
		}
	}
}
