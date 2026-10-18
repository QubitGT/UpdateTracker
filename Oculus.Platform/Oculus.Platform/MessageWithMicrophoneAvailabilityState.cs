using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithMicrophoneAvailabilityState : Message<MicrophoneAvailabilityState>
{
	public MessageWithMicrophoneAvailabilityState(IntPtr c_message)
		: base(c_message)
	{
	}

	public override MicrophoneAvailabilityState GetMicrophoneAvailabilityState()
	{
		return base.Data;
	}

	protected override MicrophoneAvailabilityState GetDataFromMessage(IntPtr c_message)
	{
		return new MicrophoneAvailabilityState(CAPI.ovr_Message_GetMicrophoneAvailabilityState(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
