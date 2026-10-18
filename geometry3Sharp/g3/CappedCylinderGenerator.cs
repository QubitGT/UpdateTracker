using System;

namespace g3;

public class CappedCylinderGenerator : MeshGenerator
{
	public float BaseRadius = 1f;

	public float TopRadius = 1f;

	public float Height = 1f;

	public float StartAngleDeg;

	public float EndAngleDeg = 360f;

	public int Slices = 16;

	public bool NoSharedVertices;

	public override MeshGenerator Generate()
	{
		bool flag = EndAngleDeg - StartAngleDeg > 359.99f;
		int num = ((NoSharedVertices && flag) ? (Slices + 1) : Slices);
		int num2 = ((!NoSharedVertices) ? 1 : (Slices + 1));
		int num3 = ((NoSharedVertices && !flag) ? 8 : 0);
		vertices = new VectorArray3d(2 * num + 2 * num2 + num3);
		uv = new VectorArray2f(vertices.Count);
		normals = new VectorArray3f(vertices.Count);
		int num4 = 2 * Slices;
		int num5 = 2 * Slices;
		int num6 = ((!flag) ? 4 : 0);
		triangles = new IndexArray3i(num4 + num5 + num6);
		groups = new int[triangles.Count];
		float num7 = (EndAngleDeg - StartAngleDeg) * (MathF.PI / 180f);
		float num8 = StartAngleDeg * (MathF.PI / 180f);
		float num9 = (flag ? (num7 / (float)Slices) : (num7 / (float)(Slices - 1)));
		for (int i = 0; i < num; i++)
		{
			float num10 = num8 + (float)i * num9;
			double num11 = Math.Cos(num10);
			double num12 = Math.Sin(num10);
			vertices[i] = new Vector3d((double)BaseRadius * num11, 0.0, (double)BaseRadius * num12);
			vertices[num + i] = new Vector3d((double)TopRadius * num11, Height, (double)TopRadius * num12);
			float x = (float)i / (float)Slices;
			uv[i] = new Vector2f(x, 0f);
			uv[num + i] = new Vector2f(x, 1f);
			Vector3f vector3f = new Vector3f((float)num11, 0f, (float)num12);
			vector3f.Normalize();
			VectorArray3f vectorArray3f = normals;
			int i2 = i;
			Vector3f value = (normals[num + i] = vector3f);
			vectorArray3f[i2] = value;
		}
		int tri_counter = 0;
		for (int j = 0; j < num - 1; j++)
		{
			groups[tri_counter] = 1;
			triangles.Set(tri_counter++, j, j + 1, num + j + 1, Clockwise);
			groups[tri_counter] = 1;
			triangles.Set(tri_counter++, j, num + j + 1, num + j, Clockwise);
		}
		if (flag && !NoSharedVertices)
		{
			groups[tri_counter] = 1;
			triangles.Set(tri_counter++, num - 1, 0, num, Clockwise);
			groups[tri_counter] = 1;
			triangles.Set(tri_counter++, num - 1, num, 2 * num - 1, Clockwise);
		}
		int num13 = 2 * num;
		vertices[num13] = new Vector3d(0.0, 0.0, 0.0);
		uv[num13] = new Vector2f(0.5f, 0.5f);
		normals[num13] = new Vector3f(0f, -1f, 0f);
		int num14 = 2 * num + 1;
		vertices[num14] = new Vector3d(0.0, Height, 0.0);
		uv[num14] = new Vector2f(0.5f, 0.5f);
		normals[num14] = new Vector3f(0f, 1f, 0f);
		if (NoSharedVertices)
		{
			int num15 = 2 * num + 2;
			for (int k = 0; k < Slices; k++)
			{
				float num16 = num8 + (float)k * num9;
				double num17 = Math.Cos(num16);
				double num18 = Math.Sin(num16);
				vertices[num15 + k] = new Vector3d((double)BaseRadius * num17, 0.0, (double)BaseRadius * num18);
				uv[num15 + k] = new Vector2f(0.5 * (1.0 + num17), 0.5 * (1.0 + num18));
				normals[num15 + k] = -Vector3f.AxisY;
			}
			append_disc(Slices, num13, num15, flag, Clockwise, ref tri_counter, 2);
			int num19 = 2 * num + 2 + Slices;
			for (int l = 0; l < Slices; l++)
			{
				float num20 = num8 + (float)l * num9;
				double num21 = Math.Cos(num20);
				double num22 = Math.Sin(num20);
				vertices[num19 + l] = new Vector3d((double)TopRadius * num21, Height, (double)TopRadius * num22);
				uv[num19 + l] = new Vector2f(0.5 * (1.0 + num21), 0.5 * (1.0 + num22));
				normals[num19 + l] = Vector3f.AxisY;
			}
			append_disc(Slices, num14, num19, flag, !Clockwise, ref tri_counter, 3);
			if (!flag)
			{
				int num23 = 2 * num + 2 + 2 * Slices;
				VectorArray3d vectorArray3d = vertices;
				Vector3d value2 = (vertices[num23 + 5] = vertices[num13]);
				vectorArray3d[num23] = value2;
				VectorArray3d vectorArray3d2 = vertices;
				int i3 = num23 + 1;
				value2 = (vertices[num23 + 4] = vertices[num14]);
				vectorArray3d2[i3] = value2;
				vertices[num23 + 2] = vertices[num];
				vertices[num23 + 3] = vertices[0];
				vertices[num23 + 6] = vertices[num - 1];
				vertices[num23 + 7] = vertices[2 * num - 1];
				VectorArray3f vectorArray3f2 = normals;
				VectorArray3f vectorArray3f3 = normals;
				int i4 = num23 + 1;
				VectorArray3f vectorArray3f4 = normals;
				int i5 = num23 + 2;
				Vector3f vector3f4 = (normals[num23 + 3] = estimate_normal(num23, num23 + 1, num23 + 2));
				Vector3f vector3f6 = (vectorArray3f4[i5] = vector3f4);
				Vector3f value = (vectorArray3f3[i4] = vector3f6);
				vectorArray3f2[num23] = value;
				VectorArray3f vectorArray3f5 = normals;
				int i6 = num23 + 4;
				VectorArray3f vectorArray3f6 = normals;
				int i7 = num23 + 5;
				VectorArray3f vectorArray3f7 = normals;
				int i8 = num23 + 6;
				vector3f4 = (normals[num23 + 7] = estimate_normal(num23 + 4, num23 + 5, num23 + 6));
				vector3f6 = (vectorArray3f7[i8] = vector3f4);
				value = (vectorArray3f6[i7] = vector3f6);
				vectorArray3f5[i6] = value;
				VectorArray2f vectorArray2f = uv;
				Vector2f value3 = (uv[num23 + 5] = new Vector2f(0f, 0f));
				vectorArray2f[num23] = value3;
				VectorArray2f vectorArray2f2 = uv;
				int i9 = num23 + 1;
				value3 = (uv[num23 + 4] = new Vector2f(0f, 1f));
				vectorArray2f2[i9] = value3;
				VectorArray2f vectorArray2f3 = uv;
				int i10 = num23 + 2;
				value3 = (uv[num23 + 7] = new Vector2f(1f, 1f));
				vectorArray2f3[i10] = value3;
				VectorArray2f vectorArray2f4 = uv;
				int i11 = num23 + 3;
				value3 = (uv[num23 + 6] = new Vector2f(1f, 0f));
				vectorArray2f4[i11] = value3;
				append_rectangle(num23, num23 + 1, num23 + 2, num23 + 3, !Clockwise, ref tri_counter, 4);
				append_rectangle(num23 + 4, num23 + 5, num23 + 6, num23 + 7, !Clockwise, ref tri_counter, 5);
			}
		}
		else
		{
			append_disc(Slices, num13, 0, flag, Clockwise, ref tri_counter, 2);
			append_disc(Slices, num14, num, flag, !Clockwise, ref tri_counter, 3);
			if (!flag)
			{
				append_rectangle(num13, 0, num, num14, Clockwise, ref tri_counter, 4);
				append_rectangle(num - 1, num13, num14, 2 * num - 1, Clockwise, ref tri_counter, 5);
			}
		}
		return this;
	}
}
