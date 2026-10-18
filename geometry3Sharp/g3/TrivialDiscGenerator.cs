using System;

namespace g3;

public class TrivialDiscGenerator : MeshGenerator
{
	public float Radius = 1f;

	public float StartAngleDeg;

	public float EndAngleDeg = 360f;

	public int Slices = 32;

	public override MeshGenerator Generate()
	{
		vertices = new VectorArray3d(Slices + 1);
		uv = new VectorArray2f(Slices + 1);
		normals = new VectorArray3f(Slices + 1);
		triangles = new IndexArray3i(Slices);
		int num = 0;
		vertices[num] = Vector3d.Zero;
		uv[num] = new Vector2f(0.5f, 0.5f);
		normals[num] = Vector3f.AxisY;
		num++;
		bool flag = EndAngleDeg - StartAngleDeg > 359.99f;
		float num2 = (EndAngleDeg - StartAngleDeg) * (MathF.PI / 180f);
		float num3 = StartAngleDeg * (MathF.PI / 180f);
		float num4 = (flag ? (num2 / (float)Slices) : (num2 / (float)(Slices - 1)));
		for (int i = 0; i < Slices; i++)
		{
			float num5 = num3 + (float)i * num4;
			double num6 = Math.Cos(num5);
			double num7 = Math.Sin(num5);
			vertices[num] = new Vector3d((double)Radius * num6, 0.0, (double)Radius * num7);
			uv[num] = new Vector2f(0.5 * (1.0 + num6), 0.5 * (1.0 + num7));
			normals[num] = Vector3f.AxisY;
			num++;
		}
		int num8 = 0;
		for (int j = 1; j < Slices; j++)
		{
			triangles.Set(num8++, j, 0, j + 1, Clockwise);
		}
		if (flag)
		{
			triangles.Set(num8++, Slices, 0, 1, Clockwise);
		}
		return this;
	}
}
