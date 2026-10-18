using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.Universal;

internal class XRPassUniversal : XRPass
{
	internal bool isLateLatchEnabled { get; set; }

	internal bool canMarkLateLatch { get; set; }

	internal bool hasMarkedLateLatch { get; set; }

	internal bool canFoveateIntermediatePasses { get; set; }

	public static XRPass Create(XRPassCreateInfo createInfo)
	{
		XRPassUniversal xRPassUniversal = GenericPool<XRPassUniversal>.Get();
		xRPassUniversal.InitBase(createInfo);
		xRPassUniversal.isLateLatchEnabled = false;
		xRPassUniversal.canMarkLateLatch = false;
		xRPassUniversal.hasMarkedLateLatch = false;
		xRPassUniversal.canFoveateIntermediatePasses = true;
		return xRPassUniversal;
	}

	public override void Release()
	{
		GenericPool<XRPassUniversal>.Release(this);
	}
}
