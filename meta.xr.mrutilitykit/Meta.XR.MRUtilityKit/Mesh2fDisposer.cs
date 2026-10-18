using System;

namespace Meta.XR.MRUtilityKit;

internal struct Mesh2fDisposer : IDisposable
{
	public MRUKNativeFuncs.MrukMesh2f Mesh;

	internal Mesh2fDisposer(MRUKNativeFuncs.MrukMesh2f mesh)
	{
		Mesh = mesh;
	}

	public void Dispose()
	{
		MRUKNativeFuncs.FreeMesh(ref Mesh);
	}
}
