using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace UnityEngine.Rendering.RenderGraphModule;

[DebuggerDisplay("TextureResource ({desc.name})")]
internal class TextureResource : RenderGraphResource<TextureDesc, RTHandle>
{
	private static int m_TextureCreationIndex;

	public override string GetName()
	{
		if (imported && !shared)
		{
			if (graphicsResource == null)
			{
				return "null resource";
			}
			return graphicsResource.name;
		}
		return desc.name;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override int GetDescHashCode()
	{
		return desc.GetHashCode();
	}

	public override void CreateGraphicsResource()
	{
		string text = GetName();
		if (text == "")
		{
			text = $"RenderGraphTexture_{m_TextureCreationIndex++}";
		}
		RTHandleAllocInfo rTHandleAllocInfo = new RTHandleAllocInfo(text);
		rTHandleAllocInfo.slices = desc.slices;
		rTHandleAllocInfo.format = desc.format;
		rTHandleAllocInfo.filterMode = desc.filterMode;
		rTHandleAllocInfo.wrapModeU = desc.wrapMode;
		rTHandleAllocInfo.wrapModeV = desc.wrapMode;
		rTHandleAllocInfo.wrapModeW = desc.wrapMode;
		rTHandleAllocInfo.dimension = desc.dimension;
		rTHandleAllocInfo.enableRandomWrite = desc.enableRandomWrite;
		rTHandleAllocInfo.useMipMap = desc.useMipMap;
		rTHandleAllocInfo.autoGenerateMips = desc.autoGenerateMips;
		rTHandleAllocInfo.anisoLevel = desc.anisoLevel;
		rTHandleAllocInfo.mipMapBias = desc.mipMapBias;
		rTHandleAllocInfo.isShadowMap = desc.isShadowMap;
		rTHandleAllocInfo.msaaSamples = desc.msaaSamples;
		rTHandleAllocInfo.bindTextureMS = desc.bindTextureMS;
		rTHandleAllocInfo.useDynamicScale = desc.useDynamicScale;
		rTHandleAllocInfo.useDynamicScaleExplicit = desc.useDynamicScaleExplicit;
		rTHandleAllocInfo.memoryless = desc.memoryless;
		rTHandleAllocInfo.vrUsage = desc.vrUsage;
		rTHandleAllocInfo.enableShadingRate = desc.enableShadingRate;
		RTHandleAllocInfo info = rTHandleAllocInfo;
		switch (desc.sizeMode)
		{
		case TextureSizeMode.Explicit:
			graphicsResource = RTHandles.Alloc(desc.width, desc.height, info);
			break;
		case TextureSizeMode.Scale:
			graphicsResource = RTHandles.Alloc(desc.scale, info);
			break;
		case TextureSizeMode.Functor:
			graphicsResource = RTHandles.Alloc(desc.func, info);
			break;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override void UpdateGraphicsResource()
	{
		if (graphicsResource != null)
		{
			graphicsResource.m_Name = GetName();
		}
	}

	public override void ReleaseGraphicsResource()
	{
		if (graphicsResource != null)
		{
			graphicsResource.Release();
		}
		base.ReleaseGraphicsResource();
	}

	public override void LogCreation(RenderGraphLogger logger)
	{
		logger.LogLine($"Created Texture: {desc.name} (Cleared: {desc.clearBuffer})");
	}

	public override void LogRelease(RenderGraphLogger logger)
	{
		logger.LogLine("Released Texture: " + desc.name);
	}
}
