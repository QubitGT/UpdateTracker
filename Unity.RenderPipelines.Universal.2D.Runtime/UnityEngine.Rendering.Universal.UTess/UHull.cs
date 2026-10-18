using Unity.Mathematics;

namespace UnityEngine.Rendering.Universal.UTess;

internal struct UHull
{
	public float2 a;

	public float2 b;

	public int idx;

	public ArraySlice<int> ilarray;

	public int ilcount;

	public ArraySlice<int> iuarray;

	public int iucount;
}
