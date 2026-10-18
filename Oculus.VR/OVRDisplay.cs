using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.XR;

[HelpURL("https://developer.oculus.com/reference/unity/v67/class_o_v_r_display")]
public class OVRDisplay
{
	public struct EyeFov
	{
		public float UpFov;

		public float DownFov;

		public float LeftFov;

		public float RightFov;
	}

	public struct EyeRenderDesc
	{
		public Vector2 resolution;

		public Vector2 fov;

		public EyeFov fullFov;
	}

	public struct LatencyData
	{
		public float render;

		public float timeWarp;

		public float postPresent;

		public float renderError;

		public float timeWarpError;
	}

	protected bool needsConfigureTexture;

	protected EyeRenderDesc[] eyeDescs = new EyeRenderDesc[2];

	protected bool recenterRequested;

	protected int recenterRequestedFrameCount = int.MaxValue;

	protected int localTrackingSpaceRecenterCount;

	[Obsolete("Deprecated. Acceleration is not supported in OpenXR", false)]
	public Vector3 acceleration
	{
		get
		{
			if (!OVRManager.isHmdPresent)
			{
				return Vector3.zero;
			}
			Vector3 retVec = Vector3.zero;
			if (OVRNodeStateProperties.GetNodeStatePropertyVector3(XRNode.Head, NodeStatePropertyType.Acceleration, OVRPlugin.Node.Head, OVRPlugin.Step.Render, out retVec))
			{
				return retVec;
			}
			return Vector3.zero;
		}
	}

	[Obsolete("Deprecated. Acceleration is not supported in OpenXR", false)]
	public Vector3 angularAcceleration
	{
		get
		{
			if (!OVRManager.isHmdPresent)
			{
				return Vector3.zero;
			}
			Vector3 retVec = Vector3.zero;
			if (OVRNodeStateProperties.GetNodeStatePropertyVector3(XRNode.Head, NodeStatePropertyType.AngularAcceleration, OVRPlugin.Node.Head, OVRPlugin.Step.Render, out retVec))
			{
				return retVec;
			}
			return Vector3.zero;
		}
	}

	public Vector3 velocity
	{
		get
		{
			if (!OVRManager.isHmdPresent)
			{
				return Vector3.zero;
			}
			Vector3 retVec = Vector3.zero;
			if (OVRNodeStateProperties.GetNodeStatePropertyVector3(XRNode.Head, NodeStatePropertyType.Velocity, OVRPlugin.Node.Head, OVRPlugin.Step.Render, out retVec))
			{
				return retVec;
			}
			return Vector3.zero;
		}
	}

	public Vector3 angularVelocity
	{
		get
		{
			if (!OVRManager.isHmdPresent)
			{
				return Vector3.zero;
			}
			Vector3 retVec = Vector3.zero;
			if (OVRNodeStateProperties.GetNodeStatePropertyVector3(XRNode.Head, NodeStatePropertyType.AngularVelocity, OVRPlugin.Node.Head, OVRPlugin.Step.Render, out retVec))
			{
				return retVec;
			}
			return Vector3.zero;
		}
	}

	public LatencyData latency
	{
		get
		{
			if (!OVRManager.isHmdPresent)
			{
				return default(LatencyData);
			}
			string input = OVRPlugin.latency;
			Regex regex = new Regex("Render: ([0-9]+[.][0-9]+)ms, TimeWarp: ([0-9]+[.][0-9]+)ms, PostPresent: ([0-9]+[.][0-9]+)ms", RegexOptions.None);
			LatencyData result = default(LatencyData);
			Match match = regex.Match(input);
			if (match.Success)
			{
				result.render = float.Parse(match.Groups[1].Value);
				result.timeWarp = float.Parse(match.Groups[2].Value);
				result.postPresent = float.Parse(match.Groups[3].Value);
			}
			return result;
		}
	}

	public float appFramerate
	{
		get
		{
			if (!OVRManager.isHmdPresent)
			{
				return 0f;
			}
			return OVRPlugin.GetAppFramerate();
		}
	}

	public float[] displayFrequenciesAvailable => OVRPlugin.systemDisplayFrequenciesAvailable;

	public float displayFrequency
	{
		get
		{
			return OVRPlugin.systemDisplayFrequency;
		}
		set
		{
			OVRPlugin.systemDisplayFrequency = value;
		}
	}

	public event Action RecenteredPose;

	public OVRDisplay()
	{
		UpdateTextures();
	}

	public void Update()
	{
		UpdateTextures();
		if (recenterRequested && Time.frameCount > recenterRequestedFrameCount)
		{
			Debug.Log("Recenter event detected");
			if (this.RecenteredPose != null)
			{
				this.RecenteredPose();
			}
			recenterRequested = false;
			recenterRequestedFrameCount = int.MaxValue;
		}
		if (OVRPlugin.GetSystemHeadsetType() < OVRPlugin.SystemHeadset.Oculus_Quest || OVRPlugin.GetSystemHeadsetType() >= OVRPlugin.SystemHeadset.Rift_DK1)
		{
			return;
		}
		int num = OVRPlugin.GetLocalTrackingSpaceRecenterCount();
		if (localTrackingSpaceRecenterCount != num)
		{
			Debug.Log("Recenter event detected");
			if (this.RecenteredPose != null)
			{
				this.RecenteredPose();
			}
			localTrackingSpaceRecenterCount = num;
		}
	}

	public void RecenterPose()
	{
		OVRManager.GetCurrentInputSubsystem()?.TryRecenter();
		recenterRequested = true;
		recenterRequestedFrameCount = Time.frameCount;
		OVRMixedReality.RecenterPose();
	}

	public EyeRenderDesc GetEyeRenderDesc(XRNode eye)
	{
		return eyeDescs[(int)eye];
	}

	protected void UpdateTextures()
	{
		ConfigureEyeDesc(XRNode.LeftEye);
		ConfigureEyeDesc(XRNode.RightEye);
	}

	protected void ConfigureEyeDesc(XRNode eye)
	{
		if (OVRManager.isHmdPresent)
		{
			int eyeTextureWidth = XRSettings.eyeTextureWidth;
			int eyeTextureHeight = XRSettings.eyeTextureHeight;
			eyeDescs[(int)eye] = default(EyeRenderDesc);
			eyeDescs[(int)eye].resolution = new Vector2(eyeTextureWidth, eyeTextureHeight);
			if (OVRPlugin.GetNodeFrustum2((OVRPlugin.Node)eye, out var frustum))
			{
				eyeDescs[(int)eye].fullFov.LeftFov = 57.29578f * Mathf.Atan(frustum.Fov.LeftTan);
				eyeDescs[(int)eye].fullFov.RightFov = 57.29578f * Mathf.Atan(frustum.Fov.RightTan);
				eyeDescs[(int)eye].fullFov.UpFov = 57.29578f * Mathf.Atan(frustum.Fov.UpTan);
				eyeDescs[(int)eye].fullFov.DownFov = 57.29578f * Mathf.Atan(frustum.Fov.DownTan);
			}
			else
			{
				OVRPlugin.Frustumf eyeFrustum = OVRPlugin.GetEyeFrustum((OVRPlugin.Eye)eye);
				eyeDescs[(int)eye].fullFov.LeftFov = 57.29578f * eyeFrustum.fovX * 0.5f;
				eyeDescs[(int)eye].fullFov.RightFov = 57.29578f * eyeFrustum.fovX * 0.5f;
				eyeDescs[(int)eye].fullFov.UpFov = 57.29578f * eyeFrustum.fovY * 0.5f;
				eyeDescs[(int)eye].fullFov.DownFov = 57.29578f * eyeFrustum.fovY * 0.5f;
			}
			float num = Mathf.Max(eyeDescs[(int)eye].fullFov.LeftFov, eyeDescs[(int)eye].fullFov.RightFov);
			float num2 = Mathf.Max(eyeDescs[(int)eye].fullFov.UpFov, eyeDescs[(int)eye].fullFov.DownFov);
			eyeDescs[(int)eye].fov.x = num * 2f;
			eyeDescs[(int)eye].fov.y = num2 * 2f;
			if (!OVRPlugin.AsymmetricFovEnabled)
			{
				eyeDescs[(int)eye].fullFov.LeftFov = num;
				eyeDescs[(int)eye].fullFov.RightFov = num;
				eyeDescs[(int)eye].fullFov.UpFov = num2;
				eyeDescs[(int)eye].fullFov.DownFov = num2;
			}
		}
	}
}
