using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithAppDownloadProgressResult : Message<AppDownloadProgressResult>
{
	public MessageWithAppDownloadProgressResult(IntPtr c_message)
		: base(c_message)
	{
	}

	public override AppDownloadProgressResult GetAppDownloadProgressResult()
	{
		return base.Data;
	}

	protected override AppDownloadProgressResult GetDataFromMessage(IntPtr c_message)
	{
		return new AppDownloadProgressResult(CAPI.ovr_Message_GetAppDownloadProgressResult(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
