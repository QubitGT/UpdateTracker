using System;
using UnityEngine.Experimental.Rendering;
using UnityEngine.XR;

namespace UnityEngine.Rendering;

[Serializable]
public class XRSRPSettings
{
	public static bool enabled => XRSettings.enabled;

	public static bool isDeviceActive
	{
		get
		{
			if (enabled)
			{
				return XRSettings.isDeviceActive;
			}
			return false;
		}
	}

	public static string loadedDeviceName
	{
		get
		{
			if (enabled)
			{
				return XRSettings.loadedDeviceName;
			}
			return "No XR device loaded";
		}
	}

	public static string[] supportedDevices
	{
		get
		{
			if (enabled)
			{
				return XRSettings.supportedDevices;
			}
			return new string[1];
		}
	}

	public static RenderTextureDescriptor eyeTextureDesc
	{
		get
		{
			if (enabled)
			{
				return XRSettings.eyeTextureDesc;
			}
			return new RenderTextureDescriptor(0, 0);
		}
	}

	public static int eyeTextureWidth
	{
		get
		{
			if (enabled)
			{
				return XRSettings.eyeTextureWidth;
			}
			return 0;
		}
	}

	public static int eyeTextureHeight
	{
		get
		{
			if (enabled)
			{
				return XRSettings.eyeTextureHeight;
			}
			return 0;
		}
	}

	public static float occlusionMeshScale
	{
		get
		{
			if (enabled)
			{
				return XRSystem.GetOcclusionMeshScale();
			}
			return 0f;
		}
		set
		{
			if (enabled)
			{
				XRSystem.SetOcclusionMeshScale(value);
			}
		}
	}

	public static bool useVisibilityMesh
	{
		get
		{
			if (enabled)
			{
				return XRSystem.GetUseVisibilityMesh();
			}
			return false;
		}
		set
		{
			if (enabled)
			{
				XRSystem.SetUseVisibilityMesh(value);
			}
		}
	}

	public static int mirrorViewMode
	{
		get
		{
			if (enabled)
			{
				return XRSystem.GetMirrorViewMode();
			}
			return 0;
		}
		set
		{
			if (enabled)
			{
				XRSystem.SetMirrorViewMode(value);
			}
		}
	}
}
