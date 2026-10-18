using System.Collections.Generic;

namespace UnityEngine.Rendering.Universal;

internal struct LayerBatch
{
	public int startLayerID;

	public int endLayerValue;

	public SortingLayerRange layerRange;

	public LightStats lightStats;

	public bool useNormals;

	private unsafe fixed int renderTargetIds[4];

	private unsafe fixed bool renderTargetUsed[4];

	public List<Light2D> lights;

	public List<int> shadowIndices;

	public List<ShadowCasterGroup2D> shadowCasters;

	internal int[] activeBlendStylesIndices;

	public unsafe void InitRTIds(int index)
	{
		for (int i = 0; i < 4; i++)
		{
			renderTargetUsed[i] = false;
			renderTargetIds[i] = Shader.PropertyToID($"_LightTexture_{index}_{i}");
		}
		lights = new List<Light2D>();
		shadowIndices = new List<int>();
		shadowCasters = new List<ShadowCasterGroup2D>();
	}

	public unsafe RenderTargetIdentifier GetRTId(CommandBuffer cmd, RenderTextureDescriptor desc, int index)
	{
		if (!renderTargetUsed[index])
		{
			cmd.GetTemporaryRT(renderTargetIds[index], desc, FilterMode.Bilinear);
			renderTargetUsed[index] = true;
		}
		return new RenderTargetIdentifier(renderTargetIds[index]);
	}

	public unsafe void ReleaseRT(CommandBuffer cmd)
	{
		for (int i = 0; i < 4; i++)
		{
			if (renderTargetUsed[i])
			{
				cmd.ReleaseTemporaryRT(renderTargetIds[i]);
				renderTargetUsed[i] = false;
			}
		}
	}
}
