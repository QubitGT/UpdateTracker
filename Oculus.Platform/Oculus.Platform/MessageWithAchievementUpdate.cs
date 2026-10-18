using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithAchievementUpdate : Message<AchievementUpdate>
{
	public MessageWithAchievementUpdate(IntPtr c_message)
		: base(c_message)
	{
	}

	public override AchievementUpdate GetAchievementUpdate()
	{
		return base.Data;
	}

	protected override AchievementUpdate GetDataFromMessage(IntPtr c_message)
	{
		return new AchievementUpdate(CAPI.ovr_Message_GetAchievementUpdate(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
