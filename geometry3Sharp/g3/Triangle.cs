namespace g3;

internal struct Triangle
{
	public const int InvalidMaterialID = -1;

	public const int InvalidGroupID = -1;

	public Index3i vIndices;

	public Index3i vNormals;

	public Index3i vUVs;

	public int nMaterialID;

	public int nGroupID;

	public void clear()
	{
		nMaterialID = -1;
		nGroupID = -1;
		vIndices = (vNormals = (vUVs = new Index3i(-1, -1, -1)));
	}

	public void set_vertex(int j, int vi, int ni = -1, int ui = -1)
	{
		vIndices[j] = vi;
		if (ni != -1)
		{
			vNormals[j] = ni;
		}
		if (ui != -1)
		{
			vUVs[j] = ui;
		}
	}

	public void move_vertex(int jFrom, int jTo)
	{
		vIndices[jTo] = vIndices[jFrom];
		vNormals[jTo] = vNormals[jFrom];
		vUVs[jTo] = vUVs[jFrom];
	}

	public bool is_complex()
	{
		for (int i = 0; i < 3; i++)
		{
			if (vNormals[i] != -1 && vNormals[i] != vNormals[i])
			{
				return true;
			}
			if (vUVs[i] != -1 && vUVs[i] != vUVs[i])
			{
				return true;
			}
		}
		return false;
	}
}
