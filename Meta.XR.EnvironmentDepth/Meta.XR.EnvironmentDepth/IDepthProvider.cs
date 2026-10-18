using UnityEngine;

namespace Meta.XR.EnvironmentDepth;

internal interface IDepthProvider
{
	bool IsSupported { get; }

	bool RemoveHands { set; }

	void SetDepthEnabled(bool isEnabled, bool removeHands);

	bool TryGetUpdatedDepthTexture(out RenderTexture depthTexture, DepthFrameDesc[] frameDescriptors);
}
