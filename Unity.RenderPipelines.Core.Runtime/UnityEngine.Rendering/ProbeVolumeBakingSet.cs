using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.IO.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine.Serialization;

namespace UnityEngine.Rendering;

public sealed class ProbeVolumeBakingSet : ScriptableObject, ISerializationCallbackReceiver
{
	internal enum Version
	{
		Initial,
		RemoveProbeVolumeSceneData,
		AssetsAlwaysReferenced
	}

	[Serializable]
	internal class PerScenarioDataInfo
	{
		public int sceneHash;

		public ProbeVolumeStreamableAsset cellDataAsset;

		public ProbeVolumeStreamableAsset cellOptionalDataAsset;

		public ProbeVolumeStreamableAsset cellProbeOcclusionDataAsset;

		private bool m_HasValidData;

		public void Initialize(ProbeVolumeSHBands shBands)
		{
			m_HasValidData = ComputeHasValidData(shBands);
		}

		public bool IsValid()
		{
			if (cellDataAsset != null)
			{
				return cellDataAsset.IsValid();
			}
			return false;
		}

		public bool HasValidData(ProbeVolumeSHBands shBands)
		{
			return m_HasValidData;
		}

		public bool ComputeHasValidData(ProbeVolumeSHBands shBands)
		{
			if (cellDataAsset.FileExists())
			{
				if (shBands != ProbeVolumeSHBands.SphericalHarmonicsL1)
				{
					return cellOptionalDataAsset.FileExists();
				}
				return true;
			}
			return false;
		}
	}

	[Serializable]
	internal struct CellCounts
	{
		public int bricksCount;

		public int chunksCount;

		public void Add(CellCounts o)
		{
			bricksCount += o.bricksCount;
			chunksCount += o.chunksCount;
		}
	}

	[Serializable]
	private struct SerializedPerSceneCellList
	{
		public string sceneGUID;

		public List<int> cellList;
	}

	[Serializable]
	internal struct ProbeLayerMask
	{
		public RenderingLayerMask mask;

		public string name;
	}

	[SerializeField]
	internal bool singleSceneMode = true;

	[SerializeField]
	internal bool dialogNoProbeVolumeInSetShown;

	[SerializeField]
	internal ProbeVolumeBakingProcessSettings settings;

	[SerializeField]
	private List<string> m_SceneGUIDs = new List<string>();

	[SerializeField]
	[Obsolete("This is now contained in the SceneBakeData structure")]
	[FormerlySerializedAs("scenesToNotBake")]
	internal List<string> obsoleteScenesToNotBake = new List<string>();

	[SerializeField]
	[FormerlySerializedAs("lightingScenarios")]
	internal List<string> m_LightingScenarios = new List<string>();

	[SerializeField]
	internal SerializedDictionary<int, ProbeReferenceVolume.CellDesc> cellDescs = new SerializedDictionary<int, ProbeReferenceVolume.CellDesc>();

	internal Dictionary<int, ProbeReferenceVolume.CellData> cellDataMap = new Dictionary<int, ProbeReferenceVolume.CellData>();

	private List<int> m_TotalIndexList = new List<int>();

	[SerializeField]
	private List<SerializedPerSceneCellList> m_SerializedPerSceneCellList;

	internal Dictionary<string, List<int>> perSceneCellLists = new Dictionary<string, List<int>>();

	[SerializeField]
	internal ProbeVolumeStreamableAsset cellSharedDataAsset;

	[SerializeField]
	internal SerializedDictionary<string, PerScenarioDataInfo> scenarios = new SerializedDictionary<string, PerScenarioDataInfo>();

	[SerializeField]
	internal ProbeVolumeStreamableAsset cellBricksDataAsset;

	[SerializeField]
	internal ProbeVolumeStreamableAsset cellSupportDataAsset;

	[SerializeField]
	internal int chunkSizeInBricks;

	[SerializeField]
	internal Vector3Int maxCellPosition;

	[SerializeField]
	internal Vector3Int minCellPosition;

	[SerializeField]
	internal Bounds globalBounds;

	[SerializeField]
	internal int bakedSimplificationLevels = -1;

	[SerializeField]
	internal float bakedMinDistanceBetweenProbes = -1f;

	[SerializeField]
	internal bool bakedProbeOcclusion;

	[SerializeField]
	internal int bakedSkyOcclusionValue = -1;

	[SerializeField]
	internal int bakedSkyShadingDirectionValue = -1;

	[SerializeField]
	internal Vector3 bakedProbeOffset = Vector3.zero;

	[SerializeField]
	internal int bakedMaskCount = 1;

	[SerializeField]
	internal uint4 bakedLayerMasks;

	[SerializeField]
	internal int maxSHChunkCount = -1;

	[SerializeField]
	internal int L0ChunkSize;

	[SerializeField]
	internal int L1ChunkSize;

	[SerializeField]
	internal int L2TextureChunkSize;

	[SerializeField]
	internal int ProbeOcclusionChunkSize;

	[SerializeField]
	internal int sharedValidityMaskChunkSize;

	[SerializeField]
	internal int sharedSkyOcclusionL0L1ChunkSize;

	[SerializeField]
	internal int sharedSkyShadingDirectionIndicesChunkSize;

	[SerializeField]
	internal int sharedDataChunkSize;

	[SerializeField]
	internal int supportPositionChunkSize;

	[SerializeField]
	internal int supportValidityChunkSize;

	[SerializeField]
	internal int supportTouchupChunkSize;

	[SerializeField]
	internal int supportLayerMaskChunkSize;

	[SerializeField]
	internal int supportOffsetsChunkSize;

	[SerializeField]
	internal int supportDataChunkSize;

	[SerializeField]
	internal string lightingScenario = ProbeReferenceVolume.defaultLightingScenario;

	private string m_OtherScenario;

	private float m_ScenarioBlendingFactor;

	private ReadCommandArray m_ReadCommandArray;

	private NativeArray<ReadCommand> m_ReadCommandBuffer;

	private Stack<NativeArray<byte>> m_ReadOperationScratchBuffers = new Stack<NativeArray<byte>>();

	private List<int> m_PrunedIndexList = new List<int>();

	private List<int> m_PrunedScenarioIndexList = new List<int>();

	internal const int k_MaxSkyOcclusionBakingSamples = 8192;

	[SerializeField]
	private Version version = CoreUtils.GetLastEnumValue<Version>();

	[SerializeField]
	internal bool freezePlacement;

	[SerializeField]
	public Vector3 probeOffset = Vector3.zero;

	[Range(2f, 5f)]
	public int simplificationLevels = 3;

	[Min(0.1f)]
	public float minDistanceBetweenProbes = 1f;

	public LayerMask renderersLayerMask = -1;

	[Min(0f)]
	public float minRendererVolumeSize = 0.1f;

	public bool skyOcclusion;

	[Logarithmic(1, 8192)]
	public int skyOcclusionBakingSamples = 2048;

	[Range(0f, 5f)]
	public int skyOcclusionBakingBounces = 2;

	[Range(0f, 1f)]
	public float skyOcclusionAverageAlbedo = 0.6f;

	public bool skyOcclusionBackFaceCulling;

	public bool skyOcclusionShadingDirection;

	[SerializeField]
	internal bool useRenderingLayers;

	[SerializeField]
	internal ProbeLayerMask[] renderingLayerMasks;

	private bool m_HasSupportData;

	private bool m_SharedDataIsValid;

	private bool m_UseStreamingAsset = true;

	internal bool hasDilation
	{
		get
		{
			if (settings.dilationSettings.enableDilation)
			{
				return settings.dilationSettings.dilationDistance > 0f;
			}
			return false;
		}
	}

	public IReadOnlyList<string> sceneGUIDs => m_SceneGUIDs;

	public IReadOnlyList<string> lightingScenarios => m_LightingScenarios;

	internal bool bakedSkyOcclusion
	{
		get
		{
			if (bakedSkyOcclusionValue > 0)
			{
				return true;
			}
			return false;
		}
		set
		{
			bakedSkyOcclusionValue = (value ? 1 : 0);
		}
	}

	internal bool bakedSkyShadingDirection
	{
		get
		{
			if (bakedSkyShadingDirectionValue > 0)
			{
				return true;
			}
			return false;
		}
		set
		{
			bakedSkyShadingDirectionValue = (value ? 1 : 0);
		}
	}

	internal string otherScenario => m_OtherScenario;

	internal float scenarioBlendingFactor => m_ScenarioBlendingFactor;

	public int cellSizeInBricks => GetCellSizeInBricks(bakedSimplificationLevels);

	public int maxSubdivision => GetMaxSubdivision(bakedSimplificationLevels);

	public float minBrickSize => GetMinBrickSize(bakedMinDistanceBetweenProbes);

	public float cellSizeInMeters => (float)cellSizeInBricks * minBrickSize;

	internal uint4 ComputeRegionMasks()
	{
		uint4 result = 0u;
		if (!useRenderingLayers || renderingLayerMasks == null)
		{
			result.x = uint.MaxValue;
		}
		else
		{
			for (int i = 0; i < renderingLayerMasks.Length; i++)
			{
				result[i] = renderingLayerMasks[i].mask;
			}
		}
		return result;
	}

	internal static int GetCellSizeInBricks(int simplificationLevels)
	{
		return (int)Mathf.Pow(3f, simplificationLevels);
	}

	internal static int GetMaxSubdivision(int simplificationLevels)
	{
		return simplificationLevels + 1;
	}

	internal static float GetMinBrickSize(float minDistanceBetweenProbes)
	{
		return Mathf.Max(0.01f, minDistanceBetweenProbes * 3f);
	}

	private void OnValidate()
	{
		singleSceneMode &= m_SceneGUIDs.Count <= 1;
		if (m_LightingScenarios.Count == 0)
		{
			m_LightingScenarios = new List<string> { ProbeReferenceVolume.defaultLightingScenario };
		}
		settings.Upgrade();
	}

	private void OnEnable()
	{
		Migrate();
		m_HasSupportData = ComputeHasSupportData();
		m_SharedDataIsValid = ComputeHasValidSharedData();
	}

	internal void Migrate()
	{
		if (version != CoreUtils.GetLastEnumValue<Version>())
		{
			_ = version;
			_ = 1;
			if (version < Version.AssetsAlwaysReferenced)
			{
				_ = ProbeReferenceVolume.instance.isInitialized;
			}
		}
		if (sharedValidityMaskChunkSize == 0)
		{
			sharedValidityMaskChunkSize = ProbeBrickPool.GetChunkSizeInProbeCount();
		}
		if (settings.virtualOffsetSettings.validityThreshold == 0f)
		{
			settings.virtualOffsetSettings.validityThreshold = 0.25f;
		}
	}

	private bool ComputeHasValidSharedData()
	{
		if (cellSharedDataAsset != null && cellSharedDataAsset.FileExists())
		{
			return cellBricksDataAsset.FileExists();
		}
		return false;
	}

	internal bool HasValidSharedData()
	{
		return m_SharedDataIsValid;
	}

	internal bool CheckCompatibleCellLayout()
	{
		if (simplificationLevels == bakedSimplificationLevels && minDistanceBetweenProbes == bakedMinDistanceBetweenProbes && skyOcclusion == bakedSkyOcclusion && skyOcclusionShadingDirection == bakedSkyShadingDirection && settings.virtualOffsetSettings.useVirtualOffset == (supportOffsetsChunkSize != 0))
		{
			return useRenderingLayers == (bakedMaskCount != 1);
		}
		return false;
	}

	private bool ComputeHasSupportData()
	{
		if (cellSupportDataAsset != null && cellSupportDataAsset.IsValid())
		{
			return cellSupportDataAsset.FileExists();
		}
		return false;
	}

	internal bool HasSupportData()
	{
		return m_HasSupportData;
	}

	public bool HasBakedData(string scenario = null)
	{
		if (scenario == null)
		{
			return scenarios.ContainsKey(ProbeReferenceVolume.defaultLightingScenario);
		}
		if (!ProbeReferenceVolume.instance.supportLightingScenarios && scenario != ProbeReferenceVolume.defaultLightingScenario)
		{
			return false;
		}
		return scenarios.ContainsKey(scenario);
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		if (!m_LightingScenarios.Contains(lightingScenario))
		{
			if (m_LightingScenarios.Count != 0)
			{
				lightingScenario = m_LightingScenarios[0];
			}
			else
			{
				lightingScenario = ProbeReferenceVolume.defaultLightingScenario;
			}
		}
		perSceneCellLists.Clear();
		foreach (SerializedPerSceneCellList serializedPerSceneCell in m_SerializedPerSceneCellList)
		{
			perSceneCellLists.Add(serializedPerSceneCell.sceneGUID, serializedPerSceneCell.cellList);
		}
		if (m_OtherScenario == "")
		{
			m_OtherScenario = null;
		}
		if (bakedSimplificationLevels == -1)
		{
			bakedSimplificationLevels = simplificationLevels;
			bakedMinDistanceBetweenProbes = minDistanceBetweenProbes;
		}
		if (bakedSkyOcclusionValue == -1)
		{
			bakedSkyOcclusion = false;
		}
		if (bakedSkyShadingDirectionValue == -1)
		{
			bakedSkyShadingDirection = false;
		}
		if (cellDescs.Count == 0)
		{
			return;
		}
		Dictionary<int, ProbeReferenceVolume.CellDesc>.ValueCollection.Enumerator enumerator2 = cellDescs.Values.GetEnumerator();
		enumerator2.MoveNext();
		if (enumerator2.Current.bricksCount != 0)
		{
			return;
		}
		foreach (ProbeReferenceVolume.CellDesc value in cellDescs.Values)
		{
			value.bricksCount = value.probeCount / 64;
		}
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
		m_SerializedPerSceneCellList = new List<SerializedPerSceneCellList>();
		foreach (KeyValuePair<string, List<int>> perSceneCellList in perSceneCellLists)
		{
			m_SerializedPerSceneCellList.Add(new SerializedPerSceneCellList
			{
				sceneGUID = perSceneCellList.Key,
				cellList = perSceneCellList.Value
			});
		}
	}

	internal void Initialize(bool useStreamingAsset)
	{
		foreach (KeyValuePair<string, PerScenarioDataInfo> scenario in scenarios)
		{
			scenario.Value.Initialize(ProbeReferenceVolume.instance.shBands);
		}
		if (!useStreamingAsset)
		{
			m_UseStreamingAsset = false;
			m_TotalIndexList.Clear();
			foreach (int key in cellDescs.Keys)
			{
				m_TotalIndexList.Add(key);
			}
			ResolveAllCellData();
		}
		if (ProbeReferenceVolume.instance.supportScenarioBlending)
		{
			BlendLightingScenario(null, 0f);
		}
	}

	internal void Cleanup()
	{
		if (cellSharedDataAsset != null)
		{
			cellSharedDataAsset.Dispose();
			foreach (KeyValuePair<string, PerScenarioDataInfo> scenario in scenarios)
			{
				if (scenario.Value.IsValid())
				{
					scenario.Value.cellDataAsset.Dispose();
					scenario.Value.cellOptionalDataAsset.Dispose();
					scenario.Value.cellProbeOcclusionDataAsset.Dispose();
				}
			}
		}
		if (m_ReadCommandBuffer.IsCreated)
		{
			m_ReadCommandBuffer.Dispose();
		}
		foreach (NativeArray<byte> readOperationScratchBuffer in m_ReadOperationScratchBuffers)
		{
			readOperationScratchBuffer.Dispose();
		}
		m_ReadOperationScratchBuffers.Clear();
	}

	internal void SetActiveScenario(string scenario, bool verbose = true)
	{
		if (lightingScenario == scenario)
		{
			return;
		}
		if (!m_LightingScenarios.Contains(scenario))
		{
			if (verbose)
			{
				Debug.LogError("Scenario '" + scenario + "' does not exist.");
			}
			return;
		}
		if (!scenarios.ContainsKey(scenario) && verbose)
		{
			Debug.LogError("Scenario '" + scenario + "' has not been baked.");
		}
		lightingScenario = scenario;
		m_ScenarioBlendingFactor = 0f;
		if (ProbeReferenceVolume.instance.supportScenarioBlending)
		{
			ProbeReferenceVolume.instance.ScenarioBlendingChanged(scenarioChanged: true);
		}
		else
		{
			ProbeReferenceVolume.instance.UnloadAllCells();
		}
	}

	internal void BlendLightingScenario(string otherScenario, float blendingFactor)
	{
		if (!string.IsNullOrEmpty(otherScenario) && !ProbeReferenceVolume.instance.supportScenarioBlending)
		{
			return;
		}
		if (otherScenario != null && !m_LightingScenarios.Contains(otherScenario))
		{
			Debug.LogError("Scenario '" + otherScenario + "' does not exist.");
			return;
		}
		if (otherScenario != null && !scenarios.ContainsKey(otherScenario))
		{
			Debug.LogError("Scenario '" + otherScenario + "' has not been baked.");
			return;
		}
		blendingFactor = Mathf.Clamp01(blendingFactor);
		if (otherScenario == lightingScenario || string.IsNullOrEmpty(otherScenario))
		{
			otherScenario = null;
		}
		if (otherScenario == null)
		{
			blendingFactor = 0f;
		}
		if (!(otherScenario == m_OtherScenario) || !Mathf.Approximately(blendingFactor, m_ScenarioBlendingFactor))
		{
			bool scenarioChanged = otherScenario != m_OtherScenario;
			m_OtherScenario = otherScenario;
			m_ScenarioBlendingFactor = blendingFactor;
			ProbeReferenceVolume.instance.ScenarioBlendingChanged(scenarioChanged);
		}
	}

	internal int GetBakingHashCode()
	{
		return ((((maxCellPosition.GetHashCode() * 23 + minCellPosition.GetHashCode()) * 23 + globalBounds.GetHashCode()) * 23 + cellSizeInBricks.GetHashCode()) * 23 + simplificationLevels.GetHashCode()) * 23 + minDistanceBetweenProbes.GetHashCode();
	}

	private static int AlignUp16(int count)
	{
		int num = 16;
		int num2 = count % num;
		return count + ((num2 != 0) ? (num - num2) : 0);
	}

	private NativeArray<T> GetSubArray<T>(NativeArray<byte> input, int count, ref int offset) where T : struct
	{
		int num = count * UnsafeUtility.SizeOf<T>();
		if (offset + num > input.Length)
		{
			return default(NativeArray<T>);
		}
		NativeArray<T> result = input.GetSubArray(offset, num).Reinterpret<T>(1);
		offset = AlignUp16(offset + num);
		return result;
	}

	private NativeArray<byte> RequestScratchBuffer(int size)
	{
		if (m_ReadOperationScratchBuffers.Count == 0)
		{
			return new NativeArray<byte>(size, Allocator.Persistent);
		}
		NativeArray<byte> result = m_ReadOperationScratchBuffers.Pop();
		if (result.Length < size)
		{
			result.Dispose();
			return new NativeArray<byte>(size, Allocator.Persistent);
		}
		return result;
	}

	private unsafe bool FileExists(string path)
	{
		FileInfoResult fileInfoResult = default(FileInfoResult);
		AsyncReadManager.GetFileInfo(path, &fileInfoResult).JobHandle.Complete();
		return fileInfoResult.FileState == FileState.Exists;
	}

	private unsafe NativeArray<T> LoadStreambleAssetData<T>(ProbeVolumeStreamableAsset asset, List<int> cellIndices) where T : struct
	{
		if (!m_UseStreamingAsset)
		{
			return asset.asset.GetData<byte>().Reinterpret<T>(1);
		}
		if (!FileExists(asset.GetAssetPath()))
		{
			asset.RefreshAssetPath();
			if (!FileExists(asset.GetAssetPath()))
			{
				if (asset.HasValidAssetReference())
				{
					return asset.asset.GetData<byte>().Reinterpret<T>(1);
				}
				return default(NativeArray<T>);
			}
		}
		if (!m_ReadCommandBuffer.IsCreated || m_ReadCommandBuffer.Length < cellIndices.Count)
		{
			if (m_ReadCommandBuffer.IsCreated)
			{
				m_ReadCommandBuffer.Dispose();
			}
			m_ReadCommandBuffer = new NativeArray<ReadCommand>(cellIndices.Count, Allocator.Persistent);
		}
		int num = 0;
		int num2 = 0;
		foreach (int cellIndex in cellIndices)
		{
			_ = cellDescs[cellIndex];
			ProbeVolumeStreamableAsset.StreamableCellDesc streamableCellDesc = asset.streamableCellDescs[cellIndex];
			ReadCommand value = default(ReadCommand);
			value.Offset = streamableCellDesc.offset;
			value.Size = streamableCellDesc.elementCount * asset.elementSize;
			value.Buffer = null;
			m_ReadCommandBuffer[num2++] = value;
			num += (int)value.Size;
		}
		NativeArray<byte> nativeArray = RequestScratchBuffer(num);
		num2 = 0;
		long num3 = 0L;
		byte* unsafePtr = (byte*)nativeArray.GetUnsafePtr();
		foreach (int cellIndex2 in cellIndices)
		{
			_ = cellIndex2;
			ReadCommand value2 = m_ReadCommandBuffer[num2];
			value2.Buffer = unsafePtr + num3;
			num3 += value2.Size;
			m_ReadCommandBuffer[num2++] = value2;
		}
		m_ReadCommandArray.CommandCount = cellIndices.Count;
		m_ReadCommandArray.ReadCommands = (ReadCommand*)m_ReadCommandBuffer.GetUnsafePtr();
		FileHandle fileHandle = asset.OpenFile();
		ReadHandle readHandle = AsyncReadManager.Read(in fileHandle, m_ReadCommandArray);
		readHandle.JobHandle.Complete();
		asset.CloseFile();
		readHandle.Dispose();
		return nativeArray.Reinterpret<T>(1);
	}

	private void ReleaseStreamableAssetData<T>(NativeArray<T> buffer) where T : struct
	{
		if (m_UseStreamingAsset)
		{
			m_ReadOperationScratchBuffers.Push(buffer.Reinterpret<byte>(UnsafeUtility.SizeOf<T>()));
		}
	}

	private void PruneCellIndexList(List<int> cellIndices, List<int> prunedIndexList)
	{
		prunedIndexList.Clear();
		foreach (int cellIndex in cellIndices)
		{
			if (!cellDataMap.ContainsKey(cellIndex))
			{
				prunedIndexList.Add(cellIndex);
			}
		}
	}

	private void PruneCellIndexListForScenario(List<int> cellIndices, PerScenarioDataInfo scenarioData, List<int> prunedIndexList)
	{
		prunedIndexList.Clear();
		foreach (int cellIndex in cellIndices)
		{
			if (scenarioData.cellDataAsset.streamableCellDescs.ContainsKey(cellIndex))
			{
				prunedIndexList.Add(cellIndex);
			}
		}
	}

	internal List<int> GetSceneCellIndexList(string sceneGUID)
	{
		if (perSceneCellLists.TryGetValue(sceneGUID, out var value))
		{
			return value;
		}
		return null;
	}

	private bool ResolveAllCellData()
	{
		if (ResolveSharedCellData(m_TotalIndexList))
		{
			return ResolvePerScenarioCellData(m_TotalIndexList);
		}
		return false;
	}

	internal bool ResolveCellData(List<int> cellIndices)
	{
		if (!m_UseStreamingAsset)
		{
			return true;
		}
		if (cellIndices == null)
		{
			return false;
		}
		PruneCellIndexList(cellIndices, m_PrunedIndexList);
		if (ProbeReferenceVolume.instance.diskStreamingEnabled)
		{
			foreach (int prunedIndex in m_PrunedIndexList)
			{
				ProbeReferenceVolume.CellData cellData = new ProbeReferenceVolume.CellData();
				foreach (KeyValuePair<string, PerScenarioDataInfo> scenario in scenarios)
				{
					cellData.scenarios.Add(scenario.Key, default(ProbeReferenceVolume.CellData.PerScenarioData));
				}
				cellDataMap.Add(prunedIndex, cellData);
			}
			return true;
		}
		if (ResolveSharedCellData(m_PrunedIndexList))
		{
			return ResolvePerScenarioCellData(m_PrunedIndexList);
		}
		return false;
	}

	private void ResolveSharedCellData(List<int> cellIndices, NativeArray<ProbeBrickIndex.Brick> bricksData, NativeArray<byte> cellSharedData, NativeArray<byte> cellSupportData)
	{
		ProbeReferenceVolume instance = ProbeReferenceVolume.instance;
		bool flag = cellSupportData.Length != 0;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		for (int i = 0; i < cellIndices.Count; i++)
		{
			int key = cellIndices[i];
			ProbeReferenceVolume.CellData cellData = new ProbeReferenceVolume.CellData();
			ProbeReferenceVolume.CellDesc cellDesc = cellDescs[key];
			int bricksCount = cellDesc.bricksCount;
			int shChunkCount = cellDesc.shChunkCount;
			NativeArray<ProbeBrickIndex.Brick> subArray = bricksData.GetSubArray(num3, bricksCount);
			NativeArray<byte> subArray2 = cellSharedData.GetSubArray(num, sharedValidityMaskChunkSize * shChunkCount);
			num += sharedValidityMaskChunkSize * shChunkCount;
			cellData.bricks = (m_UseStreamingAsset ? new NativeArray<ProbeBrickIndex.Brick>(subArray, Allocator.Persistent) : subArray);
			cellData.validityNeighMaskData = (m_UseStreamingAsset ? new NativeArray<byte>(subArray2, Allocator.Persistent) : subArray2);
			if (bakedSkyOcclusion)
			{
				if (instance.skyOcclusion)
				{
					NativeArray<ushort> nativeArray = cellSharedData.GetSubArray(num, sharedSkyOcclusionL0L1ChunkSize * shChunkCount).Reinterpret<ushort>(1);
					cellData.skyOcclusionDataL0L1 = (m_UseStreamingAsset ? new NativeArray<ushort>(nativeArray, Allocator.Persistent) : nativeArray);
				}
				num += sharedSkyOcclusionL0L1ChunkSize * shChunkCount;
				if (bakedSkyShadingDirection)
				{
					if (instance.skyOcclusion && instance.skyOcclusionShadingDirection)
					{
						NativeArray<byte> subArray3 = cellSharedData.GetSubArray(num, sharedSkyShadingDirectionIndicesChunkSize * shChunkCount);
						cellData.skyShadingDirectionIndices = (m_UseStreamingAsset ? new NativeArray<byte>(subArray3, Allocator.Persistent) : subArray3);
					}
					num += sharedSkyShadingDirectionIndicesChunkSize * shChunkCount;
				}
			}
			if (flag)
			{
				NativeArray<Vector3> nativeArray2 = cellSupportData.GetSubArray(num2, shChunkCount * supportPositionChunkSize).Reinterpret<Vector3>(1);
				num2 += shChunkCount * supportPositionChunkSize;
				cellData.probePositions = (m_UseStreamingAsset ? new NativeArray<Vector3>(nativeArray2, Allocator.Persistent) : nativeArray2);
				NativeArray<float> nativeArray3 = cellSupportData.GetSubArray(num2, shChunkCount * supportValidityChunkSize).Reinterpret<float>(1);
				num2 += shChunkCount * supportValidityChunkSize;
				cellData.validity = (m_UseStreamingAsset ? new NativeArray<float>(nativeArray3, Allocator.Persistent) : nativeArray3);
				NativeArray<float> nativeArray4 = cellSupportData.GetSubArray(num2, shChunkCount * supportTouchupChunkSize).Reinterpret<float>(1);
				num2 += shChunkCount * supportTouchupChunkSize;
				cellData.touchupVolumeInteraction = (m_UseStreamingAsset ? new NativeArray<float>(nativeArray4, Allocator.Persistent) : nativeArray4);
				if (supportLayerMaskChunkSize != 0)
				{
					NativeArray<byte> nativeArray5 = cellSupportData.GetSubArray(num2, shChunkCount * supportLayerMaskChunkSize).Reinterpret<byte>(1);
					num2 += shChunkCount * supportLayerMaskChunkSize;
					cellData.layer = (m_UseStreamingAsset ? new NativeArray<byte>(nativeArray5, Allocator.Persistent) : nativeArray5);
				}
				if (supportOffsetsChunkSize != 0)
				{
					NativeArray<Vector3> nativeArray6 = cellSupportData.GetSubArray(num2, shChunkCount * supportOffsetsChunkSize).Reinterpret<Vector3>(1);
					num2 += shChunkCount * supportOffsetsChunkSize;
					cellData.offsetVectors = (m_UseStreamingAsset ? new NativeArray<Vector3>(nativeArray6, Allocator.Persistent) : nativeArray6);
				}
			}
			cellDataMap.Add(key, cellData);
			num3 += bricksCount;
			num4 += shChunkCount;
		}
	}

	internal bool ResolveSharedCellData(List<int> cellIndices)
	{
		if (cellSharedDataAsset == null || !cellSharedDataAsset.IsValid())
		{
			return false;
		}
		if (!HasValidSharedData())
		{
			Debug.LogError("One or more data file missing for baking set " + base.name + ". Cannot load shared data.");
			return false;
		}
		NativeArray<byte> nativeArray = LoadStreambleAssetData<byte>(cellSharedDataAsset, cellIndices);
		NativeArray<ProbeBrickIndex.Brick> nativeArray2 = LoadStreambleAssetData<ProbeBrickIndex.Brick>(cellBricksDataAsset, cellIndices);
		bool num = HasSupportData();
		NativeArray<byte> nativeArray3 = (num ? LoadStreambleAssetData<byte>(cellSupportDataAsset, cellIndices) : default(NativeArray<byte>));
		ResolveSharedCellData(cellIndices, nativeArray2, nativeArray, nativeArray3);
		ReleaseStreamableAssetData(nativeArray);
		ReleaseStreamableAssetData(nativeArray2);
		if (num)
		{
			ReleaseStreamableAssetData(nativeArray3);
		}
		return true;
	}

	internal bool ResolvePerScenarioCellData(List<int> cellIndices)
	{
		bool flag = ProbeReferenceVolume.instance.shBands == ProbeVolumeSHBands.SphericalHarmonicsL2;
		foreach (KeyValuePair<string, PerScenarioDataInfo> scenario in scenarios)
		{
			string key = scenario.Key;
			PerScenarioDataInfo value = scenario.Value;
			PruneCellIndexListForScenario(cellIndices, value, m_PrunedScenarioIndexList);
			if (!value.HasValidData(ProbeReferenceVolume.instance.shBands))
			{
				Debug.LogError("One or more data file missing for baking set " + key + " scenario " + lightingScenario + ". Cannot load scenario data.");
				return false;
			}
			NativeArray<byte> nativeArray = LoadStreambleAssetData<byte>(value.cellDataAsset, m_PrunedScenarioIndexList);
			NativeArray<byte> nativeArray2 = (flag ? LoadStreambleAssetData<byte>(value.cellOptionalDataAsset, m_PrunedScenarioIndexList) : default(NativeArray<byte>));
			NativeArray<byte> nativeArray3 = (bakedProbeOcclusion ? LoadStreambleAssetData<byte>(value.cellProbeOcclusionDataAsset, m_PrunedScenarioIndexList) : default(NativeArray<byte>));
			if (!ResolvePerScenarioCellData(nativeArray, nativeArray2, nativeArray3, key, m_PrunedScenarioIndexList))
			{
				Debug.LogError("Baked data for scenario '" + key + "' cannot be loaded.");
				return false;
			}
			ReleaseStreamableAssetData(nativeArray);
			if (flag)
			{
				ReleaseStreamableAssetData(nativeArray2);
			}
			if (bakedProbeOcclusion)
			{
				ReleaseStreamableAssetData(nativeArray3);
			}
		}
		return true;
	}

	internal bool ResolvePerScenarioCellData(NativeArray<byte> cellData, NativeArray<byte> cellOptionalData, NativeArray<byte> cellProbeOcclusionData, string scenario, List<int> cellIndices)
	{
		if (!cellData.IsCreated)
		{
			return false;
		}
		bool isCreated = cellOptionalData.IsCreated;
		bool flag = cellProbeOcclusionData.IsCreated && cellProbeOcclusionData.Length > 0;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		for (int i = 0; i < cellIndices.Count; i++)
		{
			int key = cellIndices[i];
			ProbeReferenceVolume.CellData cellData2 = cellDataMap[key];
			ProbeReferenceVolume.CellDesc cellDesc = cellDescs[key];
			ProbeReferenceVolume.CellData.PerScenarioData value = default(ProbeReferenceVolume.CellData.PerScenarioData);
			int shChunkCount = cellDesc.shChunkCount;
			NativeArray<ushort> nativeArray = cellData.GetSubArray(num, L0ChunkSize * shChunkCount).Reinterpret<ushort>(1);
			NativeArray<byte> subArray = cellData.GetSubArray(num + L0ChunkSize * shChunkCount, L1ChunkSize * shChunkCount);
			NativeArray<byte> subArray2 = cellData.GetSubArray(num + (L0ChunkSize + L1ChunkSize) * shChunkCount, L1ChunkSize * shChunkCount);
			value.shL0L1RxData = (m_UseStreamingAsset ? new NativeArray<ushort>(nativeArray, Allocator.Persistent) : nativeArray);
			value.shL1GL1RyData = (m_UseStreamingAsset ? new NativeArray<byte>(subArray, Allocator.Persistent) : subArray);
			value.shL1BL1RzData = (m_UseStreamingAsset ? new NativeArray<byte>(subArray2, Allocator.Persistent) : subArray2);
			if (isCreated)
			{
				int num4 = shChunkCount * L2TextureChunkSize;
				NativeArray<byte> subArray3 = cellOptionalData.GetSubArray(num2, num4);
				NativeArray<byte> subArray4 = cellOptionalData.GetSubArray(num2 + num4, num4);
				NativeArray<byte> subArray5 = cellOptionalData.GetSubArray(num2 + num4 * 2, num4);
				NativeArray<byte> subArray6 = cellOptionalData.GetSubArray(num2 + num4 * 3, num4);
				value.shL2Data_0 = (m_UseStreamingAsset ? new NativeArray<byte>(subArray3, Allocator.Persistent) : subArray3);
				value.shL2Data_1 = (m_UseStreamingAsset ? new NativeArray<byte>(subArray4, Allocator.Persistent) : subArray4);
				value.shL2Data_2 = (m_UseStreamingAsset ? new NativeArray<byte>(subArray5, Allocator.Persistent) : subArray5);
				value.shL2Data_3 = (m_UseStreamingAsset ? new NativeArray<byte>(subArray6, Allocator.Persistent) : subArray6);
			}
			if (flag)
			{
				NativeArray<byte> subArray7 = cellProbeOcclusionData.GetSubArray(num3, ProbeOcclusionChunkSize * shChunkCount);
				value.probeOcclusion = (m_UseStreamingAsset ? new NativeArray<byte>(subArray7, Allocator.Persistent) : subArray7);
			}
			num += (L0ChunkSize + 2 * L1ChunkSize) * shChunkCount;
			num2 += L2TextureChunkSize * 4 * shChunkCount;
			num3 += ProbeOcclusionChunkSize * shChunkCount;
			cellData2.scenarios.Add(scenario, value);
		}
		return true;
	}

	internal void ReleaseCell(int cellIndex)
	{
		cellDataMap[cellIndex].Cleanup(cleanScenarioList: true);
		cellDataMap.Remove(cellIndex);
	}

	internal ProbeReferenceVolume.CellDesc GetCellDesc(int cellIndex)
	{
		if (cellDescs.TryGetValue(cellIndex, out var value))
		{
			return value;
		}
		return null;
	}

	internal ProbeReferenceVolume.CellData GetCellData(int cellIndex)
	{
		if (cellDataMap.TryGetValue(cellIndex, out var value))
		{
			return value;
		}
		return null;
	}

	internal int GetChunkGPUMemory(ProbeVolumeSHBands shBands)
	{
		int num = L0ChunkSize + 2 * L1ChunkSize + sharedDataChunkSize;
		if (shBands == ProbeVolumeSHBands.SphericalHarmonicsL2)
		{
			num += 4 * L2TextureChunkSize;
		}
		if (bakedProbeOcclusion)
		{
			num += ProbeOcclusionChunkSize;
		}
		return num;
	}

	internal bool HasSameSceneGUIDs(ProbeVolumeBakingSet other)
	{
		IReadOnlyList<string> readOnlyList = other.sceneGUIDs;
		if (m_SceneGUIDs.Count != readOnlyList.Count)
		{
			return false;
		}
		for (int i = 0; i < m_SceneGUIDs.Count; i++)
		{
			if (m_SceneGUIDs[i] != readOnlyList[i])
			{
				return false;
			}
		}
		return true;
	}
}
