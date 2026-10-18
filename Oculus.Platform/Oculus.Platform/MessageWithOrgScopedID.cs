using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithOrgScopedID : Message<OrgScopedID>
{
	public MessageWithOrgScopedID(IntPtr c_message)
		: base(c_message)
	{
	}

	public override OrgScopedID GetOrgScopedID()
	{
		return base.Data;
	}

	protected override OrgScopedID GetDataFromMessage(IntPtr c_message)
	{
		return new OrgScopedID(CAPI.ovr_Message_GetOrgScopedID(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
