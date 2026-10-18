using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithAssetDetailsList : Message<AssetDetailsList>
{
	public MessageWithAssetDetailsList(IntPtr c_message)
		: base(c_message)
	{
	}

	public override AssetDetailsList GetAssetDetailsList()
	{
		return base.Data;
	}

	protected override AssetDetailsList GetDataFromMessage(IntPtr c_message)
	{
		return new AssetDetailsList(CAPI.ovr_Message_GetAssetDetailsArray(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
