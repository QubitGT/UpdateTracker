using System;

namespace Voxels;

[Serializable]
public struct Voxel
{
	public byte Material;

	public byte Density;

	public Voxel(byte material, byte density)
	{
		Material = material;
		Density = density;
	}
}
