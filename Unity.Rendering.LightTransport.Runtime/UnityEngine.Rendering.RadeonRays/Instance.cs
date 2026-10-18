namespace UnityEngine.Rendering.RadeonRays;

internal struct Instance
{
	public uint meshAccelStructOffset;

	public uint instanceMask;

	public uint vertexOffset;

	public uint meshAccelStructLeavesOffset;

	public bool triangleCullingEnabled;

	public bool invertTriangleCulling;

	public uint userInstanceID;

	public Transform localToWorldTransform;
}
