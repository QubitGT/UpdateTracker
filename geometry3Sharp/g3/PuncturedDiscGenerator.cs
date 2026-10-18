using System;

namespace g3;

public class PuncturedDiscGenerator : MeshGenerator
{
	public float OuterRadius = 1f;

	public float InnerRadius = 0.5f;

	public float StartAngleDeg;

	public float EndAngleDeg = 360f;

	public int Slices = 32;

	public override MeshGenerator Generate()
	{
		vertices = new VectorArray3d(2 * Slices);
		uv = new VectorArray2f(2 * Slices);
		normals = new VectorArray3f(2 * Slices);
		triangles = new IndexArray3i(2 * Slices);
		bool flag = EndAngleDeg - StartAngleDeg > 359.99f;
		float num = (EndAngleDeg - StartAngleDeg) * (MathF.PI / 180f);
		float num2 = StartAngleDeg * (MathF.PI / 180f);
		float num3 = (flag ? (num / (float)Slices) : (num / (float)(Slices - 1)));
		float num4 = InnerRadius / OuterRadius;
		for (int i = 0; i < Slices; i++)
		{
			float num5 = num2 + (float)i * num3;
			double num6 = Math.Cos(num5);
			double num7 = Math.Sin(num5);
			vertices[i] = new Vector3d((double)InnerRadius * num6, 0.0, (double)InnerRadius * num7);
			vertices[Slices + i] = new Vector3d((double)OuterRadius * num6, 0.0, (double)OuterRadius * num7);
			uv[i] = new Vector2f(0.5 * (1.0 + (double)num4 * num6), 0.5 * (1.0 + (double)num4 * num7));
			uv[Slices + i] = new Vector2f(0.5 * (1.0 + num6), 0.5 * (1.0 + num7));
			VectorArray3f vectorArray3f = normals;
			int i2 = i;
			Vector3f value = (normals[Slices + i] = Vector3f.AxisY);
			vectorArray3f[i2] = value;
		}
		int num8 = 0;
		for (int j = 0; j < Slices - 1; j++)
		{
			triangles.Set(num8++, j, j + 1, Slices + j + 1, Clockwise);
			triangles.Set(num8++, j, Slices + j + 1, Slices + j, Clockwise);
		}
		if (flag)
		{
			triangles.Set(num8++, Slices - 1, 0, Slices, Clockwise);
			triangles.Set(num8++, Slices - 1, Slices, 2 * Slices - 1, Clockwise);
		}
		return this;
	}
}
