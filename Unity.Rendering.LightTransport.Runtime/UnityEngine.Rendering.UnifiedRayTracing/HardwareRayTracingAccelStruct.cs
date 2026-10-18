using System;
using System.Collections.Generic;

namespace UnityEngine.Rendering.UnifiedRayTracing;

internal sealed class HardwareRayTracingAccelStruct : IRayTracingAccelStruct, IDisposable
{
	private readonly Shader m_HWMaterialShader;

	private Material m_RayTracingMaterial;

	private readonly RayTracingAccelerationStructureBuildFlags m_BuildFlags;

	private readonly Dictionary<int, Mesh> m_Meshes = new Dictionary<int, Mesh>();

	private readonly ReferenceCounter m_Counter;

	public RayTracingAccelerationStructure accelStruct { get; }

	internal HardwareRayTracingAccelStruct(AccelerationStructureOptions options, Shader hwMaterialShader, ReferenceCounter counter, bool enableCompaction)
	{
		m_HWMaterialShader = hwMaterialShader;
		LoadRayTracingMaterial();
		m_BuildFlags = (RayTracingAccelerationStructureBuildFlags)options.buildFlags;
		accelStruct = new RayTracingAccelerationStructure(new RayTracingAccelerationStructure.Settings
		{
			rayTracingModeMask = RayTracingAccelerationStructure.RayTracingModeMask.Everything,
			managementMode = RayTracingAccelerationStructure.ManagementMode.Manual,
			enableCompaction = enableCompaction,
			layerMask = 255,
			buildFlagsStaticGeometries = m_BuildFlags
		});
		m_Counter = counter;
		m_Counter.Inc();
	}

	public void Dispose()
	{
		m_Counter.Dec();
		accelStruct?.Dispose();
		if (m_RayTracingMaterial != null)
		{
			Utils.Destroy(m_RayTracingMaterial);
		}
	}

	public int AddInstance(MeshInstanceDesc meshInstance)
	{
		LoadRayTracingMaterial();
		RayTracingMeshInstanceConfig config = new RayTracingMeshInstanceConfig(meshInstance.mesh, (uint)meshInstance.subMeshIndex, m_RayTracingMaterial);
		config.mask = meshInstance.mask;
		config.enableTriangleCulling = meshInstance.enableTriangleCulling;
		config.frontTriangleCounterClockwise = meshInstance.frontTriangleCounterClockwise;
		int num = accelStruct.AddInstance(in config, meshInstance.localToWorldMatrix, null, meshInstance.instanceID);
		m_Meshes.Add(num, meshInstance.mesh);
		return num;
	}

	public void RemoveInstance(int instanceHandle)
	{
		m_Meshes.Remove(instanceHandle);
		accelStruct.RemoveInstance(instanceHandle);
	}

	public void ClearInstances()
	{
		m_Meshes.Clear();
		accelStruct.ClearInstances();
	}

	public void UpdateInstanceTransform(int instanceHandle, Matrix4x4 localToWorldMatrix)
	{
		accelStruct.UpdateInstanceTransform(instanceHandle, localToWorldMatrix);
	}

	public void UpdateInstanceID(int instanceHandle, uint instanceID)
	{
		accelStruct.UpdateInstanceID(instanceHandle, instanceID);
	}

	public void UpdateInstanceMask(int instanceHandle, uint mask)
	{
		accelStruct.UpdateInstanceMask(instanceHandle, mask);
	}

	public void Build(CommandBuffer cmd, GraphicsBuffer scratchBuffer)
	{
		RayTracingAccelerationStructure.BuildSettings buildSettings = new RayTracingAccelerationStructure.BuildSettings();
		buildSettings.buildFlags = m_BuildFlags;
		buildSettings.relativeOrigin = Vector3.zero;
		RayTracingAccelerationStructure.BuildSettings buildSettings2 = buildSettings;
		cmd.BuildRayTracingAccelerationStructure(accelStruct, buildSettings2);
	}

	public ulong GetBuildScratchBufferRequiredSizeInBytes()
	{
		return 0uL;
	}

	private void LoadRayTracingMaterial()
	{
		if (m_RayTracingMaterial == null)
		{
			m_RayTracingMaterial = new Material(m_HWMaterialShader);
		}
	}
}
