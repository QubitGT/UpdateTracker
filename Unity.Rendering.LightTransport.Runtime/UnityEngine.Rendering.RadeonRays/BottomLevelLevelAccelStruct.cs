namespace UnityEngine.Rendering.RadeonRays;

internal struct BottomLevelLevelAccelStruct
{
	public GraphicsBuffer bvh;

	public uint bvhOffset;

	public GraphicsBuffer bvhLeaves;

	public uint bvhLeavesOffset;
}
