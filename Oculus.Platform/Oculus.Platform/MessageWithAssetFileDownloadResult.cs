using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithAssetFileDownloadResult : Message<AssetFileDownloadResult>
{
	public MessageWithAssetFileDownloadResult(IntPtr c_message)
		: base(c_message)
	{
	}

	public override AssetFileDownloadResult GetAssetFileDownloadResult()
	{
		return base.Data;
	}

	protected override AssetFileDownloadResult GetDataFromMessage(IntPtr c_message)
	{
		return new AssetFileDownloadResult(CAPI.ovr_Message_GetAssetFileDownloadResult(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
