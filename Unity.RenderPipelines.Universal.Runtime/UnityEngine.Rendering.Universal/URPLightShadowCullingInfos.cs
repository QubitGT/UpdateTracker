using Unity.Collections;

namespace UnityEngine.Rendering.Universal;

internal struct URPLightShadowCullingInfos
{
	public NativeArray<ShadowSliceData> slices;

	public uint slicesValidMask;

	public readonly bool IsSliceValid(int i)
	{
		return (slicesValidMask & (1 << i)) != 0;
	}
}
