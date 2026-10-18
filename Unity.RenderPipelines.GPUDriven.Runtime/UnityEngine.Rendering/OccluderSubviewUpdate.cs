namespace UnityEngine.Rendering;

public struct OccluderSubviewUpdate
{
	public int subviewIndex;

	public int depthSliceIndex;

	public Vector2Int depthOffset;

	public Matrix4x4 viewMatrix;

	public Matrix4x4 invViewMatrix;

	public Matrix4x4 gpuProjMatrix;

	public Vector3 viewOffsetWorldSpace;

	public OccluderSubviewUpdate(int subviewIndex)
	{
		this.subviewIndex = subviewIndex;
		depthSliceIndex = 0;
		depthOffset = Vector2Int.zero;
		viewMatrix = Matrix4x4.identity;
		invViewMatrix = Matrix4x4.identity;
		gpuProjMatrix = Matrix4x4.identity;
		viewOffsetWorldSpace = Vector3.zero;
	}
}
