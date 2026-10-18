using System;

namespace g3;

public class GriddedRectGenerator : TrivialRectGenerator
{
	public int EdgeVertices = 8;

	public override MeshGenerator Generate()
	{
		if (!MathUtil.InRange(IndicesMap.a, 1, 3) || !MathUtil.InRange(IndicesMap.b, 1, 3))
		{
			throw new Exception("GriddedRectGenerator: Invalid IndicesMap!");
		}
		int num = ((EdgeVertices > 1) ? EdgeVertices : 2);
		int num2 = num - 1;
		int nCount = num * num;
		vertices = new VectorArray3d(nCount);
		uv = new VectorArray2f(vertices.Count);
		normals = new VectorArray3f(vertices.Count);
		triangles = new IndexArray3i(2 * num2 * num2);
		groups = new int[triangles.Count];
		Vector3d v = make_vertex((0f - Width) / 2f, (0f - Height) / 2f);
		Vector3d v2 = make_vertex(Width / 2f, (0f - Height) / 2f);
		Vector3d v3 = make_vertex(Width / 2f, Height / 2f);
		Vector3d v4 = make_vertex((0f - Width) / 2f, Height / 2f);
		float x = 0f;
		float x2 = 1f;
		float y = 0f;
		float y2 = 1f;
		if (UVMode != 0)
		{
			if (Width > Height)
			{
				float num3 = Height / Width;
				if (UVMode == UVModes.CenteredUVRectangle)
				{
					y = 0.5f - num3 / 2f;
					y2 = 0.5f + num3 / 2f;
				}
				else
				{
					y2 = num3;
				}
			}
			else if (Height > Width)
			{
				float num4 = Width / Height;
				if (UVMode == UVModes.CenteredUVRectangle)
				{
					x = 0.5f - num4 / 2f;
					x2 = 0.5f + num4 / 2f;
				}
				else
				{
					x2 = num4;
				}
			}
		}
		Vector2f v5 = new Vector2f(x, y);
		Vector2f v6 = new Vector2f(x2, y);
		Vector2f v7 = new Vector2f(x2, y2);
		Vector2f v8 = new Vector2f(x, y2);
		int num5 = 0;
		int num6 = 0;
		int num7 = num5;
		for (int i = 0; i < num; i++)
		{
			double num8 = (double)i / (double)(num - 1);
			for (int j = 0; j < num; j++)
			{
				double num9 = (double)j / (double)(num - 1);
				normals[num5] = Normal;
				uv[num5] = bilerp(ref v5, ref v6, ref v7, ref v8, (float)num9, (float)num8);
				vertices[num5++] = bilerp(ref v, ref v2, ref v3, ref v4, num9, num8);
			}
		}
		for (int k = 0; k < num2; k++)
		{
			for (int l = 0; l < num2; l++)
			{
				int num10 = num7 + k * num + l;
				int num11 = num7 + (k + 1) * num + l;
				int c = num10 + 1;
				int num12 = num11 + 1;
				groups[num6] = 0;
				triangles.Set(num6++, num10, num12, c, Clockwise);
				groups[num6] = 0;
				triangles.Set(num6++, num10, num11, num12, Clockwise);
			}
		}
		return this;
	}
}
