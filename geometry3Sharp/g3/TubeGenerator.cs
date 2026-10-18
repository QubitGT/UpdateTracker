using System.Collections.Generic;

namespace g3;

public class TubeGenerator : MeshGenerator
{
	public List<Vector3d> Vertices;

	public Polygon2d Polygon;

	public bool Capped = true;

	public bool OverrideCapCenter;

	public Vector2d CapCenter = Vector2d.Zero;

	public bool ClosedLoop;

	public Frame3f Frame = Frame3f.Identity;

	public bool NoSharedVertices = true;

	public int startCapCenterIndex = -1;

	public int endCapCenterIndex = -1;

	public TubeGenerator()
	{
	}

	public TubeGenerator(Polygon2d tubePath, Frame3f pathPlane, Polygon2d tubeShape, int nPlaneNormal = 2)
	{
		Vertices = new List<Vector3d>();
		foreach (Vector2d vertex in tubePath.Vertices)
		{
			Vertices.Add(pathPlane.FromPlaneUV((Vector2f)vertex, nPlaneNormal));
		}
		Polygon = new Polygon2d(tubeShape);
		ClosedLoop = true;
		Capped = false;
	}

	public TubeGenerator(PolyLine2d tubePath, Frame3f pathPlane, Polygon2d tubeShape, int nPlaneNormal = 2)
	{
		Vertices = new List<Vector3d>();
		foreach (Vector2d vertex in tubePath.Vertices)
		{
			Vertices.Add(pathPlane.FromPlaneUV((Vector2f)vertex, nPlaneNormal));
		}
		Polygon = new Polygon2d(tubeShape);
		ClosedLoop = false;
		Capped = true;
	}

	public TubeGenerator(DCurve3 tubePath, Polygon2d tubeShape)
	{
		Vertices = new List<Vector3d>(tubePath.Vertices);
		Polygon = new Polygon2d(tubeShape);
		ClosedLoop = tubePath.Closed;
		Capped = !ClosedLoop;
	}

	public override MeshGenerator Generate()
	{
		if (Polygon == null)
		{
			Polygon = Polygon2d.MakeCircle(1.0, 8);
		}
		int count = Vertices.Count;
		int vertexCount = Polygon.VertexCount;
		int num = ((ClosedLoop && NoSharedVertices) ? (count + 1) : count);
		int num2 = (NoSharedVertices ? (vertexCount + 1) : vertexCount);
		int num3 = ((!NoSharedVertices) ? 1 : (vertexCount + 1));
		if (!Capped || ClosedLoop)
		{
			num3 = 0;
		}
		vertices = new VectorArray3d(num * num2 + 2 * num3);
		uv = new VectorArray2f(vertices.Count);
		normals = new VectorArray3f(vertices.Count);
		int num4 = (ClosedLoop ? count : (count - 1)) * (2 * vertexCount);
		int num5 = ((Capped && !ClosedLoop) ? (2 * vertexCount) : 0);
		triangles = new IndexArray3i(num4 + num5);
		Frame3f copy = new Frame3f(Frame);
		Vector3d tangent = CurveUtils.GetTangent(Vertices, 0, ClosedLoop);
		copy.Origin = (Vector3f)Vertices[0];
		copy.AlignAxis(2, (Vector3f)tangent);
		Frame3f frame3f = new Frame3f(copy);
		double arcLength = Polygon.ArcLength;
		double num6 = CurveUtils.ArcLength(Vertices, ClosedLoop);
		double num7 = 0.0;
		for (int i = 0; i < num; i++)
		{
			int num8 = i % count;
			Vector3d tangent2 = CurveUtils.GetTangent(Vertices, num8, ClosedLoop);
			copy.Origin = (Vector3f)Vertices[num8];
			copy.AlignAxis(2, (Vector3f)tangent2);
			int num9 = i * num2;
			double num10 = 0.0;
			for (int j = 0; j < num2; j++)
			{
				int i2 = num9 + j;
				Vector2d vector2d = Polygon.Vertices[j % vertexCount];
				Vector2d v = Polygon.Vertices[(j + 1) % vertexCount];
				Vector3d vector3d = copy.FromPlaneUV((Vector2f)vector2d, 2);
				vertices[i2] = vector3d;
				uv[i2] = new Vector2f(num7, num10);
				num10 += vector2d.Distance(v) / arcLength;
				Vector3f value = (Vector3f)(vector3d - copy.Origin).Normalized;
				normals[i2] = value;
			}
			int index = (i + 1) % count;
			double num11 = Vertices[num8].Distance(Vertices[index]);
			num7 += num11 / num6;
		}
		int tri_counter = 0;
		int num12 = ((ClosedLoop && !NoSharedVertices) ? num : (num - 1));
		for (int k = 0; k < num12; k++)
		{
			int num13 = k * num2;
			int num14 = num13 + num2;
			if (ClosedLoop && k == num12 - 1 && !NoSharedVertices)
			{
				num14 = 0;
			}
			for (int l = 0; l < num2 - 1; l++)
			{
				triangles.Set(tri_counter++, num13 + l, num13 + l + 1, num14 + l + 1, Clockwise);
				triangles.Set(tri_counter++, num13 + l, num14 + l + 1, num14 + l, Clockwise);
			}
			if (!NoSharedVertices)
			{
				int num15 = num2 - 1;
				triangles.Set(tri_counter++, num13 + num15, num13, num14, Clockwise);
				triangles.Set(tri_counter++, num13 + num15, num14, num14 + num15, Clockwise);
			}
		}
		if (Capped && !ClosedLoop)
		{
			Vector2d vector2d2 = (OverrideCapCenter ? CapCenter : Polygon.Bounds.Center);
			int num16 = num * num2;
			vertices[num16] = frame3f.FromPlaneUV((Vector2f)vector2d2, 2);
			uv[num16] = new Vector2f(0.5f, 0.5f);
			normals[num16] = -frame3f.Z;
			startCapCenterIndex = num16;
			int num17 = num16 + 1;
			vertices[num17] = copy.FromPlaneUV((Vector2f)vector2d2, 2);
			uv[num17] = new Vector2f(0.5f, 0.5f);
			normals[num17] = copy.Z;
			endCapCenterIndex = num17;
			if (NoSharedVertices)
			{
				int num18 = 0;
				int num19 = num17 + 1;
				for (int m = 0; m < vertexCount; m++)
				{
					vertices[num19 + m] = vertices[num18 + m];
					Vector2d vector2d3 = ((Polygon[m] - vector2d2).Normalized + Vector2d.One) * 0.5;
					uv[num19 + m] = (Vector2f)vector2d3;
					normals[num19 + m] = normals[num16];
				}
				append_disc(vertexCount, num16, num19, bClosed: true, Clockwise, ref tri_counter);
				int num20 = num2 * (num - 1);
				int num21 = num19 + vertexCount;
				for (int n = 0; n < vertexCount; n++)
				{
					vertices[num21 + n] = vertices[num20 + n];
					uv[num21 + n] = uv[num19 + n];
					normals[num21 + n] = normals[num17];
				}
				append_disc(vertexCount, num17, num21, bClosed: true, !Clockwise, ref tri_counter);
			}
			else
			{
				append_disc(vertexCount, num16, 0, bClosed: true, Clockwise, ref tri_counter);
				append_disc(vertexCount, num17, num2 * (num - 1), bClosed: true, !Clockwise, ref tri_counter);
			}
		}
		return this;
	}
}
