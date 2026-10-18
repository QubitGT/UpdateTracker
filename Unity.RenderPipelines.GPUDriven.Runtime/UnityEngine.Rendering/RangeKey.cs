using System;

namespace UnityEngine.Rendering;

internal struct RangeKey : IEquatable<RangeKey>
{
	public byte layer;

	public uint renderingLayerMask;

	public MotionVectorGenerationMode motionMode;

	public ShadowCastingMode shadowCastingMode;

	public bool staticShadowCaster;

	public int rendererPriority;

	public bool supportsIndirect;

	public bool Equals(RangeKey other)
	{
		if (layer == other.layer && renderingLayerMask == other.renderingLayerMask && motionMode == other.motionMode && shadowCastingMode == other.shadowCastingMode && staticShadowCaster == other.staticShadowCaster && rendererPriority == other.rendererPriority)
		{
			return supportsIndirect == other.supportsIndirect;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (((int)((int)(((13 * 23 + layer) * 23 + (int)renderingLayerMask) * 23 + motionMode) * 23 + shadowCastingMode) * 23 + (staticShadowCaster ? 1 : 0)) * 23 + rendererPriority) * 23 + (supportsIndirect ? 1 : 0);
	}
}
