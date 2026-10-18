using System;
using System.IO;
using UnityEngine;

public class OVRMixedRealityCaptureSettings : ScriptableObject, OVRMixedRealityCaptureConfiguration
{
	public bool enableMixedReality;

	public LayerMask extraHiddenLayers;

	public LayerMask extraVisibleLayers;

	public bool dynamicCullingMask = true;

	public OVRManager.CompositionMethod compositionMethod;

	public Color externalCompositionBackdropColorRift = Color.green;

	public Color externalCompositionBackdropColorQuest = Color.clear;

	[Obsolete("Deprecated", false)]
	public OVRManager.CameraDevice capturingCameraDevice;

	public bool flipCameraFrameHorizontally;

	public bool flipCameraFrameVertically;

	public float handPoseStateLatency;

	public float sandwichCompositionRenderLatency;

	public int sandwichCompositionBufferedFrames = 8;

	public Color chromaKeyColor = Color.green;

	public float chromaKeySimilarity = 0.6f;

	public float chromaKeySmoothRange = 0.03f;

	public float chromaKeySpillRange = 0.04f;

	public bool useDynamicLighting;

	[Obsolete("Deprecated", false)]
	public OVRManager.DepthQuality depthQuality = OVRManager.DepthQuality.Medium;

	public float dynamicLightingSmoothFactor = 8f;

	public float dynamicLightingDepthVariationClampingValue = 0.001f;

	[Obsolete("Deprecated", false)]
	public OVRManager.VirtualGreenScreenType virtualGreenScreenType;

	public float virtualGreenScreenTopY;

	public float virtualGreenScreenBottomY;

	public bool virtualGreenScreenApplyDepthCulling;

	public float virtualGreenScreenDepthTolerance = 0.2f;

	public OVRManager.MrcActivationMode mrcActivationMode;

	private const string configFileName = "mrc.config";

	bool OVRMixedRealityCaptureConfiguration.enableMixedReality
	{
		get
		{
			return enableMixedReality;
		}
		set
		{
			enableMixedReality = value;
		}
	}

	LayerMask OVRMixedRealityCaptureConfiguration.extraHiddenLayers
	{
		get
		{
			return extraHiddenLayers;
		}
		set
		{
			extraHiddenLayers = value;
		}
	}

	LayerMask OVRMixedRealityCaptureConfiguration.extraVisibleLayers
	{
		get
		{
			return extraVisibleLayers;
		}
		set
		{
			extraVisibleLayers = value;
		}
	}

	bool OVRMixedRealityCaptureConfiguration.dynamicCullingMask
	{
		get
		{
			return dynamicCullingMask;
		}
		set
		{
			dynamicCullingMask = value;
		}
	}

	OVRManager.CompositionMethod OVRMixedRealityCaptureConfiguration.compositionMethod
	{
		get
		{
			return compositionMethod;
		}
		set
		{
			compositionMethod = value;
		}
	}

	Color OVRMixedRealityCaptureConfiguration.externalCompositionBackdropColorRift
	{
		get
		{
			return externalCompositionBackdropColorRift;
		}
		set
		{
			externalCompositionBackdropColorRift = value;
		}
	}

	Color OVRMixedRealityCaptureConfiguration.externalCompositionBackdropColorQuest
	{
		get
		{
			return externalCompositionBackdropColorQuest;
		}
		set
		{
			externalCompositionBackdropColorQuest = value;
		}
	}

	[Obsolete("Deprecated", false)]
	OVRManager.CameraDevice OVRMixedRealityCaptureConfiguration.capturingCameraDevice
	{
		get
		{
			return capturingCameraDevice;
		}
		set
		{
			capturingCameraDevice = value;
		}
	}

	bool OVRMixedRealityCaptureConfiguration.flipCameraFrameHorizontally
	{
		get
		{
			return flipCameraFrameHorizontally;
		}
		set
		{
			flipCameraFrameHorizontally = value;
		}
	}

	bool OVRMixedRealityCaptureConfiguration.flipCameraFrameVertically
	{
		get
		{
			return flipCameraFrameVertically;
		}
		set
		{
			flipCameraFrameVertically = value;
		}
	}

	float OVRMixedRealityCaptureConfiguration.handPoseStateLatency
	{
		get
		{
			return handPoseStateLatency;
		}
		set
		{
			handPoseStateLatency = value;
		}
	}

	float OVRMixedRealityCaptureConfiguration.sandwichCompositionRenderLatency
	{
		get
		{
			return sandwichCompositionRenderLatency;
		}
		set
		{
			sandwichCompositionRenderLatency = value;
		}
	}

	int OVRMixedRealityCaptureConfiguration.sandwichCompositionBufferedFrames
	{
		get
		{
			return sandwichCompositionBufferedFrames;
		}
		set
		{
			sandwichCompositionBufferedFrames = value;
		}
	}

	Color OVRMixedRealityCaptureConfiguration.chromaKeyColor
	{
		get
		{
			return chromaKeyColor;
		}
		set
		{
			chromaKeyColor = value;
		}
	}

	float OVRMixedRealityCaptureConfiguration.chromaKeySimilarity
	{
		get
		{
			return chromaKeySimilarity;
		}
		set
		{
			chromaKeySimilarity = value;
		}
	}

	float OVRMixedRealityCaptureConfiguration.chromaKeySmoothRange
	{
		get
		{
			return chromaKeySmoothRange;
		}
		set
		{
			chromaKeySmoothRange = value;
		}
	}

	float OVRMixedRealityCaptureConfiguration.chromaKeySpillRange
	{
		get
		{
			return chromaKeySpillRange;
		}
		set
		{
			chromaKeySpillRange = value;
		}
	}

	bool OVRMixedRealityCaptureConfiguration.useDynamicLighting
	{
		get
		{
			return useDynamicLighting;
		}
		set
		{
			useDynamicLighting = value;
		}
	}

	[Obsolete("Deprecated", false)]
	OVRManager.DepthQuality OVRMixedRealityCaptureConfiguration.depthQuality
	{
		get
		{
			return depthQuality;
		}
		set
		{
			depthQuality = value;
		}
	}

	float OVRMixedRealityCaptureConfiguration.dynamicLightingSmoothFactor
	{
		get
		{
			return dynamicLightingSmoothFactor;
		}
		set
		{
			dynamicLightingSmoothFactor = value;
		}
	}

	float OVRMixedRealityCaptureConfiguration.dynamicLightingDepthVariationClampingValue
	{
		get
		{
			return dynamicLightingDepthVariationClampingValue;
		}
		set
		{
			dynamicLightingDepthVariationClampingValue = value;
		}
	}

	[Obsolete("Deprecated", false)]
	OVRManager.VirtualGreenScreenType OVRMixedRealityCaptureConfiguration.virtualGreenScreenType
	{
		get
		{
			return virtualGreenScreenType;
		}
		set
		{
			virtualGreenScreenType = value;
		}
	}

	float OVRMixedRealityCaptureConfiguration.virtualGreenScreenTopY
	{
		get
		{
			return virtualGreenScreenTopY;
		}
		set
		{
			virtualGreenScreenTopY = value;
		}
	}

	float OVRMixedRealityCaptureConfiguration.virtualGreenScreenBottomY
	{
		get
		{
			return virtualGreenScreenBottomY;
		}
		set
		{
			virtualGreenScreenBottomY = value;
		}
	}

	bool OVRMixedRealityCaptureConfiguration.virtualGreenScreenApplyDepthCulling
	{
		get
		{
			return virtualGreenScreenApplyDepthCulling;
		}
		set
		{
			virtualGreenScreenApplyDepthCulling = value;
		}
	}

	float OVRMixedRealityCaptureConfiguration.virtualGreenScreenDepthTolerance
	{
		get
		{
			return virtualGreenScreenDepthTolerance;
		}
		set
		{
			virtualGreenScreenDepthTolerance = value;
		}
	}

	OVRManager.MrcActivationMode OVRMixedRealityCaptureConfiguration.mrcActivationMode
	{
		get
		{
			return mrcActivationMode;
		}
		set
		{
			mrcActivationMode = value;
		}
	}

	OVRManager.InstantiateMrcCameraDelegate OVRMixedRealityCaptureConfiguration.instantiateMixedRealityCameraGameObject { get; set; }

	public void WriteToConfigurationFile()
	{
		string contents = JsonUtility.ToJson(this, prettyPrint: true);
		try
		{
			string text = Path.Combine(Application.dataPath, "mrc.config");
			Debug.Log("Write OVRMixedRealityCaptureSettings to " + text);
			File.WriteAllText(text, contents);
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Exception caught " + ex.Message);
		}
	}

	public void CombineWithConfigurationFile()
	{
		try
		{
			string text = Path.Combine(Application.dataPath, "mrc.config");
			if (File.Exists(text))
			{
				Debug.Log("MixedRealityCapture configuration file found at " + text);
				string json = File.ReadAllText(text);
				Debug.Log("Apply MixedRealityCapture configuration");
				JsonUtility.FromJsonOverwrite(json, this);
			}
			else
			{
				Debug.Log("MixedRealityCapture configuration file doesn't exist at " + text);
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Exception caught " + ex.Message);
		}
	}
}
