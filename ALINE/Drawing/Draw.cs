using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Drawing;

public static class Draw
{
	internal static CommandBuilder builder;

	internal static CommandBuilder ingame_builder;

	public static ref CommandBuilder ingame
	{
		get
		{
			DrawingManager.Init();
			return ref ingame_builder;
		}
	}

	public static ref CommandBuilder editor
	{
		get
		{
			DrawingManager.Init();
			return ref builder;
		}
	}

	public static CommandBuilder2D xy
	{
		get
		{
			DrawingManager.Init();
			return new CommandBuilder2D(builder, xy: true);
		}
	}

	public static CommandBuilder2D xz
	{
		get
		{
			DrawingManager.Init();
			return new CommandBuilder2D(builder, xy: false);
		}
	}

	[BurstDiscard]
	public static CommandBuilder.ScopeEmpty WithMatrix(Matrix4x4 matrix)
	{
		return default(CommandBuilder.ScopeEmpty);
	}

	[BurstDiscard]
	public static CommandBuilder.ScopeEmpty WithMatrix(float3x3 matrix)
	{
		return default(CommandBuilder.ScopeEmpty);
	}

	[BurstDiscard]
	public static CommandBuilder.ScopeEmpty WithColor(Color color)
	{
		return default(CommandBuilder.ScopeEmpty);
	}

	[BurstDiscard]
	public static CommandBuilder.ScopeEmpty WithDuration(float duration)
	{
		return default(CommandBuilder.ScopeEmpty);
	}

	[BurstDiscard]
	public static CommandBuilder.ScopeEmpty WithLineWidth(float pixels, bool automaticJoins = true)
	{
		return default(CommandBuilder.ScopeEmpty);
	}

	[BurstDiscard]
	public static CommandBuilder.ScopeEmpty InLocalSpace(Transform transform)
	{
		return default(CommandBuilder.ScopeEmpty);
	}

	[BurstDiscard]
	public static CommandBuilder.ScopeEmpty InScreenSpace(Camera camera)
	{
		return default(CommandBuilder.ScopeEmpty);
	}

	[BurstDiscard]
	public static void PushMatrix(Matrix4x4 matrix)
	{
	}

	[BurstDiscard]
	public static void PushMatrix(float4x4 matrix)
	{
	}

	[BurstDiscard]
	public static void PushSetMatrix(Matrix4x4 matrix)
	{
	}

	[BurstDiscard]
	public static void PushSetMatrix(float4x4 matrix)
	{
	}

	[BurstDiscard]
	public static void PopMatrix()
	{
	}

	[BurstDiscard]
	public static void PushColor(Color color)
	{
	}

	[BurstDiscard]
	public static void PopColor()
	{
	}

	[BurstDiscard]
	public static void PushDuration(float duration)
	{
	}

	[BurstDiscard]
	public static void PopDuration()
	{
	}

	[BurstDiscard]
	[Obsolete("Renamed to PushDuration for consistency")]
	public static void PushPersist(float duration)
	{
	}

	[BurstDiscard]
	[Obsolete("Renamed to PopDuration for consistency")]
	public static void PopPersist()
	{
	}

	[BurstDiscard]
	public static void PushLineWidth(float pixels, bool automaticJoins = true)
	{
	}

	[BurstDiscard]
	public static void PopLineWidth()
	{
	}

	[BurstDiscard]
	public static void Line(float3 a, float3 b)
	{
	}

	[BurstDiscard]
	public static void Line(Vector3 a, Vector3 b)
	{
	}

	[BurstDiscard]
	public static void Line(Vector3 a, Vector3 b, Color color)
	{
	}

	[BurstDiscard]
	public static void Ray(float3 origin, float3 direction)
	{
	}

	[BurstDiscard]
	public static void Ray(Ray ray, float length)
	{
	}

	[BurstDiscard]
	public static void Arc(float3 center, float3 start, float3 end)
	{
	}

	[BurstDiscard]
	[Obsolete("Use Draw.xz.Circle instead")]
	public static void CircleXZ(float3 center, float radius, float startAngle = 0f, float endAngle = MathF.PI * 2f)
	{
	}

	[BurstDiscard]
	[Obsolete("Use Draw.xy.Circle instead")]
	public static void CircleXY(float3 center, float radius, float startAngle = 0f, float endAngle = MathF.PI * 2f)
	{
	}

	[BurstDiscard]
	public static void Circle(float3 center, float3 normal, float radius)
	{
	}

	[BurstDiscard]
	public static void SolidArc(float3 center, float3 start, float3 end)
	{
	}

	[BurstDiscard]
	[Obsolete("Use Draw.xz.SolidCircle instead")]
	public static void SolidCircleXZ(float3 center, float radius, float startAngle = 0f, float endAngle = MathF.PI * 2f)
	{
	}

	[BurstDiscard]
	[Obsolete("Use Draw.xy.SolidCircle instead")]
	public static void SolidCircleXY(float3 center, float radius, float startAngle = 0f, float endAngle = MathF.PI * 2f)
	{
	}

	[BurstDiscard]
	public static void SolidCircle(float3 center, float3 normal, float radius)
	{
	}

	[BurstDiscard]
	public static void SphereOutline(float3 center, float radius)
	{
	}

	[BurstDiscard]
	public static void WireCylinder(float3 bottom, float3 top, float radius)
	{
	}

	[BurstDiscard]
	public static void WireCylinder(float3 position, float3 up, float height, float radius)
	{
	}

	[BurstDiscard]
	public static void WireCapsule(float3 start, float3 end, float radius)
	{
	}

	[BurstDiscard]
	public static void WireCapsule(float3 position, float3 direction, float length, float radius)
	{
	}

	[BurstDiscard]
	public static void WireSphere(float3 position, float radius)
	{
	}

	[BurstDiscard]
	public static void Polyline(List<Vector3> points, bool cycle = false)
	{
	}

	[BurstDiscard]
	public static void Polyline(Vector3[] points, bool cycle = false)
	{
	}

	[BurstDiscard]
	public static void Polyline(float3[] points, bool cycle = false)
	{
	}

	[BurstDiscard]
	public static void Polyline(NativeArray<float3> points, bool cycle = false)
	{
	}

	[BurstDiscard]
	public static void DashedLine(float3 a, float3 b, float dash, float gap)
	{
	}

	[BurstDiscard]
	public static void DashedPolyline(List<Vector3> points, float dash, float gap)
	{
	}

	[BurstDiscard]
	public static void WireBox(float3 center, float3 size)
	{
	}

	[BurstDiscard]
	public static void WireBox(float3 center, quaternion rotation, float3 size)
	{
	}

	[BurstDiscard]
	public static void WireBox(Bounds bounds)
	{
	}

	[BurstDiscard]
	public static void WireMesh(Mesh mesh)
	{
	}

	[BurstDiscard]
	public static void WireMesh(NativeArray<float3> vertices, NativeArray<int> triangles)
	{
	}

	[BurstDiscard]
	public static void SolidMesh(Mesh mesh)
	{
	}

	[BurstDiscard]
	public static void SolidMesh(List<Vector3> vertices, List<int> triangles, List<Color> colors)
	{
	}

	[BurstDiscard]
	public static void SolidMesh(Vector3[] vertices, int[] triangles, Color[] colors, int vertexCount, int indexCount)
	{
	}

	[BurstDiscard]
	public static void Cross(float3 position, float size = 1f)
	{
	}

	[BurstDiscard]
	[Obsolete("Use Draw.xz.Cross instead")]
	public static void CrossXZ(float3 position, float size = 1f)
	{
	}

	[BurstDiscard]
	[Obsolete("Use Draw.xy.Cross instead")]
	public static void CrossXY(float3 position, float size = 1f)
	{
	}

	[BurstDiscard]
	public static void Bezier(float3 p0, float3 p1, float3 p2, float3 p3)
	{
	}

	[BurstDiscard]
	public static void CatmullRom(List<Vector3> points)
	{
	}

	[BurstDiscard]
	public static void CatmullRom(float3 p0, float3 p1, float3 p2, float3 p3)
	{
	}

	[BurstDiscard]
	public static void Arrow(float3 from, float3 to)
	{
	}

	[BurstDiscard]
	public static void Arrow(float3 from, float3 to, float3 up, float headSize)
	{
	}

	[BurstDiscard]
	public static void ArrowRelativeSizeHead(float3 from, float3 to, float3 up, float headFraction)
	{
	}

	[BurstDiscard]
	public static void Arrowhead(float3 center, float3 direction, float radius)
	{
	}

	[BurstDiscard]
	public static void Arrowhead(float3 center, float3 direction, float3 up, float radius)
	{
	}

	[BurstDiscard]
	public static void ArrowheadArc(float3 origin, float3 direction, float offset, float width = 60f)
	{
	}

	[BurstDiscard]
	public static void WireGrid(float3 center, quaternion rotation, int2 cells, float2 totalSize)
	{
	}

	[BurstDiscard]
	public static void WireTriangle(float3 a, float3 b, float3 c)
	{
	}

	[BurstDiscard]
	[Obsolete("Use Draw.xz.WireRectangle instead")]
	public static void WireRectangleXZ(float3 center, float2 size)
	{
	}

	[BurstDiscard]
	public static void WireRectangle(float3 center, quaternion rotation, float2 size)
	{
	}

	[BurstDiscard]
	[Obsolete("Use Draw.xy.WireRectangle instead")]
	public static void WireRectangle(Rect rect)
	{
	}

	[BurstDiscard]
	public static void WireTriangle(float3 center, quaternion rotation, float radius)
	{
	}

	[BurstDiscard]
	public static void WirePentagon(float3 center, quaternion rotation, float radius)
	{
	}

	[BurstDiscard]
	public static void WireHexagon(float3 center, quaternion rotation, float radius)
	{
	}

	[BurstDiscard]
	public static void WirePolygon(float3 center, int vertices, quaternion rotation, float radius)
	{
	}

	[BurstDiscard]
	[Obsolete("Use Draw.xy.SolidRectangle instead")]
	public static void SolidRectangle(Rect rect)
	{
	}

	[BurstDiscard]
	public static void SolidPlane(float3 center, float3 normal, float2 size)
	{
	}

	[BurstDiscard]
	public static void SolidPlane(float3 center, quaternion rotation, float2 size)
	{
	}

	[BurstDiscard]
	public static void WirePlane(float3 center, float3 normal, float2 size)
	{
	}

	[BurstDiscard]
	public static void WirePlane(float3 center, quaternion rotation, float2 size)
	{
	}

	[BurstDiscard]
	public static void PlaneWithNormal(float3 center, float3 normal, float2 size)
	{
	}

	[BurstDiscard]
	public static void PlaneWithNormal(float3 center, quaternion rotation, float2 size)
	{
	}

	[BurstDiscard]
	public static void SolidTriangle(float3 a, float3 b, float3 c)
	{
	}

	[BurstDiscard]
	public static void SolidBox(float3 center, float3 size)
	{
	}

	[BurstDiscard]
	public static void SolidBox(Bounds bounds)
	{
	}

	[BurstDiscard]
	public static void SolidBox(float3 center, quaternion rotation, float3 size)
	{
	}

	[BurstDiscard]
	public static void Label3D(float3 position, quaternion rotation, string text, float size)
	{
	}

	[BurstDiscard]
	public static void Label3D(float3 position, quaternion rotation, string text, float size, LabelAlignment alignment)
	{
	}

	[BurstDiscard]
	public static void Label2D(float3 position, string text, float sizeInPixels = 14f)
	{
	}

	[BurstDiscard]
	public static void Label2D(float3 position, string text, float sizeInPixels, LabelAlignment alignment)
	{
	}

	[BurstDiscard]
	public static void Label2D(float3 position, ref FixedString32Bytes text, float sizeInPixels = 14f)
	{
	}

	[BurstDiscard]
	public static void Label2D(float3 position, ref FixedString64Bytes text, float sizeInPixels = 14f)
	{
	}

	[BurstDiscard]
	public static void Label2D(float3 position, ref FixedString128Bytes text, float sizeInPixels = 14f)
	{
	}

	[BurstDiscard]
	public static void Label2D(float3 position, ref FixedString512Bytes text, float sizeInPixels = 14f)
	{
	}

	[BurstDiscard]
	public static void Label2D(float3 position, ref FixedString32Bytes text, float sizeInPixels, LabelAlignment alignment)
	{
	}

	[BurstDiscard]
	public static void Label2D(float3 position, ref FixedString64Bytes text, float sizeInPixels, LabelAlignment alignment)
	{
	}

	[BurstDiscard]
	public static void Label2D(float3 position, ref FixedString128Bytes text, float sizeInPixels, LabelAlignment alignment)
	{
	}

	[BurstDiscard]
	public static void Label2D(float3 position, ref FixedString512Bytes text, float sizeInPixels, LabelAlignment alignment)
	{
	}

	[BurstDiscard]
	public static void Label3D(float3 position, quaternion rotation, ref FixedString32Bytes text, float size)
	{
	}

	[BurstDiscard]
	public static void Label3D(float3 position, quaternion rotation, ref FixedString64Bytes text, float size)
	{
	}

	[BurstDiscard]
	public static void Label3D(float3 position, quaternion rotation, ref FixedString128Bytes text, float size)
	{
	}

	[BurstDiscard]
	public static void Label3D(float3 position, quaternion rotation, ref FixedString512Bytes text, float size)
	{
	}

	[BurstDiscard]
	public static void Label3D(float3 position, quaternion rotation, ref FixedString32Bytes text, float size, LabelAlignment alignment)
	{
	}

	[BurstDiscard]
	public static void Label3D(float3 position, quaternion rotation, ref FixedString64Bytes text, float size, LabelAlignment alignment)
	{
	}

	[BurstDiscard]
	public static void Label3D(float3 position, quaternion rotation, ref FixedString128Bytes text, float size, LabelAlignment alignment)
	{
	}

	[BurstDiscard]
	public static void Label3D(float3 position, quaternion rotation, ref FixedString512Bytes text, float size, LabelAlignment alignment)
	{
	}

	[BurstDiscard]
	public static void Line(float3 a, float3 b, Color color)
	{
	}

	[BurstDiscard]
	public static void Ray(float3 origin, float3 direction, Color color)
	{
	}

	[BurstDiscard]
	public static void Ray(Ray ray, float length, Color color)
	{
	}

	[BurstDiscard]
	public static void Arc(float3 center, float3 start, float3 end, Color color)
	{
	}

	[BurstDiscard]
	[Obsolete("Use Draw.xz.Circle instead")]
	public static void CircleXZ(float3 center, float radius, float startAngle, float endAngle, Color color)
	{
	}

	[BurstDiscard]
	[Obsolete("Use Draw.xz.Circle instead")]
	public static void CircleXZ(float3 center, float radius, Color color)
	{
	}

	[BurstDiscard]
	[Obsolete("Use Draw.xy.Circle instead")]
	public static void CircleXY(float3 center, float radius, float startAngle, float endAngle, Color color)
	{
	}

	[BurstDiscard]
	[Obsolete("Use Draw.xy.Circle instead")]
	public static void CircleXY(float3 center, float radius, Color color)
	{
	}

	[BurstDiscard]
	public static void Circle(float3 center, float3 normal, float radius, Color color)
	{
	}

	[BurstDiscard]
	public static void SolidArc(float3 center, float3 start, float3 end, Color color)
	{
	}

	[BurstDiscard]
	[Obsolete("Use Draw.xz.SolidCircle instead")]
	public static void SolidCircleXZ(float3 center, float radius, float startAngle, float endAngle, Color color)
	{
	}

	[BurstDiscard]
	[Obsolete("Use Draw.xz.SolidCircle instead")]
	public static void SolidCircleXZ(float3 center, float radius, Color color)
	{
	}

	[BurstDiscard]
	[Obsolete("Use Draw.xy.SolidCircle instead")]
	public static void SolidCircleXY(float3 center, float radius, float startAngle, float endAngle, Color color)
	{
	}

	[BurstDiscard]
	[Obsolete("Use Draw.xy.SolidCircle instead")]
	public static void SolidCircleXY(float3 center, float radius, Color color)
	{
	}

	[BurstDiscard]
	public static void SolidCircle(float3 center, float3 normal, float radius, Color color)
	{
	}

	[BurstDiscard]
	public static void SphereOutline(float3 center, float radius, Color color)
	{
	}

	[BurstDiscard]
	public static void WireCylinder(float3 bottom, float3 top, float radius, Color color)
	{
	}

	[BurstDiscard]
	public static void WireCylinder(float3 position, float3 up, float height, float radius, Color color)
	{
	}

	[BurstDiscard]
	public static void WireCapsule(float3 start, float3 end, float radius, Color color)
	{
	}

	[BurstDiscard]
	public static void WireCapsule(float3 position, float3 direction, float length, float radius, Color color)
	{
	}

	[BurstDiscard]
	public static void WireSphere(float3 position, float radius, Color color)
	{
	}

	[BurstDiscard]
	public static void Polyline(List<Vector3> points, bool cycle, Color color)
	{
	}

	[BurstDiscard]
	public static void Polyline(List<Vector3> points, Color color)
	{
	}

	[BurstDiscard]
	public static void Polyline(Vector3[] points, bool cycle, Color color)
	{
	}

	[BurstDiscard]
	public static void Polyline(Vector3[] points, Color color)
	{
	}

	[BurstDiscard]
	public static void Polyline(float3[] points, bool cycle, Color color)
	{
	}

	[BurstDiscard]
	public static void Polyline(float3[] points, Color color)
	{
	}

	[BurstDiscard]
	public static void Polyline(NativeArray<float3> points, bool cycle, Color color)
	{
	}

	[BurstDiscard]
	public static void Polyline(NativeArray<float3> points, Color color)
	{
	}

	[BurstDiscard]
	public static void DashedLine(float3 a, float3 b, float dash, float gap, Color color)
	{
	}

	[BurstDiscard]
	public static void DashedPolyline(List<Vector3> points, float dash, float gap, Color color)
	{
	}

	[BurstDiscard]
	public static void WireBox(float3 center, float3 size, Color color)
	{
	}

	[BurstDiscard]
	public static void WireBox(float3 center, quaternion rotation, float3 size, Color color)
	{
	}

	[BurstDiscard]
	public static void WireBox(Bounds bounds, Color color)
	{
	}

	[BurstDiscard]
	public static void WireMesh(Mesh mesh, Color color)
	{
	}

	[BurstDiscard]
	public static void WireMesh(NativeArray<float3> vertices, NativeArray<int> triangles, Color color)
	{
	}

	[BurstDiscard]
	public static void SolidMesh(Mesh mesh, Color color)
	{
	}

	[BurstDiscard]
	public static void Cross(float3 position, float size, Color color)
	{
	}

	[BurstDiscard]
	public static void Cross(float3 position, Color color)
	{
	}

	[BurstDiscard]
	[Obsolete("Use Draw.xz.Cross instead")]
	public static void CrossXZ(float3 position, float size, Color color)
	{
	}

	[BurstDiscard]
	[Obsolete("Use Draw.xz.Cross instead")]
	public static void CrossXZ(float3 position, Color color)
	{
	}

	[BurstDiscard]
	[Obsolete("Use Draw.xy.Cross instead")]
	public static void CrossXY(float3 position, float size, Color color)
	{
	}

	[BurstDiscard]
	[Obsolete("Use Draw.xy.Cross instead")]
	public static void CrossXY(float3 position, Color color)
	{
	}

	[BurstDiscard]
	public static void Bezier(float3 p0, float3 p1, float3 p2, float3 p3, Color color)
	{
	}

	[BurstDiscard]
	public static void CatmullRom(List<Vector3> points, Color color)
	{
	}

	[BurstDiscard]
	public static void CatmullRom(float3 p0, float3 p1, float3 p2, float3 p3, Color color)
	{
	}

	[BurstDiscard]
	public static void Arrow(float3 from, float3 to, Color color)
	{
	}

	[BurstDiscard]
	public static void Arrow(float3 from, float3 to, float3 up, float headSize, Color color)
	{
	}

	[BurstDiscard]
	public static void ArrowRelativeSizeHead(float3 from, float3 to, float3 up, float headFraction, Color color)
	{
	}

	[BurstDiscard]
	public static void Arrowhead(float3 center, float3 direction, float radius, Color color)
	{
	}

	[BurstDiscard]
	public static void Arrowhead(float3 center, float3 direction, float3 up, float radius, Color color)
	{
	}

	[BurstDiscard]
	public static void ArrowheadArc(float3 origin, float3 direction, float offset, float width, Color color)
	{
	}

	[BurstDiscard]
	public static void ArrowheadArc(float3 origin, float3 direction, float offset, Color color)
	{
	}

	[BurstDiscard]
	public static void WireGrid(float3 center, quaternion rotation, int2 cells, float2 totalSize, Color color)
	{
	}

	[BurstDiscard]
	public static void WireTriangle(float3 a, float3 b, float3 c, Color color)
	{
	}

	[BurstDiscard]
	[Obsolete("Use Draw.xz.WireRectangle instead")]
	public static void WireRectangleXZ(float3 center, float2 size, Color color)
	{
	}

	[BurstDiscard]
	public static void WireRectangle(float3 center, quaternion rotation, float2 size, Color color)
	{
	}

	[BurstDiscard]
	[Obsolete("Use Draw.xy.WireRectangle instead")]
	public static void WireRectangle(Rect rect, Color color)
	{
	}

	[BurstDiscard]
	public static void WireTriangle(float3 center, quaternion rotation, float radius, Color color)
	{
	}

	[BurstDiscard]
	public static void WirePentagon(float3 center, quaternion rotation, float radius, Color color)
	{
	}

	[BurstDiscard]
	public static void WireHexagon(float3 center, quaternion rotation, float radius, Color color)
	{
	}

	[BurstDiscard]
	public static void WirePolygon(float3 center, int vertices, quaternion rotation, float radius, Color color)
	{
	}

	[BurstDiscard]
	[Obsolete("Use Draw.xy.SolidRectangle instead")]
	public static void SolidRectangle(Rect rect, Color color)
	{
	}

	[BurstDiscard]
	public static void SolidPlane(float3 center, float3 normal, float2 size, Color color)
	{
	}

	[BurstDiscard]
	public static void SolidPlane(float3 center, quaternion rotation, float2 size, Color color)
	{
	}

	[BurstDiscard]
	public static void WirePlane(float3 center, float3 normal, float2 size, Color color)
	{
	}

	[BurstDiscard]
	public static void WirePlane(float3 center, quaternion rotation, float2 size, Color color)
	{
	}

	[BurstDiscard]
	public static void PlaneWithNormal(float3 center, float3 normal, float2 size, Color color)
	{
	}

	[BurstDiscard]
	public static void PlaneWithNormal(float3 center, quaternion rotation, float2 size, Color color)
	{
	}

	[BurstDiscard]
	public static void SolidTriangle(float3 a, float3 b, float3 c, Color color)
	{
	}

	[BurstDiscard]
	public static void SolidBox(float3 center, float3 size, Color color)
	{
	}

	[BurstDiscard]
	public static void SolidBox(Bounds bounds, Color color)
	{
	}

	[BurstDiscard]
	public static void SolidBox(float3 center, quaternion rotation, float3 size, Color color)
	{
	}

	[BurstDiscard]
	public static void Label3D(float3 position, quaternion rotation, string text, float size, Color color)
	{
	}

	[BurstDiscard]
	public static void Label3D(float3 position, quaternion rotation, string text, float size, LabelAlignment alignment, Color color)
	{
	}

	[BurstDiscard]
	public static void Label2D(float3 position, string text, float sizeInPixels, Color color)
	{
	}

	[BurstDiscard]
	public static void Label2D(float3 position, string text, Color color)
	{
	}

	[BurstDiscard]
	public static void Label2D(float3 position, string text, float sizeInPixels, LabelAlignment alignment, Color color)
	{
	}

	[BurstDiscard]
	public static void Label2D(float3 position, ref FixedString32Bytes text, float sizeInPixels, Color color)
	{
	}

	[BurstDiscard]
	public static void Label2D(float3 position, ref FixedString32Bytes text, Color color)
	{
	}

	[BurstDiscard]
	public static void Label2D(float3 position, ref FixedString64Bytes text, float sizeInPixels, Color color)
	{
	}

	[BurstDiscard]
	public static void Label2D(float3 position, ref FixedString64Bytes text, Color color)
	{
	}

	[BurstDiscard]
	public static void Label2D(float3 position, ref FixedString128Bytes text, float sizeInPixels, Color color)
	{
	}

	[BurstDiscard]
	public static void Label2D(float3 position, ref FixedString128Bytes text, Color color)
	{
	}

	[BurstDiscard]
	public static void Label2D(float3 position, ref FixedString512Bytes text, float sizeInPixels, Color color)
	{
	}

	[BurstDiscard]
	public static void Label2D(float3 position, ref FixedString512Bytes text, Color color)
	{
	}

	[BurstDiscard]
	public static void Label2D(float3 position, ref FixedString32Bytes text, float sizeInPixels, LabelAlignment alignment, Color color)
	{
	}

	[BurstDiscard]
	public static void Label2D(float3 position, ref FixedString64Bytes text, float sizeInPixels, LabelAlignment alignment, Color color)
	{
	}

	[BurstDiscard]
	public static void Label2D(float3 position, ref FixedString128Bytes text, float sizeInPixels, LabelAlignment alignment, Color color)
	{
	}

	[BurstDiscard]
	public static void Label2D(float3 position, ref FixedString512Bytes text, float sizeInPixels, LabelAlignment alignment, Color color)
	{
	}

	[BurstDiscard]
	public static void Label3D(float3 position, quaternion rotation, ref FixedString32Bytes text, float size, Color color)
	{
	}

	[BurstDiscard]
	public static void Label3D(float3 position, quaternion rotation, ref FixedString64Bytes text, float size, Color color)
	{
	}

	[BurstDiscard]
	public static void Label3D(float3 position, quaternion rotation, ref FixedString128Bytes text, float size, Color color)
	{
	}

	[BurstDiscard]
	public static void Label3D(float3 position, quaternion rotation, ref FixedString512Bytes text, float size, Color color)
	{
	}

	[BurstDiscard]
	public static void Label3D(float3 position, quaternion rotation, ref FixedString32Bytes text, float size, LabelAlignment alignment, Color color)
	{
	}

	[BurstDiscard]
	public static void Label3D(float3 position, quaternion rotation, ref FixedString64Bytes text, float size, LabelAlignment alignment, Color color)
	{
	}

	[BurstDiscard]
	public static void Label3D(float3 position, quaternion rotation, ref FixedString128Bytes text, float size, LabelAlignment alignment, Color color)
	{
	}

	[BurstDiscard]
	public static void Label3D(float3 position, quaternion rotation, ref FixedString512Bytes text, float size, LabelAlignment alignment, Color color)
	{
	}
}
