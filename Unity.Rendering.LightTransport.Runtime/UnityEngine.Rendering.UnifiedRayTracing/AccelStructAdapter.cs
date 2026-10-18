using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace UnityEngine.Rendering.UnifiedRayTracing;

internal sealed class AccelStructAdapter : IDisposable
{
	private struct InstanceIDs
	{
		public int InstanceID;

		public int AccelStructID;
	}

	private IRayTracingAccelStruct _accelStruct;

	private AccelStructInstances _instances;

	private readonly Dictionary<int, InstanceIDs[]> _objectHandleToInstances;

	internal AccelStructInstances Instances => _instances;

	public GeometryPool GeometryPool => _instances.geometryPool;

	public AccelStructAdapter(IRayTracingAccelStruct accelStruct, GeometryPool geometryPool)
	{
		_objectHandleToInstances = new Dictionary<int, InstanceIDs[]>();
		base._002Ector();
		_accelStruct = accelStruct;
		_instances = new AccelStructInstances(geometryPool);
	}

	public AccelStructAdapter(IRayTracingAccelStruct accelStruct, RayTracingResources resources)
	{
		GeometryPoolDesc desc = GeometryPoolDesc.NewDefault();
		this._002Ector(accelStruct, new GeometryPool(in desc, resources.geometryPoolKernels, resources.copyBuffer));
	}

	public IRayTracingAccelStruct GetAccelerationStructure()
	{
		return _accelStruct;
	}

	public void Bind(CommandBuffer cmd, string propertyName, IRayTracingShader shader)
	{
		shader.SetAccelerationStructure(cmd, propertyName, _accelStruct);
		_instances.Bind(cmd, shader);
	}

	public void Dispose()
	{
		_instances?.Dispose();
		_instances = null;
		_accelStruct?.Dispose();
		_accelStruct = null;
		_objectHandleToInstances.Clear();
	}

	public void AddInstance(int objectHandle, Component meshRendererOrTerrain, Span<uint> perSubMeshMask, Span<uint> perSubMeshMaterialIDs, uint renderingLayerMask)
	{
		if (meshRendererOrTerrain is Terrain terrain)
		{
			TerrainDesc terrainDesc = default(TerrainDesc);
			terrainDesc.terrain = terrain;
			terrainDesc.localToWorldMatrix = terrain.transform.localToWorldMatrix;
			terrainDesc.mask = perSubMeshMask[0];
			terrainDesc.renderingLayerMask = renderingLayerMask;
			terrainDesc.materialID = perSubMeshMaterialIDs[0];
			terrainDesc.enableTriangleCulling = true;
			terrainDesc.frontTriangleCounterClockwise = false;
			AddInstance(objectHandle, terrainDesc);
		}
		else
		{
			MeshRenderer meshRenderer = (MeshRenderer)meshRendererOrTerrain;
			Mesh sharedMesh = meshRenderer.GetComponent<MeshFilter>().sharedMesh;
			AddInstance(objectHandle, sharedMesh, meshRenderer.transform.localToWorldMatrix, perSubMeshMask, perSubMeshMaterialIDs, renderingLayerMask);
		}
	}

	public void AddInstance(int objectHandle, Mesh mesh, Matrix4x4 localToWorldMatrix, Span<uint> perSubMeshMask, Span<uint> perSubMeshMaterialIDs, uint renderingLayerMask)
	{
		int subMeshCount = mesh.subMeshCount;
		InstanceIDs[] array = new InstanceIDs[subMeshCount];
		for (int i = 0; i < subMeshCount; i++)
		{
			MeshInstanceDesc meshInstanceDesc = new MeshInstanceDesc(mesh, i);
			meshInstanceDesc.localToWorldMatrix = localToWorldMatrix;
			meshInstanceDesc.mask = perSubMeshMask[i];
			MeshInstanceDesc meshInstance = meshInstanceDesc;
			array[i].InstanceID = _instances.AddInstance(meshInstance, perSubMeshMaterialIDs[i], renderingLayerMask);
			meshInstance.instanceID = (uint)array[i].InstanceID;
			array[i].AccelStructID = _accelStruct.AddInstance(meshInstance);
		}
		_objectHandleToInstances.Add(objectHandle, array);
	}

	private void AddInstance(int objectHandle, TerrainDesc terrainDesc)
	{
		List<InstanceIDs> instanceHandles = new List<InstanceIDs>();
		AddHeightmap(terrainDesc, ref instanceHandles);
		AddTrees(terrainDesc, ref instanceHandles);
		_objectHandleToInstances.Add(objectHandle, instanceHandles.ToArray());
	}

	private void AddHeightmap(TerrainDesc terrainDesc, ref List<InstanceIDs> instanceHandles)
	{
		Mesh mesh = TerrainToMesh.Convert(terrainDesc.terrain);
		MeshInstanceDesc instanceDesc = new MeshInstanceDesc(mesh);
		instanceDesc.localToWorldMatrix = terrainDesc.localToWorldMatrix;
		instanceDesc.mask = terrainDesc.mask;
		instanceDesc.enableTriangleCulling = terrainDesc.enableTriangleCulling;
		instanceDesc.frontTriangleCounterClockwise = terrainDesc.frontTriangleCounterClockwise;
		instanceHandles.Add(AddInstance(instanceDesc, terrainDesc.materialID, terrainDesc.renderingLayerMask));
	}

	private void AddTrees(TerrainDesc terrainDesc, ref List<InstanceIDs> instanceHandles)
	{
		TerrainData terrainData = terrainDesc.terrain.terrainData;
		float4x4 float4x = terrainDesc.localToWorldMatrix;
		float3 @float = new float3(terrainData.heightmapResolution, 1f, terrainData.heightmapResolution) * terrainData.heightmapScale;
		float3 float2 = new float3(float4x[3].x, float4x[3].y, float4x[3].z);
		TreeInstance[] treeInstances = terrainData.treeInstances;
		for (int i = 0; i < treeInstances.Length; i++)
		{
			TreeInstance treeInstance = treeInstances[i];
			Matrix4x4 localToWorldMatrix = Matrix4x4.TRS(float2 + new float3(treeInstance.position) * @float, Quaternion.AngleAxis(treeInstance.rotation, Vector3.up), new Vector3(treeInstance.widthScale, treeInstance.heightScale, treeInstance.widthScale));
			GameObject prefab = terrainData.treePrototypes[treeInstance.prototypeIndex].prefab;
			GameObject gameObject = prefab.gameObject;
			if (prefab.TryGetComponent<LODGroup>(out var component))
			{
				LOD[] lODs = component.GetLODs();
				if (lODs.Length != 0 && lODs[0].renderers.Length != 0)
				{
					gameObject = (lODs[0].renderers[0] as MeshRenderer).gameObject;
				}
			}
			if (gameObject.TryGetComponent<MeshFilter>(out var component2))
			{
				Mesh sharedMesh = component2.sharedMesh;
				for (int j = 0; j < sharedMesh.subMeshCount; j++)
				{
					MeshInstanceDesc instanceDesc = new MeshInstanceDesc(sharedMesh, j);
					instanceDesc.localToWorldMatrix = localToWorldMatrix;
					instanceDesc.mask = terrainDesc.mask;
					instanceDesc.enableTriangleCulling = terrainDesc.enableTriangleCulling;
					instanceDesc.frontTriangleCounterClockwise = terrainDesc.frontTriangleCounterClockwise;
					instanceHandles.Add(AddInstance(instanceDesc, terrainDesc.materialID, (uint)(1 << prefab.gameObject.layer)));
				}
			}
		}
	}

	private InstanceIDs AddInstance(MeshInstanceDesc instanceDesc, uint materialID, uint renderingLayerMask)
	{
		InstanceIDs result = default(InstanceIDs);
		result.InstanceID = _instances.AddInstance(instanceDesc, materialID, renderingLayerMask);
		instanceDesc.instanceID = (uint)result.InstanceID;
		result.AccelStructID = _accelStruct.AddInstance(instanceDesc);
		return result;
	}

	public void RemoveInstance(int objectHandle)
	{
		_objectHandleToInstances.TryGetValue(objectHandle, out var value);
		InstanceIDs[] array = value;
		for (int i = 0; i < array.Length; i++)
		{
			InstanceIDs instanceIDs = array[i];
			_instances.RemoveInstance(instanceIDs.InstanceID);
			_accelStruct.RemoveInstance(instanceIDs.AccelStructID);
		}
		_objectHandleToInstances.Remove(objectHandle);
	}

	public void UpdateInstanceTransform(int objectHandle, Matrix4x4 localToWorldMatrix)
	{
		_objectHandleToInstances.TryGetValue(objectHandle, out var value);
		InstanceIDs[] array = value;
		for (int i = 0; i < array.Length; i++)
		{
			InstanceIDs instanceIDs = array[i];
			_instances.UpdateInstanceTransform(instanceIDs.InstanceID, localToWorldMatrix);
			_accelStruct.UpdateInstanceTransform(instanceIDs.AccelStructID, localToWorldMatrix);
		}
	}

	public void UpdateInstanceMaterialIDs(int objectHandle, Span<uint> perSubMeshMaterialIDs)
	{
		_objectHandleToInstances.TryGetValue(objectHandle, out var value);
		int num = 0;
		InstanceIDs[] array = value;
		for (int i = 0; i < array.Length; i++)
		{
			InstanceIDs instanceIDs = array[i];
			_instances.UpdateInstanceMaterialID(instanceIDs.InstanceID, perSubMeshMaterialIDs[num++]);
		}
	}

	public void UpdateInstanceMask(int objectHandle, Span<uint> perSubMeshMask)
	{
		_objectHandleToInstances.TryGetValue(objectHandle, out var value);
		int num = 0;
		InstanceIDs[] array = value;
		for (int i = 0; i < array.Length; i++)
		{
			InstanceIDs instanceIDs = array[i];
			_instances.UpdateInstanceMask(instanceIDs.InstanceID, perSubMeshMask[num]);
			_accelStruct.UpdateInstanceMask(instanceIDs.AccelStructID, perSubMeshMask[num]);
			num++;
		}
	}

	public void UpdateInstanceMask(int objectHandle, uint mask)
	{
		_objectHandleToInstances.TryGetValue(objectHandle, out var value);
		uint[] array = new uint[value.Length];
		Array.Fill(array, mask);
		int num = 0;
		InstanceIDs[] array2 = value;
		for (int i = 0; i < array2.Length; i++)
		{
			InstanceIDs instanceIDs = array2[i];
			_instances.UpdateInstanceMask(instanceIDs.InstanceID, array[num]);
			_accelStruct.UpdateInstanceMask(instanceIDs.AccelStructID, array[num]);
			num++;
		}
	}

	public void Build(CommandBuffer cmd, ref GraphicsBuffer scratchBuffer)
	{
		RayTracingHelper.ResizeScratchBufferForBuild(_accelStruct, ref scratchBuffer);
		_accelStruct.Build(cmd, scratchBuffer);
	}

	public void NextFrame()
	{
		_instances.NextFrame();
	}

	public bool GetInstanceIDs(int rendererID, out int[] instanceIDs)
	{
		if (!_objectHandleToInstances.TryGetValue(rendererID, out var value))
		{
			instanceIDs = null;
			return false;
		}
		instanceIDs = Array.ConvertAll(value, (InstanceIDs item) => item.InstanceID);
		return true;
	}
}
