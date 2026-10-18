using System;
using UnityEngine;

namespace Meta.XR.EnvironmentDepth;

internal class DepthProviderNotSupported : IDepthProvider
{
	bool IDepthProvider.IsSupported => false;

	bool IDepthProvider.RemoveHands
	{
		set
		{
		}
	}

	void IDepthProvider.SetDepthEnabled(bool isEnabled, bool removeHands)
	{
	}

	bool IDepthProvider.TryGetUpdatedDepthTexture(out RenderTexture depthTexture, DepthFrameDesc[] frameDescriptors)
	{
		throw new NotSupportedException();
	}
}
