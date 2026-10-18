using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithPurchaseList : Message<PurchaseList>
{
	public MessageWithPurchaseList(IntPtr c_message)
		: base(c_message)
	{
	}

	public override PurchaseList GetPurchaseList()
	{
		return base.Data;
	}

	protected override PurchaseList GetDataFromMessage(IntPtr c_message)
	{
		return new PurchaseList(CAPI.ovr_Message_GetPurchaseArray(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
