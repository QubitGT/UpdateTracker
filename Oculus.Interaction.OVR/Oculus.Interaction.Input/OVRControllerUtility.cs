using Meta.XR.Util;

namespace Oculus.Interaction.Input;

[Feature(Feature.Interaction)]
public static class OVRControllerUtility
{
	public static float GetPinchAmount(OVRInput.Controller ovrController)
	{
		return OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, ovrController);
	}

	public static float GetIndexCurl(OVRInput.Controller ovrController)
	{
		if (SupportsAnalogIndex(ovrController))
		{
			return OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTriggerCurl, ovrController);
		}
		return (OVRInput.Get(OVRInput.NearTouch.PrimaryIndexTrigger, ovrController) || OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, ovrController) != 0f) ? 1 : 0;
	}

	public static float GetIndexSlide(OVRInput.Controller ovrController)
	{
		if (SupportsAnalogIndex(ovrController))
		{
			return OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTriggerSlide, ovrController);
		}
		return 0f;
	}

	private static bool SupportsAnalogIndex(OVRInput.Controller ovrController)
	{
		if (ovrController != OVRInput.Controller.LTouch && ovrController != OVRInput.Controller.RTouch)
		{
			return false;
		}
		return OVRInput.GetCurrentInteractionProfile((ovrController != OVRInput.Controller.LTouch) ? OVRInput.Hand.HandRight : OVRInput.Hand.HandLeft) != OVRInput.InteractionProfile.Touch;
	}
}
