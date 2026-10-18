using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithAssetFileDownloadCancelResult : Message<AssetFileDownloadCancelResult>
{
	public MessageWithAssetFileDownloadCancelResult(IntPtr c_message)
		: base(c_message)
	{
	}

	public override AssetFileDownloadCancelResult GetAssetFileDownloadCancelResult()
	{
		return base.Data;
	}

	protected override AssetFileDownloadCancelResult GetDataFromMessage(IntPtr c_message)
	{
		return new AssetFileDownloadCancelResult(CAPI.ovr_Message_GetAssetFileDownloadCancelResult(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
