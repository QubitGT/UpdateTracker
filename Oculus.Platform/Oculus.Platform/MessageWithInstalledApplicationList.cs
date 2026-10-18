using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithInstalledApplicationList : Message<InstalledApplicationList>
{
	public MessageWithInstalledApplicationList(IntPtr c_message)
		: base(c_message)
	{
	}

	public override InstalledApplicationList GetInstalledApplicationList()
	{
		return base.Data;
	}

	protected override InstalledApplicationList GetDataFromMessage(IntPtr c_message)
	{
		return new InstalledApplicationList(CAPI.ovr_Message_GetInstalledApplicationArray(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
