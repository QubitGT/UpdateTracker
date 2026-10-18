using System;
using System.Linq;

namespace g3;

public class VerticalGeneralizedCylinderGenerator : MeshGenerator
{
	public CircularSection[] Sections;

	public int Slices = 16;

	public bool Capped = true;

	public bool NoSharedVertices = true;

	public int startCapCenterIndex = -1;

	public int endCapCenterIndex = -1;

	public override MeshGenerator Generate()
	{
		int num = (NoSharedVertices ? (2 * (Sections.Length - 1)) : Sections.Length);
		int num2 = (NoSharedVertices ? (Slices + 1) : Slices);
		int num3 = ((!NoSharedVertices) ? 1 : (Slices + 1));
		if (!Capped)
		{
			num3 = 0;
		}
		vertices = new VectorArray3d(num * num2 + 2 * num3);
		uv = new VectorArray2f(vertices.Count);
		normals = new VectorArray3f(vertices.Count);
		int num4 = (Sections.Length - 1) * (2 * Slices);
		int num5 = (Capped ? (2 * Slices) : 0);
		triangles = new IndexArray3i(num4 + num5);
		float num6 = (float)(Math.PI * 2.0 / (double)Slices);
		float num7 = Sections.Last().SectionY - Sections[0].SectionY;
		if (num7 == 0f)
		{
			num7 = 1f;
		}
		int num8 = 0;
		for (int i = 0; i < Sections.Length; i++)
		{
			int num9 = num8 * num2;
			float sectionY = Sections[i].SectionY;
			float y = (sectionY - Sections[0].SectionY) / num7;
			for (int j = 0; j < num2; j++)
			{
				int i2 = num9 + j;
				float num10 = (float)j * num6;
				double num11 = Math.Cos(num10);
				double num12 = Math.Sin(num10);
				vertices[i2] = new Vector3d((double)Sections[i].Radius * num11, sectionY, (double)Sections[i].Radius * num12);
				float x = (float)j / (float)(Slices - 1);
				uv[i2] = new Vector2f(x, y);
				Vector3f value = new Vector3f((float)num11, 0f, (float)num12);
				value.Normalize();
				normals[i2] = value;
			}
			num8++;
			if (NoSharedVertices && i != 0 && i != Sections.Length - 1)
			{
				duplicate_vertex_span(num9, num2);
				num8++;
			}
		}
		int tri_counter = 0;
		num8 = 0;
		for (int k = 0; k < Sections.Length - 1; k++)
		{
			int num13 = num8 * num2;
			int num14 = num13 + num2;
			num8 += ((!NoSharedVertices) ? 1 : 2);
			for (int l = 0; l < num2 - 1; l++)
			{
				triangles.Set(tri_counter++, num13 + l, num13 + l + 1, num14 + l + 1, Clockwise);
				triangles.Set(tri_counter++, num13 + l, num14 + l + 1, num14 + l, Clockwise);
			}
			if (!NoSharedVertices)
			{
				triangles.Set(tri_counter++, num14 - 1, num13, num14, Clockwise);
				triangles.Set(tri_counter++, num14 - 1, num14, num14 + num2 - 1, Clockwise);
			}
		}
		if (Capped)
		{
			CircularSection circularSection = Sections[0];
			CircularSection circularSection2 = Sections.Last();
			int num15 = num * num2;
			vertices[num15] = new Vector3d(0.0, circularSection.SectionY, 0.0);
			uv[num15] = new Vector2f(0.5f, 0.5f);
			normals[num15] = new Vector3f(0f, -1f, 0f);
			startCapCenterIndex = num15;
			int num16 = num15 + 1;
			vertices[num16] = new Vector3d(0.0, circularSection2.SectionY, 0.0);
			uv[num16] = new Vector2f(0.5f, 0.5f);
			normals[num16] = new Vector3f(0f, 1f, 0f);
			endCapCenterIndex = num16;
			if (NoSharedVertices)
			{
				int num17 = num16 + 1;
				for (int m = 0; m < Slices; m++)
				{
					float num18 = (float)m * num6;
					double num19 = Math.Cos(num18);
					double num20 = Math.Sin(num18);
					vertices[num17 + m] = new Vector3d((double)circularSection.Radius * num19, circularSection.SectionY, (double)circularSection.Radius * num20);
					uv[num17 + m] = new Vector2f(0.5 * (1.0 + num19), 0.5 * (1.0 + num20));
					normals[num17 + m] = -Vector3f.AxisY;
				}
				append_disc(Slices, num15, num17, bClosed: true, Clockwise, ref tri_counter);
				int num21 = num17 + Slices;
				for (int n = 0; n < Slices; n++)
				{
					float num22 = (float)n * num6;
					double num23 = Math.Cos(num22);
					double num24 = Math.Sin(num22);
					vertices[num21 + n] = new Vector3d((double)circularSection2.Radius * num23, circularSection2.SectionY, (double)circularSection2.Radius * num24);
					uv[num21 + n] = new Vector2f(0.5 * (1.0 + num23), 0.5 * (1.0 + num24));
					normals[num21 + n] = Vector3f.AxisY;
				}
				append_disc(Slices, num16, num21, bClosed: true, !Clockwise, ref tri_counter);
			}
			else
			{
				append_disc(Slices, num15, 0, bClosed: true, Clockwise, ref tri_counter);
				append_disc(Slices, num16, num2 * (Sections.Length - 1), bClosed: true, !Clockwise, ref tri_counter);
			}
		}
		return this;
	}
}
