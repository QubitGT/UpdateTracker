using System;

namespace g3;

public class Curve3Curve3RevolveGenerator : MeshGenerator
{
	public Vector3d[] Curve;

	public Vector3d[] Axis;

	public bool Capped = true;

	public int Slices = 16;

	public bool NoSharedVertices = true;

	public int startCapCenterIndex = -1;

	public int endCapCenterIndex = -1;

	public override MeshGenerator Generate()
	{
		double num = CurveUtils.ArcLength(Curve);
		SampledArcLengthParam sampledArcLengthParam = new SampledArcLengthParam(Axis, Axis.Length);
		double num2 = sampledArcLengthParam.ArcLength / num;
		int num3 = Curve.Length;
		int num4 = (NoSharedVertices ? (Slices + 1) : Slices);
		int num5 = ((!NoSharedVertices) ? 1 : (Slices + 1));
		if (!Capped)
		{
			num5 = 0;
		}
		vertices = new VectorArray3d(num4 * num3 + 2 * num5);
		uv = new VectorArray2f(vertices.Count);
		normals = new VectorArray3f(vertices.Count);
		int num6 = (num3 - 1) * (2 * Slices);
		int num7 = (Capped ? (2 * Slices) : 0);
		triangles = new IndexArray3i(num6 + num7);
		float num8 = (float)(Math.PI * 2.0 / (double)Slices);
		double num9 = 0.0;
		CurveSample curveSample = sampledArcLengthParam.Sample(num9);
		Frame3f frame3f = new Frame3f((Vector3f)curveSample.position, (Vector3f)curveSample.tangent, 1);
		Frame3f frame3f2 = frame3f;
		for (int i = 0; i < num3; i++)
		{
			if (i > 0)
			{
				num9 += (Curve[i] - Curve[i - 1]).Length;
				curveSample = sampledArcLengthParam.Sample(num9 * num2);
				frame3f2.Origin = (Vector3f)curveSample.position;
				frame3f2.AlignAxis(1, (Vector3f)curveSample.tangent);
			}
			Vector3d vector3d = Curve[i];
			Vector3f vector3f = frame3f2.ToFrameP((Vector3f)vector3d);
			float x = (float)i / (float)(num3 - 1);
			int num10 = i * num4;
			for (int j = 0; j < num4; j++)
			{
				float angleRad = (float)j * num8;
				Vector3f v = Quaternionf.AxisAngleR(Vector3f.AxisY, angleRad) * vector3f;
				Vector3d vector3d2 = frame3f2.FromFrameP(v);
				int i2 = num10 + j;
				vertices[i2] = vector3d2;
				float y = (float)j / (float)num4;
				uv[i2] = new Vector2f(x, y);
				Vector3f value = (Vector3f)(vector3d2 - frame3f2.Origin).Normalized;
				normals[i2] = value;
			}
		}
		int tri_counter = 0;
		for (int k = 0; k < num3 - 1; k++)
		{
			int num11 = k * num4;
			int num12 = num11 + num4;
			for (int l = 0; l < num4 - 1; l++)
			{
				triangles.Set(tri_counter++, num11 + l, num11 + l + 1, num12 + l + 1, Clockwise);
				triangles.Set(tri_counter++, num11 + l, num12 + l + 1, num12 + l, Clockwise);
			}
			if (!NoSharedVertices)
			{
				triangles.Set(tri_counter++, num12 - 1, num11, num12, Clockwise);
				triangles.Set(tri_counter++, num12 - 1, num12, num12 + num4 - 1, Clockwise);
			}
		}
		if (Capped)
		{
			Vector3d zero = Vector3d.Zero;
			Vector3d zero2 = Vector3d.Zero;
			for (int m = 0; m < Slices; m++)
			{
				zero += vertices[m];
				zero2 += vertices[(num3 - 1) * num4 + m];
			}
			zero /= (double)Slices;
			zero2 /= (double)Slices;
			Frame3f frame3f3 = frame3f;
			frame3f3.Origin = (Vector3f)zero;
			Frame3f frame3f4 = frame3f2;
			frame3f4.Origin = (Vector3f)zero2;
			int num13 = num3 * num4;
			vertices[num13] = frame3f3.Origin;
			uv[num13] = new Vector2f(0.5f, 0.5f);
			normals[num13] = -frame3f3.Z;
			startCapCenterIndex = num13;
			int num14 = num13 + 1;
			vertices[num14] = frame3f4.Origin;
			uv[num14] = new Vector2f(0.5f, 0.5f);
			normals[num14] = frame3f4.Z;
			endCapCenterIndex = num14;
			if (NoSharedVertices)
			{
				int num15 = 0;
				int num16 = num14 + 1;
				for (int n = 0; n < Slices; n++)
				{
					vertices[num16 + n] = vertices[num15 + n];
					float num17 = (float)n * num8;
					double num18 = Math.Cos(num17);
					double num19 = Math.Sin(num17);
					uv[num16 + n] = new Vector2f(0.5 * (1.0 + num18), 0.5 * (1.0 + num19));
					normals[num16 + n] = normals[num13];
				}
				append_disc(Slices, num13, num16, bClosed: true, Clockwise, ref tri_counter);
				int num20 = num4 * (num3 - 1);
				int num21 = num16 + Slices;
				for (int num22 = 0; num22 < Slices; num22++)
				{
					vertices[num21 + num22] = vertices[num20 + num22];
					float num23 = (float)num22 * num8;
					double num24 = Math.Cos(num23);
					double num25 = Math.Sin(num23);
					uv[num21 + num22] = new Vector2f(0.5 * (1.0 + num24), 0.5 * (1.0 + num25));
					normals[num21 + num22] = normals[num14];
				}
				append_disc(Slices, num14, num21, bClosed: true, !Clockwise, ref tri_counter);
			}
			else
			{
				append_disc(Slices, num13, 0, bClosed: true, Clockwise, ref tri_counter);
				append_disc(Slices, num14, num4 * (num3 - 1), bClosed: true, !Clockwise, ref tri_counter);
			}
		}
		return this;
	}
}
