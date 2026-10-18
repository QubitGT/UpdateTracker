using Unity.Collections;

namespace UnityEngine.Rendering.Universal;

internal interface IEdgeStore
{
	NativeArray<ShadowEdge> GetOutsideEdges(NativeArray<Vector3> vertices, NativeArray<int> indices);
}
