using System.Runtime.InteropServices;
using UnityEngine.XR.OpenXR.Features;

namespace Meta.XR;

public class MetaXRFoveationFeature : OpenXRFeature
{
	public const string extensionList = "XR_FB_foveation XR_FB_foveation_configuration XR_FB_foveation_vulkan ";

	public const string featureId = "com.meta.openxr.feature.foveation";

	private static ulong _xrSession;

	private static uint _foveatedRenderingLevel;

	private static uint _useDynamicFoveation;

	public static OVRManager.FoveatedRenderingLevel foveatedRenderingLevel
	{
		get
		{
			FBGetFoveationLevel(out var level);
			return (OVRManager.FoveatedRenderingLevel)level;
		}
		set
		{
			if (value == OVRManager.FoveatedRenderingLevel.HighTop)
			{
				_foveatedRenderingLevel = 3u;
			}
			else
			{
				_foveatedRenderingLevel = (uint)value;
			}
			FBSetFoveationLevel(_xrSession, _foveatedRenderingLevel, 0f, _useDynamicFoveation);
		}
	}

	public static bool useDynamicFoveatedRendering
	{
		get
		{
			FBGetFoveationLevel(out var level);
			return level != 0;
		}
		set
		{
			if (value)
			{
				_useDynamicFoveation = 1u;
			}
			else
			{
				_useDynamicFoveation = 0u;
			}
			FBSetFoveationLevel(_xrSession, _foveatedRenderingLevel, 0f, _useDynamicFoveation);
		}
	}

	protected override void OnSessionCreate(ulong xrSession)
	{
		_xrSession = xrSession;
	}

	[DllImport("UnityOpenXR")]
	private static extern void FBSetFoveationLevel(ulong session, uint level, float verticalOffset, uint dynamic);

	[DllImport("UnityOpenXR")]
	private static extern void FBGetFoveationLevel(out uint level);

	[DllImport("UnityOpenXR")]
	private static extern void FBGetFoveationDynamic(out uint dynamic);
}
