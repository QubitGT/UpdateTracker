using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithAppDownloadResult : Message<AppDownloadResult>
{
	public MessageWithAppDownloadResult(IntPtr c_message)
		: base(c_message)
	{
	}

	public override AppDownloadResult GetAppDownloadResult()
	{
		return base.Data;
	}

	protected override AppDownloadResult GetDataFromMessage(IntPtr c_message)
	{
		return new AppDownloadResult(CAPI.ovr_Message_GetAppDownloadResult(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
