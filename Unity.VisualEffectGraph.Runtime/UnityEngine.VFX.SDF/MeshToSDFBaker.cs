using System;
using System.Collections.Generic;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace UnityEngine.VFX.SDF;

public class MeshToSDFBaker : IDisposable
{
	private static class ShaderProperties
	{
		internal static int indicesBuffer = Shader.PropertyToID("indices");

		internal static int verticesBuffer = Shader.PropertyToID("vertices");

		internal static int vertexPositionOffset = Shader.PropertyToID("vertexPositionOffset");

		internal static int vertexStride = Shader.PropertyToID("vertexStride");

		internal static int indexStride = Shader.PropertyToID("indexStride");

		internal static int coordFlipBuffer = Shader.PropertyToID("coordFlip");

		internal static int verticesOutBuffer = Shader.PropertyToID("verticesOut");

		internal static int aabbBuffer = Shader.PropertyToID("aabb");

		internal static int worldToClip = Shader.PropertyToID("worldToClip");

		internal static int currentAxis = Shader.PropertyToID("currentAxis");

		internal static int voxelsBuffer = Shader.PropertyToID("voxelsBuffer");

		internal static int rw_trianglesUV = Shader.PropertyToID("rw_trianglesUV");

		internal static int trianglesUV = Shader.PropertyToID("trianglesUV");

		internal static int voxelsTexture = Shader.PropertyToID("voxels");

		internal static int voxelsTmpTexture = Shader.PropertyToID("voxelsTmp");

		internal static int rayMap = Shader.PropertyToID("rayMap");

		internal static int rayMapTmp = Shader.PropertyToID("rayMapTmp");

		internal static int rw_rayMapTmp = Shader.PropertyToID("rw_rayMapTmp");

		internal static int nTriangles = Shader.PropertyToID("nTriangles");

		internal static int minBoundsExtended = Shader.PropertyToID("minBoundsExtended");

		internal static int maxBoundsExtended = Shader.PropertyToID("maxBoundsExtended");

		internal static int maxExtent = Shader.PropertyToID("maxExtent");

		internal static int upperBoundCount = Shader.PropertyToID("upperBoundCount");

		internal static int counter = Shader.PropertyToID("counter");

		internal static int dimX = Shader.PropertyToID("dimX");

		internal static int dimY = Shader.PropertyToID("dimY");

		internal static int dimZ = Shader.PropertyToID("dimZ");

		internal static int size = Shader.PropertyToID("size");

		internal static int inputBuffer = Shader.PropertyToID("Input");

		internal static int inputCounter = Shader.PropertyToID("inputCounter");

		internal static int auxBuffer = Shader.PropertyToID("auxBuffer");

		internal static int resultBuffer = Shader.PropertyToID("Result");

		internal static int numElem = Shader.PropertyToID("numElem");

		internal static int exclusive = Shader.PropertyToID("exclusive");

		internal static int dispatchWidth = Shader.PropertyToID("dispatchWidth");

		internal static int src = Shader.PropertyToID("src");

		internal static int dest = Shader.PropertyToID("dest");

		internal static int signMap = Shader.PropertyToID("signMap");

		internal static int threshold = Shader.PropertyToID("threshold");

		internal static int signMapTmp = Shader.PropertyToID("signMapTmp");

		internal static int normalizeFactor = Shader.PropertyToID("normalizeFactor");

		internal static int numNeighbours = Shader.PropertyToID("numNeighbours");

		internal static int passId = Shader.PropertyToID("passId");

		internal static int needNormalize = Shader.PropertyToID("needNormalize");

		internal static int offset = Shader.PropertyToID("offset");

		internal static int offsetRayMap = Shader.PropertyToID("offsetRayMap");

		internal static int triangleIDs = Shader.PropertyToID("triangleIDs");

		internal static int accumCounter = Shader.PropertyToID("accumCounter");

		internal static int distanceTexture = Shader.PropertyToID("distanceTexture");

		internal static int sdfOffset = Shader.PropertyToID("sdfOffset");
	}

	internal class Kernels
	{
		internal int inBucketSum = -1;

		internal int blockSums = -1;

		internal int finalSum = -1;

		internal int toTextureNormalized = -1;

		internal int copyTextures = -1;

		internal int jfa = -1;

		internal int distanceTransform = -1;

		internal int copyBuffers = -1;

		internal int generateRayMapLocal = -1;

		internal int rayMapScanX = -1;

		internal int rayMapScanY = -1;

		internal int rayMapScanZ = -1;

		internal int signPass6Rays = -1;

		internal int signPassNeighbors = -1;

		internal int toBlockSumBuffer = -1;

		internal int clearTexturesAndBuffers = -1;

		internal int copyToBuffer = -1;

		internal int generateTrianglesUV = -1;

		internal int conservativeRasterization = -1;

		internal int chooseDirectionTriangleOnly = -1;

		internal int surfaceClosing = -1;

		internal Kernels(ComputeShader computeShader)
		{
			inBucketSum = computeShader.FindKernel("InBucketSum");
			blockSums = computeShader.FindKernel("BlockSums");
			finalSum = computeShader.FindKernel("FinalSum");
			toTextureNormalized = computeShader.FindKernel("ToTextureNormalized");
			copyTextures = computeShader.FindKernel("CopyTextures");
			jfa = computeShader.FindKernel("JFA");
			distanceTransform = computeShader.FindKernel("DistanceTransform");
			copyBuffers = computeShader.FindKernel("CopyBuffers");
			generateRayMapLocal = computeShader.FindKernel("GenerateRayMapLocal");
			rayMapScanX = computeShader.FindKernel("RayMapScanX");
			rayMapScanY = computeShader.FindKernel("RayMapScanY");
			rayMapScanZ = computeShader.FindKernel("RayMapScanZ");
			signPass6Rays = computeShader.FindKernel("SignPass6Rays");
			signPassNeighbors = computeShader.FindKernel("SignPassNeighbors");
			toBlockSumBuffer = computeShader.FindKernel("ToBlockSumBuffer");
			clearTexturesAndBuffers = computeShader.FindKernel("ClearTexturesAndBuffers");
			copyToBuffer = computeShader.FindKernel("CopyToBuffer");
			generateTrianglesUV = computeShader.FindKernel("GenerateTrianglesUV");
			conservativeRasterization = computeShader.FindKernel("ConservativeRasterization");
			chooseDirectionTriangleOnly = computeShader.FindKernel("ChooseDirectionTriangleOnly");
			surfaceClosing = computeShader.FindKernel("SurfaceClosing");
		}
	}

	private RenderTexture[] m_RayMaps;

	private RenderTexture[] m_SignMaps;

	private RenderTexture[] m_RenderTextureViews;

	private GraphicsBuffer m_CounterBuffer;

	private GraphicsBuffer m_AccumCounterBuffer;

	private GraphicsBuffer m_TrianglesInVoxels;

	private GraphicsBuffer m_TrianglesUV;

	private GraphicsBuffer m_TmpBuffer;

	private GraphicsBuffer m_AccumSumBlocks;

	private GraphicsBuffer m_SumBlocksBuffer;

	private GraphicsBuffer m_InSumBlocksBuffer;

	private GraphicsBuffer m_SumBlocksAdditional;

	private GraphicsBuffer m_IndicesBuffer;

	private GraphicsBuffer m_VerticesBuffer;

	private GraphicsBuffer m_VerticesOutBuffer;

	private GraphicsBuffer m_CoordFlipBuffer;

	private GraphicsBuffer m_AabbBuffer;

	private int m_VertexBufferOffset;

	private int m_ThreadGroupSize = 512;

	private int m_SignPassesCount;

	private float m_InOutThreshold;

	private Material[] m_Material;

	private Matrix4x4[] m_WorldToClip;

	private Matrix4x4[] m_ProjMat;

	private Matrix4x4[] m_ViewMat;

	private int m_nStepsJFA;

	private Kernels m_Kernels;

	private Mesh m_Mesh;

	private RenderTexture m_textureVoxel;

	private RenderTexture m_textureVoxelBis;

	private RenderTexture m_DistanceTexture;

	private GraphicsBuffer m_bufferVoxel;

	private ComputeShader m_computeShader;

	private int m_maxResolution;

	private float m_MaxExtent;

	private float m_SdfOffset;

	private int nTriangles;

	private Vector3 m_SizeBox;

	private Vector3 m_Center;

	private CommandBuffer m_Cmd;

	private bool m_OwnsCommandBuffer = true;

	private bool m_IsDisposed;

	private int[] m_Dimensions = new int[3];

	private int[] m_OffsetRayMap = new int[3];

	private float[] m_MinBoundsExtended = new float[3];

	private float[] m_MaxBoundsExtended = new float[3];

	private int m_RayMapUseCounter;

	internal static uint kMaxRecommandedGridSize = 16777216u;

	internal static uint kMaxAbsoluteGridSize = 134217728u;

	private static int kNbActualRT = 0;

	internal VFXRuntimeResources m_RuntimeResources;

	public RenderTexture SdfTexture => m_DistanceTexture;

	private static Mesh InitMeshFromList(List<Mesh> meshes, List<Matrix4x4> transforms)
	{
		int count = meshes.Count;
		if (count != transforms.Count)
		{
			throw new ArgumentException("The number of meshes must be the same as the number of transforms");
		}
		List<CombineInstance> list = new List<CombineInstance>();
		for (int i = 0; i < count; i++)
		{
			Mesh mesh = meshes[i];
			for (int j = 0; j < mesh.subMeshCount; j++)
			{
				CombineInstance item = default(CombineInstance);
				item.mesh = meshes[i];
				item.subMeshIndex = j;
				item.transform = transforms[i];
				list.Add(item);
			}
		}
		Mesh mesh2 = new Mesh();
		mesh2.indexFormat = IndexFormat.UInt32;
		mesh2.CombineMeshes(list.ToArray());
		return mesh2;
	}

	private void InitCommandBuffer()
	{
		if (m_Cmd == null)
		{
			m_Cmd = new CommandBuffer
			{
				name = "SDFBakingCommand"
			};
		}
	}

	private int GetTotalVoxelCount()
	{
		return m_Dimensions[0] * m_Dimensions[1] * m_Dimensions[2];
	}

	private void InitSizeBox()
	{
		m_MaxExtent = Mathf.Max(m_SizeBox.x, Mathf.Max(m_SizeBox.y, m_SizeBox.z));
		float num = 0f;
		if (m_MaxExtent == m_SizeBox.x)
		{
			m_Dimensions[0] = Mathf.Max(Mathf.RoundToInt((float)m_maxResolution * m_SizeBox.x / m_MaxExtent), 1);
			m_Dimensions[1] = Mathf.Max(Mathf.CeilToInt((float)m_maxResolution * m_SizeBox.y / m_MaxExtent), 1);
			m_Dimensions[2] = Mathf.Max(Mathf.CeilToInt((float)m_maxResolution * m_SizeBox.z / m_MaxExtent), 1);
			num = m_MaxExtent / (float)m_Dimensions[0];
		}
		else if (m_MaxExtent == m_SizeBox.y)
		{
			m_Dimensions[1] = Mathf.Max(Mathf.RoundToInt((float)m_maxResolution * m_SizeBox.y / m_MaxExtent), 1);
			m_Dimensions[0] = Mathf.Max(Mathf.CeilToInt((float)m_maxResolution * m_SizeBox.x / m_MaxExtent), 1);
			m_Dimensions[2] = Mathf.Max(Mathf.CeilToInt((float)m_maxResolution * m_SizeBox.z / m_MaxExtent), 1);
			num = m_MaxExtent / (float)m_Dimensions[1];
		}
		else if (m_MaxExtent == m_SizeBox.z)
		{
			m_Dimensions[2] = Mathf.Max(Mathf.RoundToInt((float)m_maxResolution * m_SizeBox.z / m_MaxExtent), 1);
			m_Dimensions[1] = Mathf.Max(Mathf.CeilToInt((float)m_maxResolution * m_SizeBox.y / m_MaxExtent), 1);
			m_Dimensions[0] = Mathf.Max(Mathf.CeilToInt((float)m_maxResolution * m_SizeBox.x / m_MaxExtent), 1);
			num = m_MaxExtent / (float)m_Dimensions[2];
		}
		if (GetTotalVoxelCount() > kMaxAbsoluteGridSize)
		{
			throw new ArgumentException($"The size of the voxel grid is too big (>2^{Mathf.Log(kMaxAbsoluteGridSize, 2f)}), reduce the resolution, or provide a thinner bounding box.");
		}
		for (int i = 0; i < 3; i++)
		{
			m_SizeBox[i] = (float)m_Dimensions[i] * num;
		}
	}

	public Vector3Int GetGridSize()
	{
		return new Vector3Int(m_Dimensions[0], m_Dimensions[1], m_Dimensions[2]);
	}

	public Vector3 GetActualBoxSize()
	{
		return m_SizeBox;
	}

	public MeshToSDFBaker(Vector3 sizeBox, Vector3 center, int maxRes, Mesh mesh, int signPassesCount = 1, float threshold = 0.5f, float sdfOffset = 0f, CommandBuffer cmd = null)
	{
		LoadRuntimeResources();
		m_Mesh = mesh;
		if (cmd != null)
		{
			m_Cmd = cmd;
			m_OwnsCommandBuffer = false;
		}
		SetParameters(sizeBox, center, maxRes, signPassesCount, threshold, sdfOffset);
		Init();
	}

	public MeshToSDFBaker(Vector3 sizeBox, Vector3 center, int maxRes, List<Mesh> meshes, List<Matrix4x4> transforms, int signPassesCount = 1, float threshold = 0.5f, float sdfOffset = 0f, CommandBuffer cmd = null)
		: this(sizeBox, center, maxRes, InitMeshFromList(meshes, transforms), signPassesCount, threshold, sdfOffset, cmd)
	{
	}

	~MeshToSDFBaker()
	{
		if (!m_IsDisposed)
		{
			Debug.LogWarning("Dispose() should be called explicitly when an MeshToSDFBaker instance is finished being used.");
		}
	}

	public void Reinit(Vector3 sizeBox, Vector3 center, int maxRes, Mesh mesh, int signPassesCount = 1, float threshold = 0.5f, float sdfOffset = 0f)
	{
		m_Mesh = mesh;
		SetParameters(sizeBox, center, maxRes, signPassesCount, threshold, sdfOffset);
		Init();
	}

	public void Reinit(Vector3 sizeBox, Vector3 center, int maxRes, List<Mesh> meshes, List<Matrix4x4> transforms, int signPassesCount = 1, float threshold = 0.5f, float sdfOffset = 0f)
	{
		Reinit(sizeBox, center, maxRes, InitMeshFromList(meshes, transforms), signPassesCount, threshold, sdfOffset);
	}

	private void SetParameters(Vector3 sizeBox, Vector3 center, int maxRes, int signPassesCount, float threshold, float sdfOffset)
	{
		if (m_SignPassesCount >= 20)
		{
			throw new ArgumentException("The signPassCount argument should be smaller than 20.");
		}
		m_SignPassesCount = signPassesCount;
		m_InOutThreshold = threshold;
		m_SdfOffset = sdfOffset;
		m_Center = center;
		m_SizeBox = sizeBox;
		m_maxResolution = maxRes;
	}

	private void LoadRuntimeResources()
	{
		if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3)
		{
			Debug.LogWarning("MeshToSDFBaker compute shaders are not supported on OpenGLES3");
		}
		m_RuntimeResources = VFXRuntimeResources.runtimeResources;
		if (m_RuntimeResources == null)
		{
			throw new InvalidOperationException("VFX Runtime Resources could not be loaded.");
		}
	}

	private void InitTextures()
	{
		RenderTextureDescriptor renderTextureDescriptor = default(RenderTextureDescriptor);
		renderTextureDescriptor.graphicsFormat = GraphicsFormat.R16G16B16A16_SFloat;
		renderTextureDescriptor.dimension = TextureDimension.Tex3D;
		renderTextureDescriptor.enableRandomWrite = true;
		renderTextureDescriptor.width = m_Dimensions[0];
		renderTextureDescriptor.height = m_Dimensions[1];
		renderTextureDescriptor.volumeDepth = m_Dimensions[2];
		renderTextureDescriptor.msaaSamples = 1;
		RenderTextureDescriptor rtDesc = renderTextureDescriptor;
		renderTextureDescriptor = default(RenderTextureDescriptor);
		renderTextureDescriptor.graphicsFormat = GraphicsFormat.R16_SFloat;
		renderTextureDescriptor.dimension = TextureDimension.Tex3D;
		renderTextureDescriptor.enableRandomWrite = true;
		renderTextureDescriptor.width = m_Dimensions[0];
		renderTextureDescriptor.height = m_Dimensions[1];
		renderTextureDescriptor.volumeDepth = m_Dimensions[2];
		renderTextureDescriptor.msaaSamples = 1;
		RenderTextureDescriptor rtDesc2 = renderTextureDescriptor;
		renderTextureDescriptor = default(RenderTextureDescriptor);
		renderTextureDescriptor.graphicsFormat = GraphicsFormat.R32_SFloat;
		renderTextureDescriptor.dimension = TextureDimension.Tex3D;
		renderTextureDescriptor.enableRandomWrite = true;
		renderTextureDescriptor.width = m_Dimensions[0];
		renderTextureDescriptor.height = m_Dimensions[1];
		renderTextureDescriptor.volumeDepth = m_Dimensions[2];
		renderTextureDescriptor.msaaSamples = 1;
		RenderTextureDescriptor rtDesc3 = renderTextureDescriptor;
		CreateRenderTextureIfNeeded(ref m_textureVoxel, rtDesc);
		CreateRenderTextureIfNeeded(ref m_textureVoxelBis, rtDesc);
		if (m_RayMaps == null)
		{
			m_RayMaps = new RenderTexture[2];
		}
		if (m_SignMaps == null)
		{
			m_SignMaps = new RenderTexture[2];
		}
		for (int i = 0; i < 2; i++)
		{
			CreateRenderTextureIfNeeded(ref m_RayMaps[i], rtDesc);
			CreateRenderTextureIfNeeded(ref m_SignMaps[i], rtDesc3);
		}
		CreateRenderTextureIfNeeded(ref m_DistanceTexture, rtDesc2);
		CreateGraphicsBufferIfNeeded(ref m_bufferVoxel, GetTotalVoxelCount(), 16);
		InitPrefixSumBuffers();
	}

	private void Init()
	{
		m_Mesh.vertexBufferTarget |= GraphicsBuffer.Target.Raw;
		m_Mesh.indexBufferTarget |= GraphicsBuffer.Target.Raw;
		InitSizeBox();
		InitCommandBuffer();
		m_ThreadGroupSize = 512;
		m_computeShader = m_RuntimeResources.sdfRayMapCS;
		if (m_computeShader == null)
		{
			throw new InvalidOperationException("VFX Runtime Resources could not be loaded correctly.");
		}
		if (m_Kernels == null)
		{
			m_Kernels = new Kernels(m_computeShader);
		}
		InitTextures();
		RenderTextureDescriptor rtDesc = default(RenderTextureDescriptor);
		rtDesc.width = m_Dimensions[0];
		rtDesc.height = m_Dimensions[1];
		rtDesc.graphicsFormat = GraphicsFormat.R8G8B8A8_SRGB;
		rtDesc.volumeDepth = 1;
		rtDesc.msaaSamples = 1;
		rtDesc.dimension = TextureDimension.Tex2D;
		if (m_RenderTextureViews == null)
		{
			m_RenderTextureViews = new RenderTexture[3];
		}
		for (int i = 0; i < 3; i++)
		{
			switch (i)
			{
			case 0:
				rtDesc.width = m_Dimensions[0];
				rtDesc.height = m_Dimensions[1];
				CreateRenderTextureIfNeeded(ref m_RenderTextureViews[i], rtDesc);
				break;
			case 1:
				rtDesc.width = m_Dimensions[2];
				rtDesc.height = m_Dimensions[0];
				CreateRenderTextureIfNeeded(ref m_RenderTextureViews[i], rtDesc);
				break;
			case 2:
				rtDesc.width = m_Dimensions[1];
				rtDesc.height = m_Dimensions[2];
				CreateRenderTextureIfNeeded(ref m_RenderTextureViews[i], rtDesc);
				break;
			}
		}
		if (m_Material == null || m_Material[0] == null || m_Material[1] == null || m_Material[2] == null)
		{
			m_Material = new Material[3];
			Shader sdfRayMapShader = m_RuntimeResources.sdfRayMapShader;
			if (sdfRayMapShader == null)
			{
				throw new InvalidOperationException("VFX Runtime Resources could not be loaded correctly.");
			}
			for (int j = 0; j < 3; j++)
			{
				m_Material[j] = new Material(sdfRayMapShader);
			}
		}
		if (m_WorldToClip == null)
		{
			m_WorldToClip = new Matrix4x4[3];
		}
		if (m_ProjMat == null)
		{
			m_ProjMat = new Matrix4x4[3];
		}
		if (m_ViewMat == null)
		{
			m_ViewMat = new Matrix4x4[3];
		}
		UpdateCameras();
	}

	private void UpdateCameras()
	{
		Vector3 pos = m_Center + Vector3.back * (m_SizeBox.z * 0.5f + 1f);
		Quaternion identity = Quaternion.identity;
		float num = 1f;
		float far = num + m_SizeBox.z;
		m_WorldToClip[0] = ComputeOrthographicWorldToClip(pos, identity, m_SizeBox.x, m_SizeBox.y, num, far, out m_ProjMat[0], out m_ViewMat[0]);
		pos = m_Center + Vector3.down * (m_SizeBox.y * 0.5f + 1f);
		identity = Quaternion.Euler(-90f, -90f, 0f);
		far = num + m_SizeBox.y;
		m_WorldToClip[1] = ComputeOrthographicWorldToClip(pos, identity, m_SizeBox.z, m_SizeBox.x, num, far, out m_ProjMat[1], out m_ViewMat[1]);
		pos = m_Center + Vector3.left * (m_SizeBox.x * 0.5f + 1f);
		identity = Quaternion.Euler(0f, 90f, 90f);
		far = num + m_SizeBox.x;
		m_WorldToClip[2] = ComputeOrthographicWorldToClip(pos, identity, m_SizeBox.y, m_SizeBox.z, num, far, out m_ProjMat[2], out m_ViewMat[2]);
	}

	private Matrix4x4 ComputeOrthographicWorldToClip(Vector3 pos, Quaternion rot, float width, float height, float near, float far, out Matrix4x4 proj, out Matrix4x4 view)
	{
		proj = Matrix4x4.Ortho((0f - width) / 2f, width / 2f, (0f - height) / 2f, height / 2f, near, far);
		proj = GL.GetGPUProjectionMatrix(proj, renderIntoTexture: false);
		view = Matrix4x4.TRS(pos, rot, new Vector3(1f, 1f, -1f)).inverse;
		return proj * view;
	}

	private int iDivUp(int a, int b)
	{
		if (a % b == 0)
		{
			return a / b;
		}
		return a / b + 1;
	}

	private Vector2Int GetThreadGroupsCount(int nbThreads, int threadCountPerGroup)
	{
		Vector2Int zero = Vector2Int.zero;
		int num = (nbThreads + threadCountPerGroup - 1) / threadCountPerGroup;
		zero.y = 1 + num / 65535;
		zero.x = num / zero.y;
		return zero;
	}

	private void PrefixSumCount()
	{
		int totalVoxelCount = GetTotalVoxelCount();
		m_Cmd.BeginSample("BakeSDF.PrefixSum");
		m_Cmd.SetComputeIntParam(m_computeShader, ShaderProperties.numElem, totalVoxelCount);
		m_Cmd.SetComputeBufferParam(m_computeShader, m_Kernels.inBucketSum, ShaderProperties.inputBuffer, m_CounterBuffer);
		m_Cmd.SetComputeBufferParam(m_computeShader, m_Kernels.inBucketSum, ShaderProperties.resultBuffer, m_TmpBuffer);
		Vector2Int threadGroupsCount = GetThreadGroupsCount(totalVoxelCount, m_ThreadGroupSize);
		m_Cmd.SetComputeIntParam(m_computeShader, ShaderProperties.dispatchWidth, threadGroupsCount.x);
		m_Cmd.DispatchCompute(m_computeShader, m_Kernels.inBucketSum, threadGroupsCount.x, threadGroupsCount.y, 1);
		int num = iDivUp(totalVoxelCount, m_ThreadGroupSize);
		if (num > m_ThreadGroupSize)
		{
			m_Cmd.SetComputeBufferParam(m_computeShader, m_Kernels.toBlockSumBuffer, ShaderProperties.inputCounter, m_CounterBuffer);
			m_Cmd.SetComputeBufferParam(m_computeShader, m_Kernels.toBlockSumBuffer, ShaderProperties.inputBuffer, m_TmpBuffer);
			m_Cmd.SetComputeBufferParam(m_computeShader, m_Kernels.toBlockSumBuffer, ShaderProperties.resultBuffer, m_SumBlocksBuffer);
			m_Cmd.DispatchCompute(m_computeShader, m_Kernels.toBlockSumBuffer, Mathf.CeilToInt((float)totalVoxelCount / (float)(m_ThreadGroupSize * m_ThreadGroupSize)), 1, 1);
			m_Cmd.SetComputeIntParam(m_computeShader, ShaderProperties.numElem, num);
			m_Cmd.SetComputeBufferParam(m_computeShader, m_Kernels.inBucketSum, ShaderProperties.inputBuffer, m_SumBlocksBuffer);
			m_Cmd.SetComputeBufferParam(m_computeShader, m_Kernels.inBucketSum, ShaderProperties.resultBuffer, m_InSumBlocksBuffer);
			m_Cmd.DispatchCompute(m_computeShader, m_Kernels.inBucketSum, Mathf.CeilToInt((float)totalVoxelCount / (float)(m_ThreadGroupSize * m_ThreadGroupSize)), 1, 1);
			m_Cmd.SetComputeBufferParam(m_computeShader, m_Kernels.blockSums, ShaderProperties.inputCounter, m_SumBlocksBuffer);
			m_Cmd.SetComputeBufferParam(m_computeShader, m_Kernels.blockSums, ShaderProperties.inputBuffer, m_InSumBlocksBuffer);
			m_Cmd.SetComputeBufferParam(m_computeShader, m_Kernels.blockSums, ShaderProperties.resultBuffer, m_SumBlocksAdditional);
			m_Cmd.DispatchCompute(m_computeShader, m_Kernels.blockSums, Mathf.CeilToInt((float)totalVoxelCount / (float)(m_ThreadGroupSize * m_ThreadGroupSize * m_ThreadGroupSize)), 1, 1);
			m_Cmd.SetComputeIntParam(m_computeShader, ShaderProperties.exclusive, 0);
			m_Cmd.SetComputeBufferParam(m_computeShader, m_Kernels.finalSum, ShaderProperties.inputBuffer, m_InSumBlocksBuffer);
			m_Cmd.SetComputeBufferParam(m_computeShader, m_Kernels.finalSum, ShaderProperties.auxBuffer, m_SumBlocksAdditional);
			m_Cmd.SetComputeBufferParam(m_computeShader, m_Kernels.finalSum, ShaderProperties.inputCounter, m_SumBlocksBuffer);
			m_Cmd.SetComputeBufferParam(m_computeShader, m_Kernels.finalSum, ShaderProperties.resultBuffer, m_AccumSumBlocks);
			m_Cmd.DispatchCompute(m_computeShader, m_Kernels.finalSum, Mathf.CeilToInt((float)totalVoxelCount / (float)(m_ThreadGroupSize * m_ThreadGroupSize)), 1, 1);
		}
		else
		{
			m_Cmd.SetComputeBufferParam(m_computeShader, m_Kernels.blockSums, ShaderProperties.inputCounter, m_CounterBuffer);
			m_Cmd.SetComputeBufferParam(m_computeShader, m_Kernels.blockSums, ShaderProperties.inputBuffer, m_TmpBuffer);
			m_Cmd.SetComputeBufferParam(m_computeShader, m_Kernels.blockSums, ShaderProperties.resultBuffer, m_AccumSumBlocks);
			m_Cmd.DispatchCompute(m_computeShader, m_Kernels.blockSums, Mathf.CeilToInt((float)totalVoxelCount / (float)(m_ThreadGroupSize * m_ThreadGroupSize)), 1, 1);
		}
		m_Cmd.SetComputeIntParam(m_computeShader, ShaderProperties.numElem, totalVoxelCount);
		m_Cmd.SetComputeIntParam(m_computeShader, ShaderProperties.exclusive, 0);
		m_Cmd.SetComputeBufferParam(m_computeShader, m_Kernels.finalSum, ShaderProperties.inputBuffer, m_TmpBuffer);
		m_Cmd.SetComputeBufferParam(m_computeShader, m_Kernels.finalSum, ShaderProperties.auxBuffer, m_AccumSumBlocks);
		m_Cmd.SetComputeBufferParam(m_computeShader, m_Kernels.finalSum, ShaderProperties.inputCounter, m_CounterBuffer);
		m_Cmd.SetComputeBufferParam(m_computeShader, m_Kernels.finalSum, ShaderProperties.resultBuffer, m_AccumCounterBuffer);
		m_Cmd.DispatchCompute(m_computeShader, m_Kernels.finalSum, threadGroupsCount.x, threadGroupsCount.y, 1);
		m_Cmd.EndSample("BakeSDF.PrefixSum");
	}

	private void SurfaceClosing()
	{
		m_Cmd.BeginSample("BakeSDF.SurfaceClosing");
		if (m_SignPassesCount == 0)
		{
			m_InOutThreshold *= 6f;
		}
		m_Cmd.SetComputeFloatParam(m_computeShader, ShaderProperties.threshold, m_InOutThreshold);
		m_Cmd.SetComputeTextureParam(m_computeShader, m_Kernels.surfaceClosing, ShaderProperties.signMap, GetSignMapPrincipal(m_SignPassesCount));
		m_Cmd.SetComputeTextureParam(m_computeShader, m_Kernels.surfaceClosing, ShaderProperties.voxelsTexture, GetTextureVoxelPrincipal(0));
		m_Cmd.DispatchCompute(m_computeShader, m_Kernels.surfaceClosing, iDivUp(m_Dimensions[0], 4), iDivUp(m_Dimensions[1], 4), iDivUp(m_Dimensions[2], 4));
		m_Cmd.EndSample("BakeSDF.SurfaceClosing");
	}

	private RenderTexture GetTextureVoxelPrincipal(int step)
	{
		if (step % 2 == 0)
		{
			return m_textureVoxel;
		}
		return m_textureVoxelBis;
	}

	private RenderTexture GetTextureVoxelBis(int step)
	{
		if (step % 2 == 0)
		{
			return m_textureVoxelBis;
		}
		return m_textureVoxel;
	}

	private void JFA()
	{
		m_Cmd.BeginSample("BakeSDF.JFA");
		m_Cmd.SetComputeBufferParam(m_computeShader, m_Kernels.toTextureNormalized, ShaderProperties.voxelsBuffer, m_bufferVoxel);
		m_Cmd.SetComputeTextureParam(m_computeShader, m_Kernels.toTextureNormalized, ShaderProperties.voxelsTexture, GetTextureVoxelPrincipal(0));
		m_Cmd.DispatchCompute(m_computeShader, m_Kernels.toTextureNormalized, Mathf.CeilToInt((float)m_Dimensions[0] / 4f), Mathf.CeilToInt((float)m_Dimensions[1] / 4f), Mathf.CeilToInt((float)m_Dimensions[2] / 4f));
		m_Cmd.SetComputeTextureParam(m_computeShader, m_Kernels.jfa, ShaderProperties.voxelsTexture, GetTextureVoxelPrincipal(0), 0);
		m_Cmd.SetComputeTextureParam(m_computeShader, m_Kernels.jfa, ShaderProperties.voxelsTmpTexture, GetTextureVoxelBis(0), 0);
		m_Cmd.SetComputeTextureParam(m_computeShader, m_Kernels.copyTextures, ShaderProperties.voxelsTexture, GetTextureVoxelPrincipal(0), 0);
		m_Cmd.SetComputeTextureParam(m_computeShader, m_Kernels.copyTextures, ShaderProperties.voxelsTmpTexture, GetTextureVoxelBis(0), 0);
		m_Cmd.SetComputeIntParam(m_computeShader, ShaderProperties.offset, 1);
		m_Cmd.DispatchCompute(m_computeShader, m_Kernels.jfa, Mathf.CeilToInt((float)m_Dimensions[0] / 4f), Mathf.CeilToInt((float)m_Dimensions[1] / 4f), Mathf.CeilToInt((float)m_Dimensions[2] / 4f));
		m_Cmd.DispatchCompute(m_computeShader, m_Kernels.copyTextures, Mathf.CeilToInt((float)m_Dimensions[0] / 4f), Mathf.CeilToInt((float)m_Dimensions[1] / 4f), Mathf.CeilToInt((float)m_Dimensions[2] / 4f));
		m_nStepsJFA = Mathf.CeilToInt(Mathf.Log(m_maxResolution, 2f));
		for (int i = 1; i <= m_nStepsJFA; i++)
		{
			int val = Mathf.FloorToInt(Mathf.Pow(2f, m_nStepsJFA - i) + 0.5f);
			m_Cmd.SetComputeIntParam(m_computeShader, ShaderProperties.offset, val);
			m_Cmd.SetComputeTextureParam(m_computeShader, m_Kernels.jfa, ShaderProperties.voxelsTexture, GetTextureVoxelPrincipal(i), 0);
			m_Cmd.SetComputeTextureParam(m_computeShader, m_Kernels.jfa, ShaderProperties.voxelsTmpTexture, GetTextureVoxelBis(i), 0);
			m_Cmd.DispatchCompute(m_computeShader, m_Kernels.jfa, Mathf.CeilToInt((float)m_Dimensions[0] / 4f), Mathf.CeilToInt((float)m_Dimensions[1] / 4f), Mathf.CeilToInt((float)m_Dimensions[2] / 4f));
		}
		m_Cmd.EndSample("BakeSDF.JFA");
	}

	private void GenerateRayMap()
	{
		m_RayMapUseCounter = 0;
		m_Cmd.BeginSample("BakeSDF.Raymap");
		m_Cmd.SetComputeBufferParam(m_computeShader, m_Kernels.generateRayMapLocal, ShaderProperties.accumCounter, m_AccumCounterBuffer);
		m_Cmd.SetComputeBufferParam(m_computeShader, m_Kernels.generateRayMapLocal, ShaderProperties.triangleIDs, m_TrianglesInVoxels);
		m_Cmd.SetComputeBufferParam(m_computeShader, m_Kernels.generateRayMapLocal, ShaderProperties.trianglesUV, m_TrianglesUV);
		m_Cmd.BeginSample("BakeSDF.LocalRaymap");
		for (int i = 0; i < 8; i++)
		{
			m_OffsetRayMap[0] = i & 1;
			m_OffsetRayMap[1] = (i & 2) >> 1;
			m_OffsetRayMap[2] = (i & 4) >> 2;
			m_Cmd.SetComputeTextureParam(m_computeShader, m_Kernels.generateRayMapLocal, ShaderProperties.rayMap, GetRayMapPrincipal(m_RayMapUseCounter));
			m_Cmd.SetComputeTextureParam(m_computeShader, m_Kernels.generateRayMapLocal, ShaderProperties.rayMapTmp, GetRayMapBis(m_RayMapUseCounter));
			m_Cmd.SetComputeIntParams(m_computeShader, ShaderProperties.offsetRayMap, m_OffsetRayMap);
			m_Cmd.DispatchCompute(m_computeShader, m_Kernels.generateRayMapLocal, Mathf.CeilToInt((float)m_Dimensions[0] / 8f), Mathf.CeilToInt((float)m_Dimensions[1] / 8f), Mathf.CeilToInt((float)m_Dimensions[2] / 8f));
			m_RayMapUseCounter++;
		}
		m_Cmd.EndSample("BakeSDF.LocalRaymap");
		m_Cmd.BeginSample("BakeSDF.GlobalRaymap");
		m_Cmd.SetComputeTextureParam(m_computeShader, m_Kernels.rayMapScanX, ShaderProperties.rayMap, GetRayMapPrincipal(m_RayMapUseCounter));
		m_Cmd.SetComputeTextureParam(m_computeShader, m_Kernels.rayMapScanX, ShaderProperties.rayMapTmp, GetRayMapBis(m_RayMapUseCounter));
		m_Cmd.DispatchCompute(m_computeShader, m_Kernels.rayMapScanX, 1, Mathf.CeilToInt((float)m_Dimensions[1] / 8f), Mathf.CeilToInt((float)m_Dimensions[2] / 8f));
		m_RayMapUseCounter++;
		m_Cmd.SetComputeTextureParam(m_computeShader, m_Kernels.rayMapScanY, ShaderProperties.rayMap, GetRayMapPrincipal(m_RayMapUseCounter));
		m_Cmd.SetComputeTextureParam(m_computeShader, m_Kernels.rayMapScanY, ShaderProperties.rayMapTmp, GetRayMapBis(m_RayMapUseCounter));
		m_Cmd.DispatchCompute(m_computeShader, m_Kernels.rayMapScanY, Mathf.CeilToInt((float)m_Dimensions[0] / 8f), 1, Mathf.CeilToInt((float)m_Dimensions[2] / 8f));
		m_RayMapUseCounter++;
		m_Cmd.SetComputeTextureParam(m_computeShader, m_Kernels.rayMapScanZ, ShaderProperties.rayMap, GetRayMapPrincipal(m_RayMapUseCounter));
		m_Cmd.SetComputeTextureParam(m_computeShader, m_Kernels.rayMapScanZ, ShaderProperties.rayMapTmp, GetRayMapBis(m_RayMapUseCounter));
		m_Cmd.DispatchCompute(m_computeShader, m_Kernels.rayMapScanZ, Mathf.CeilToInt((float)m_Dimensions[0] / 8f), Mathf.CeilToInt((float)m_Dimensions[1] / 8f), 1);
		m_Cmd.EndSample("BakeSDF.GlobalRaymap");
		m_Cmd.EndSample("BakeSDF.Raymap");
	}

	private RenderTexture GetRayMapPrincipal(int step)
	{
		return m_RayMaps[step % 2];
	}

	private RenderTexture GetRayMapBis(int step)
	{
		return m_RayMaps[(step + 1) % 2];
	}

	private RenderTexture GetSignMapPrincipal(int step)
	{
		return m_SignMaps[step % 2];
	}

	private RenderTexture GetSignMapBis(int step)
	{
		return m_SignMaps[(step + 1) % 2];
	}

	private void SignPass()
	{
		m_Cmd.BeginSample("BakeSDF.SignPass");
		m_Cmd.SetComputeTextureParam(m_computeShader, m_Kernels.signPass6Rays, ShaderProperties.rayMap, GetRayMapPrincipal(m_RayMapUseCounter));
		m_Cmd.SetComputeTextureParam(m_computeShader, m_Kernels.signPass6Rays, ShaderProperties.signMap, GetSignMapPrincipal(0));
		m_Cmd.DispatchCompute(m_computeShader, m_Kernels.signPass6Rays, Mathf.CeilToInt((float)m_Dimensions[0] / 4f), Mathf.CeilToInt((float)m_Dimensions[1] / 4f), Mathf.CeilToInt((float)m_Dimensions[2] / 4f));
		m_Cmd.SetComputeTextureParam(m_computeShader, m_Kernels.signPassNeighbors, ShaderProperties.rayMap, GetRayMapPrincipal(m_RayMapUseCounter));
		int num = 8;
		float num2 = 6f;
		m_Cmd.SetComputeFloatParam(m_computeShader, ShaderProperties.normalizeFactor, num2);
		m_Cmd.SetComputeIntParam(m_computeShader, ShaderProperties.numNeighbours, num);
		int signPassesCount = m_SignPassesCount;
		for (int i = 1; i <= signPassesCount; i++)
		{
			m_Cmd.SetComputeIntParam(m_computeShader, ShaderProperties.passId, i);
			m_Cmd.SetComputeFloatParam(m_computeShader, ShaderProperties.normalizeFactor, num2);
			m_Cmd.SetComputeTextureParam(m_computeShader, m_Kernels.signPassNeighbors, ShaderProperties.signMap, GetSignMapPrincipal(i));
			m_Cmd.SetComputeTextureParam(m_computeShader, m_Kernels.signPassNeighbors, ShaderProperties.signMapTmp, GetSignMapBis(i));
			m_Cmd.SetComputeIntParam(m_computeShader, ShaderProperties.needNormalize, (i == signPassesCount) ? 1 : 0);
			m_Cmd.DispatchCompute(m_computeShader, m_Kernels.signPassNeighbors, Mathf.CeilToInt((float)m_Dimensions[0] / 4f), Mathf.CeilToInt((float)m_Dimensions[1] / 4f), Mathf.CeilToInt((float)m_Dimensions[2] / 4f));
			num2 += (float)(num * 6) * num2;
		}
		m_Cmd.EndSample("BakeSDF.SignPass");
	}

	public void BakeSDF()
	{
		if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3)
		{
			throw new NotSupportedException("MeshToSDFBaker compute shaders are not supported on OpenGLES3");
		}
		m_Cmd.BeginSample("BakeSDF");
		UpdateCameras();
		m_Cmd.SetComputeIntParams(m_computeShader, ShaderProperties.size, m_Dimensions);
		CreateGraphicsBufferIfNeeded(ref m_bufferVoxel, GetTotalVoxelCount(), 16);
		InitPrefixSumBuffers();
		InitMeshBuffers();
		int num = (int)Mathf.Pow(m_maxResolution, 2f) * (int)Mathf.Pow(nTriangles, 0.5f);
		num = (int)Mathf.Max((long)nTriangles * 30L, num);
		num = Mathf.Min(402653184, num);
		m_Cmd.SetComputeIntParam(m_computeShader, ShaderProperties.upperBoundCount, num);
		ClearRenderTexturesAndBuffers();
		InitGeometryBuffers(num);
		BuildGeometry();
		FirstDraw();
		PrefixSumCount();
		SecondDraw();
		GenerateRayMap();
		SignPass();
		SurfaceClosing();
		JFA();
		PerformDistanceTransformWinding();
		m_Cmd.EndSample("BakeSDF");
		if (m_OwnsCommandBuffer)
		{
			m_Cmd.ClearRandomWriteTargets();
			Graphics.ExecuteCommandBuffer(m_Cmd);
			m_Cmd.Clear();
		}
	}

	private void InitMeshBuffers()
	{
		if (m_Mesh.GetVertexAttributeFormat(VertexAttribute.Position) != 0)
		{
			throw new ArgumentException("The SDF Baker only supports the VertexAttributeFormat Float32 for the Position attribute.");
		}
		int vertexAttributeStream = m_Mesh.GetVertexAttributeStream(VertexAttribute.Position);
		m_VertexBufferOffset = m_Mesh.GetVertexAttributeOffset(VertexAttribute.Position);
		m_VerticesBuffer?.Dispose();
		m_IndicesBuffer?.Dispose();
		m_VerticesBuffer = m_Mesh.GetVertexBuffer(vertexAttributeStream);
		m_IndicesBuffer = m_Mesh.GetIndexBuffer();
		nTriangles = 0;
		for (int i = 0; i < m_Mesh.subMeshCount; i++)
		{
			nTriangles += m_Mesh.GetSubMesh(i).indexCount;
		}
		nTriangles /= 3;
	}

	private void FirstDraw()
	{
		m_Cmd.BeginSample("BakeSDF.FirstDraw");
		for (int i = 0; i < 3; i++)
		{
			m_Material[i].SetInt(ShaderProperties.dimX, m_Dimensions[0]);
			m_Material[i].SetInt(ShaderProperties.dimY, m_Dimensions[1]);
			m_Material[i].SetInt(ShaderProperties.dimZ, m_Dimensions[2]);
			m_Material[i].SetInt(ShaderProperties.currentAxis, i);
			m_Material[i].SetBuffer(ShaderProperties.verticesBuffer, m_VerticesOutBuffer);
			m_Material[i].SetBuffer(ShaderProperties.coordFlipBuffer, m_CoordFlipBuffer);
		}
		for (int j = 0; j < 3; j++)
		{
			m_Cmd.ClearRandomWriteTargets();
			m_Cmd.SetRenderTarget(m_RenderTextureViews[j]);
			m_Cmd.ClearRenderTarget(clearDepth: true, clearColor: true, Color.black, 1f);
			m_Cmd.SetRandomWriteTarget(4 + kNbActualRT, m_AabbBuffer, preserveCounterValue: false);
			m_Cmd.SetRandomWriteTarget(1 + kNbActualRT, m_bufferVoxel, preserveCounterValue: false);
			m_Cmd.SetRandomWriteTarget(2 + kNbActualRT, m_CounterBuffer, preserveCounterValue: false);
			m_Cmd.SetViewProjectionMatrices(m_ViewMat[j], m_ProjMat[j]);
			m_Cmd.DrawProcedural(Matrix4x4.identity, m_Material[j], 0, MeshTopology.Triangles, nTriangles * 3);
		}
		m_Cmd.ClearRandomWriteTargets();
		m_Cmd.EndSample("BakeSDF.FirstDraw");
	}

	private void SecondDraw()
	{
		m_Cmd.BeginSample("BakeSDF.SecondDraw");
		for (int i = 0; i < 3; i++)
		{
			m_Cmd.ClearRandomWriteTargets();
			m_Cmd.SetRenderTarget(m_RenderTextureViews[i]);
			m_Cmd.ClearRenderTarget(clearDepth: true, clearColor: true, Color.black, 1f);
			m_Cmd.SetRandomWriteTarget(4 + kNbActualRT, m_AabbBuffer, preserveCounterValue: false);
			m_Cmd.SetRandomWriteTarget(3 + kNbActualRT, m_TrianglesInVoxels, preserveCounterValue: false);
			m_Cmd.SetRandomWriteTarget(2 + kNbActualRT, m_AccumCounterBuffer, preserveCounterValue: false);
			m_Cmd.SetViewProjectionMatrices(m_ViewMat[i], m_ProjMat[i]);
			m_Cmd.DrawProcedural(Matrix4x4.identity, m_Material[i], 1, MeshTopology.Triangles, nTriangles * 3);
		}
		m_Cmd.ClearRandomWriteTargets();
		m_Cmd.EndSample("BakeSDF.SecondDraw");
	}

	private void BuildGeometry()
	{
		m_Cmd.BeginSample("BakeSDF.FakeGeometryShader");
		Vector3 vector = m_Center - m_SizeBox * 0.5f;
		Vector3 vector2 = m_Center + m_SizeBox * 0.5f;
		for (int i = 0; i < 3; i++)
		{
			m_MinBoundsExtended[i] = vector[i];
			m_MaxBoundsExtended[i] = vector2[i];
		}
		m_Cmd.SetComputeFloatParams(m_computeShader, ShaderProperties.minBoundsExtended, m_MinBoundsExtended);
		m_Cmd.SetComputeFloatParams(m_computeShader, ShaderProperties.maxBoundsExtended, m_MaxBoundsExtended);
		m_Cmd.SetComputeFloatParam(m_computeShader, ShaderProperties.maxExtent, m_MaxExtent);
		m_Cmd.SetComputeIntParam(m_computeShader, ShaderProperties.nTriangles, nTriangles);
		m_Cmd.SetComputeIntParam(m_computeShader, ShaderProperties.vertexPositionOffset, m_VertexBufferOffset);
		m_Cmd.SetComputeIntParam(m_computeShader, ShaderProperties.vertexStride, m_VerticesBuffer.stride);
		m_Cmd.SetComputeIntParam(m_computeShader, ShaderProperties.indexStride, m_IndicesBuffer.stride);
		m_Cmd.SetComputeBufferParam(m_computeShader, m_Kernels.chooseDirectionTriangleOnly, ShaderProperties.indicesBuffer, m_IndicesBuffer);
		m_Cmd.SetComputeBufferParam(m_computeShader, m_Kernels.chooseDirectionTriangleOnly, ShaderProperties.verticesBuffer, m_VerticesBuffer);
		m_Cmd.SetComputeBufferParam(m_computeShader, m_Kernels.chooseDirectionTriangleOnly, ShaderProperties.coordFlipBuffer, m_CoordFlipBuffer);
		m_Cmd.SetComputeBufferParam(m_computeShader, m_Kernels.conservativeRasterization, ShaderProperties.indicesBuffer, m_IndicesBuffer);
		m_Cmd.SetComputeBufferParam(m_computeShader, m_Kernels.conservativeRasterization, ShaderProperties.verticesBuffer, m_VerticesBuffer);
		m_Cmd.SetComputeBufferParam(m_computeShader, m_Kernels.conservativeRasterization, ShaderProperties.verticesOutBuffer, m_VerticesOutBuffer);
		m_Cmd.SetComputeBufferParam(m_computeShader, m_Kernels.conservativeRasterization, ShaderProperties.coordFlipBuffer, m_CoordFlipBuffer);
		m_Cmd.SetComputeBufferParam(m_computeShader, m_Kernels.conservativeRasterization, ShaderProperties.aabbBuffer, m_AabbBuffer);
		m_Cmd.SetComputeBufferParam(m_computeShader, m_Kernels.generateTrianglesUV, ShaderProperties.rw_trianglesUV, m_TrianglesUV);
		m_Cmd.SetComputeBufferParam(m_computeShader, m_Kernels.generateTrianglesUV, ShaderProperties.indicesBuffer, m_IndicesBuffer);
		m_Cmd.SetComputeBufferParam(m_computeShader, m_Kernels.generateTrianglesUV, ShaderProperties.verticesBuffer, m_VerticesBuffer);
		m_Cmd.DispatchCompute(m_computeShader, m_Kernels.generateTrianglesUV, Mathf.CeilToInt((float)nTriangles / 64f), 1, 1);
		m_Cmd.DispatchCompute(m_computeShader, m_Kernels.chooseDirectionTriangleOnly, Mathf.CeilToInt((float)nTriangles / 64f), 1, 1);
		for (int j = 0; j < 3; j++)
		{
			m_Cmd.SetComputeIntParam(m_computeShader, ShaderProperties.currentAxis, j);
			m_Cmd.SetComputeMatrixParam(m_computeShader, ShaderProperties.worldToClip, m_WorldToClip[j]);
			m_Cmd.DispatchCompute(m_computeShader, m_Kernels.conservativeRasterization, Mathf.CeilToInt((float)nTriangles / 64f), 1, 1);
		}
		m_Cmd.EndSample("BakeSDF.FakeGeometryShader");
	}

	private void InitGeometryBuffers(int upperBoundCount)
	{
		CreateGraphicsBufferIfNeeded(ref m_VerticesOutBuffer, 3 * nTriangles, 16);
		CreateGraphicsBufferIfNeeded(ref m_CoordFlipBuffer, nTriangles, 4);
		CreateGraphicsBufferIfNeeded(ref m_AabbBuffer, nTriangles, 16);
		CreateGraphicsBufferIfNeeded(ref m_TrianglesInVoxels, upperBoundCount, 4);
		CreateGraphicsBufferIfNeeded(ref m_TrianglesUV, nTriangles, 36);
	}

	private void InitPrefixSumBuffers()
	{
		CreateGraphicsBufferIfNeeded(ref m_CounterBuffer, GetTotalVoxelCount(), 4);
		CreateGraphicsBufferIfNeeded(ref m_AccumCounterBuffer, GetTotalVoxelCount(), 4);
		CreateGraphicsBufferIfNeeded(ref m_AccumSumBlocks, Mathf.CeilToInt((float)GetTotalVoxelCount() / (float)m_ThreadGroupSize), 4);
		CreateGraphicsBufferIfNeeded(ref m_SumBlocksBuffer, Mathf.CeilToInt((float)GetTotalVoxelCount() / (float)m_ThreadGroupSize), 4);
		CreateGraphicsBufferIfNeeded(ref m_InSumBlocksBuffer, Mathf.CeilToInt((float)GetTotalVoxelCount() / (float)m_ThreadGroupSize), 4);
		CreateGraphicsBufferIfNeeded(ref m_TmpBuffer, GetTotalVoxelCount(), 4);
		CreateGraphicsBufferIfNeeded(ref m_SumBlocksAdditional, Mathf.CeilToInt((float)GetTotalVoxelCount() / (float)(m_ThreadGroupSize * m_ThreadGroupSize)), 4);
	}

	private void ClearRenderTexturesAndBuffers()
	{
		m_Cmd.BeginSample("BakeSDF.ClearTexturesAndBuffers");
		m_Cmd.SetComputeTextureParam(m_computeShader, m_Kernels.clearTexturesAndBuffers, ShaderProperties.voxelsTexture, m_textureVoxel, 0);
		m_Cmd.SetComputeTextureParam(m_computeShader, m_Kernels.clearTexturesAndBuffers, ShaderProperties.voxelsTmpTexture, m_textureVoxelBis, 0);
		m_Cmd.SetComputeTextureParam(m_computeShader, m_Kernels.clearTexturesAndBuffers, ShaderProperties.rayMap, m_RayMaps[0], 0);
		m_Cmd.SetComputeTextureParam(m_computeShader, m_Kernels.clearTexturesAndBuffers, ShaderProperties.rw_rayMapTmp, m_RayMaps[1], 0);
		m_Cmd.SetComputeTextureParam(m_computeShader, m_Kernels.clearTexturesAndBuffers, ShaderProperties.signMap, m_SignMaps[0], 0);
		m_Cmd.SetComputeTextureParam(m_computeShader, m_Kernels.clearTexturesAndBuffers, ShaderProperties.signMapTmp, m_SignMaps[1]);
		m_Cmd.SetComputeBufferParam(m_computeShader, m_Kernels.clearTexturesAndBuffers, ShaderProperties.voxelsBuffer, m_bufferVoxel);
		m_Cmd.SetComputeBufferParam(m_computeShader, m_Kernels.clearTexturesAndBuffers, ShaderProperties.counter, m_CounterBuffer);
		m_Cmd.SetComputeBufferParam(m_computeShader, m_Kernels.clearTexturesAndBuffers, ShaderProperties.accumCounter, m_AccumCounterBuffer);
		m_Cmd.DispatchCompute(m_computeShader, m_Kernels.clearTexturesAndBuffers, Mathf.CeilToInt((float)m_Dimensions[0] / 8f), Mathf.CeilToInt((float)m_Dimensions[1] / 8f), Mathf.CeilToInt((float)m_Dimensions[2] / 8f));
		m_Cmd.EndSample("BakeSDF.ClearTexturesAndBuffers");
	}

	private void PerformDistanceTransformWinding()
	{
		m_Cmd.BeginSample("BakeSDF.DistanceTransform");
		m_Cmd.SetComputeFloatParam(m_computeShader, ShaderProperties.threshold, m_InOutThreshold);
		m_Cmd.SetComputeFloatParam(m_computeShader, ShaderProperties.sdfOffset, m_SdfOffset);
		m_Cmd.SetComputeTextureParam(m_computeShader, m_Kernels.distanceTransform, ShaderProperties.voxelsTexture, GetTextureVoxelPrincipal(m_nStepsJFA + 1));
		m_Cmd.SetComputeTextureParam(m_computeShader, m_Kernels.distanceTransform, ShaderProperties.distanceTexture, m_DistanceTexture);
		m_Cmd.SetComputeBufferParam(m_computeShader, m_Kernels.distanceTransform, ShaderProperties.accumCounter, m_AccumCounterBuffer);
		m_Cmd.SetComputeBufferParam(m_computeShader, m_Kernels.distanceTransform, ShaderProperties.triangleIDs, m_TrianglesInVoxels);
		m_Cmd.SetComputeBufferParam(m_computeShader, m_Kernels.distanceTransform, ShaderProperties.trianglesUV, m_TrianglesUV);
		m_Cmd.SetComputeTextureParam(m_computeShader, m_Kernels.distanceTransform, ShaderProperties.signMap, GetSignMapPrincipal(m_SignPassesCount));
		m_Cmd.DispatchCompute(m_computeShader, m_Kernels.distanceTransform, Mathf.CeilToInt((float)m_Dimensions[0] / 8f), Mathf.CeilToInt((float)m_Dimensions[1] / 8f), Mathf.CeilToInt((float)m_Dimensions[2] / 8f));
		m_Cmd.EndSample("BakeSDF.DistanceTransform");
	}

	private void ReleaseBuffersAndTextures()
	{
		ReleaseRenderTexture(ref m_textureVoxel);
		ReleaseRenderTexture(ref m_textureVoxelBis);
		ReleaseRenderTexture(ref m_DistanceTexture);
		for (int i = 0; i < 3; i++)
		{
			ReleaseRenderTexture(ref m_RenderTextureViews[i]);
			if (Application.isPlaying)
			{
				Object.Destroy(m_Material[i]);
			}
			else
			{
				Object.DestroyImmediate(m_Material[i]);
			}
		}
		for (int j = 0; j < 2; j++)
		{
			ReleaseRenderTexture(ref m_SignMaps[j]);
			ReleaseRenderTexture(ref m_RayMaps[j]);
		}
		ReleaseGraphicsBuffer(ref m_bufferVoxel);
		ReleaseGraphicsBuffer(ref m_TrianglesUV);
		ReleaseGraphicsBuffer(ref m_TrianglesInVoxels);
		ReleaseGraphicsBuffer(ref m_IndicesBuffer);
		ReleaseGraphicsBuffer(ref m_VerticesBuffer);
		ReleaseGraphicsBuffer(ref m_VerticesOutBuffer);
		ReleaseGraphicsBuffer(ref m_CoordFlipBuffer);
		ReleaseGraphicsBuffer(ref m_AabbBuffer);
		ReleaseGraphicsBuffer(ref m_TmpBuffer);
		ReleaseGraphicsBuffer(ref m_AccumSumBlocks);
		ReleaseGraphicsBuffer(ref m_SumBlocksBuffer);
		ReleaseGraphicsBuffer(ref m_InSumBlocksBuffer);
		ReleaseGraphicsBuffer(ref m_SumBlocksAdditional);
		ReleaseGraphicsBuffer(ref m_CounterBuffer);
		ReleaseGraphicsBuffer(ref m_AccumCounterBuffer);
	}

	public void Dispose()
	{
		ReleaseBuffersAndTextures();
		GC.SuppressFinalize(this);
		m_IsDisposed = true;
	}

	private void CreateGraphicsBufferIfNeeded(ref GraphicsBuffer gb, int length, int stride)
	{
		if (gb == null || gb.count != length || gb.stride != stride)
		{
			ReleaseGraphicsBuffer(ref gb);
			gb = new GraphicsBuffer(GraphicsBuffer.Target.Structured, length, stride);
			m_IsDisposed = false;
		}
	}

	private void ReleaseGraphicsBuffer(ref GraphicsBuffer gb)
	{
		if (gb != null)
		{
			gb.Release();
		}
		gb = null;
	}

	private void CreateRenderTextureIfNeeded(ref RenderTexture rt, RenderTextureDescriptor rtDesc)
	{
		if (!(rt != null) || rt.width != rtDesc.width || rt.height != rtDesc.height || rt.volumeDepth != rtDesc.volumeDepth || rt.graphicsFormat != rtDesc.graphicsFormat)
		{
			ReleaseRenderTexture(ref rt);
			rt = new RenderTexture(rtDesc);
			rt.hideFlags = HideFlags.DontSave;
			rt.Create();
			m_IsDisposed = false;
		}
	}

	private void ReleaseRenderTexture(ref RenderTexture rt)
	{
		if (rt != null)
		{
			rt.Release();
			if (Application.isPlaying)
			{
				Object.Destroy(rt);
			}
			else
			{
				Object.DestroyImmediate(rt);
			}
		}
		rt = null;
	}
}
