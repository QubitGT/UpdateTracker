using System;

namespace g3;

public class ConeGenerator : MeshGenerator
{
	public float BaseRadius = 1f;

	public float Height = 1f;

	public float StartAngleDeg;

	public float EndAngleDeg = 360f;

	public int Slices = 16;

	public bool NoSharedVertices;

	public override MeshGenerator Generate()
	{
		bool flag = EndAngleDeg - StartAngleDeg > 359.99f;
		int num = ((NoSharedVertices && flag) ? (Slices + 1) : Slices);
		int num2 = ((!NoSharedVertices) ? 1 : num);
		int num3 = ((!NoSharedVertices) ? 1 : (Slices + 1));
		int num4 = ((NoSharedVertices && !flag) ? 6 : 0);
		vertices = new VectorArray3d(num + num2 + num3 + num4);
		uv = new VectorArray2f(vertices.Count);
		normals = new VectorArray3f(vertices.Count);
		int num5 = (NoSharedVertices ? (2 * Slices) : Slices);
		int slices = Slices;
		int num6 = ((!flag) ? 2 : 0);
		triangles = new IndexArray3i(num5 + slices + num6);
		float num7 = (EndAngleDeg - StartAngleDeg) * (MathF.PI / 180f);
		float num8 = StartAngleDeg * (MathF.PI / 180f);
		float num9 = (flag ? (num7 / (float)Slices) : (num7 / (float)(Slices - 1)));
		for (int i = 0; i < num; i++)
		{
			float num10 = num8 + (float)i * num9;
			double num11 = Math.Cos(num10);
			double num12 = Math.Sin(num10);
			vertices[i] = new Vector3d((double)BaseRadius * num11, 0.0, (double)BaseRadius * num12);
			uv[i] = new Vector2f(0.5 * (1.0 + num11), 0.5 * (1.0 + num12));
			Vector3f value = new Vector3f(num11 * (double)Height, BaseRadius / Height, num12 * (double)Height);
			value.Normalize();
			normals[i] = value;
			if (NoSharedVertices)
			{
				vertices[num + i] = new Vector3d(0.0, Height, 0.0);
				uv[num + i] = new Vector2f(0.5f, 0.5f);
				normals[num + i] = value;
			}
		}
		if (!NoSharedVertices)
		{
			vertices[num] = new Vector3d(0.0, Height, 0.0);
			normals[num] = Vector3f.AxisY;
			uv[num] = new Vector2f(0.5f, 0.5f);
		}
		int tri_counter = 0;
		if (NoSharedVertices)
		{
			for (int j = 0; j < num - 1; j++)
			{
				triangles.Set(tri_counter++, j, j + 1, num + j + 1, Clockwise);
				triangles.Set(tri_counter++, j, num + j + 1, num + j, Clockwise);
			}
		}
		else
		{
			append_disc(Slices, num, 0, flag, !Clockwise, ref tri_counter);
		}
		int num13 = num + num2;
		vertices[num13] = new Vector3d(0.0, 0.0, 0.0);
		uv[num13] = new Vector2f(0.5f, 0.5f);
		normals[num13] = new Vector3f(0f, -1f, 0f);
		if (NoSharedVertices)
		{
			int num14 = num13 + 1;
			for (int k = 0; k < Slices; k++)
			{
				float num15 = num8 + (float)k * num9;
				double num16 = Math.Cos(num15);
				double num17 = Math.Sin(num15);
				vertices[num14 + k] = new Vector3d((double)BaseRadius * num16, 0.0, (double)BaseRadius * num17);
				uv[num14 + k] = new Vector2f(0.5 * (1.0 + num16), 0.5 * (1.0 + num17));
				normals[num14 + k] = -Vector3f.AxisY;
			}
			append_disc(Slices, num13, num14, flag, Clockwise, ref tri_counter);
			if (!flag)
			{
				int num18 = num14 + Slices;
				VectorArray3d vectorArray3d = vertices;
				Vector3d value2 = (vertices[num18 + 4] = vertices[num13]);
				vectorArray3d[num18] = value2;
				VectorArray3d vectorArray3d2 = vertices;
				int i2 = num18 + 1;
				value2 = (vertices[num18 + 3] = new Vector3d(0.0, Height, 0.0));
				vectorArray3d2[i2] = value2;
				vertices[num18 + 2] = vertices[0];
				vertices[num18 + 5] = vertices[num - 1];
				VectorArray3f vectorArray3f = normals;
				VectorArray3f vectorArray3f2 = normals;
				int i3 = num18 + 1;
				Vector3f vector3f2 = (normals[num18 + 2] = estimate_normal(num18, num18 + 1, num18 + 2));
				Vector3f value3 = (vectorArray3f2[i3] = vector3f2);
				vectorArray3f[num18] = value3;
				VectorArray3f vectorArray3f3 = normals;
				int i4 = num18 + 3;
				VectorArray3f vectorArray3f4 = normals;
				int i5 = num18 + 4;
				vector3f2 = (normals[num18 + 5] = estimate_normal(num18 + 3, num18 + 4, num18 + 5));
				value3 = (vectorArray3f4[i5] = vector3f2);
				vectorArray3f3[i4] = value3;
				VectorArray2f vectorArray2f = uv;
				Vector2f value4 = (uv[num18 + 4] = new Vector2f(0f, 0f));
				vectorArray2f[num18] = value4;
				VectorArray2f vectorArray2f2 = uv;
				int i6 = num18 + 1;
				value4 = (uv[num18 + 3] = new Vector2f(0f, 1f));
				vectorArray2f2[i6] = value4;
				VectorArray2f vectorArray2f3 = uv;
				int i7 = num18 + 2;
				value4 = (uv[num18 + 5] = new Vector2f(1f, 0f));
				vectorArray2f3[i7] = value4;
				triangles.Set(tri_counter++, num18, num18 + 1, num18 + 2, !Clockwise);
				triangles.Set(tri_counter++, num18 + 3, num18 + 4, num18 + 5, !Clockwise);
			}
		}
		else
		{
			append_disc(Slices, num13, 0, flag, Clockwise, ref tri_counter);
			if (!flag)
			{
				triangles.Set(tri_counter++, num13, num, 0, !Clockwise);
				triangles.Set(tri_counter++, num13, num, num - 1, Clockwise);
			}
		}
		return this;
	}
}
