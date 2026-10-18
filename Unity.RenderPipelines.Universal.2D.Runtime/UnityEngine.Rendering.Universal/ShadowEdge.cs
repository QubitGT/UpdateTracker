namespace UnityEngine.Rendering.Universal;

internal struct ShadowEdge
{
	public int v0;

	public int v1;

	public ShadowEdge(int indexA, int indexB)
	{
		v0 = indexA;
		v1 = indexB;
	}

	public void Reverse()
	{
		int num = v0;
		v0 = v1;
		v1 = num;
	}
}
