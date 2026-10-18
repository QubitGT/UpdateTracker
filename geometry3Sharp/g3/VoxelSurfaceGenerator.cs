using System;
using System.Collections.Generic;

namespace g3;

public class VoxelSurfaceGenerator
{
	public IBinaryVoxelGrid Voxels;

	public bool SkipInteriorFaces = true;

	public bool CapAtBoundary = true;

	public bool Clockwise;

	public Func<Vector3i, Colorf> ColorSourceF;

	public int MaxMeshElementCount = int.MaxValue;

	public List<DMesh3> Meshes;

	private DMesh3 cur_mesh;

	private void append_mesh()
	{
		if (Meshes == null || Meshes.Count == 0)
		{
			Meshes = new List<DMesh3>();
		}
		cur_mesh = new DMesh3(MeshComponents.VertexNormals);
		if (ColorSourceF != null)
		{
			cur_mesh.EnableVertexColors(Colorf.White);
		}
		Meshes.Add(cur_mesh);
	}

	private void check_counts_or_append(int newV, int newT)
	{
		if (cur_mesh.MaxVertexID + newV >= MaxMeshElementCount || cur_mesh.MaxTriangleID + newT >= MaxMeshElementCount)
		{
			append_mesh();
		}
	}

	public void Generate()
	{
		append_mesh();
		AxisAlignedBox3i gridBounds = Voxels.GridBounds;
		gridBounds.Max -= Vector3i.One;
		int[] array = new int[4];
		foreach (Vector3i item in Voxels.NonZeros())
		{
			check_counts_or_append(6, 2);
			Box3d unitZeroCentered = Box3d.UnitZeroCentered;
			unitZeroCentered.Center = (Vector3d)item;
			for (int i = 0; i < 6; i++)
			{
				Index3i index3i = item + gIndices.GridOffsets6[i];
				if (gridBounds.Contains(index3i))
				{
					if (SkipInteriorFaces && Voxels.Get(index3i))
					{
						continue;
					}
				}
				else if (!CapAtBoundary)
				{
					continue;
				}
				int value = gIndices.BoxFaceNormals[i];
				Vector3f n = (Vector3f)(Math.Sign(value) * unitZeroCentered.Axis(Math.Abs(value) - 1));
				NewVertexInfo info = new NewVertexInfo(Vector3d.Zero, n);
				if (ColorSourceF != null)
				{
					info.c = ColorSourceF(item);
					info.bHaveC = true;
				}
				for (int j = 0; j < 4; j++)
				{
					info.v = unitZeroCentered.Corner(gIndices.BoxFaces[i, j]);
					array[j] = cur_mesh.AppendVertex(info);
				}
				Index3i tv = new Index3i(array[0], array[1], array[2], Clockwise);
				Index3i tv2 = new Index3i(array[0], array[2], array[3], Clockwise);
				cur_mesh.AppendTriangle(tv);
				cur_mesh.AppendTriangle(tv2);
			}
		}
	}
}
