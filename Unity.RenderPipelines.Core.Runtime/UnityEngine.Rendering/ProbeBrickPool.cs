using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering;

internal class ProbeBrickPool
{
	[DebuggerDisplay("Chunk ({x}, {y}, {z})")]
	public struct BrickChunkAlloc
	{
		public int x;

		public int y;

		public int z;

		internal int flattenIndex(int sx, int sy)
		{
			return z * (sx * sy) + y * sx + x;
		}
	}

	public struct DataLocation
	{
		internal Texture TexL0_L1rx;

		internal Texture TexL1_G_ry;

		internal Texture TexL1_B_rz;

		internal Texture TexL2_0;

		internal Texture TexL2_1;

		internal Texture TexL2_2;

		internal Texture TexL2_3;

		internal Texture TexProbeOcclusion;

		internal Texture TexValidity;

		internal Texture TexSkyOcclusion;

		internal Texture TexSkyShadingDirectionIndices;

		internal int width;

		internal int height;

		internal int depth;

		internal void Cleanup()
		{
			CoreUtils.Destroy(TexL0_L1rx);
			CoreUtils.Destroy(TexL1_G_ry);
			CoreUtils.Destroy(TexL1_B_rz);
			CoreUtils.Destroy(TexL2_0);
			CoreUtils.Destroy(TexL2_1);
			CoreUtils.Destroy(TexL2_2);
			CoreUtils.Destroy(TexL2_3);
			CoreUtils.Destroy(TexProbeOcclusion);
			CoreUtils.Destroy(TexValidity);
			CoreUtils.Destroy(TexSkyOcclusion);
			CoreUtils.Destroy(TexSkyShadingDirectionIndices);
			TexL0_L1rx = null;
			TexL1_G_ry = null;
			TexL1_B_rz = null;
			TexL2_0 = null;
			TexL2_1 = null;
			TexL2_2 = null;
			TexL2_3 = null;
			TexProbeOcclusion = null;
			TexValidity = null;
			TexSkyOcclusion = null;
			TexSkyShadingDirectionIndices = null;
		}
	}

	internal static readonly int _Out_L0_L1Rx = Shader.PropertyToID("_Out_L0_L1Rx");

	internal static readonly int _Out_L1G_L1Ry = Shader.PropertyToID("_Out_L1G_L1Ry");

	internal static readonly int _Out_L1B_L1Rz = Shader.PropertyToID("_Out_L1B_L1Rz");

	internal static readonly int _Out_Shared = Shader.PropertyToID("_Out_Shared");

	internal static readonly int _Out_ProbeOcclusion = Shader.PropertyToID("_Out_ProbeOcclusion");

	internal static readonly int _Out_SkyOcclusionL0L1 = Shader.PropertyToID("_Out_SkyOcclusionL0L1");

	internal static readonly int _Out_SkyShadingDirectionIndices = Shader.PropertyToID("_Out_SkyShadingDirectionIndices");

	internal static readonly int _Out_L2_0 = Shader.PropertyToID("_Out_L2_0");

	internal static readonly int _Out_L2_1 = Shader.PropertyToID("_Out_L2_1");

	internal static readonly int _Out_L2_2 = Shader.PropertyToID("_Out_L2_2");

	internal static readonly int _Out_L2_3 = Shader.PropertyToID("_Out_L2_3");

	internal static readonly int _ProbeVolumeScratchBufferLayout = Shader.PropertyToID("CellStreamingScratchBufferLayout");

	internal static readonly int _ProbeVolumeScratchBuffer = Shader.PropertyToID("_ScratchBuffer");

	private const int kChunkSizeInBricks = 128;

	internal const int kBrickCellCount = 3;

	internal const int kBrickProbeCountPerDim = 4;

	internal const int kBrickProbeCountTotal = 64;

	internal const int kChunkProbeCountPerDim = 512;

	private const int kMaxPoolWidth = 2048;

	internal DataLocation m_Pool;

	private BrickChunkAlloc m_NextFreeChunk;

	private Stack<BrickChunkAlloc> m_FreeList;

	private int m_AvailableChunkCount;

	private ProbeVolumeSHBands m_SHBands;

	private bool m_ContainsValidity;

	private bool m_ContainsProbeOcclusion;

	private bool m_ContainsRenderingLayers;

	private bool m_ContainsSkyOcclusion;

	private bool m_ContainsSkyShadingDirection;

	private static ComputeShader s_DataUploadCS;

	private static int s_DataUploadKernel;

	private static ComputeShader s_DataUploadL2CS;

	private static int s_DataUploadL2Kernel;

	private static LocalKeyword s_DataUpload_Shared;

	private static LocalKeyword s_DataUpload_ProbeOcclusion;

	private static LocalKeyword s_DataUpload_SkyOcclusion;

	private static LocalKeyword s_DataUpload_SkyShadingDirection;

	internal int estimatedVMemCost { get; private set; }

	internal static int DivRoundUp(int x, int y)
	{
		return (x + y - 1) / y;
	}

	internal static void Initialize()
	{
		if (SystemInfo.supportsComputeShaders)
		{
			s_DataUploadCS = GraphicsSettings.GetRenderPipelineSettings<ProbeVolumeRuntimeResources>()?.probeVolumeUploadDataCS;
			s_DataUploadL2CS = GraphicsSettings.GetRenderPipelineSettings<ProbeVolumeRuntimeResources>()?.probeVolumeUploadDataL2CS;
			if (s_DataUploadCS != null)
			{
				s_DataUploadKernel = (s_DataUploadCS ? s_DataUploadCS.FindKernel("UploadData") : (-1));
				s_DataUpload_Shared = new LocalKeyword(s_DataUploadCS, "PROBE_VOLUMES_SHARED_DATA");
				s_DataUpload_ProbeOcclusion = new LocalKeyword(s_DataUploadCS, "PROBE_VOLUMES_PROBE_OCCLUSION");
				s_DataUpload_SkyOcclusion = new LocalKeyword(s_DataUploadCS, "PROBE_VOLUMES_SKY_OCCLUSION");
				s_DataUpload_SkyShadingDirection = new LocalKeyword(s_DataUploadCS, "PROBE_VOLUMES_SKY_SHADING_DIRECTION");
			}
			if (s_DataUploadL2CS != null)
			{
				s_DataUploadL2Kernel = (s_DataUploadL2CS ? s_DataUploadL2CS.FindKernel("UploadDataL2") : (-1));
			}
		}
	}

	internal Texture GetValidityTexture()
	{
		return m_Pool.TexValidity;
	}

	internal Texture GetSkyOcclusionTexture()
	{
		return m_Pool.TexSkyOcclusion;
	}

	internal Texture GetSkyShadingDirectionIndicesTexture()
	{
		return m_Pool.TexSkyShadingDirectionIndices;
	}

	internal Texture GetProbeOcclusionTexture()
	{
		return m_Pool.TexProbeOcclusion;
	}

	internal ProbeBrickPool(ProbeVolumeTextureMemoryBudget memoryBudget, ProbeVolumeSHBands shBands, bool allocateValidityData = false, bool allocateRenderingLayerData = false, bool allocateSkyOcclusion = false, bool allocateSkyShadingData = false, bool allocateProbeOcclusionData = false)
	{
		m_NextFreeChunk.x = (m_NextFreeChunk.y = (m_NextFreeChunk.z = 0));
		m_SHBands = shBands;
		m_ContainsValidity = allocateValidityData;
		m_ContainsProbeOcclusion = allocateProbeOcclusionData;
		m_ContainsRenderingLayers = allocateRenderingLayerData;
		m_ContainsSkyOcclusion = allocateSkyOcclusion;
		m_ContainsSkyShadingDirection = allocateSkyShadingData;
		m_FreeList = new Stack<BrickChunkAlloc>(256);
		DerivePoolSizeFromBudget(memoryBudget, out var width, out var height, out var depth);
		AllocatePool(width, height, depth);
		m_AvailableChunkCount = m_Pool.width / 512 * (m_Pool.height / 4) * (m_Pool.depth / 4);
	}

	internal void AllocatePool(int width, int height, int depth)
	{
		m_Pool = CreateDataLocation(width * height * depth, compressed: false, m_SHBands, "APV", allocateRendertexture: true, m_ContainsValidity, m_ContainsRenderingLayers, m_ContainsSkyOcclusion, m_ContainsSkyShadingDirection, m_ContainsProbeOcclusion, out var allocatedBytes);
		estimatedVMemCost = allocatedBytes;
	}

	public int GetRemainingChunkCount()
	{
		return m_AvailableChunkCount;
	}

	internal void EnsureTextureValidity()
	{
		if (m_Pool.TexL0_L1rx == null)
		{
			m_Pool.Cleanup();
			AllocatePool(m_Pool.width, m_Pool.height, m_Pool.depth);
		}
	}

	internal bool EnsureTextureValidity(bool renderingLayers, bool skyOcclusion, bool skyDirection, bool probeOcclusion)
	{
		if (m_ContainsRenderingLayers != renderingLayers || m_ContainsSkyOcclusion != skyOcclusion || m_ContainsSkyShadingDirection != skyDirection || m_ContainsProbeOcclusion != probeOcclusion)
		{
			m_Pool.Cleanup();
			m_ContainsRenderingLayers = renderingLayers;
			m_ContainsSkyOcclusion = skyOcclusion;
			m_ContainsSkyShadingDirection = skyDirection;
			m_ContainsProbeOcclusion = probeOcclusion;
			AllocatePool(m_Pool.width, m_Pool.height, m_Pool.depth);
			return false;
		}
		return true;
	}

	internal static int GetChunkSizeInBrickCount()
	{
		return 128;
	}

	internal static int GetChunkSizeInProbeCount()
	{
		return 8192;
	}

	internal int GetPoolWidth()
	{
		return m_Pool.width;
	}

	internal int GetPoolHeight()
	{
		return m_Pool.height;
	}

	internal Vector3Int GetPoolDimensions()
	{
		return new Vector3Int(m_Pool.width, m_Pool.height, m_Pool.depth);
	}

	internal void GetRuntimeResources(ref ProbeReferenceVolume.RuntimeResources rr)
	{
		rr.L0_L1rx = m_Pool.TexL0_L1rx as RenderTexture;
		rr.L1_G_ry = m_Pool.TexL1_G_ry as RenderTexture;
		rr.L1_B_rz = m_Pool.TexL1_B_rz as RenderTexture;
		rr.L2_0 = m_Pool.TexL2_0 as RenderTexture;
		rr.L2_1 = m_Pool.TexL2_1 as RenderTexture;
		rr.L2_2 = m_Pool.TexL2_2 as RenderTexture;
		rr.L2_3 = m_Pool.TexL2_3 as RenderTexture;
		rr.ProbeOcclusion = m_Pool.TexProbeOcclusion as RenderTexture;
		rr.Validity = m_Pool.TexValidity as RenderTexture;
		rr.SkyOcclusionL0L1 = m_Pool.TexSkyOcclusion as RenderTexture;
		rr.SkyShadingDirectionIndices = m_Pool.TexSkyShadingDirectionIndices as RenderTexture;
	}

	internal void Clear()
	{
		m_FreeList.Clear();
		m_NextFreeChunk.x = (m_NextFreeChunk.y = (m_NextFreeChunk.z = 0));
	}

	internal static int GetChunkCount(int brickCount)
	{
		int num = 128;
		return (brickCount + num - 1) / num;
	}

	internal bool Allocate(int numberOfBrickChunks, List<BrickChunkAlloc> outAllocations, bool ignoreErrorLog)
	{
		while (m_FreeList.Count > 0 && numberOfBrickChunks > 0)
		{
			outAllocations.Add(m_FreeList.Pop());
			numberOfBrickChunks--;
			m_AvailableChunkCount--;
		}
		for (uint num = 0u; num < numberOfBrickChunks; num++)
		{
			if (m_NextFreeChunk.z >= m_Pool.depth)
			{
				if (!ignoreErrorLog)
				{
					Debug.LogError("Cannot allocate more brick chunks, probe volume brick pool is full.");
				}
				Deallocate(outAllocations);
				outAllocations.Clear();
				return false;
			}
			outAllocations.Add(m_NextFreeChunk);
			m_AvailableChunkCount--;
			m_NextFreeChunk.x += 512;
			if (m_NextFreeChunk.x >= m_Pool.width)
			{
				m_NextFreeChunk.x = 0;
				m_NextFreeChunk.y += 4;
				if (m_NextFreeChunk.y >= m_Pool.height)
				{
					m_NextFreeChunk.y = 0;
					m_NextFreeChunk.z += 4;
				}
			}
		}
		return true;
	}

	internal void Deallocate(List<BrickChunkAlloc> allocations)
	{
		m_AvailableChunkCount += allocations.Count;
		foreach (BrickChunkAlloc allocation in allocations)
		{
			m_FreeList.Push(allocation);
		}
	}

	internal void Update(DataLocation source, List<BrickChunkAlloc> srcLocations, List<BrickChunkAlloc> dstLocations, int destStartIndex, ProbeVolumeSHBands bands)
	{
		for (int i = 0; i < srcLocations.Count; i++)
		{
			BrickChunkAlloc brickChunkAlloc = srcLocations[i];
			BrickChunkAlloc brickChunkAlloc2 = dstLocations[destStartIndex + i];
			for (int j = 0; j < 4; j++)
			{
				int srcWidth = Mathf.Min(512, source.width - brickChunkAlloc.x);
				Graphics.CopyTexture(source.TexL0_L1rx, brickChunkAlloc.z + j, 0, brickChunkAlloc.x, brickChunkAlloc.y, srcWidth, 4, m_Pool.TexL0_L1rx, brickChunkAlloc2.z + j, 0, brickChunkAlloc2.x, brickChunkAlloc2.y);
				Graphics.CopyTexture(source.TexL1_G_ry, brickChunkAlloc.z + j, 0, brickChunkAlloc.x, brickChunkAlloc.y, srcWidth, 4, m_Pool.TexL1_G_ry, brickChunkAlloc2.z + j, 0, brickChunkAlloc2.x, brickChunkAlloc2.y);
				Graphics.CopyTexture(source.TexL1_B_rz, brickChunkAlloc.z + j, 0, brickChunkAlloc.x, brickChunkAlloc.y, srcWidth, 4, m_Pool.TexL1_B_rz, brickChunkAlloc2.z + j, 0, brickChunkAlloc2.x, brickChunkAlloc2.y);
				if (m_ContainsValidity)
				{
					Graphics.CopyTexture(source.TexValidity, brickChunkAlloc.z + j, 0, brickChunkAlloc.x, brickChunkAlloc.y, srcWidth, 4, m_Pool.TexValidity, brickChunkAlloc2.z + j, 0, brickChunkAlloc2.x, brickChunkAlloc2.y);
				}
				if (m_ContainsSkyOcclusion)
				{
					Graphics.CopyTexture(source.TexSkyOcclusion, brickChunkAlloc.z + j, 0, brickChunkAlloc.x, brickChunkAlloc.y, srcWidth, 4, m_Pool.TexSkyOcclusion, brickChunkAlloc2.z + j, 0, brickChunkAlloc2.x, brickChunkAlloc2.y);
					if (m_ContainsSkyShadingDirection)
					{
						Graphics.CopyTexture(source.TexSkyShadingDirectionIndices, brickChunkAlloc.z + j, 0, brickChunkAlloc.x, brickChunkAlloc.y, srcWidth, 4, m_Pool.TexSkyShadingDirectionIndices, brickChunkAlloc2.z + j, 0, brickChunkAlloc2.x, brickChunkAlloc2.y);
					}
				}
				if (bands == ProbeVolumeSHBands.SphericalHarmonicsL2)
				{
					Graphics.CopyTexture(source.TexL2_0, brickChunkAlloc.z + j, 0, brickChunkAlloc.x, brickChunkAlloc.y, srcWidth, 4, m_Pool.TexL2_0, brickChunkAlloc2.z + j, 0, brickChunkAlloc2.x, brickChunkAlloc2.y);
					Graphics.CopyTexture(source.TexL2_1, brickChunkAlloc.z + j, 0, brickChunkAlloc.x, brickChunkAlloc.y, srcWidth, 4, m_Pool.TexL2_1, brickChunkAlloc2.z + j, 0, brickChunkAlloc2.x, brickChunkAlloc2.y);
					Graphics.CopyTexture(source.TexL2_2, brickChunkAlloc.z + j, 0, brickChunkAlloc.x, brickChunkAlloc.y, srcWidth, 4, m_Pool.TexL2_2, brickChunkAlloc2.z + j, 0, brickChunkAlloc2.x, brickChunkAlloc2.y);
					Graphics.CopyTexture(source.TexL2_3, brickChunkAlloc.z + j, 0, brickChunkAlloc.x, brickChunkAlloc.y, srcWidth, 4, m_Pool.TexL2_3, brickChunkAlloc2.z + j, 0, brickChunkAlloc2.x, brickChunkAlloc2.y);
				}
				if (m_ContainsProbeOcclusion)
				{
					Graphics.CopyTexture(source.TexProbeOcclusion, brickChunkAlloc.z + j, 0, brickChunkAlloc.x, brickChunkAlloc.y, srcWidth, 4, m_Pool.TexProbeOcclusion, brickChunkAlloc2.z + j, 0, brickChunkAlloc2.x, brickChunkAlloc2.y);
				}
			}
		}
	}

	internal void Update(CommandBuffer cmd, ProbeReferenceVolume.CellStreamingScratchBuffer dataBuffer, ProbeReferenceVolume.CellStreamingScratchBufferLayout layout, List<BrickChunkAlloc> dstLocations, bool updateSharedData, Texture validityTexture, ProbeVolumeSHBands bands, bool skyOcclusion, Texture skyOcclusionTexture, bool skyShadingDirections, Texture skyShadingDirectionsTexture, bool probeOcclusion)
	{
		using (new ProfilingScope(cmd, ProfilingSampler.Get(CoreProfileId.APVDiskStreamingUpdatePool)))
		{
			int count = dstLocations.Count;
			cmd.SetComputeTextureParam(s_DataUploadCS, s_DataUploadKernel, _Out_L0_L1Rx, m_Pool.TexL0_L1rx);
			cmd.SetComputeTextureParam(s_DataUploadCS, s_DataUploadKernel, _Out_L1G_L1Ry, m_Pool.TexL1_G_ry);
			cmd.SetComputeTextureParam(s_DataUploadCS, s_DataUploadKernel, _Out_L1B_L1Rz, m_Pool.TexL1_B_rz);
			if (updateSharedData)
			{
				cmd.EnableKeyword(s_DataUploadCS, in s_DataUpload_Shared);
				cmd.SetComputeTextureParam(s_DataUploadCS, s_DataUploadKernel, _Out_Shared, validityTexture);
				if (skyOcclusion)
				{
					cmd.EnableKeyword(s_DataUploadCS, in s_DataUpload_SkyOcclusion);
					cmd.SetComputeTextureParam(s_DataUploadCS, s_DataUploadKernel, _Out_SkyOcclusionL0L1, skyOcclusionTexture);
					if (skyShadingDirections)
					{
						cmd.SetComputeTextureParam(s_DataUploadCS, s_DataUploadKernel, _Out_SkyShadingDirectionIndices, skyShadingDirectionsTexture);
						cmd.EnableKeyword(s_DataUploadCS, in s_DataUpload_SkyShadingDirection);
					}
					else
					{
						cmd.DisableKeyword(s_DataUploadCS, in s_DataUpload_SkyShadingDirection);
					}
				}
			}
			else
			{
				cmd.DisableKeyword(s_DataUploadCS, in s_DataUpload_Shared);
				cmd.DisableKeyword(s_DataUploadCS, in s_DataUpload_SkyOcclusion);
				cmd.DisableKeyword(s_DataUploadCS, in s_DataUpload_SkyShadingDirection);
			}
			if (bands == ProbeVolumeSHBands.SphericalHarmonicsL2)
			{
				cmd.SetComputeTextureParam(s_DataUploadL2CS, s_DataUploadL2Kernel, _Out_L2_0, m_Pool.TexL2_0);
				cmd.SetComputeTextureParam(s_DataUploadL2CS, s_DataUploadL2Kernel, _Out_L2_1, m_Pool.TexL2_1);
				cmd.SetComputeTextureParam(s_DataUploadL2CS, s_DataUploadL2Kernel, _Out_L2_2, m_Pool.TexL2_2);
				cmd.SetComputeTextureParam(s_DataUploadL2CS, s_DataUploadL2Kernel, _Out_L2_3, m_Pool.TexL2_3);
			}
			if (probeOcclusion)
			{
				cmd.EnableKeyword(s_DataUploadCS, in s_DataUpload_ProbeOcclusion);
				cmd.SetComputeTextureParam(s_DataUploadCS, s_DataUploadKernel, _Out_ProbeOcclusion, m_Pool.TexProbeOcclusion);
			}
			else
			{
				cmd.DisableKeyword(s_DataUploadCS, in s_DataUpload_ProbeOcclusion);
			}
			int threadGroupsX = DivRoundUp(2048, 64);
			ConstantBuffer.Push(cmd, in layout, s_DataUploadCS, _ProbeVolumeScratchBufferLayout);
			cmd.SetComputeBufferParam(s_DataUploadCS, s_DataUploadKernel, _ProbeVolumeScratchBuffer, dataBuffer.buffer);
			cmd.DispatchCompute(s_DataUploadCS, s_DataUploadKernel, threadGroupsX, 1, count);
			if (bands == ProbeVolumeSHBands.SphericalHarmonicsL2)
			{
				ConstantBuffer.Push(cmd, in layout, s_DataUploadL2CS, _ProbeVolumeScratchBufferLayout);
				cmd.SetComputeBufferParam(s_DataUploadL2CS, s_DataUploadL2Kernel, _ProbeVolumeScratchBuffer, dataBuffer.buffer);
				cmd.DispatchCompute(s_DataUploadL2CS, s_DataUploadL2Kernel, threadGroupsX, 1, count);
			}
		}
	}

	internal void UpdateValidity(DataLocation source, List<BrickChunkAlloc> srcLocations, List<BrickChunkAlloc> dstLocations, int destStartIndex)
	{
		for (int i = 0; i < srcLocations.Count; i++)
		{
			BrickChunkAlloc brickChunkAlloc = srcLocations[i];
			BrickChunkAlloc brickChunkAlloc2 = dstLocations[destStartIndex + i];
			for (int j = 0; j < 4; j++)
			{
				int srcWidth = Mathf.Min(512, source.width - brickChunkAlloc.x);
				Graphics.CopyTexture(source.TexValidity, brickChunkAlloc.z + j, 0, brickChunkAlloc.x, brickChunkAlloc.y, srcWidth, 4, m_Pool.TexValidity, brickChunkAlloc2.z + j, 0, brickChunkAlloc2.x, brickChunkAlloc2.y);
			}
		}
	}

	internal static Vector3Int ProbeCountToDataLocSize(int numProbes)
	{
		int num = numProbes / 64;
		int num2 = 512;
		int num3 = (num + num2 * num2 - 1) / (num2 * num2);
		int num5;
		int num4;
		if (num3 > 1)
		{
			num5 = (num4 = num2);
		}
		else
		{
			num4 = (num + num2 - 1) / num2;
			num5 = ((num4 <= 1) ? num : num2);
		}
		num5 *= 4;
		num4 *= 4;
		num3 *= 4;
		return new Vector3Int(num5, num4, num3);
	}

	private static int EstimateMemoryCost(int width, int height, int depth, GraphicsFormat format)
	{
		return width * height * depth * format switch
		{
			GraphicsFormat.R8G8B8A8_UNorm => 4, 
			GraphicsFormat.R16G16B16A16_SFloat => 8, 
			_ => 1, 
		};
	}

	internal static int EstimateMemoryCostForBlending(ProbeVolumeTextureMemoryBudget memoryBudget, bool compressed, ProbeVolumeSHBands bands)
	{
		if (memoryBudget == (ProbeVolumeTextureMemoryBudget)0)
		{
			return 0;
		}
		DerivePoolSizeFromBudget(memoryBudget, out var width, out var height, out var depth);
		Vector3Int vector3Int = ProbeCountToDataLocSize(width * height * depth);
		width = vector3Int.x;
		height = vector3Int.y;
		depth = vector3Int.z;
		int num = 0;
		GraphicsFormat format = GraphicsFormat.R16G16B16A16_SFloat;
		GraphicsFormat format2 = (compressed ? GraphicsFormat.RGBA_BC7_UNorm : GraphicsFormat.R8G8B8A8_UNorm);
		num += EstimateMemoryCost(width, height, depth, format);
		num += EstimateMemoryCost(width, height, depth, format2) * 2;
		if (bands == ProbeVolumeSHBands.SphericalHarmonicsL2)
		{
			num += EstimateMemoryCost(width, height, depth, format2) * 3;
		}
		return num;
	}

	public static Texture CreateDataTexture(int width, int height, int depth, GraphicsFormat format, string name, bool allocateRendertexture, ref int allocatedBytes)
	{
		allocatedBytes += EstimateMemoryCost(width, height, depth, format);
		Texture texture;
		if (allocateRendertexture)
		{
			RenderTextureDescriptor desc = default(RenderTextureDescriptor);
			desc.width = width;
			desc.height = height;
			desc.volumeDepth = depth;
			desc.graphicsFormat = format;
			desc.mipCount = 1;
			desc.enableRandomWrite = SystemInfo.supportsComputeShaders;
			desc.dimension = TextureDimension.Tex3D;
			desc.msaaSamples = 1;
			texture = new RenderTexture(desc);
		}
		else
		{
			texture = new Texture3D(width, height, depth, format, TextureCreationFlags.None, 1);
		}
		texture.hideFlags = HideFlags.HideAndDontSave;
		texture.name = name;
		if (allocateRendertexture)
		{
			(texture as RenderTexture).Create();
		}
		return texture;
	}

	public static DataLocation CreateDataLocation(int numProbes, bool compressed, ProbeVolumeSHBands bands, string name, bool allocateRendertexture, bool allocateValidityData, bool allocateRenderingLayers, bool allocateSkyOcclusionData, bool allocateSkyShadingDirectionData, bool allocateProbeOcclusionData, out int allocatedBytes)
	{
		Vector3Int vector3Int = ProbeCountToDataLocSize(numProbes);
		int x = vector3Int.x;
		int y = vector3Int.y;
		int z = vector3Int.z;
		GraphicsFormat format = GraphicsFormat.R16G16B16A16_SFloat;
		GraphicsFormat format2 = (compressed ? GraphicsFormat.RGBA_BC7_UNorm : GraphicsFormat.R8G8B8A8_UNorm);
		GraphicsFormat format3 = (allocateRenderingLayers ? GraphicsFormat.R32_SFloat : (SystemInfo.IsFormatSupported(GraphicsFormat.R8_UNorm, GraphicsFormatUsage.Sample | GraphicsFormatUsage.LoadStore) ? GraphicsFormat.R8_UNorm : GraphicsFormat.R8G8B8A8_UNorm));
		allocatedBytes = 0;
		DataLocation result = default(DataLocation);
		result.TexL0_L1rx = CreateDataTexture(x, y, z, format, name + "_TexL0_L1rx", allocateRendertexture, ref allocatedBytes);
		result.TexL1_G_ry = CreateDataTexture(x, y, z, format2, name + "_TexL1_G_ry", allocateRendertexture, ref allocatedBytes);
		result.TexL1_B_rz = CreateDataTexture(x, y, z, format2, name + "_TexL1_B_rz", allocateRendertexture, ref allocatedBytes);
		if (allocateValidityData)
		{
			result.TexValidity = CreateDataTexture(x, y, z, format3, name + "_Validity", allocateRendertexture, ref allocatedBytes);
		}
		else
		{
			result.TexValidity = null;
		}
		if (allocateSkyOcclusionData)
		{
			result.TexSkyOcclusion = CreateDataTexture(x, y, z, GraphicsFormat.R16G16B16A16_SFloat, name + "_SkyOcclusion", allocateRendertexture, ref allocatedBytes);
		}
		else
		{
			result.TexSkyOcclusion = null;
		}
		if (allocateSkyShadingDirectionData)
		{
			result.TexSkyShadingDirectionIndices = CreateDataTexture(x, y, z, GraphicsFormat.R8_UNorm, name + "_SkyShadingDirectionIndices", allocateRendertexture, ref allocatedBytes);
		}
		else
		{
			result.TexSkyShadingDirectionIndices = null;
		}
		if (allocateProbeOcclusionData)
		{
			result.TexProbeOcclusion = CreateDataTexture(x, y, z, GraphicsFormat.R8G8B8A8_UNorm, name + "_ProbeOcclusion", allocateRendertexture, ref allocatedBytes);
		}
		else
		{
			result.TexProbeOcclusion = null;
		}
		if (bands == ProbeVolumeSHBands.SphericalHarmonicsL2)
		{
			result.TexL2_0 = CreateDataTexture(x, y, z, format2, name + "_TexL2_0", allocateRendertexture, ref allocatedBytes);
			result.TexL2_1 = CreateDataTexture(x, y, z, format2, name + "_TexL2_1", allocateRendertexture, ref allocatedBytes);
			result.TexL2_2 = CreateDataTexture(x, y, z, format2, name + "_TexL2_2", allocateRendertexture, ref allocatedBytes);
			result.TexL2_3 = CreateDataTexture(x, y, z, format2, name + "_TexL2_3", allocateRendertexture, ref allocatedBytes);
		}
		else
		{
			result.TexL2_0 = null;
			result.TexL2_1 = null;
			result.TexL2_2 = null;
			result.TexL2_3 = null;
		}
		result.width = x;
		result.height = y;
		result.depth = z;
		return result;
	}

	private static void DerivePoolSizeFromBudget(ProbeVolumeTextureMemoryBudget memoryBudget, out int width, out int height, out int depth)
	{
		width = (int)memoryBudget;
		height = (int)memoryBudget;
		depth = 4;
	}

	internal void Cleanup()
	{
		m_Pool.Cleanup();
	}
}
