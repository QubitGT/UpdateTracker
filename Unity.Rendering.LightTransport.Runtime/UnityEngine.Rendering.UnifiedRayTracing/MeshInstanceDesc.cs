namespace UnityEngine.Rendering.UnifiedRayTracing;

internal struct MeshInstanceDesc
{
	public Mesh mesh;

	public int subMeshIndex;

	public Matrix4x4 localToWorldMatrix;

	public uint mask;

	public uint instanceID;

	public bool enableTriangleCulling;

	public bool frontTriangleCounterClockwise;

	public MeshInstanceDesc(Mesh mesh, int subMeshIndex = 0)
	{
		this.mesh = mesh;
		this.subMeshIndex = subMeshIndex;
		localToWorldMatrix = Matrix4x4.identity;
		mask = uint.MaxValue;
		instanceID = uint.MaxValue;
		enableTriangleCulling = true;
		frontTriangleCounterClockwise = false;
	}
}
