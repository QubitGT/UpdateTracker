using System;

namespace g3;

public struct WriteOptions
{
	public bool bWriteBinary;

	public bool bPerVertexNormals;

	public bool bPerVertexColors;

	public bool bPerVertexUVs;

	public bool bWriteGroups;

	public bool bCombineMeshes;

	public int RealPrecisionDigits;

	public bool bWriteMaterials;

	public string MaterialFilePath;

	public string groupNamePrefix;

	public Func<int, string> GroupNameF;

	public Action<int, int> ProgressFunc;

	public Func<string> AsciiHeaderFunc;

	public static readonly WriteOptions Defaults = new WriteOptions
	{
		bWriteBinary = false,
		bPerVertexNormals = false,
		bPerVertexColors = false,
		bWriteGroups = false,
		bPerVertexUVs = false,
		bCombineMeshes = false,
		bWriteMaterials = false,
		ProgressFunc = null,
		RealPrecisionDigits = 15
	};
}
