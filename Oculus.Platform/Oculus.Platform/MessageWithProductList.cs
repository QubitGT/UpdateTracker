using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithProductList : Message<ProductList>
{
	public MessageWithProductList(IntPtr c_message)
		: base(c_message)
	{
	}

	public override ProductList GetProductList()
	{
		return base.Data;
	}

	protected override ProductList GetDataFromMessage(IntPtr c_message)
	{
		return new ProductList(CAPI.ovr_Message_GetProductArray(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
