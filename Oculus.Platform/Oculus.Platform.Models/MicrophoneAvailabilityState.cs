using System;

namespace Oculus.Platform.Models;

public class MicrophoneAvailabilityState
{
	public readonly bool MicrophoneAvailable;

	public MicrophoneAvailabilityState(IntPtr o)
	{
		MicrophoneAvailable = CAPI.ovr_MicrophoneAvailabilityState_GetMicrophoneAvailable(o);
	}
}
