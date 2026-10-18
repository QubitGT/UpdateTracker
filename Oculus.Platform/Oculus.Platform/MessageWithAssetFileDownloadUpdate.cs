using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithAssetFileDownloadUpdate : Message<AssetFileDownloadUpdate>
{
	public MessageWithAssetFileDownloadUpdate(IntPtr c_message)
		: base(c_message)
	{
	}

	public override AssetFileDownloadUpdate GetAssetFileDownloadUpdate()
	{
		return base.Data;
	}

	protected override AssetFileDownloadUpdate GetDataFromMessage(IntPtr c_message)
	{
		return new AssetFileDownloadUpdate(CAPI.ovr_Message_GetAssetFileDownloadUpdate(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
