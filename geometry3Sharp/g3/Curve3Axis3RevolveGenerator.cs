using System;

namespace g3;

public class Curve3Axis3RevolveGenerator : MeshGenerator
{
	public Vector3d[] Curve;

	public Frame3f Axis = Frame3f.Identity;

	public int RevolveAxis = 1;

	public bool Capped = true;

	public int Slices = 16;

	public bool NoSharedVertices = true;

	public int startCapCenterIndex = -1;

	public int endCapCenterIndex = -1;

	public override MeshGenerator Generate()
	{
		int num = Curve.Length;
		int num2 = (NoSharedVertices ? (Slices + 1) : Slices);
		int num3 = ((!NoSharedVertices) ? 1 : (Slices + 1));
		if (!Capped)
		{
			num3 = 0;
		}
		vertices = new VectorArray3d(num2 * num + 2 * num3);
		uv = new VectorArray2f(vertices.Count);
		normals = new VectorArray3f(vertices.Count);
		int num4 = (num - 1) * (2 * Slices);
		int num5 = (Capped ? (2 * Slices) : 0);
		triangles = new IndexArray3i(num4 + num5);
		float num6 = (float)(Math.PI * 2.0 / (double)Slices);
		Frame3f axis = Axis;
		for (int i = 0; i < num; i++)
		{
			Vector3d vector3d = Curve[i];
			Vector3f vector3f = axis.ToFrameP((Vector3f)vector3d);
			float x = (float)i / (float)(num - 1);
			int num7 = i * num2;
			for (int j = 0; j < num2; j++)
			{
				float angleRad = (float)j * num6;
				Vector3f v = Quaternionf.AxisAngleR(Vector3f.AxisY, angleRad) * vector3f;
				Vector3d vector3d2 = axis.FromFrameP(v);
				int i2 = num7 + j;
				vertices[i2] = vector3d2;
				float y = (float)j / (float)num2;
				uv[i2] = new Vector2f(x, y);
				Vector3f value = (Vector3f)(vector3d2 - axis.Origin).Normalized;
				normals[i2] = value;
			}
		}
		int tri_counter = 0;
		for (int k = 0; k < num - 1; k++)
		{
			int num8 = k * num2;
			int num9 = num8 + num2;
			for (int l = 0; l < num2 - 1; l++)
			{
				triangles.Set(tri_counter++, num8 + l, num8 + l + 1, num9 + l + 1, Clockwise);
				triangles.Set(tri_counter++, num8 + l, num9 + l + 1, num9 + l, Clockwise);
			}
			if (!NoSharedVertices)
			{
				triangles.Set(tri_counter++, num9 - 1, num8, num9, Clockwise);
				triangles.Set(tri_counter++, num9 - 1, num9, num9 + num2 - 1, Clockwise);
			}
		}
		if (Capped)
		{
			Vector3d zero = Vector3d.Zero;
			Vector3d zero2 = Vector3d.Zero;
			for (int m = 0; m < Slices; m++)
			{
				zero += vertices[m];
				zero2 += vertices[(num - 1) * num2 + m];
			}
			zero /= (double)Slices;
			zero2 /= (double)Slices;
			Frame3f frame3f = axis;
			frame3f.Origin = (Vector3f)zero;
			Frame3f frame3f2 = axis;
			frame3f2.Origin = (Vector3f)zero2;
			int num10 = num * num2;
			vertices[num10] = frame3f.Origin;
			uv[num10] = new Vector2f(0.5f, 0.5f);
			normals[num10] = -frame3f.Z;
			startCapCenterIndex = num10;
			int num11 = num10 + 1;
			vertices[num11] = frame3f2.Origin;
			uv[num11] = new Vector2f(0.5f, 0.5f);
			normals[num11] = frame3f2.Z;
			endCapCenterIndex = num11;
			if (NoSharedVertices)
			{
				int num12 = 0;
				int num13 = num11 + 1;
				for (int n = 0; n < Slices; n++)
				{
					vertices[num13 + n] = vertices[num12 + n];
					float num14 = (float)n * num6;
					double num15 = Math.Cos(num14);
					double num16 = Math.Sin(num14);
					uv[num13 + n] = new Vector2f(0.5 * (1.0 + num15), 0.5 * (1.0 + num16));
					normals[num13 + n] = normals[num10];
				}
				append_disc(Slices, num10, num13, bClosed: true, Clockwise, ref tri_counter);
				int num17 = num2 * (num - 1);
				int num18 = num13 + Slices;
				for (int num19 = 0; num19 < Slices; num19++)
				{
					vertices[num18 + num19] = vertices[num17 + num19];
					float num20 = (float)num19 * num6;
					double num21 = Math.Cos(num20);
					double num22 = Math.Sin(num20);
					uv[num18 + num19] = new Vector2f(0.5 * (1.0 + num21), 0.5 * (1.0 + num22));
					normals[num18 + num19] = normals[num11];
				}
				append_disc(Slices, num11, num18, bClosed: true, !Clockwise, ref tri_counter);
			}
			else
			{
				append_disc(Slices, num10, 0, bClosed: true, Clockwise, ref tri_counter);
				append_disc(Slices, num11, num2 * (num - 1), bClosed: true, !Clockwise, ref tri_counter);
			}
		}
		return this;
	}
}
