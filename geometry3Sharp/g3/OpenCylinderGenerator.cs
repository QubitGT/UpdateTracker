using System;

namespace g3;

public class OpenCylinderGenerator : MeshGenerator
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
		vertices = new VectorArray3d(2 * num);
		uv = new VectorArray2f(vertices.Count);
		normals = new VectorArray3f(vertices.Count);
		triangles = new IndexArray3i(2 * Slices);
		float num2 = (EndAngleDeg - StartAngleDeg) * (MathF.PI / 180f);
		float num3 = StartAngleDeg * (MathF.PI / 180f);
		float num4 = (flag ? (num2 / (float)Slices) : (num2 / (float)(Slices - 1)));
		for (int i = 0; i < num; i++)
		{
			float num5 = num3 + (float)i * num4;
			double num6 = Math.Cos(num5);
			double num7 = Math.Sin(num5);
			vertices[i] = new Vector3d((double)BaseRadius * num6, 0.0, (double)BaseRadius * num7);
			vertices[num + i] = new Vector3d((double)TopRadius * num6, Height, (double)TopRadius * num7);
			float x = (float)i / (float)Slices;
			uv[i] = new Vector2f(x, 0f);
			uv[num + i] = new Vector2f(x, 1f);
			Vector3f vector3f = new Vector3f((float)num6, 0f, (float)num7);
			vector3f.Normalize();
			VectorArray3f vectorArray3f = normals;
			int i2 = i;
			Vector3f value = (normals[num + i] = vector3f);
			vectorArray3f[i2] = value;
		}
		int num8 = 0;
		for (int j = 0; j < num - 1; j++)
		{
			triangles.Set(num8++, j, j + 1, num + j + 1, Clockwise);
			triangles.Set(num8++, j, num + j + 1, num + j, Clockwise);
		}
		if (flag && !NoSharedVertices)
		{
			triangles.Set(num8++, num - 1, 0, num, Clockwise);
			triangles.Set(num8++, num - 1, num, 2 * num - 1, Clockwise);
		}
		return this;
	}
}
