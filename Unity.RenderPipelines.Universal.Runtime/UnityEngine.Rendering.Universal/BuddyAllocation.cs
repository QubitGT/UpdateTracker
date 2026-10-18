using Unity.Mathematics;

namespace UnityEngine.Rendering.Universal;

internal struct BuddyAllocation
{
	public int level;

	public int index;

	public uint2 index2D => SpaceFillingCurves.DecodeMorton2D((uint)index);

	public BuddyAllocation(int level, int index)
	{
		this.level = level;
		this.index = index;
	}
}
