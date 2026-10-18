using System;

namespace g3;

public class TrivialRectGenerator : MeshGenerator
{
	public enum UVModes
	{
		FullUVSquare,
		CenteredUVRectangle,
		BottomCornerUVRectangle
	}

	public float Width = 1f;

	public float Height = 1f;

	public Vector3f Normal = Vector3f.AxisY;

	public Index2i IndicesMap = new Index2i(1, 3);

	public UVModes UVMode;

	protected virtual Vector3d make_vertex(float x, float y)
	{
		Vector3d zero = Vector3d.Zero;
		zero[Math.Abs(IndicesMap.a) - 1] = ((IndicesMap.a < 0) ? (0f - x) : x);
		zero[Math.Abs(IndicesMap.b) - 1] = ((IndicesMap.b < 0) ? (0f - y) : y);
		return zero;
	}

	public override MeshGenerator Generate()
	{
		if (!MathUtil.InRange(IndicesMap.a, 1, 3) || !MathUtil.InRange(IndicesMap.b, 1, 3))
		{
			throw new Exception("TrivialRectGenerator: Invalid IndicesMap!");
		}
		vertices = new VectorArray3d(4);
		uv = new VectorArray2f(4);
		normals = new VectorArray3f(4);
		triangles = new IndexArray3i(2);
		vertices[0] = make_vertex((0f - Width) / 2f, (0f - Height) / 2f);
		vertices[1] = make_vertex(Width / 2f, (0f - Height) / 2f);
		vertices[2] = make_vertex(Width / 2f, Height / 2f);
		vertices[3] = make_vertex((0f - Width) / 2f, Height / 2f);
		VectorArray3f vectorArray3f = normals;
		VectorArray3f vectorArray3f2 = normals;
		VectorArray3f vectorArray3f3 = normals;
		Vector3f vector3f = (normals[3] = Normal);
		Vector3f vector3f3 = (vectorArray3f3[2] = vector3f);
		Vector3f value = (vectorArray3f2[1] = vector3f3);
		vectorArray3f[0] = value;
		float x = 0f;
		float x2 = 1f;
		float y = 0f;
		float y2 = 1f;
		if (UVMode != 0)
		{
			if (Width > Height)
			{
				float num = Height / Width;
				if (UVMode == UVModes.CenteredUVRectangle)
				{
					y = 0.5f - num / 2f;
					y2 = 0.5f + num / 2f;
				}
				else
				{
					y2 = num;
				}
			}
			else if (Height > Width)
			{
				float num2 = Width / Height;
				if (UVMode == UVModes.CenteredUVRectangle)
				{
					x = 0.5f - num2 / 2f;
					x2 = 0.5f + num2 / 2f;
				}
				else
				{
					x2 = num2;
				}
			}
		}
		uv[0] = new Vector2f(x, y);
		uv[1] = new Vector2f(x2, y);
		uv[2] = new Vector2f(x2, y2);
		uv[3] = new Vector2f(x, y2);
		if (Clockwise)
		{
			triangles.Set(0, 0, 1, 2);
			triangles.Set(1, 0, 2, 3);
		}
		else
		{
			triangles.Set(0, 0, 2, 1);
			triangles.Set(1, 0, 3, 2);
		}
		return this;
	}
}
