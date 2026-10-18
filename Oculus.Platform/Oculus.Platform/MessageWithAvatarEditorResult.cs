using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithAvatarEditorResult : Message<AvatarEditorResult>
{
	public MessageWithAvatarEditorResult(IntPtr c_message)
		: base(c_message)
	{
	}

	public override AvatarEditorResult GetAvatarEditorResult()
	{
		return base.Data;
	}

	protected override AvatarEditorResult GetDataFromMessage(IntPtr c_message)
	{
		return new AvatarEditorResult(CAPI.ovr_Message_GetAvatarEditorResult(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
