using System;

namespace UnityEngine.Rendering;

internal class VrsResources : IDisposable
{
	internal ProfilingSampler conversionProfilingSampler = new ProfilingSampler("VrsConversion");

	internal ProfilingSampler visualizationProfilingSampler = new ProfilingSampler("VrsVisualization");

	internal GraphicsBuffer conversionLutBuffer;

	internal GraphicsBuffer visualizationLutBuffer;

	internal ComputeShader textureComputeShader;

	internal int textureReduceKernel = -1;

	internal int textureCopyKernel = -1;

	internal Vector2Int tileSize;

	internal GraphicsBuffer validatedShadingRateFragmentSizeBuffer;

	private Shader m_VisualizationShader;

	private Material m_VisualizationMaterial;

	internal Material visualizationMaterial
	{
		get
		{
			if (m_VisualizationMaterial == null)
			{
				m_VisualizationMaterial = new Material(m_VisualizationShader);
			}
			return m_VisualizationMaterial;
		}
	}

	internal VrsResources(VrsRenderPipelineRuntimeResources resources)
	{
		InitializeResources(resources);
	}

	~VrsResources()
	{
		Dispose();
		GC.SuppressFinalize(this);
	}

	public void Dispose()
	{
		DisposeResources();
	}

	private void InitializeResources(VrsRenderPipelineRuntimeResources resources)
	{
		if (!InitComputeShader(resources))
		{
			DisposeResources();
			return;
		}
		m_VisualizationShader = resources.visualizationShader;
		conversionLutBuffer = resources.conversionLookupTable.CreateBuffer();
		visualizationLutBuffer = resources.visualizationLookupTable.CreateBuffer(forVisualization: true);
		AllocFragmentSizeBuffer();
	}

	private void DisposeResources()
	{
		conversionLutBuffer?.Dispose();
		conversionLutBuffer = null;
		visualizationLutBuffer?.Dispose();
		visualizationLutBuffer = null;
		validatedShadingRateFragmentSizeBuffer?.Dispose();
		validatedShadingRateFragmentSizeBuffer = null;
		m_VisualizationShader = null;
		m_VisualizationMaterial = null;
	}

	private void AllocFragmentSizeBuffer()
	{
		uint[] array = new uint[Vrs.shadingRateFragmentSizeCount];
		ShadingRateFragmentSize shadingRateFragmentSize = ShadingRateFragmentSize.FragmentSize1x1;
		uint value = ShadingRateInfo.QueryNativeValue(shadingRateFragmentSize);
		ShadingRateFragmentSize[] availableFragmentSizes = ShadingRateInfo.availableFragmentSizes;
		foreach (ShadingRateFragmentSize shadingRateFragmentSize2 in availableFragmentSizes)
		{
			Array.Fill(array, value, (int)shadingRateFragmentSize, shadingRateFragmentSize2 - shadingRateFragmentSize + 1);
			shadingRateFragmentSize = shadingRateFragmentSize2;
			value = ShadingRateInfo.QueryNativeValue(shadingRateFragmentSize);
		}
		Array.Fill(array, value, (int)shadingRateFragmentSize, (int)(8 - shadingRateFragmentSize + 1));
		validatedShadingRateFragmentSizeBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, array.Length, 4);
		validatedShadingRateFragmentSizeBuffer.SetData(array);
	}

	private bool InitComputeShader(VrsRenderPipelineRuntimeResources resources)
	{
		if (!ShadingRateInfo.supportsPerImageTile)
		{
			return false;
		}
		if (!SystemInfo.supportsComputeShaders)
		{
			return false;
		}
		tileSize = ShadingRateInfo.imageTileSize;
		if (tileSize.x != tileSize.y || (tileSize.x != 8 && tileSize.x != 16 && tileSize.x != 32))
		{
			Debug.LogError($"VRS unsupported tile size: {tileSize.x}x{tileSize.y}.");
			return false;
		}
		ComputeShader computeShader = resources.textureComputeShader;
		if ((object)computeShader != null && computeShader.keywordSpace.keywordCount == 0)
		{
			textureReduceKernel = -1;
			textureCopyKernel = -1;
			return false;
		}
		textureComputeShader = resources.textureComputeShader;
		textureComputeShader.EnableKeyword(string.Format("{0}{1}", "VRS_TILE_SIZE_", tileSize.x));
		textureReduceKernel = TryFindKernel(textureComputeShader, "TextureReduce");
		textureCopyKernel = TryFindKernel(textureComputeShader, "TextureReduce");
		if (textureReduceKernel == -1 || textureCopyKernel == -1)
		{
			return false;
		}
		return true;
	}

	private static int TryFindKernel(ComputeShader computeShader, string name)
	{
		if (!computeShader.HasKernel(name))
		{
			return -1;
		}
		return computeShader.FindKernel(name);
	}
}
