using System;
using UnityEngine;

public class OVRRuntimeSettings : OVRRuntimeAssetsBase
{
	private const string _assetName = "OculusRuntimeSettings";

	private static OVRRuntimeSettings _instance;

	private static readonly OVRHandSkeletonVersion NewProjectDefaultSkeletonVersion = OVRHandSkeletonVersion.OpenXR;

	[SerializeField]
	private OVRHandSkeletonVersion handSkeletonVersion = NewProjectDefaultSkeletonVersion;

	public OVRManager.ColorSpace colorSpace = OVRManager.ColorSpace.P3;

	[SerializeField]
	private bool requestsVisualFaceTracking = true;

	[SerializeField]
	private bool requestsAudioFaceTracking = true;

	[SerializeField]
	private bool enableFaceTrackingVisemesOutput;

	[SerializeField]
	private string telemetryProjectGuid;

	[SerializeField]
	private OVRPlugin.BodyTrackingFidelity2 bodyTrackingFidelity = OVRPlugin.BodyTrackingFidelity2.Low;

	[SerializeField]
	private OVRPlugin.BodyJointSet bodyTrackingJointSet;

	[SerializeField]
	private bool allowVisibilityMesh;

	public bool QuestVisibilityMeshOverriden;

	public OVRHandSkeletonVersion HandSkeletonVersion
	{
		get
		{
			return handSkeletonVersion;
		}
		set
		{
			handSkeletonVersion = value;
		}
	}

	public static OVRRuntimeSettings Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = GetRuntimeSettings();
			}
			return _instance;
		}
	}

	public bool RequestsVisualFaceTracking
	{
		get
		{
			return requestsVisualFaceTracking;
		}
		set
		{
			requestsVisualFaceTracking = value;
		}
	}

	public bool RequestsAudioFaceTracking
	{
		get
		{
			return requestsAudioFaceTracking;
		}
		set
		{
			requestsAudioFaceTracking = value;
		}
	}

	public bool EnableFaceTrackingVisemesOutput
	{
		get
		{
			return enableFaceTrackingVisemesOutput;
		}
		set
		{
			enableFaceTrackingVisemesOutput = value;
			OVRPlugin.SetFaceTrackingVisemesEnabled(enableFaceTrackingVisemesOutput);
		}
	}

	internal string TelemetryProjectGuid
	{
		get
		{
			if (string.IsNullOrEmpty(telemetryProjectGuid))
			{
				telemetryProjectGuid = Guid.NewGuid().ToString();
			}
			return telemetryProjectGuid;
		}
	}

	public OVRPlugin.BodyTrackingFidelity2 BodyTrackingFidelity
	{
		get
		{
			return bodyTrackingFidelity;
		}
		set
		{
			bodyTrackingFidelity = value;
		}
	}

	public OVRPlugin.BodyJointSet BodyTrackingJointSet
	{
		get
		{
			return bodyTrackingJointSet;
		}
		set
		{
			bodyTrackingJointSet = value;
		}
	}

	public bool VisibilityMesh
	{
		get
		{
			return allowVisibilityMesh;
		}
		set
		{
			allowVisibilityMesh = value;
		}
	}

	public static OVRRuntimeSettings GetRuntimeSettings()
	{
		OVRRuntimeAssetsBase.LoadAsset(out OVRRuntimeSettings assetInstance, "OculusRuntimeSettings", (Action<OVRRuntimeSettings>)HandleSettingsCreated);
		if (assetInstance == null)
		{
			Debug.LogWarning("Failed to load runtime settings. Using default runtime settings instead.");
			assetInstance = ScriptableObject.CreateInstance<OVRRuntimeSettings>();
			HandleSettingsCreated(assetInstance);
		}
		return assetInstance;
	}

	private static void HandleSettingsCreated(OVRRuntimeSettings settings)
	{
	}
}
