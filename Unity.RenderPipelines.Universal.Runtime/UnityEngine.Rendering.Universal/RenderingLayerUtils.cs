using System;
using System.Collections.Generic;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.Universal;

internal static class RenderingLayerUtils
{
	public enum Event
	{
		DepthNormalPrePass,
		Opaque
	}

	public enum MaskSize
	{
		Bits8,
		Bits16,
		Bits24,
		Bits32
	}

	public static void CombineRendererEvents(bool isDeferred, int msaaSampleCount, Event rendererEvent, ref Event combinedEvent)
	{
		if (msaaSampleCount > 1 && !isDeferred)
		{
			combinedEvent = Event.DepthNormalPrePass;
		}
		else
		{
			combinedEvent = Combine(combinedEvent, rendererEvent);
		}
	}

	public static bool RequireRenderingLayers(UniversalRenderer universalRenderer, List<ScriptableRendererFeature> rendererFeatures, int msaaSampleCount, out Event combinedEvent, out MaskSize combinedMaskSize)
	{
		RenderingMode renderingModeActual = universalRenderer.renderingModeActual;
		bool accurateGbufferNormals = universalRenderer.accurateGbufferNormals;
		return RequireRenderingLayers(rendererFeatures, renderingModeActual, accurateGbufferNormals, msaaSampleCount, out combinedEvent, out combinedMaskSize);
	}

	internal static bool RequireRenderingLayers(List<ScriptableRendererFeature> rendererFeatures, RenderingMode renderingMode, bool accurateGbufferNormals, int msaaSampleCount, out Event combinedEvent, out MaskSize combinedMaskSize)
	{
		combinedEvent = Event.Opaque;
		combinedMaskSize = MaskSize.Bits8;
		bool isDeferred = renderingMode == RenderingMode.Deferred || renderingMode == RenderingMode.DeferredPlus;
		bool flag = false;
		foreach (ScriptableRendererFeature rendererFeature in rendererFeatures)
		{
			if (rendererFeature.isActive)
			{
				flag |= rendererFeature.RequireRenderingLayers(isDeferred, accurateGbufferNormals, out var atEvent, out var maskSize);
				combinedEvent = Combine(combinedEvent, atEvent);
				combinedMaskSize = Combine(combinedMaskSize, maskSize);
			}
		}
		if (msaaSampleCount > 1 && combinedEvent == Event.Opaque)
		{
			combinedEvent = Event.DepthNormalPrePass;
		}
		if ((bool)RenderPipelineGlobalSettings<UniversalRenderPipelineGlobalSettings, UniversalRenderPipeline>.instance)
		{
			MaskSize maskSize2 = GetMaskSize(RenderingLayerMask.GetRenderingLayerCount());
			combinedMaskSize = Combine(combinedMaskSize, maskSize2);
		}
		return flag;
	}

	public static void SetupProperties(CommandBuffer cmd, MaskSize maskSize)
	{
		SetupProperties(CommandBufferHelpers.GetRasterCommandBuffer(cmd), maskSize);
	}

	internal static void SetupProperties(RasterCommandBuffer cmd, MaskSize maskSize)
	{
		int bits = GetBits(maskSize);
		uint value = ((bits != 32) ? ((uint)((1 << bits) - 1)) : uint.MaxValue);
		cmd.SetGlobalInt(ShaderPropertyId.renderingLayerMaxInt, (int)value);
	}

	public static GraphicsFormat GetFormat(MaskSize maskSize)
	{
		switch (maskSize)
		{
		case MaskSize.Bits8:
			return GraphicsFormat.R8_UInt;
		case MaskSize.Bits16:
			return GraphicsFormat.R16_UInt;
		case MaskSize.Bits24:
		case MaskSize.Bits32:
			return GraphicsFormat.R32_UInt;
		default:
			throw new NotImplementedException();
		}
	}

	public static uint ToValidRenderingLayers(uint renderingLayers)
	{
		if ((bool)RenderPipelineGlobalSettings<UniversalRenderPipelineGlobalSettings, UniversalRenderPipeline>.instance)
		{
			return RenderingLayerMask.GetDefinedRenderingLayersCombinedMaskValue() & renderingLayers;
		}
		return renderingLayers;
	}

	private static MaskSize GetMaskSize(int bits)
	{
		return ((bits + 7) / 8) switch
		{
			0 => MaskSize.Bits8, 
			1 => MaskSize.Bits8, 
			2 => MaskSize.Bits16, 
			3 => MaskSize.Bits24, 
			4 => MaskSize.Bits32, 
			_ => MaskSize.Bits32, 
		};
	}

	private static int GetBits(MaskSize maskSize)
	{
		return maskSize switch
		{
			MaskSize.Bits8 => 8, 
			MaskSize.Bits16 => 16, 
			MaskSize.Bits24 => 24, 
			MaskSize.Bits32 => 32, 
			_ => throw new NotImplementedException(), 
		};
	}

	private static Event Combine(Event a, Event b)
	{
		return (Event)Mathf.Min((int)a, (int)b);
	}

	private static MaskSize Combine(MaskSize a, MaskSize b)
	{
		return (MaskSize)Mathf.Max((int)a, (int)b);
	}
}
