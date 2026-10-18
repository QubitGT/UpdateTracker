using System.Runtime.InteropServices;
using UnityEngine.XR.OpenXR.Features;

namespace Meta.XR;

public class MetaXRSpaceWarp : OpenXRFeature
{
	public const string extensionList = "XR_FB_space_warp";

	public const string featureId = "com.meta.openxr.feature.spacewarp";

	public static void SetSpaceWarp(bool enabled)
	{
		MetaSetSpaceWarp(enabled);
	}

	public static void SetAppSpacePosition(float x, float y, float z)
	{
		MetaSetAppSpacePosition(x, y, z);
	}

	public static void SetAppSpaceRotation(float x, float y, float z, float w)
	{
		MetaSetAppSpaceRotation(x, y, z, w);
	}

	[DllImport("UnityOpenXR")]
	private static extern void MetaSetSpaceWarp(bool enabled);

	[DllImport("UnityOpenXR")]
	private static extern void MetaSetAppSpacePosition(float x, float y, float z);

	[DllImport("UnityOpenXR")]
	private static extern void MetaSetAppSpaceRotation(float x, float y, float z, float w);
}
