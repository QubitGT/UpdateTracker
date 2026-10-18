using System;
using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace UnityEngine.Rendering.RadeonRays;

internal class RadeonRaysAPI : IDisposable
{
	private readonly HlbvhBuilder buildBvh;

	private readonly HlbvhTopLevelBuilder buildTopLevelBvh;

	private readonly RestructureBvh restructureBvh;

	public const GraphicsBuffer.Target BufferTarget = GraphicsBuffer.Target.Structured;

	public RadeonRaysAPI(RadeonRaysShaders shaders)
	{
		buildBvh = new HlbvhBuilder(shaders);
		buildTopLevelBvh = new HlbvhTopLevelBuilder(shaders);
		restructureBvh = new RestructureBvh(shaders);
	}

	public void Dispose()
	{
		restructureBvh.Dispose();
	}

	public static int BvhInternalNodeSizeInDwords()
	{
		return Marshal.SizeOf<BvhNode>() / 4;
	}

	public static int BvhInternalNodeSizeInBytes()
	{
		return Marshal.SizeOf<BvhNode>();
	}

	public static int BvhLeafNodeSizeInBytes()
	{
		return Marshal.SizeOf<uint4>();
	}

	public static int BvhLeafNodeSizeInDwords()
	{
		return Marshal.SizeOf<uint4>() / 4;
	}

	public void BuildMeshAccelStruct(CommandBuffer cmd, MeshBuildInfo buildInfo, BuildFlags buildFlags, GraphicsBuffer scratchBuffer, in BottomLevelLevelAccelStruct result)
	{
		if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal)
		{
			buildFlags |= BuildFlags.PreferFastBuild;
		}
		buildBvh.Execute(cmd, buildInfo.vertices, buildInfo.verticesStartOffset, buildInfo.vertexStride, buildInfo.triangleIndices, buildInfo.indicesStartOffset, buildInfo.baseIndex, buildInfo.indexFormat, buildInfo.triangleCount, scratchBuffer, in result);
		if ((buildFlags & BuildFlags.PreferFastBuild) == 0)
		{
			restructureBvh.Execute(cmd, buildInfo.vertices, buildInfo.verticesStartOffset, buildInfo.vertexStride, buildInfo.triangleCount, scratchBuffer, in result);
		}
	}

	public MeshBuildMemoryRequirements GetMeshBuildMemoryRequirements(MeshBuildInfo buildInfo, BuildFlags buildFlags)
	{
		if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal)
		{
			buildFlags |= BuildFlags.PreferFastBuild;
		}
		MeshBuildMemoryRequirements result = default(MeshBuildMemoryRequirements);
		result.bvhSizeInDwords = buildBvh.GetResultDataSizeInDwords(buildInfo.triangleCount);
		result.bvhLeavesSizeInDwords = (ulong)(buildInfo.triangleCount * BvhLeafNodeSizeInDwords());
		result.buildScratchSizeInDwords = buildBvh.GetScratchDataSizeInDwords(buildInfo.triangleCount);
		ulong y = (((buildFlags & BuildFlags.PreferFastBuild) == 0) ? restructureBvh.GetScratchDataSizeInDwords(buildInfo.triangleCount) : 0);
		result.buildScratchSizeInDwords = math.max(result.buildScratchSizeInDwords, y);
		return result;
	}

	public TopLevelAccelStruct BuildSceneAccelStruct(CommandBuffer cmd, GraphicsBuffer meshAccelStructsBuffer, Instance[] instances, GraphicsBuffer scratchBuffer)
	{
		TopLevelAccelStruct accelStruct = default(TopLevelAccelStruct);
		if (instances.Length == 0)
		{
			buildTopLevelBvh.CreateEmpty(ref accelStruct);
			return accelStruct;
		}
		buildTopLevelBvh.AllocateResultBuffers((uint)instances.Length, ref accelStruct);
		InstanceInfo[] array = new InstanceInfo[instances.Length];
		for (uint num = 0u; num < instances.Length; num++)
		{
			array[num] = new InstanceInfo
			{
				blasOffset = (int)instances[num].meshAccelStructOffset,
				instanceMask = (int)instances[num].instanceMask,
				vertexOffset = (int)instances[num].vertexOffset,
				indexOffset = (int)instances[num].meshAccelStructLeavesOffset,
				localToWorldTransform = instances[num].localToWorldTransform,
				triangleCullingEnabled = (instances[num].triangleCullingEnabled ? 1 : 0),
				invertTriangleCulling = (instances[num].invertTriangleCulling ? 1 : 0),
				userInstanceID = instances[num].userInstanceID
			};
		}
		accelStruct.instanceInfos.SetData(array);
		accelStruct.bottomLevelBvhs = meshAccelStructsBuffer;
		accelStruct.instanceCount = (uint)instances.Length;
		buildTopLevelBvh.Execute(cmd, scratchBuffer, ref accelStruct);
		return accelStruct;
	}

	public TopLevelAccelStruct CreateSceneAccelStructBuffers(GraphicsBuffer meshAccelStructsBuffer, uint tlasSizeInDwords, Instance[] instances)
	{
		TopLevelAccelStruct accelStruct = default(TopLevelAccelStruct);
		if (instances.Length == 0)
		{
			buildTopLevelBvh.CreateEmpty(ref accelStruct);
			return accelStruct;
		}
		InstanceInfo[] array = new InstanceInfo[instances.Length];
		for (uint num = 0u; num < instances.Length; num++)
		{
			array[num] = new InstanceInfo
			{
				blasOffset = (int)instances[num].meshAccelStructOffset,
				instanceMask = (int)instances[num].instanceMask,
				vertexOffset = (int)instances[num].vertexOffset,
				indexOffset = (int)instances[num].meshAccelStructLeavesOffset,
				localToWorldTransform = instances[num].localToWorldTransform,
				triangleCullingEnabled = (instances[num].triangleCullingEnabled ? 1 : 0),
				invertTriangleCulling = (instances[num].invertTriangleCulling ? 1 : 0),
				userInstanceID = instances[num].userInstanceID,
				worldToLocalTransform = instances[num].localToWorldTransform.Inverse()
			};
		}
		accelStruct.instanceInfos = new GraphicsBuffer(GraphicsBuffer.Target.Structured, instances.Length, Marshal.SizeOf<InstanceInfo>());
		accelStruct.instanceInfos.SetData(array);
		accelStruct.bottomLevelBvhs = meshAccelStructsBuffer;
		accelStruct.topLevelBvh = new GraphicsBuffer(GraphicsBuffer.Target.Structured, (int)tlasSizeInDwords / BvhInternalNodeSizeInDwords(), Marshal.SizeOf<BvhNode>());
		accelStruct.instanceCount = (uint)instances.Length;
		return accelStruct;
	}

	public SceneBuildMemoryRequirements GetSceneBuildMemoryRequirements(uint instanceCount)
	{
		SceneBuildMemoryRequirements result = default(SceneBuildMemoryRequirements);
		result.buildScratchSizeInDwords = buildTopLevelBvh.GetScratchDataSizeInDwords(instanceCount);
		return result;
	}

	public SceneMemoryRequirements GetSceneMemoryRequirements(MeshBuildInfo[] buildInfos, BuildFlags buildFlags)
	{
		if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal)
		{
			buildFlags |= BuildFlags.PreferFastBuild;
		}
		SceneMemoryRequirements sceneMemoryRequirements = new SceneMemoryRequirements();
		sceneMemoryRequirements.buildScratchSizeInDwords = 0uL;
		sceneMemoryRequirements.bottomLevelBvhSizeInNodes = new ulong[buildInfos.Length];
		sceneMemoryRequirements.bottomLevelBvhOffsetInNodes = new uint[buildInfos.Length];
		sceneMemoryRequirements.bottomLevelBvhLeavesSizeInNodes = new ulong[buildInfos.Length];
		sceneMemoryRequirements.bottomLevelBvhLeavesOffsetInNodes = new uint[buildInfos.Length];
		int num = 0;
		uint num2 = 0u;
		uint num3 = 0u;
		foreach (MeshBuildInfo buildInfo in buildInfos)
		{
			MeshBuildMemoryRequirements meshBuildMemoryRequirements = GetMeshBuildMemoryRequirements(buildInfo, buildFlags);
			sceneMemoryRequirements.buildScratchSizeInDwords = math.max(sceneMemoryRequirements.buildScratchSizeInDwords, meshBuildMemoryRequirements.buildScratchSizeInDwords);
			sceneMemoryRequirements.bottomLevelBvhSizeInNodes[num] = meshBuildMemoryRequirements.bvhSizeInDwords / (ulong)BvhInternalNodeSizeInDwords();
			sceneMemoryRequirements.bottomLevelBvhOffsetInNodes[num] = num2;
			sceneMemoryRequirements.bottomLevelBvhLeavesSizeInNodes[num] = meshBuildMemoryRequirements.bvhLeavesSizeInDwords / (ulong)BvhLeafNodeSizeInDwords();
			sceneMemoryRequirements.bottomLevelBvhLeavesOffsetInNodes[num] = num3;
			num2 += (uint)(int)(meshBuildMemoryRequirements.bvhSizeInDwords / (ulong)BvhInternalNodeSizeInDwords());
			num3 += (uint)(int)(meshBuildMemoryRequirements.bvhLeavesSizeInDwords / (ulong)BvhLeafNodeSizeInDwords());
			num++;
		}
		sceneMemoryRequirements.totalBottomLevelBvhSizeInNodes = num2;
		sceneMemoryRequirements.totalBottomLevelBvhLeavesSizeInNodes = num3;
		ulong scratchDataSizeInDwords = buildTopLevelBvh.GetScratchDataSizeInDwords((uint)buildInfos.Length);
		sceneMemoryRequirements.buildScratchSizeInDwords = math.max(sceneMemoryRequirements.buildScratchSizeInDwords, scratchDataSizeInDwords);
		return sceneMemoryRequirements;
	}

	public static ulong GetTraceMemoryRequirements(uint rayCount)
	{
		return 64 * rayCount;
	}
}
