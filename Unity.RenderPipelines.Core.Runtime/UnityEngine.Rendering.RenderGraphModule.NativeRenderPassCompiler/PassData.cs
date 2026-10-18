using System;
using System.Runtime.CompilerServices;

namespace UnityEngine.Rendering.RenderGraphModule.NativeRenderPassCompiler;

internal struct PassData
{
	public int passId;

	public RenderGraphPassType type;

	public bool hasFoveatedRasterization;

	public int tag;

	public ShadingRateFragmentSize shadingRateFragmentSize;

	public ShadingRateCombiner primitiveShadingRateCombiner;

	public ShadingRateCombiner fragmentShadingRateCombiner;

	public PassMergeState mergeState;

	public int nativePassIndex;

	public int nativeSubPassIndex;

	public int firstInput;

	public int numInputs;

	public int firstOutput;

	public int numOutputs;

	public int firstFragment;

	public int numFragments;

	public int firstFragmentInput;

	public int numFragmentInputs;

	public int firstRandomAccessResource;

	public int numRandomAccessResources;

	public int firstCreate;

	public int numCreated;

	public int firstDestroy;

	public int numDestroyed;

	public int shadingRateImageIndex;

	public int fragmentInfoWidth;

	public int fragmentInfoHeight;

	public int fragmentInfoVolumeDepth;

	public int fragmentInfoSamples;

	public int waitOnGraphicsFencePassId;

	public bool asyncCompute;

	public bool hasSideEffects;

	public bool culled;

	public bool beginNativeSubpass;

	public bool fragmentInfoValid;

	public bool fragmentInfoHasDepth;

	public bool insertGraphicsFence;

	public bool hasShadingRateStates;

	public bool fragmentInfoHasShadingRateImage => shadingRateImageIndex > 0;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Name GetName(CompilerContextData ctx)
	{
		return ctx.GetFullPassName(passId);
	}

	public PassData(in RenderGraphPass pass, int passIndex)
	{
		passId = passIndex;
		type = pass.type;
		asyncCompute = pass.enableAsyncCompute;
		hasSideEffects = !pass.allowPassCulling;
		hasFoveatedRasterization = pass.enableFoveatedRasterization;
		mergeState = PassMergeState.None;
		nativePassIndex = -1;
		nativeSubPassIndex = -1;
		beginNativeSubpass = false;
		culled = false;
		tag = 0;
		firstInput = 0;
		numInputs = 0;
		firstOutput = 0;
		numOutputs = 0;
		firstFragment = 0;
		numFragments = 0;
		firstRandomAccessResource = 0;
		numRandomAccessResources = 0;
		firstFragmentInput = 0;
		numFragmentInputs = 0;
		firstCreate = 0;
		numCreated = 0;
		firstDestroy = 0;
		numDestroyed = 0;
		fragmentInfoValid = false;
		fragmentInfoWidth = 0;
		fragmentInfoHeight = 0;
		fragmentInfoVolumeDepth = 0;
		fragmentInfoSamples = 0;
		fragmentInfoHasDepth = false;
		insertGraphicsFence = false;
		waitOnGraphicsFencePassId = -1;
		hasShadingRateStates = pass.hasShadingRateStates;
		shadingRateFragmentSize = pass.shadingRateFragmentSize;
		primitiveShadingRateCombiner = pass.primitiveShadingRateCombiner;
		fragmentShadingRateCombiner = pass.fragmentShadingRateCombiner;
		shadingRateImageIndex = -1;
	}

	public void ResetAndInitialize(in RenderGraphPass pass, int passIndex)
	{
		passId = passIndex;
		type = pass.type;
		asyncCompute = pass.enableAsyncCompute;
		hasSideEffects = !pass.allowPassCulling;
		hasFoveatedRasterization = pass.enableFoveatedRasterization;
		mergeState = PassMergeState.None;
		nativePassIndex = -1;
		nativeSubPassIndex = -1;
		beginNativeSubpass = false;
		culled = false;
		tag = 0;
		firstInput = 0;
		numInputs = 0;
		firstOutput = 0;
		numOutputs = 0;
		firstFragment = 0;
		numFragments = 0;
		firstFragmentInput = 0;
		numFragmentInputs = 0;
		firstRandomAccessResource = 0;
		numRandomAccessResources = 0;
		firstCreate = 0;
		numCreated = 0;
		firstDestroy = 0;
		numDestroyed = 0;
		fragmentInfoValid = false;
		fragmentInfoWidth = 0;
		fragmentInfoHeight = 0;
		fragmentInfoVolumeDepth = 0;
		fragmentInfoSamples = 0;
		fragmentInfoHasDepth = false;
		insertGraphicsFence = false;
		waitOnGraphicsFencePassId = -1;
		hasShadingRateStates = pass.hasShadingRateStates;
		shadingRateFragmentSize = pass.shadingRateFragmentSize;
		primitiveShadingRateCombiner = pass.primitiveShadingRateCombiner;
		fragmentShadingRateCombiner = pass.fragmentShadingRateCombiner;
		shadingRateImageIndex = -1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly ReadOnlySpan<PassOutputData> Outputs(CompilerContextData ctx)
	{
		return NativeListExtensions.MakeReadOnlySpan(ref ctx.outputData, firstOutput, numOutputs);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly ReadOnlySpan<PassInputData> Inputs(CompilerContextData ctx)
	{
		return NativeListExtensions.MakeReadOnlySpan(ref ctx.inputData, firstInput, numInputs);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly ReadOnlySpan<PassFragmentData> Fragments(CompilerContextData ctx)
	{
		return NativeListExtensions.MakeReadOnlySpan(ref ctx.fragmentData, firstFragment, numFragments);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly PassFragmentData ShadingRateImage(CompilerContextData ctx)
	{
		return ctx.fragmentData[shadingRateImageIndex];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly ReadOnlySpan<PassFragmentData> FragmentInputs(CompilerContextData ctx)
	{
		return NativeListExtensions.MakeReadOnlySpan(ref ctx.fragmentData, firstFragmentInput, numFragmentInputs);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly ReadOnlySpan<ResourceHandle> FirstUsedResources(CompilerContextData ctx)
	{
		return NativeListExtensions.MakeReadOnlySpan(ref ctx.createData, firstCreate, numCreated);
	}

	public ReadOnlySpan<PassRandomWriteData> RandomWriteTextures(CompilerContextData ctx)
	{
		return NativeListExtensions.MakeReadOnlySpan(ref ctx.randomAccessResourceData, firstRandomAccessResource, numRandomAccessResources);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly ReadOnlySpan<ResourceHandle> LastUsedResources(CompilerContextData ctx)
	{
		return NativeListExtensions.MakeReadOnlySpan(ref ctx.destroyData, firstDestroy, numDestroyed);
	}

	private void SetupAndValidateFragmentInfo(ResourceHandle h, CompilerContextData ctx)
	{
		ref ResourceUnversionedData reference = ref ctx.UnversionedResourceData(h);
		if (!fragmentInfoValid)
		{
			fragmentInfoWidth = reference.width;
			fragmentInfoHeight = reference.height;
			fragmentInfoSamples = reference.msaaSamples;
			fragmentInfoVolumeDepth = reference.volumeDepth;
			fragmentInfoValid = true;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void AddFragment(ResourceHandle h, CompilerContextData ctx)
	{
		SetupAndValidateFragmentInfo(h, ctx);
		numFragments++;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void AddFragmentInput(ResourceHandle h, CompilerContextData ctx)
	{
		SetupAndValidateFragmentInfo(h, ctx);
		numFragmentInputs++;
	}

	internal void AddRandomAccessResource()
	{
		numRandomAccessResources++;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void AddFirstUse(ResourceHandle h, CompilerContextData ctx)
	{
		ReadOnlySpan<ResourceHandle> readOnlySpan = FirstUsedResources(ctx);
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			ref readonly ResourceHandle reference = ref readOnlySpan[i];
			if (reference.index == h.index && reference.type == h.type)
			{
				return;
			}
		}
		ctx.createData.Add(in h);
		int num = NativeListExtensions.LastIndex(ref ctx.createData);
		if (numCreated == 0)
		{
			firstCreate = num;
		}
		numCreated++;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void AddLastUse(ResourceHandle h, CompilerContextData ctx)
	{
		ReadOnlySpan<ResourceHandle> readOnlySpan = LastUsedResources(ctx);
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			ref readonly ResourceHandle reference = ref readOnlySpan[i];
			if (reference.index == h.index && reference.type == h.type)
			{
				return;
			}
		}
		ctx.destroyData.Add(in h);
		int num = NativeListExtensions.LastIndex(ref ctx.destroyData);
		if (numDestroyed == 0)
		{
			firstDestroy = num;
		}
		numDestroyed++;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal readonly bool IsUsedAsFragment(ResourceHandle h, CompilerContextData ctx)
	{
		if (h.type != 0)
		{
			return false;
		}
		if (type != RenderGraphPassType.Raster)
		{
			return false;
		}
		ReadOnlySpan<PassFragmentData> readOnlySpan = Fragments(ctx);
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			if (readOnlySpan[i].resource.index == h.index)
			{
				return true;
			}
		}
		readOnlySpan = FragmentInputs(ctx);
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			if (readOnlySpan[i].resource.index == h.index)
			{
				return true;
			}
		}
		return false;
	}
}
