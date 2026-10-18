using System;

namespace UnityEngine.Rendering.UnifiedRayTracing;

internal interface IRayTracingAccelStruct : IDisposable
{
	int AddInstance(MeshInstanceDesc meshInstance);

	void RemoveInstance(int instanceHandle);

	void ClearInstances();

	void UpdateInstanceTransform(int instanceHandle, Matrix4x4 localToWorldMatrix);

	void UpdateInstanceID(int instanceHandle, uint instanceID);

	void UpdateInstanceMask(int instanceHandle, uint mask);

	void Build(CommandBuffer cmd, GraphicsBuffer scratchBuffer);

	ulong GetBuildScratchBufferRequiredSizeInBytes();
}
