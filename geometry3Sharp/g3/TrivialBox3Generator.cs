using System;

namespace g3;

public class TrivialBox3Generator : MeshGenerator
{
	public Box3d Box = Box3d.UnitZeroCentered;

	public bool NoSharedVertices;

	public override MeshGenerator Generate()
	{
		vertices = new VectorArray3d(NoSharedVertices ? 24 : 8);
		uv = new VectorArray2f(vertices.Count);
		normals = new VectorArray3f(vertices.Count);
		triangles = new IndexArray3i(12);
		if (!NoSharedVertices)
		{
			for (int i = 0; i < 8; i++)
			{
				vertices[i] = Box.Corner(i);
				normals[i] = (Vector3f)(vertices[i] - Box.Center[i]).Normalized;
				uv[i] = Vector2f.Zero;
			}
			int num = 0;
			for (int j = 0; j < 6; j++)
			{
				triangles.Set(num++, gIndices.BoxFaces[j, 0], gIndices.BoxFaces[j, 1], gIndices.BoxFaces[j, 2], Clockwise);
				triangles.Set(num++, gIndices.BoxFaces[j, 0], gIndices.BoxFaces[j, 2], gIndices.BoxFaces[j, 3], Clockwise);
			}
		}
		else
		{
			int num2 = 0;
			int num3 = 0;
			Vector2f[] array = new Vector2f[4]
			{
				Vector2f.Zero,
				new Vector2f(1f, 0f),
				new Vector2f(1f, 1f),
				new Vector2f(0f, 1f)
			};
			for (int k = 0; k < 6; k++)
			{
				int num4 = num3++;
				num3 += 3;
				int value = gIndices.BoxFaceNormals[k];
				Vector3f value2 = (Vector3f)(Math.Sign(value) * Box.Axis(Math.Abs(value) - 1));
				for (int l = 0; l < 4; l++)
				{
					vertices[num4 + l] = Box.Corner(gIndices.BoxFaces[k, l]);
					normals[num4 + l] = value2;
					uv[num4 + l] = array[l];
				}
				triangles.Set(num2++, num4, num4 + 1, num4 + 2, Clockwise);
				triangles.Set(num2++, num4, num4 + 2, num4 + 3, Clockwise);
			}
		}
		return this;
	}
}
