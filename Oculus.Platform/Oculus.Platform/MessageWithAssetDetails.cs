using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithAssetDetails : Message<AssetDetails>
{
	public MessageWithAssetDetails(IntPtr c_message)
		: base(c_message)
	{
	}

	public override AssetDetails GetAssetDetails()
	{
		return base.Data;
	}

	protected override AssetDetails GetDataFromMessage(IntPtr c_message)
	{
		return new AssetDetails(CAPI.ovr_Message_GetAssetDetails(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
