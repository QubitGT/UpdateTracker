public static class OVRMixedRealityCaptureConfigurationExtensions
{
	public static void ApplyTo(this OVRMixedRealityCaptureConfiguration dest, OVRMixedRealityCaptureConfiguration source)
	{
		dest.ReadFrom(source);
	}

	public static void ReadFrom(this OVRMixedRealityCaptureConfiguration dest, OVRMixedRealityCaptureConfiguration source)
	{
		dest.enableMixedReality = source.enableMixedReality;
		dest.compositionMethod = source.compositionMethod;
		dest.extraHiddenLayers = source.extraHiddenLayers;
		dest.externalCompositionBackdropColorRift = source.externalCompositionBackdropColorRift;
		dest.externalCompositionBackdropColorQuest = source.externalCompositionBackdropColorQuest;
		dest.flipCameraFrameHorizontally = source.flipCameraFrameHorizontally;
		dest.flipCameraFrameVertically = source.flipCameraFrameVertically;
		dest.handPoseStateLatency = source.handPoseStateLatency;
		dest.sandwichCompositionRenderLatency = source.sandwichCompositionRenderLatency;
		dest.sandwichCompositionBufferedFrames = source.sandwichCompositionBufferedFrames;
		dest.chromaKeyColor = source.chromaKeyColor;
		dest.chromaKeySimilarity = source.chromaKeySimilarity;
		dest.chromaKeySmoothRange = source.chromaKeySmoothRange;
		dest.chromaKeySpillRange = source.chromaKeySpillRange;
		dest.useDynamicLighting = source.useDynamicLighting;
		dest.dynamicLightingSmoothFactor = source.dynamicLightingSmoothFactor;
		dest.dynamicLightingDepthVariationClampingValue = source.dynamicLightingDepthVariationClampingValue;
		dest.virtualGreenScreenTopY = source.virtualGreenScreenTopY;
		dest.virtualGreenScreenBottomY = source.virtualGreenScreenBottomY;
		dest.virtualGreenScreenApplyDepthCulling = source.virtualGreenScreenApplyDepthCulling;
		dest.virtualGreenScreenDepthTolerance = source.virtualGreenScreenDepthTolerance;
		dest.mrcActivationMode = source.mrcActivationMode;
		dest.instantiateMixedRealityCameraGameObject = source.instantiateMixedRealityCameraGameObject;
		dest.capturingCameraDevice = source.capturingCameraDevice;
		dest.depthQuality = source.depthQuality;
		dest.virtualGreenScreenType = source.virtualGreenScreenType;
	}
}
