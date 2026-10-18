using System;

namespace g3;

public class RoundRectGenerator : MeshGenerator
{
	[Flags]
	public enum Corner
	{
		BottomLeft = 1,
		BottomRight = 2,
		TopRight = 4,
		TopLeft = 8
	}

	public enum UVModes
	{
		FullUVSquare,
		CenteredUVRectangle,
		BottomCornerUVRectangle
	}

	public float Width = 1f;

	public float Height = 1f;

	public float Radius = 0.1f;

	public int CornerSteps = 4;

	public Corner SharpCorners;

	public UVModes UVMode;

	private static int[] corner_spans = new int[12]
	{
		0, 11, 4, 1, 5, 6, 2, 7, 8, 3,
		9, 10
	};

	private static readonly float[] signx = new float[4] { 1f, 1f, -1f, -1f };

	private static readonly float[] signy = new float[4] { -1f, 1f, 1f, -1f };

	private static readonly float[] startangle = new float[4] { 270f, 0f, 90f, 180f };

	private static readonly float[] endangle = new float[4] { 360f, 90f, 180f, 270f };

	public override MeshGenerator Generate()
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < 4; i++)
		{
			if (((uint)SharpCorners & (uint)(1 << i)) != 0)
			{
				num++;
				num2 += 2;
			}
			else
			{
				num += CornerSteps;
				num2 += CornerSteps + 1;
			}
		}
		vertices = new VectorArray3d(12 + num);
		uv = new VectorArray2f(vertices.Count);
		normals = new VectorArray3f(vertices.Count);
		triangles = new IndexArray3i(10 + num2);
		float num3 = Width - 2f * Radius;
		float num4 = Height - 2f * Radius;
		vertices[0] = new Vector3d((0f - num3) / 2f, 0.0, (0f - num4) / 2f);
		vertices[1] = new Vector3d(num3 / 2f, 0.0, (0f - num4) / 2f);
		vertices[2] = new Vector3d(num3 / 2f, 0.0, num4 / 2f);
		vertices[3] = new Vector3d((0f - num3) / 2f, 0.0, num4 / 2f);
		vertices[4] = new Vector3d((0f - num3) / 2f, 0.0, (0f - Height) / 2f);
		vertices[5] = new Vector3d(num3 / 2f, 0.0, (0f - Height) / 2f);
		vertices[6] = new Vector3d(Width / 2f, 0.0, (0f - num4) / 2f);
		vertices[7] = new Vector3d(Width / 2f, 0.0, num4 / 2f);
		vertices[8] = new Vector3d(num3 / 2f, 0.0, Height / 2f);
		vertices[9] = new Vector3d((0f - num3) / 2f, 0.0, Height / 2f);
		vertices[10] = new Vector3d((0f - Width) / 2f, 0.0, num4 / 2f);
		vertices[11] = new Vector3d((0f - Width) / 2f, 0.0, (0f - num4) / 2f);
		bool bCycle = !Clockwise;
		int tri_counter = 0;
		append_rectangle(0, 1, 2, 3, bCycle, ref tri_counter);
		append_rectangle(4, 5, 1, 0, bCycle, ref tri_counter);
		append_rectangle(1, 6, 7, 2, bCycle, ref tri_counter);
		append_rectangle(3, 2, 8, 9, bCycle, ref tri_counter);
		append_rectangle(11, 0, 3, 10, bCycle, ref tri_counter);
		int vtx_counter = 12;
		for (int j = 0; j < 4; j++)
		{
			if ((int)((uint)SharpCorners & (uint)(1 << j)) > 0)
			{
				append_2d_disc_segment(corner_spans[3 * j], corner_spans[3 * j + 1], corner_spans[3 * j + 2], 1, bCycle, ref vtx_counter, ref tri_counter, -1, 1.4142135623730951 * (double)Radius);
			}
			else
			{
				append_2d_disc_segment(corner_spans[3 * j], corner_spans[3 * j + 1], corner_spans[3 * j + 2], CornerSteps, bCycle, ref vtx_counter, ref tri_counter);
			}
		}
		for (int k = 0; k < vertices.Count; k++)
		{
			normals[k] = Vector3f.AxisY;
		}
		float num5 = 0f;
		float num6 = 1f;
		float num7 = 0f;
		float num8 = 1f;
		if (UVMode != 0)
		{
			if (Width > Height)
			{
				float num9 = Height / Width;
				if (UVMode == UVModes.CenteredUVRectangle)
				{
					num7 = 0.5f - num9 / 2f;
					num8 = 0.5f + num9 / 2f;
				}
				else
				{
					num8 = num9;
				}
			}
			else if (Height > Width)
			{
				float num10 = Width / Height;
				if (UVMode == UVModes.CenteredUVRectangle)
				{
					num5 = 0.5f - num10 / 2f;
					num6 = 0.5f + num10 / 2f;
				}
				else
				{
					num6 = num10;
				}
			}
		}
		Vector3d vector3d = new Vector3d((0f - Width) / 2f, 0.0, (0f - Height) / 2f);
		for (int l = 0; l < vertices.Count; l++)
		{
			Vector3d vector3d2 = vertices[l];
			double num11 = (vector3d2.x - vector3d.x) / (double)Width;
			double num12 = (vector3d2.y - vector3d.y) / (double)Height;
			uv[l] = new Vector2f((1.0 - num11) * (double)num5 + num11 * (double)num6, (1.0 - num12) * (double)num7 + num12 * (double)num8);
		}
		return this;
	}

	public Vector3d[] GetBorderLoop()
	{
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			num = ((((uint)SharpCorners & (uint)(1 << i)) == 0) ? (num + CornerSteps) : (num + 1));
		}
		float num2 = Width - 2f * Radius;
		float num3 = Height - 2f * Radius;
		Vector3d[] array = new Vector3d[4 + num];
		int num4 = 0;
		for (int j = 0; j < 4; j++)
		{
			array[num4++] = new Vector3d(signx[j] * Width / 2f, 0.0, signy[j] * Height / 2f);
			bool flag = (int)((uint)SharpCorners & (uint)(1 << j)) > 0;
			Arc2d arc2d = new Arc2d(new Vector2d(signx[j] * num2, signy[j] * num3), flag ? (1.4142135623730951 * (double)Radius) : ((double)Radius), startangle[j], endangle[j]);
			int num5 = (flag ? 1 : CornerSteps);
			for (int k = 0; k < num5; k++)
			{
				double t = (double)(j + 1) / (double)(num5 + 1);
				Vector2d vector2d = arc2d.SampleT(t);
				array[num4++] = new Vector3d(vector2d.x, 0.0, vector2d.y);
			}
		}
		return array;
	}
}
