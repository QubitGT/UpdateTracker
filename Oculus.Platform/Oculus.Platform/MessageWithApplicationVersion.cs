using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithApplicationVersion : Message<ApplicationVersion>
{
	public MessageWithApplicationVersion(IntPtr c_message)
		: base(c_message)
	{
	}

	public override ApplicationVersion GetApplicationVersion()
	{
		return base.Data;
	}

	protected override ApplicationVersion GetDataFromMessage(IntPtr c_message)
	{
		return new ApplicationVersion(CAPI.ovr_Message_GetApplicationVersion(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
