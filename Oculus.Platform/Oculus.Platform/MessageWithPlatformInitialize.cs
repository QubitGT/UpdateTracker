using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithPlatformInitialize : Message<PlatformInitialize>
{
	public MessageWithPlatformInitialize(IntPtr c_message)
		: base(c_message)
	{
	}

	public override PlatformInitialize GetPlatformInitialize()
	{
		return base.Data;
	}

	protected override PlatformInitialize GetDataFromMessage(IntPtr c_message)
	{
		return new PlatformInitialize(CAPI.ovr_Message_GetPlatformInitialize(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
