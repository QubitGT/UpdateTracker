using Unity.Mathematics;

namespace UnityEngine.Rendering.Universal;

internal static class LayerUtility
{
	private static LayerBatch[] s_LayerBatches;

	public static uint maxTextureCount { get; private set; }

	public static void InitializeBudget(uint maxTextureCount)
	{
		LayerUtility.maxTextureCount = math.max(4u, maxTextureCount);
	}

	private static bool CanBatchLightsInLayer(int layerIndex1, int layerIndex2, SortingLayer[] sortingLayers, ILight2DCullResult lightCullResult)
	{
		int id = sortingLayers[layerIndex1].id;
		int id2 = sortingLayers[layerIndex2].id;
		foreach (Light2D visibleLight in lightCullResult.visibleLights)
		{
			if (visibleLight.IsLitLayer(id) != visibleLight.IsLitLayer(id2))
			{
				return false;
			}
		}
		foreach (ShadowCasterGroup2D visibleShadow in lightCullResult.visibleShadows)
		{
			foreach (ShadowCaster2D shadowCaster in visibleShadow.GetShadowCasters())
			{
				if (shadowCaster.IsShadowedLayer(id) != shadowCaster.IsShadowedLayer(id2))
				{
					return false;
				}
			}
		}
		return true;
	}

	private static bool CanBatchCameraSortingLayer(int startLayerIndex, SortingLayer[] sortingLayers, Renderer2DData rendererData)
	{
		if (rendererData.useCameraSortingLayerTexture)
		{
			short cameraSortingLayerBoundsIndex = Render2DLightingPass.GetCameraSortingLayerBoundsIndex(rendererData);
			return sortingLayers[startLayerIndex].value == cameraSortingLayerBoundsIndex;
		}
		return false;
	}

	private static int FindUpperBoundInBatch(int startLayerIndex, SortingLayer[] sortingLayers, Renderer2DData rendererData)
	{
		if (CanBatchCameraSortingLayer(startLayerIndex, sortingLayers, rendererData))
		{
			return startLayerIndex;
		}
		for (int i = startLayerIndex + 1; i < sortingLayers.Length; i++)
		{
			if (!CanBatchLightsInLayer(startLayerIndex, i, sortingLayers, rendererData.lightCullResult))
			{
				return i - 1;
			}
			if (CanBatchCameraSortingLayer(i, sortingLayers, rendererData))
			{
				return i;
			}
		}
		return sortingLayers.Length - 1;
	}

	private static void InitializeBatchInfos(SortingLayer[] cachedSortingLayers)
	{
		int num = cachedSortingLayers.Length;
		bool num2 = s_LayerBatches == null;
		if (s_LayerBatches == null)
		{
			s_LayerBatches = new LayerBatch[num];
		}
		if (num2)
		{
			for (int i = 0; i < s_LayerBatches.Length; i++)
			{
				s_LayerBatches[i].InitRTIds(i);
			}
		}
	}

	public static LayerBatch[] CalculateBatches(Renderer2DData rendererData, out int batchCount)
	{
		SortingLayer[] cachedSortingLayer = Light2DManager.GetCachedSortingLayer();
		InitializeBatchInfos(cachedSortingLayer);
		bool flag = false;
		batchCount = 0;
		int num = 0;
		while (num < cachedSortingLayer.Length)
		{
			int id = cachedSortingLayer[num].id;
			ref LayerBatch reference = ref s_LayerBatches[batchCount++];
			LightStats lightStatsByLayer = rendererData.lightCullResult.GetLightStatsByLayer(id, ref reference);
			int num2 = FindUpperBoundInBatch(num, cachedSortingLayer, rendererData);
			short num3 = (short)cachedSortingLayer[num].value;
			short lowerBound = ((num == 0) ? short.MinValue : num3);
			short num4 = (short)cachedSortingLayer[num2].value;
			short upperBound = ((num2 == cachedSortingLayer.Length - 1) ? short.MaxValue : num4);
			SortingLayerRange layerRange = new SortingLayerRange(lowerBound, upperBound);
			reference.startLayerID = id;
			reference.endLayerValue = num4;
			reference.layerRange = layerRange;
			reference.lightStats = lightStatsByLayer;
			flag |= reference.lightStats.useNormalMap;
			num = num2 + 1;
		}
		for (int i = 0; i < batchCount; i++)
		{
			ref LayerBatch reference2 = ref s_LayerBatches[i];
			bool flag2 = SpriteMaskUtility.HasSpriteMaskInLayerRange(reference2.layerRange);
			reference2.useNormals = reference2.lightStats.useNormalMap || (flag && flag2);
		}
		SetupActiveBlendStyles();
		return s_LayerBatches;
	}

	public static void GetFilterSettings(Renderer2DData rendererData, ref LayerBatch layerBatch, out FilteringSettings filterSettings)
	{
		filterSettings = FilteringSettings.defaultValue;
		filterSettings.renderQueueRange = RenderQueueRange.all;
		filterSettings.layerMask = rendererData.layerMask;
		filterSettings.renderingLayerMask = uint.MaxValue;
		filterSettings.sortingLayerRange = layerBatch.layerRange;
	}

	private static void SetupActiveBlendStyles()
	{
		for (int i = 0; i < s_LayerBatches.Length; i++)
		{
			ref LayerBatch reference = ref s_LayerBatches[i];
			int num = 0;
			for (int j = 0; j < RendererLighting.k_ShapeLightTextureIDs.Length; j++)
			{
				uint num2 = (uint)(1 << j);
				if ((reference.lightStats.blendStylesUsed & num2) != 0)
				{
					num++;
				}
			}
			if (reference.activeBlendStylesIndices == null || reference.activeBlendStylesIndices.Length != num)
			{
				reference.activeBlendStylesIndices = new int[num];
			}
			int num3 = 0;
			for (int k = 0; k < RendererLighting.k_ShapeLightTextureIDs.Length; k++)
			{
				uint num4 = (uint)(1 << k);
				if ((reference.lightStats.blendStylesUsed & num4) != 0)
				{
					reference.activeBlendStylesIndices[num3++] = k;
				}
			}
		}
	}
}
